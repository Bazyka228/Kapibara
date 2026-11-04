using UnityEngine;
using UnityEngine.UI;
namespace Capybara67
{
    public sealed partial class CapyGame
    {
        float fxAge=1,fxDuration=.2f,idleAge;int reaction;Vector2 fxOrigin;
        RectTransform fxRoot,earlyRing,helperLight,missFruit;
        SoftShape contactFlash;Text digitText,sparkText;
        RectTransform[] burst=new RectTransform[8],flightGuides=new RectTransform[17];
        Text[] burstLabels=new Text[8];SoftShape[] burstShapes=new SoftShape[8];
        AudioClip perfectClip;
        static readonly string[] BurstPalette={"F0AFBC","F1C485","D7EBA6","A6E5D5","B6D7F1","D3B8EE","F0AFBC","F1C485"};
        void BuildExpansionEffects()
        {
            missionLabel=Label(board,"Mission progress","",20,706,440,23,12,"C9DDBD",TextAnchor.MiddleCenter);
            bestText.rectTransform.anchoredPosition=new Vector2(20,-731);bestText.fontSize=10;bestText.rectTransform.sizeDelta=new Vector2(440,16);
            eventLabel=Label(board,"Flight modifier","",35,648,410,23,11,"E5D4AE",TextAnchor.MiddleCenter);
            sparkText=Label(board,"Spark charges","",340,270,105,29,11,"F6D897",TextAnchor.MiddleRight);
            fxRoot=Rect(board,"Pooled feedback",0,0,480,850);fxRoot.SetSiblingIndex(tapButton.transform.GetSiblingIndex());
            earlyRing=Rect(fxRoot,"Safe first ring",0,0,64,34);Shape(earlyRing,"Safe rim",-32,-17,64,34,"B9DCCA",0,true);Shape(earlyRing,"Safe hole",-27,-12,54,24,"393451",0,true);Mark(earlyRing,"1",-13,-13,26,26,16,"F5E9CC");
            helperLight=Rect(fxRoot,"Helpful firefly",0,0,28,28);Shape(helperLight,"Glow",-14,-14,28,28,"EBD19155",0,true);Shape(helperLight,"Core",-5,-5,10,10,"FFE9AE",0,true);
            contactFlash=Shape(fxRoot,"Contact flash",0,0,75,8,"D6FFE8",4,true);
            missFruit=Rect(fxRoot,"Miss fruit",0,0,30,30);Shape(missFruit,"Orange",0,0,29,27,"F2AC54",0,true);Tile(missFruit,"Leaf",18,-4,13,6,"A5D7AC",3);
            digitText=Label(fxRoot,"Combo numeral","",130,366,220,72,57,"E1F6CF",TextAnchor.MiddleCenter,true);
            for(int n=0;n<burst.Length;n++){
                burst[n]=Rect(fxRoot,"Feedback mote "+n,0,0,20,20);
                burstShapes[n]=Shape(burst[n],"Mote",4,4,9,9,"F6D897",4,true);
                burstLabels[n]=Label(burst[n],"Symbol","+",0,0,20,20,18,"F6D897",TextAnchor.MiddleCenter,true);
            }
            for(int n=0;n<17;n++)flightGuides[n]=board.Find("Flight guide "+n) as RectTransform;
            foreach(var c in confetti)c.gameObject.SetActive(false);
            perfectClip=Tone(880,.12f);
        }
        Vector2 FlightPoint(float t)
        {
            float from=round.Target==1?117:363,to=round.Target==1?363:117;
            float arc=round.Mood==FlightMood.Choice?115:155;
            if(round.Mood==FlightMood.Wind)arc+=Mathf.Sin(t*Mathf.PI)*(round.Hits%16==0?-38:38);
            return new Vector2(Mathf.Lerp(from,to,t),-(447-Mathf.Sin(t*Mathf.PI)*arc));
        }
        void StartReaction(int kind,float duration){reaction=kind;fxAge=0;fxDuration=Mathf.Min(.4f,duration);fxOrigin=fruit?fruit.anchoredPosition:new Vector2(240,-450);}
        void ExpansionFeedback(int result)
        {
            if(progress==null)return;
            int kind=result<0?(round.LastEarly?5:4):result==7?7:round.Combo%7==6?6:round.LastPerfect?3:1;
            StartReaction(kind,kind==7?.36f:kind==6?.24f:kind==3?.22f:kind==1?.14f:.26f);
            if(round.LastBonus>0){hintText.text+="  +"+round.LastBonus;sparkText.color=Mint;}
            if(result==7)hintText.text="СЕМЬ!  +67";else if(kind==6)hintText.text="ШЕСТЬ! Ещё один мандарин…";
            if(result>=0 && round.LastPerfect && sound)audioSource.PlayOneShot(perfectClip,.16f);
            if(round.MissionDone && !missionAnnounced){missionAnnounced=true;Journal("mission_completed",round.MissionName);if(kind!=7 && kind!=6)StartReaction(8,.3f);}
            Journal("mechanic_resolved",loggedMood+"; result="+result);
        }
        void AnimateExpansion()
        {
            if(progress==null || !fxRoot)return;
            if(!paused && !platformPaused){fxAge+=Time.unscaledDeltaTime;idleAge+=Time.unscaledDeltaTime;}
            bool active=fxAge<fxDuration;float t=Mathf.Clamp01(fxAge/fxDuration),wave=active?Mathf.Sin(t*Mathf.PI):0;
            if(progress.reducedEffects)wave=0;
            var skin=CapyCatalog.Find(progress.equipped[5]);int character=skin==null?0:skin.art;
            if(!round.Running && !CollectionOpen && !resultPanel.activeSelf && idleAge>5.2f){idleAge=0;StartReaction(character==2?13:12,character==2?.4f:.16f);}
            dailyButton.interactable=!round.Running;
            if(round.Running){
                missionLabel.text=progress.rounds==0?"Первые подарки — после раунда":round.MissionName+"  "+Mathf.Min(round.MissionProgress,round.MissionGoal)+"/"+round.MissionGoal+(round.MissionDone?"  ГОТОВО":"");
                eventLabel.text=round.Mood==FlightMood.Wind?"ВЕТЕР · смотри на кольцо":round.Mood==FlightMood.Choice?"ТАПНИ В ПЕРВОМ ИЛИ В КОНЕЧНОМ КОЛЬЦЕ":round.Mood==FlightMood.Rain?"ТРИ УДАРА ДОЖДЯ · ровный ритм":round.Mood==FlightMood.Helper?"СВЕТЛЯЧОК ПОКАЗЫВАЕТ МОМЕНТ":"";
                if(loggedMood!=round.Mood){loggedMood=round.Mood;Journal("mechanic_entered",round.Mood.ToString());}
            }else{missionLabel.text=progress.rounds==0?"Первый подарок — за первый раунд":"Следующее поручение: "+new[]{"10 мандаринов","3 идеальные ловли","серия из 7"}[(progress.rounds-1)%3];eventLabel.text="";}
            sparkText.text=round.Running && round.Spark>0?"ИСКРЫ: "+round.Spark:"";
            sparkText.transform.localScale=Vector3.one*(1+(active && round.LastBonus>0?.14f*wave:0));
            earlyRing.gameObject.SetActive(round.Running && round.Mood==FlightMood.Choice);
            if(earlyRing.gameObject.activeSelf)earlyRing.anchoredPosition=FlightPoint(round.EarlyCenter/round.FlightDuration);
            helperLight.gameObject.SetActive(round.Running && (round.Mood==FlightMood.Helper || round.Mood==FlightMood.Rain));
            if(helperLight.gameObject.activeSelf){helperLight.anchoredPosition=new Vector2(round.Target==1?363:117,-418);helperLight.localScale=Vector3.one*(.5f+round.Progress*.6f);}
            for(int n=0;n<17;n++)if(flightGuides[n])flightGuides[n].anchoredPosition=FlightPoint(n/16f)+new Vector2(-2,2);
            particles.gameObject.SetActive(false); // All celebratory particles share the bounded pool below.
            bool hit=active && (reaction==1 || reaction==3 || reaction==6 || reaction==7 || reaction==8);
            contactFlash.gameObject.SetActive(hit && !progress.reducedEffects);
            if(hit){contactFlash.rectTransform.anchoredPosition=fxOrigin+new Vector2(-35,0);contactFlash.transform.localScale=Vector3.one*(1+.25f*wave);contactFlash.color=new Color(.75f,1,.87f,1-t);}
            missFruit.gameObject.SetActive(active && !progress.reducedEffects && (reaction==4 || reaction==5));
            if(missFruit.gameObject.activeSelf)missFruit.anchoredPosition=fxOrigin+new Vector2(35*t,-60*t+22*wave);
            digitText.gameObject.SetActive(active && (reaction==6 || reaction==7));digitText.text=reaction==6?"6":"7!";digitText.transform.localScale=Vector3.one*(1+.12f*wave);
            var trail=CapyCatalog.Find(progress.equipped[3]);bool celebrate=active && (reaction==3 || reaction==7 || reaction==11 || hit && trail!=null);
            bool streaming=round.Running && trail!=null && !paused && !platformPaused && round.Progress>.08f && round.Progress<.87f;
            for(int n=0;n<burst.Length;n++){
                bool visible=(celebrate || streaming) && !progress.reducedEffects && n<(celebrate && reaction==7?8:4);burst[n].gameObject.SetActive(visible);if(!visible)continue;
                float angle=n*Mathf.PI*2/8;burst[n].anchoredPosition=fxOrigin+new Vector2(Mathf.Cos(angle)*(18+t*45),Mathf.Sin(angle)*(15+t*31)+t*18);
                if(!celebrate)burst[n].anchoredPosition=FlightPoint(Mathf.Max(0,round.Progress-.025f*(n+1)));
                string col=celebrate && reaction==7?BurstPalette[n]:trail?.color??"F4D991";
                int style=celebrate && reaction==7?0:trail?.art??1;
                burstLabels[n].text=style==2?"~":style==5?(n%2==0?"6":"7"):"+";
                burstLabels[n].gameObject.SetActive(style==1 || style==2 || style==5);
                burstShapes[n].gameObject.SetActive(style==0 || style==3 || style==4);burstShapes[n].radius=style==3?0:4;burstShapes[n].SetVerticesDirty();
                Color color=C(col);color.a=celebrate?1-t:.42f-.07f*n;burstShapes[n].color=burstLabels[n].color=color;burst[n].localScale=Vector3.one*(celebrate?1-.35f*t:.7f-.1f*n);
            }
            if(progress.reducedEffects)wave=0;
            capy.localEulerAngles=new Vector3(0,0,active && (reaction==7 || reaction==11)?wave*(character==1?5:2):0);
            capy.localScale=new Vector3(1+wave*.018f,1-wave*(reaction==10?.065f:.018f),1);
            if(active && (reaction==6 || reaction==7 || reaction==11)){leftPaw.localEulerAngles=new Vector3(0,0,22*wave);rightPaw.localEulerAngles=new Vector3(0,0,-22*wave);}
            if(active && (reaction==5 || reaction==10))leftPaw.localEulerAngles=new Vector3(0,0,-12*wave);
            float mouthWidth=19,mouthHeight=4;
            if(active && (reaction==8 || reaction==11 || reaction==9)){mouthWidth=38;mouthHeight=10;}
            if(active && (reaction==13 || reaction==4)){mouthWidth=13;mouthHeight=17;}
            if(!round.Running && character==5){mouthWidth=35;mouthHeight=9;}
            var emotion=CapyCatalog.Find(progress.equipped[4]);
            if(active && !round.Running && emotion!=null){
                mouthWidth=emotion.art==5?40:emotion.art==1 || emotion.art==2?14:emotion.art==4?19:29;
                mouthHeight=emotion.art==4?4:emotion.art==1 || emotion.art==2?18:10;
                if(emotion.art==3){leftPaw.localEulerAngles=new Vector3(0,0,18*wave);rightPaw.localEulerAngles=new Vector3(0,0,-18*wave);}
            }
            mouthShape.rectTransform.sizeDelta=new Vector2(mouthWidth,mouthHeight);mouthShape.radius=mouthHeight/2;mouthShape.SetVerticesDirty();
            for(int n=0;n<brows.Length;n++)brows[n].localEulerAngles=new Vector3(0,0,active && (reaction==4 || reaction==5)?(n==0?20:0):n==0?-7:7);
            if(active && (reaction==10 || reaction==12 || reaction==13))foreach(var eye in eyes)eye.localScale=new Vector3(1,Mathf.Max(.15f,1-wave),1);
            if(!round.Running && character==2)foreach(var eye in eyes)eye.localScale=new Vector3(1,.55f,1);
            if(active && !round.Running){
                if(character==1)capy.localScale=new Vector3(1-.025f*wave,1+.04f*wave,1);
                if(character==3)skinLayer.localScale=Vector3.one*(1+.025f*wave);
                if(character==4)capy.localEulerAngles=new Vector3(0,0,!progress.reducedEffects && t<.5f?3:0);
                if(character==6)capy.localEulerAngles=new Vector3(0,0,3*wave);
                if(character==7)skinLayer.localEulerAngles=new Vector3(0,0,4*wave);
                if(character==8)foreach(var eye in eyes)eye.localScale=new Vector3(1+.1f*wave,.8f,1);
                if(character==9)capy.localScale=new Vector3(1,1-.04f*wave,1);
                if(emotion!=null && emotion.art==4 && brows.Length>0)brows[0].localEulerAngles=new Vector3(0,0,18);
            }else{skinLayer.localScale=Vector3.one;skinLayer.localRotation=Quaternion.identity;}
            if(CollectionOpen && previewAvatar){previewAvatar.localScale=Vector3.one*(.70f+wave*.025f);previewAvatar.localEulerAngles=new Vector3(0,0,wave*2);}
            if(resultPanel.activeSelf && resultAvatar){
                resultAvatar.localScale=new Vector3(.68f,.68f*(1-(reaction==10?.06f:.02f)*wave),1);
                resultAvatar.Find("Result left paw").localEulerAngles=new Vector3(0,0,(reaction==11?22:-12)*wave);
                resultAvatar.Find("Result right paw").localEulerAngles=new Vector3(0,0,(reaction==11?-22:8)*wave);
                resultAvatar.Find("Mouth").GetComponent<RectTransform>().sizeDelta=new Vector2(reaction==11?34:19,reaction==11?10:4);
                foreach(var eye in resultAvatar.GetComponentsInChildren<RectTransform>())if(eye.name.StartsWith("Expressive eye"))eye.localScale=new Vector3(1,reaction==10?1-.85f*wave:1,1);
            }
            if(active && reaction==9 && !progress.reducedEffects)headLayer.anchoredPosition=new Vector2(0,12*(1-t));else headLayer.anchoredPosition=Vector2.zero;
        }
        void DisposeExpansion(){if(perfectClip)Destroy(perfectClip);}
    }
}
