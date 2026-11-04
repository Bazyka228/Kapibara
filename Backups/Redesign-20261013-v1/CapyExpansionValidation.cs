using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Capybara67.Editor
{
    [InitializeOnLoad]
    public static class CapyExpansionValidation
    {
        const string RequestPath="Temp/Capy67-expansion-check-request";
        static CapyExpansionValidation(){EditorApplication.update+=BackgroundRequest;}
        // Explicit local test request; never runs on a normal project open.
        static void BackgroundRequest()
        {
            if(!File.Exists(RequestPath) || running || EditorApplication.isCompiling || EditorApplication.isUpdating)return;
            if(!EditorApplication.isPlaying){
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                SessionState.SetBool("Capy67.autoChecks",true);SessionState.SetBool("Capy67.oldBackground",Application.runInBackground);
                Application.runInBackground=true;EditorApplication.EnterPlaymode();return;
            }
            var g=UnityEngine.Object.FindAnyObjectByType<CapyGame>();if(!g || Get(g,"progress")==null)return;
            File.Delete(RequestPath);Run();
        }
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Get(CapyGame g,string n)=>typeof(CapyGame).GetField(n,F).GetValue(g);
        static void Set(CapyGame g,string n,object v)=>typeof(CapyGame).GetField(n,F).SetValue(g,v);
        static void Call(CapyGame g,string n,params object[] a)=>typeof(CapyGame).GetMethod(n,F).Invoke(g,a);
        static int checks;static bool running;
        static void Check(bool ok,string message){if(!ok)throw new Exception("Expansion: "+message);checks++;}
        static bool Clickable(Button b)
        {
            Canvas.ForceUpdateCanvases();var r=(RectTransform)b.transform;var p=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);return hits.Count>0 && hits[0].gameObject==b.gameObject;
        }
        [MenuItem("Capybara 67/Run expansion checks")]
        public static void Run(){if(!EditorApplication.isPlaying || running){Debug.LogWarning("Enter Play Mode first; do not run checks twice.");return;}var g=UnityEngine.Object.FindAnyObjectByType<CapyGame>();g.StartCoroutine(Checks(g));}
        static IEnumerator Checks(CapyGame g)
        {
            running=true;checks=0;
            var previous=(CapyProgress)Get(g,"progress");int wallet=(int)Get(g,"pearls"),best=(int)Get(g,"best");
            bool oldSound=(bool)Get(g,"sound");Set(g,"sound",false);
            string[] keys={"capy67.save.v1","capy67.progress.v2"};string[] saves={PlayerPrefs.GetString(keys[0],""),PlayerPrefs.GetString(keys[1],"")};
            var m=(RoundModel)Get(g,"round");
            try {
                Set(g,"paused",false);Set(g,"platformPaused",false);m.Tick(67);
                ((GameObject)Get(g,"collectionPanel")).SetActive(false);((GameObject)Get(g,"resultPanel")).SetActive(false);
                var p=new CapyProgress();p.Repair();Set(g,"progress",p);Set(g,"pearls",0);
                Check(Clickable((Button)Get(g,"collectionButton")),"wardrobe pointer");Check(Clickable((Button)Get(g,"tapButton")),"tap area pointer");
                Call(g,"Action");for(int n=0;n<7;n++){m.Tick(m.FlightDuration);Call(g,"Tap");}Check(m.Score==207 && m.Spark==3,"seven and spark");
                Call(g,"TogglePause");float time=m.Remaining;Call(g,"Update");Check(m.Remaining==time,"pause freezes game");Call(g,"TogglePause");
                Call(g,"PlatformPause",true);int life=m.Lives;Call(g,"Tap");Call(g,"Update");Check(m.Lives==life && m.Remaining==time,"platform pause");Call(g,"PlatformPause",false);
                for(int n=0;n<3;n++)Call(g,"Tap");Check(!m.Running && p.rounds==1 && p.Has("panama"),"finish and first gift");int earned=(int)Get(g,"pearls");Call(g,"Finish");Check((int)Get(g,"pearls")==earned && p.rounds==1,"settlement once");
                Call(g,"ChangeOutfit");yield return null;
                Check(g.CollectionOpen,"new wardrobe opens");Set(g,"selectedId","gold");Set(g,"category",ItemSlot.Skin);Call(g,"RefreshCollection");Check(!((Button)Get(g,"equipItemButton")).interactable,"legendary locked");
                Set(g,"selectedId","cap");Set(g,"category",ItemSlot.Head);Set(g,"pearls",40);Call(g,"EquipSelected");Check(p.Has("cap") && p.equipped[0]=="cap" && (int)Get(g,"pearls")==20,"cosmetic purchase");Call(g,"EquipSelected");Check((int)Get(g,"pearls")==20,"equip does not charge twice");
                Call(g,"Persist");var loaded=JsonUtility.FromJson<CapyProgress>(PlayerPrefs.GetString(keys[1]));Check(loaded.Has("cap") && loaded.equipped[0]=="cap","save round trip");
                foreach(var item in CapyCatalog.Items){
                    p.Grant(item.id);p.Equip(item.id);Set(g,"selectedId",item.id);Set(g,"category",item.slot);Call(g,"RefreshCollection");
                    yield return null;Check(((RectTransform)Get(g,"previewHolder")).childCount>0,"preview "+item.id);
                }
                for(int n=0;n<3;n++){p.silhouette=n;Call(g,"ApplyAppearance",new object[]{null});Check(((RectTransform)Get(g,"capy")).gameObject.activeSelf,"silhouette "+n);}
                Check(CapyCatalog.Items.Length==52,"complete catalog");
                Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Capy67-expansion-checks.txt","PASS: "+checks+" Unity integration assertions. Raycasts, input, pauses, gifts, purchase, save and all 52 previews. User profile restored in finally.\n");
                File.WriteAllText("Logs/Capy67-expansion-events.txt",g.ExportJournal());
                Debug.Log("Capy67 EXPANSION PASS: "+checks);
            } finally {
                for(int n=0;n<keys.Length;n++){if(saves[n]=="")PlayerPrefs.DeleteKey(keys[n]);else PlayerPrefs.SetString(keys[n],saves[n]);}PlayerPrefs.Save();
                Set(g,"progress",previous);Set(g,"pearls",wallet);Set(g,"best",best);Set(g,"sound",oldSound);m.Tick(67);Set(g,"settled",true);
                ((GameObject)Get(g,"collectionPanel")).SetActive(false);((GameObject)Get(g,"resultPanel")).SetActive(false);
                Call(g,"ApplyAppearance",new object[]{null});Call(g,"Refresh");running=false;
                if(SessionState.GetBool("Capy67.autoChecks",false)){
                    SessionState.SetBool("Capy67.autoChecks",false);Application.runInBackground=SessionState.GetBool("Capy67.oldBackground",false);
                    EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();
                }
            }
        }
        [MenuItem("Capybara 67/Export local session journal")]
        public static void Export(){var g=UnityEngine.Object.FindAnyObjectByType<CapyGame>();if(!g || !EditorApplication.isPlaying)return;Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Capy67-expansion-events.txt",g.ExportJournal());Debug.Log("Local journal exported. Nothing sent over the network.");}
    }
}
