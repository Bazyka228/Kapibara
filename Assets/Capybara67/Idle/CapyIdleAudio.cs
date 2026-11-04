using System;
using UnityEngine;

namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  AudioSource musicSource;
  AudioClip purchaseClip,washClip,playClip,petClip,musicClip;
  readonly AudioClip[] activityNotes=new AudioClip[4];
  bool musicStarted;
  const int AudioRate=22050;
  void InitializeAudio(){
   DisposeAudio();
   audioSource=GetComponent<AudioSource>();if(!audioSource)audioSource=gameObject.AddComponent<AudioSource>();
   audioSource.playOnAwake=false;audioSource.spatialBlend=0;
   var child=transform.Find("Garden music");if(!child){child=new GameObject("Garden music").transform;child.SetParent(transform,false);}
   musicSource=child.GetComponent<AudioSource>();if(!musicSource)musicSource=child.gameObject.AddComponent<AudioSource>();
   musicSource.playOnAwake=false;musicSource.loop=true;musicSource.spatialBlend=0;musicSource.volume=.16f;
   careClip=GardenSound(0);feedClip=GardenSound(1);evolveClip=GardenSound(2);
   purchaseClip=GardenSound(3);washClip=GardenSound(4);playClip=GardenSound(5);petClip=GardenSound(6);
   for(int n=0;n<4;n++){var samples=new float[AudioRate/3];Pluck(samples,0,.33f,new[]{523.25f,587.33f,659.25f,783.99f}[n],.5f);activityNotes[n]=MakeAudio("Forest note "+n,samples);}
   musicStarted=false;
  }
  static AudioClip MakeAudio(string name,float[] data){
   for(int i=0;i<data.Length;i++)data[i]=Mathf.Clamp(data[i],-.7f,.7f)*Mathf.Min(1,Mathf.Min(i/110f,(data.Length-1-i)/110f));
   var clip=AudioClip.Create(name,data.Length,1,AudioRate,false);clip.SetData(data,0);return clip;
  }
  static void Pluck(float[] data,float start,float length,float hz,float gain){
   int offset=(int)(start*AudioRate),count=(int)(length*AudioRate);
   for(int i=0;i<count&&offset+i<data.Length;i++){
    float t=i/(float)AudioRate,x=t/length,phase=2*Mathf.PI*hz*t;
    float env=Mathf.Min(1,t/.008f)*Mathf.Exp(-5*x)*(1-x);
    data[offset+i]+=gain*env*(Mathf.Sin(phase)+.24f*Mathf.Sin(phase*2)+.08f*Mathf.Sin(phase*3));
   }
  }
  // Original synthesized motifs: no downloaded music, licensing dependency or disk access.
  static AudioClip GardenSound(int kind){
   float[] durations={.18f,.48f,1.35f,.8f,.8f,.34f,.65f};
   string[] names={"Gentle tap","Meal marimba","Growing chimes","Purchase sparkle","Bath bubbles","Play bounce","Contented chirp"};
   var data=new float[(int)(AudioRate*durations[kind])];
   if(kind==0)Pluck(data,0,.18f,392,.45f);
   if(kind==1){Pluck(data,0,.28f,523.25f,.4f);Pluck(data,.16f,.3f,659.25f,.34f);}
   if(kind==2){float[] notes={523.25f,659.25f,783.99f,1046.5f};for(int i=0;i<4;i++)Pluck(data,i*.19f,.7f,notes[i],.27f);}
   if(kind==3){Pluck(data,0,.5f,659.25f,.35f);Pluck(data,.13f,.5f,880,.3f);Pluck(data,.25f,.5f,1046.5f,.22f);}
   if(kind==4){for(int i=0;i<6;i++)Pluck(data,i*.105f,.17f,260+i%3*130,.22f);}
   if(kind==5){Pluck(data,0,.2f,330,.4f);Pluck(data,.12f,.2f,440,.25f);}
   if(kind==6){for(int i=0;i<data.Length;i++){float t=i/(float)AudioRate;float env=Mathf.Sin(Mathf.PI*t/.65f);data[i]=.25f*env*env*Mathf.Sin(2*Mathf.PI*(235*t+22*t*t)+.6f*Mathf.Sin(t*28));}}
   return MakeAudio(names[kind],data);
  }
  static AudioClip GardenMusic(){
   var data=new float[(int)(19.2f*AudioRate)];
   float[,] chords={{261.63f,329.63f,392f},{220f,261.63f,329.63f},{174.61f,220f,261.63f},{196f,246.94f,293.66f}};
   for(int bar=0;bar<8;bar++)for(int beat=0;beat<4;beat++){
    float start=(bar*4+beat)*.6f;int chord=bar%4;
    Pluck(data,start,1.0f,chords[chord,beat%3]*(beat==3?2:1),.16f);
    if(beat==0)Pluck(data,start,1.7f,chords[chord,0]*.5f,.13f);
   }
   return MakeAudio("A quiet garden — original loop",data);
  }
  void UpdateAudioState(){
   if(!ModelOrAudioReady())return;
   bool paused=PresentationPaused||!isActiveAndEnabled;
   if(paused||!Model.S.sound)audioSource.Stop();
   if(paused||!Model.S.music){if(musicSource.isPlaying)musicSource.Pause();return;}
   if(!musicClip){musicClip=GardenMusic();musicSource.clip=musicClip;}
   if(!musicStarted){musicSource.Play();musicStarted=true;}else if(!musicSource.isPlaying)musicSource.UnPause();
  }
  bool ModelOrAudioReady()=>Model!=null&&audioSource&&musicSource;
  void PauseAudio(){if(audioSource)audioSource.Stop();if(musicSource)musicSource.Pause();}
  void DisposeAudio(){
   PauseAudio();if(musicSource)musicSource.clip=null;for(int n=0;n<4;n++){if(activityNotes[n])Destroy(activityNotes[n]);activityNotes[n]=null;}
   foreach(var clip in new[]{careClip,feedClip,evolveClip,purchaseClip,washClip,playClip,petClip,musicClip})if(clip)Destroy(clip);
   careClip=feedClip=evolveClip=purchaseClip=washClip=playClip=petClip=musicClip=null;musicStarted=false;
  }
 }
}
