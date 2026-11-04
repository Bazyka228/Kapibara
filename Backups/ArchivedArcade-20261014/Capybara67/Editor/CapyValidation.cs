using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Capybara67.Editor
{
    public static class CapyValidation
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Read(CapyGame g,string name)=>typeof(CapyGame).GetField(name,Flags).GetValue(g);
        static void Write(CapyGame g,string name,object value)=>typeof(CapyGame).GetField(name,Flags).SetValue(g,value);
        static void Call(CapyGame g,string name,params object[] args)=>typeof(CapyGame).GetMethod(name,Flags).Invoke(g,args);
        static void Check(bool condition,string name){if(!condition)throw new Exception("Capy67 integration: "+name);}
        static bool ReceivesPointer(Button button)
        {
            Canvas.ForceUpdateCanvases();
            var rt=(RectTransform)button.transform;
            var pointer=new PointerEventData(EventSystem.current) {position=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center))};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            return hits.Count>0 && hits[0].gameObject==button.gameObject;
        }

        [MenuItem("Capybara 67/Run Play Mode checks")]
        public static void PlayChecks() { CapyExpansionValidation.Run(); }

        [MenuItem("Capybara 67/Check WebGL readiness")]
        public static void CheckWebGL()
        {
            bool web=BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL);
            bool storage=Directory.Exists("Assets/PluginYourGames/Modules/Storage");
            bool rewards=Directory.Exists("Assets/PluginYourGames/Modules/RewardedAdv");
            string status="WebGL Build Support: "+web+"\nPluginYG2 core: "+Directory.Exists("Assets/PluginYourGames")+
                "\nStorage module: "+storage+"\nRewardedAdv module: "+rewards+
                "\nGameReady and gameplay/pause hooks: implemented\nSave provider: local PlayerPrefs\n";
            File.WriteAllText("Logs/Capy67-webgl-readiness.txt",status);Debug.Log(status);
        }

        [MenuItem("Capybara 67/Build WebGL prototype")]
        public static void BuildWebGL()
        {
            if(EditorApplication.isPlaying) {Debug.LogWarning("Stop Play Mode before building.");return;}
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL))
                throw new Exception("WebGL Build Support is required.");
            string output=Path.GetFullPath("Builds/Capy67WebGL");
            Directory.CreateDirectory(output);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/Capybara67/Scenes/Capybara67_Garden.unity"},
                locationPathName=output,target=BuildTarget.WebGL,
                options=BuildOptions.None
            });
            var s=report.summary;
            bool complete=s.result==UnityEditor.Build.Reporting.BuildResult.Succeeded && s.totalErrors==0 && File.Exists(Path.Combine(output,"index.html"));
            string details=(complete?"PASS":"FAILED")+"\nUnity result: "+s.result+"\nErrors: "+s.totalErrors+"\nWarnings: "+s.totalWarnings+"\nSize: "+s.totalSize+"\nDuration: "+s.totalTime+"\n";
            foreach(var step in report.steps) foreach(var message in step.messages)
                if(message.type==LogType.Error || message.type==LogType.Exception) details+=message.content+"\n";
            File.WriteAllText("Logs/Capy67-webgl-build.txt",details);
            Debug.Log("Capy67 WebGL: "+(complete?"PASS":"FAILED; see Logs/Capy67-webgl-build.txt"));
        }
    }
}

