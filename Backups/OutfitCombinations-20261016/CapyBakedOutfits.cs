using UnityEngine;
using UnityEngine.UI;

namespace Capybara67.Idle {
    public sealed partial class CapyIdleGame {
        // Complete character illustrations. These replace the body and the wearable together.
        static readonly Rect[] ScarfCrops={
            new Rect(53,214,359,208),new Rect(512,87,271,315),new Rect(897,55,323,352),
            new Rect(42,522,409,249),new Rect(497,439,322,336),new Rect(909,458,314,314),
            new Rect(26,834,416,338),new Rect(463,841,445,334),new Rect(926,819,310,363)
        };
        static readonly Sprite[] ScarfPortraits=new Sprite[9];

        bool TryDrawBakedOutfit(Transform root,string[] selected,int stage,int form){
            // Unillustrated combinations keep their current behaviour until their full artwork exists.
            if(selected[1]!="scarf"||!string.IsNullOrEmpty(selected[0])||selected[5]!="classic"||form>=0||!string.IsNullOrEmpty(selected[4])||!string.IsNullOrEmpty(selected[3]))return false;
            var texture=Resources.Load<Texture2D>("Outfits/Scarf");if(!texture)return false;
            int index=stage-1;Rect crop=ScarfCrops[index],original=LifeCrops[index],frame=LifeFrames[index];
            if(!ScarfPortraits[index]){
                float scale=texture.width/1254f;
                ScarfPortraits[index]=Sprite.Create(texture,new Rect(crop.x*scale,texture.height-crop.yMax*scale,crop.width*scale,crop.height*scale),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                ScarfPortraits[index].name="Complete scarf outfit stage "+stage;
            }
            Shape(root,"Ground shadow",16,241,216,9,"65432D20",0,true);
            var pose=Rect(root,"Illustrated pose",stage==8?240:0,0,240,250);if(stage==8)pose.localScale=new Vector3(-1,1,1);
            float sx=frame.width/original.width,sy=frame.height/original.height;
            var image=Rect(pose,"Life sprite",frame.x+(crop.x-original.x)*sx,frame.y+(crop.y-original.y)*sy,crop.width*sx,crop.height*sy).gameObject.AddComponent<Image>();
            image.sprite=ScarfPortraits[index];image.raycastTarget=false;
            // A marker for review tooling; the scarf is baked into this image, never overlaid.
            image.gameObject.AddComponent<CapyBakedOutfit>().outfitId="scarf";
            if(stage==8){image.rectTransform.anchoredPosition+=new Vector2(image.rectTransform.rect.width,0);image.transform.localScale=new Vector3(-1,1,1);}
            if(root==hero){livingPose=pose;poseOrigin=pose.anchoredPosition;portraitMotion=image.gameObject.AddComponent<CapyPortraitMotion>();portraitMotion.Configure(crop,LifeEyes[index]);}
            return true;
        }
    }
    public sealed class CapyBakedOutfit:MonoBehaviour { public string outfitId; }
}
