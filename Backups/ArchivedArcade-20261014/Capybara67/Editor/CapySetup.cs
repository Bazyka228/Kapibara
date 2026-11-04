using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Capybara67.Editor
{
    [InitializeOnLoad]
    public static class CapySetup
    {
        const string ScenePath = "Assets/Capybara67/Scenes/Capybara67_Garden.unity";
        static CapySetup() { EditorApplication.delayCall += FirstImport; }
        static void FirstImport()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (!File.Exists(ScenePath) && !SceneManager.GetActiveScene().isDirty) Create();
            else if (SceneManager.GetActiveScene().path == ScenePath) RepairRenderers();
        }
        [MenuItem("Capybara 67/Repair UI renderers")]
        static void RepairRenderers()
        {
            bool changed = false;
            foreach (var shape in Resources.FindObjectsOfTypeAll<SoftShape>())
            {
                if (shape.gameObject.scene != SceneManager.GetActiveScene()) continue;
                if (shape.GetComponent<CanvasRenderer>() == null) { shape.gameObject.AddComponent<CanvasRenderer>(); changed = true; }
                shape.SetAllDirty();
            }
            var board = GameObject.Find("Portrait game board");
            if (board != null && board.GetComponent<UnityEngine.UI.RectMask2D>() == null)
            { board.AddComponent<UnityEngine.UI.RectMask2D>(); changed = true; }
            if (changed) EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        [MenuItem("Capybara 67/Open game")]
        public static void Open()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) Create(); else EditorSceneManager.OpenScene(ScenePath);
        }
        [MenuItem("Capybara 67/Create scene")]
        public static void Create()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Capy67: scene already exists; keeping edits."); return; }
            Directory.CreateDirectory("Assets/Capybara67/Scenes");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera"; camera.transform.position = new Vector3(0,0,-10);
            camera.GetComponent<Camera>().orthographic = true;
            camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor = CapyGame.C("191528");
            var game = new GameObject("Capybara 67").AddComponent<CapyGame>(); game.BuildSceneUI();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            var existing = EditorBuildSettings.scenes;
            var scenes = new EditorBuildSettingsScene[existing.Length + 1];
            scenes[0] = new EditorBuildSettingsScene(ScenePath, true); Array.Copy(existing,0,scenes,1,existing.Length);
            for(int i=1;i<scenes.Length;i++) if(scenes[i].path=="Assets/Capybara67/Scenes/Capybara67.unity") scenes[i].enabled=false;
            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets(); RunChecks();
            Debug.Log("Capy67: scene created. Press Play to catch the vibe.");
        }
        [MenuItem("Capybara 67/Run rule checks")]
        public static void RunChecks()
        {
            var m = new RoundModel(); m.Begin();
            Check(m.Running && m.Lives == 3 && m.Remaining == 67, "initial state");
            m.Tick(m.FlightDuration); Check(m.Tap() == 2 && m.Score == 20, "perfect catch");
            for (int i=0;i<6;i++) { m.Tick(m.FlightDuration); m.Tap(); }
            Check(m.Combo==7 && m.Score==207 && m.BestCombo==7,"seven bonus");
            Check(m.Tap()==-1 && m.Lives==2 && m.Combo==0,"early tap resets combo");
            m.Tick(m.FlightDuration + RoundModel.Window + .01f); Check(m.Lives==1,"missed landing");
            m.Tap(); Check(!m.Running && m.Lives==0,"third miss ends round");
            int score=m.Score; m.Tap(); m.Tick(1); Check(m.Score==score,"ended round immutable");
            m.Begin(); Check(m.Score==0 && m.Lives==3,"restart");
            m.Tick(67); Check(!m.Running && m.Remaining==0,"timer expiry");
            m.Begin(); m.Tick(m.FlightDuration-.15f); Check(m.Tap()==1,"ordinary catch window");
            Check(m.PerfectHits==0 && m.Hits==1,"ordinary is not perfect");
            Check(!CapyGame.OutfitUnlocked(1,9) && CapyGame.OutfitUnlocked(1,10),"hat unlock boundary");
            Check(!CapyGame.OutfitUnlocked(3,49) && CapyGame.OutfitUnlocked(3,50),"crown unlock boundary");
            Check(!CapyGame.OutfitUnlocked(-1,100) && !CapyGame.OutfitUnlocked(4,100),"invalid outfit");
            Debug.Log("Capy67: RULE CHECKS PASS (14 cases)");
            File.WriteAllText("Logs/Capy67-checks.txt", "PASS: 14 cases; round rules, result counters, cosmetic unlock boundaries\n");
        }
        static void Check(bool ok,string name) { if(!ok) throw new Exception("Capy67 test failed: "+name); }
    }
}
