using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Capybara67.Idle {
    // Explicit fields keep JSON independent of Unity Rect's internal serialized field names.
    [Serializable] public struct CapyOutfitRectangle {
        public float x,y,width,height;
        public static implicit operator Rect(CapyOutfitRectangle value) => new Rect(value.x,value.y,value.width,value.height);
    }
    [Serializable] public sealed class CapyOutfitStage {
        public CapyOutfitRectangle crop, frame;
        public Vector4[] eyes;
    }
    [Serializable] public sealed class CapyOutfitEntry {
        public string head, clothes, skin, resource;
        public int form = -1;
        public float sourceWidth, sourceHeight;
        public CapyOutfitStage[] stages;
        public string Key => CapyOutfitCatalog.Key(head, clothes, skin, form);
    }
    [Serializable] public sealed class CapyOutfitCatalog {
        public CapyOutfitEntry[] outfits;
        static CapyOutfitCatalog catalog;
        static Dictionary<string, CapyOutfitEntry> lookup;
        // Fast Enter Play Mode can keep static fields alive across editor runs.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCatalog() { catalog = null; lookup = null; }
        public static string Key(string head, string clothes, string skin, int form) =>
            (head ?? "") + "|" + (clothes ?? "") + "|" + skin + "|" + form;
        public static CapyOutfitEntry[] Entries {
            get {
                if (catalog == null) {
                    var json = Resources.Load<TextAsset>("Outfits/Catalog");
                    catalog = json ? JsonUtility.FromJson<CapyOutfitCatalog>(json.text) : new CapyOutfitCatalog();
                    if (catalog.outfits == null) catalog.outfits = Array.Empty<CapyOutfitEntry>();
                    var complete=Resources.Load<TextAsset>("CompleteSets/Catalog");
                    if(complete) {
                        var extra=JsonUtility.FromJson<CapyOutfitCatalog>(complete.text);
                        var all=new List<CapyOutfitEntry>(catalog.outfits);all.AddRange(extra.outfits);catalog.outfits=all.ToArray();
                    }
                    lookup = new Dictionary<string, CapyOutfitEntry>();
                    foreach (var entry in catalog.outfits) {
                        if(entry.stages==null||entry.stages.Length!=IdleModel.StageCount||entry.sourceWidth<=0||entry.sourceHeight<=0)
                            throw new InvalidOperationException("Incomplete outfit atlas: "+entry.resource);
                        foreach(var stage in entry.stages) {
                            Rect crop=stage.crop,frame=stage.frame;
                            if(crop.width<=0||crop.height<=0||crop.x<0||crop.y<0||crop.xMax>entry.sourceWidth||crop.yMax>entry.sourceHeight||frame.width<=0||frame.height<=0||stage.eyes==null)
                                throw new InvalidOperationException("Invalid outfit coordinates: "+entry.resource);
                        }
                        lookup.Add(entry.Key, entry);
                    }
                }
                return catalog.outfits;
            }
        }
        public static CapyOutfitEntry Find(string head, string clothes, string skin, int form) {
            _ = Entries;
            if(CapyCompleteSets.IsSet(head)){clothes="";skin="classic";form=-1;}
            lookup.TryGetValue(Key(head, clothes, skin, form), out var entry);
            return entry;
        }
    }

    // Only atlases currently shown by a character or shop preview occupy texture memory.
    // A lease prevents deferred destruction of old UI from invalidating the new portrait.
    internal sealed class CapyOutfitAtlas {
        static readonly Dictionary<string, CapyOutfitAtlas> live = new Dictionary<string, CapyOutfitAtlas>();
        readonly Texture2D texture;
        readonly Sprite[] sprites = new Sprite[IdleModel.StageCount];
        readonly CapyOutfitEntry entry;
        Texture2D furMask;
        int users;
        CapyOutfitAtlas(CapyOutfitEntry entry, Texture2D texture) { this.entry = entry; this.texture = texture; }
        internal static CapyOutfitAtlas Acquire(CapyOutfitEntry entry) {
            if (!live.TryGetValue(entry.Key, out var atlas)) {
                var texture = Resources.Load<Texture2D>(entry.resource);
                if (!texture) return null;
                atlas = new CapyOutfitAtlas(entry, texture); live.Add(entry.Key, atlas);
            }
            atlas.users++;
            return atlas;
        }
        internal Sprite Portrait(int index) {
            if (!sprites[index]) {
                Rect crop = entry.stages[index].crop;
                float sx = texture.width / entry.sourceWidth, sy = texture.height / entry.sourceHeight;
                sprites[index] = Sprite.Create(texture, new Rect(crop.x*sx, texture.height-crop.yMax*sy,
                    crop.width*sx, crop.height*sy), new Vector2(.5f,.5f), 100, 0, SpriteMeshType.FullRect);
                sprites[index].name = entry.Key + " stage " + (index+1);
            }
            return sprites[index];
        }
        internal Texture2D FurMask {
            get {
                if(furMask)return furMask;
                if(!CapyCompleteSets.IsSet(entry.head))return null;
                string slug=entry.resource.Substring(entry.resource.LastIndexOf('/')+1);
                var data=Resources.Load<TextAsset>("CompleteSets/Masks/"+slug);
                if(!data)throw new InvalidOperationException("Missing fur selection: "+slug);
                try{
                    using(var reader=new BinaryReader(new MemoryStream(data.bytes))){
                        if(reader.ReadInt32()!=0x43504631)throw new InvalidDataException("Invalid fur selection header");
                        int w=reader.ReadInt32(),h=reader.ReadInt32();
                        if(w!=entry.sourceWidth||h!=entry.sourceHeight)throw new InvalidDataException("Fur selection dimensions differ from portrait");
                        var pixels=new byte[w*h];
                        while(reader.BaseStream.Position<reader.BaseStream.Length){
                            int start=reader.ReadInt32(),count=reader.ReadInt32();
                            if(start<0||count<1||count>pixels.Length-start)throw new InvalidDataException("Invalid fur selection run");
                            for(int n=start;n<start+count;n++)pixels[(h-1-n/w)*w+n%w]=255;
                        }
                        furMask=new Texture2D(w,h,TextureFormat.R8,false,true){name="Fur only: "+slug,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
                        furMask.LoadRawTextureData(pixels);furMask.Apply(false,true);
                    }
                }finally{Resources.UnloadAsset(data);}
                return furMask;
            }
        }
        internal void Release() {
            if (--users > 0) return;
            live.Remove(entry.Key);
            foreach (var sprite in sprites) if (sprite) UnityEngine.Object.Destroy(sprite);
            if(furMask)UnityEngine.Object.Destroy(furMask);
            Resources.UnloadAsset(texture);
        }
    }

    public sealed partial class CapyIdleGame {
        bool TryDrawBakedOutfit(Transform root, string[] selected, int stage, int form) {
            var entry = CapyOutfitCatalog.Find(selected[0], selected[1], selected[5], form);
            if (entry == null) {
                if(CapyCompleteSets.IsSet(selected[0]))throw new InvalidOperationException("Missing complete set: "+selected[0]);
                return false;
            }
            var atlas = CapyOutfitAtlas.Acquire(entry);
            if (atlas == null) return false;
            var art = entry.stages[stage-1];
            Shape(root,"Ground shadow",16,241,216,9,"65432D20",0,true);
            var pose = Rect(root,"Illustrated pose",0,0,240,250);
            
            Rect f = art.frame;
            var image = Rect(pose,"Life sprite",f.x,f.y,f.width,f.height).gameObject.AddComponent<Image>();
            image.sprite = atlas.Portrait(stage-1); image.raycastTarget = false;
            var marker = image.gameObject.AddComponent<CapyBakedOutfit>();
            marker.outfitId = entry.clothes; marker.headId = entry.head; marker.key = entry.Key; marker.atlas = atlas;
            
            var motion=image.gameObject.AddComponent<CapyPortraitMotion>();
            motion.Configure(art.crop,art.eyes);
            var skin=IdleCatalog.Find(selected[5]);
            if(CapyCompleteSets.IsSet(entry.head)&&skin!=null)motion.SetOutfitSkin(skin.art,atlas.FurMask);
            if (root==hero) {
                livingPose=pose; poseOrigin=pose.anchoredPosition;
                portraitMotion=motion;
            }
            var emotionAnchor=CapyCompleteSets.IsSet(entry.head)?new Rect(f.x+4,f.y+f.height*.28f,f.width,f.height*.4f):LifeHeads[IdleModel.SourcePose(stage)-1];
            LifeEmotion(pose,IdleCatalog.Find(selected[4]),emotionAnchor);
            LifeAura(root,IdleCatalog.Find(selected[3]));
            return true;
        }
    }
    public sealed class CapyBakedOutfit : MonoBehaviour {
        public string outfitId, headId, key;
        internal CapyOutfitAtlas atlas;
        void OnDestroy() { atlas?.Release(); atlas=null; }
    }
}

