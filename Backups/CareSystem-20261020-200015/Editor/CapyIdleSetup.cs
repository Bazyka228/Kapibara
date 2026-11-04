using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Capybara67.Idle;
namespace Capybara67.Editor {
 [InitializeOnLoad] public static class CapyIdleSetup {
  public const string ScenePath="Assets/Capybara67/Scenes/Capybara67_Idle.unity";
  static CapyIdleSetup(){EditorApplication.update+=InstallRequest;}
  static void InstallRequest(){if(!File.Exists("Temp/Capy67-idle-install")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;if(SceneManager.GetActiveScene().isDirty)return;Open();File.Delete("Temp/Capy67-idle-install");Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Capy67-idle-install.txt","Idle scene ready; previous scenes disabled in build settings. No WebGL build performed.");}
  [MenuItem("Capybara Idle/Open clicker")]
  public static void Open(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)return;
   if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
   if(File.Exists(ScenePath))EditorSceneManager.OpenScene(ScenePath);else{
    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);camera.GetComponent<Camera>().orthographic=true;camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;camera.GetComponent<Camera>().backgroundColor=new Color(.1f,.08f,.18f);
    new GameObject("Capybara Idle").AddComponent<CapyIdleGame>();Directory.CreateDirectory("Assets/Capybara67/Scenes");EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath);
   }
   var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>{new EditorBuildSettingsScene(ScenePath,true)};foreach(var old in EditorBuildSettings.scenes)if(old.path!=ScenePath&&File.Exists(old.path))scenes.Add(new EditorBuildSettingsScene(old.path,false));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
  }
 }
}
