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
        static int awaitingRuntime;
        static CapyExpansionValidation(){EditorApplication.update+=BackgroundRequest;}
        // Explicit local test request; never runs on a normal project open.
        static void BackgroundRequest()
        {
            if(!File.Exists(RequestPath) || running || EditorApplication.isCompiling || EditorApplication.isUpdating)return;
            if(!EditorApplication.isPlaying){
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                if(!UnityEngine.Object.FindAnyObjectByType<CapyGame>()){
                    if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty){Debug.LogWarning("Capy67 validation waiting: save the current scene before switching.");return;}
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Capybara67/Scenes/Capybara67_Garden.unity");
                }
                SessionState.SetBool("Capy67.autoChecks",true);SessionState.SetBool("Capy67.oldBackground",Application.runInBackground);
                Application.runInBackground=true;EditorApplication.EnterPlaymode();return;
            }
            var g=UnityEngine.Object.FindAnyObjectByType<CapyGame>();
            if(!g || Get(g,"progress")==null || !(bool)Get(g,"redesignReady")){
                if(++awaitingRuntime>30){awaitingRuntime=0;EditorApplication.ExitPlaymode();}return;
            }
            awaitingRuntime=0;
            File.Delete(RequestPath);Run();
        }
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Get(CapyGame g,string n)=>typeof(CapyGame).GetField(n,F).GetValue(g);
        static void Set(CapyGame g,string n,object v)=>typeof(CapyGame).GetField(n,F).SetValue(g,v);
        static void Call(CapyGame g,string n,params object[] a)=>typeof(CapyGame).GetMethod(n,F).Invoke(g,a);
        static int checks;static bool running;
        static void Check(bool ok,string message){if(!ok){Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Capy67-expansion-checks.txt","FAIL v3: "+message+"; completed="+checks);throw new Exception("Expansion: "+message);}checks++;}
        static bool Clickable(Button b)
        {
            Canvas.ForceUpdateCanvases();var r=(RectTransform)b.transform;var p=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);
            bool ok=hits.Count>0 && hits[0].gameObject==b.gameObject;
            if(!ok)Debug.LogWarning("Capy67 raycast: expected "+b.name+"; active="+b.gameObject.activeInHierarchy+"; top="+(hits.Count==0?"none":hits[0].gameObject.name)+"; depth="+b.targetGraphic.depth+"; cull="+b.targetGraphic.canvasRenderer.cull+"; transparentCull="+b.targetGraphic.canvasRenderer.cullTransparentMesh+"; target="+b.targetGraphic.raycastTarget+"; pos="+p.position+"; screen="+Screen.width+"x"+Screen.height);return ok;
        }
        [MenuItem("Capybara 67/Run expansion checks")]
        public static void Run(){if(!EditorApplication.isPlaying || running){Debug.LogWarning("Enter Play Mode first; do not run checks twice.");return;}var g=UnityEngine.Object.FindAnyObjectByType<CapyGame>();g.StartCoroutine(Checks(g));}
        static IEnumerator Checks(CapyGame g)
        {
            running=true;checks=0;Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Capy67-expansion-checks.txt","RUNNING redesign v3");
            var previous=(CapyProgress)Get(g,"progress");int wallet=(int)Get(g,"pearls"),best=(int)Get(g,"best");
            bool oldSound=(bool)Get(g,"sound");Set(g,"sound",false);
            var oldDaily=Get(g,"dailyReceivedId");Set(g,"dailyReceivedId",null);
            string[] keys={"capy67.save.v1","capy67.progress.v2"};string[] saves={PlayerPrefs.GetString(keys[0],""),PlayerPrefs.GetString(keys[1],"")};
            var m=(RoundModel)Get(g,"round");
            try {
                Set(g,"paused",false);Set(g,"platformPaused",false);m.Tick(67);
                ((GameObject)Get(g,"collectionPanel")).SetActive(false);((GameObject)Get(g,"resultPanel")).SetActive(false);
                var p=new CapyProgress();p.Repair();Set(g,"progress",p);Set(g,"pearls",0);
                Call(g,"RefreshRedesign");yield return new WaitForEndOfFrame();Check(Clickable((Button)Get(g,"collectionButton")),"wardrobe pointer");
                Call(g,"OpenSettings");Call(g,"Action");Check(!m.Running,"settings blocks keyboard start");((GameObject)Get(g,"settingsPanel")).SetActive(false);
                Call(g,"OpenDaily");Call(g,"ClaimRedesignDaily");Check(p.rounds==0 && p.dailyDate=="","daily cannot bypass first round");((GameObject)Get(g,"dailyPanel")).SetActive(false);
                Call(g,"Action");Call(g,"RefreshRedesign");yield return new WaitForEndOfFrame();Check(Clickable((Button)Get(g,"tapButton")),"tap area pointer");
                Check(!((Button)Get(g,"dailyButton")).gameObject.activeSelf && !((Text)Get(g,"walletText")).gameObject.activeSelf,"no resources or gifts during flight");
                Check(((Text)Get(g,"scoreText")).fontSize>=58,"readable score");
                for(int n=0;n<7;n++){m.Tick(m.FlightDuration);Call(g,"Tap");}Check(m.Score==207 && m.Spark==3,"seven and spark");
                Call(g,"TogglePause");float time=m.Remaining;Call(g,"Update");Check(m.Remaining==time,"pause freezes game");Call(g,"TogglePause");
                Call(g,"PlatformPause",true);int life=m.Lives;Call(g,"Tap");Call(g,"Update");Check(m.Lives==life && m.Remaining==time,"platform pause");Call(g,"PlatformPause",false);
                for(int n=0;n<3;n++)Call(g,"Tap");Check(!m.Running && p.rounds==1 && p.Has("panama"),"finish and first gift");int earned=(int)Get(g,"pearls");Call(g,"Finish");Check((int)Get(g,"pearls")==earned && p.rounds==1,"settlement once");
                Call(g,"ChangeOutfit");yield return null;
                Check(g.CollectionOpen,"new wardrobe opens");Call(g,"CloseCollection");Check(((GameObject)Get(g,"resultPanel")).activeSelf,"collection returns to results");Call(g,"ChangeOutfit");Set(g,"selectedId","gold");Set(g,"category",ItemSlot.Skin);Call(g,"RefreshCollection");Check(!((Button)Get(g,"equipItemButton")).interactable,"legendary locked");
                Set(g,"selectedId","cap");Set(g,"category",ItemSlot.Head);Set(g,"pearls",40);Call(g,"EquipSelected");Check(p.Has("cap") && p.equipped[0]=="cap" && (int)Get(g,"pearls")==20,"cosmetic purchase");Call(g,"EquipSelected");Check((int)Get(g,"pearls")==20,"equip does not charge twice");
                Call(g,"Persist");var loaded=JsonUtility.FromJson<CapyProgress>(PlayerPrefs.GetString(keys[1]));Check(loaded.Has("cap") && loaded.equipped[0]=="cap","save round trip");
                foreach(var item in CapyCatalog.Items){
                    p.Grant(item.id);p.Equip(item.id);Set(g,"selectedId",item.id);Set(g,"category",item.slot);Call(g,"RefreshCollection");
                    yield return null;Check(((RectTransform)Get(g,"previewAvatar")).gameObject.activeInHierarchy,"visible preview "+item.id);
                }
                for(int n=0;n<3;n++){p.silhouette=n;Call(g,"ApplyAppearance",new object[]{null});Check(((RectTransform)Get(g,"capy")).GetComponentInChildren<CapyContour>(true).points.Length>20,"silhouette "+n);}
                ((GameObject)Get(g,"collectionPanel")).SetActive(false);Call(g,"OpenDaily");int count=p.owned.Count;p.owned.Remove("raincoat");p.dailyDate="";Call(g,"ClaimRedesignDaily");string date=p.dailyDate;int claimed=p.owned.Count;Call(g,"ClaimRedesignDaily");Check(date!="" && p.owned.Count==claimed,"daily transaction once");((GameObject)Get(g,"dailyPanel")).SetActive(false);
                Check(g.CollectionOpen && (string)Get(g,"selectedId")=="raincoat","daily gift opens its preview");Call(g,"CloseCollection");Check(((GameObject)Get(g,"dailyPanel")).activeSelf,"gift preview returns to daily screen");
                Check(CapyCatalog.Items.Length==52,"complete catalog");
                Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Capy67-expansion-checks.txt","PASS: "+checks+" Unity integration assertions. Raycasts, input, pauses, gifts, purchase, save and all 52 previews. User profile restored in finally.\n");
                File.WriteAllText("Logs/Capy67-expansion-events.txt",g.ExportJournal());
                Debug.Log("Capy67 EXPANSION PASS: "+checks);
            } finally {
                for(int n=0;n<keys.Length;n++){if(saves[n]=="")PlayerPrefs.DeleteKey(keys[n]);else PlayerPrefs.SetString(keys[n],saves[n]);}PlayerPrefs.Save();
                Set(g,"progress",previous);Set(g,"pearls",wallet);Set(g,"best",best);Set(g,"sound",oldSound);m.Tick(67);Set(g,"settled",true);
                Set(g,"dailyReceivedId",oldDaily);Set(g,"collectionFromResults",false);Set(g,"collectionFromDaily",false);
                ((GameObject)Get(g,"collectionPanel")).SetActive(false);((GameObject)Get(g,"resultPanel")).SetActive(false);
                ((GameObject)Get(g,"settingsPanel")).SetActive(false);((GameObject)Get(g,"dailyPanel")).SetActive(false);
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
