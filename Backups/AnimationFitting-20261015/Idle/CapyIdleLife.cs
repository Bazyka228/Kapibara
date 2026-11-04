using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
    public sealed partial class CapyIdleGame {
        float lifeAge,mealAge=10,evolutionAge=10,evolutionFromScale;
        int mealKind;
        RectTransform mealVisual;
        Transform nearEye,farEye,smile;
        CanvasGroup mealOpacity,evolutionOpacity;
        Text sleepMark;
        public bool IsSleeping=>Model.S.starter&&Model.S.activeSeconds-System.Math.Max(Model.S.lastCare,Model.S.lastFeed)>45;

        void BuildLifeVisuals(){
            nearEye=hero.Find("Eye");farEye=hero.Find("Far eye");smile=hero.Find("Smile");
            mealVisual=Rect(hero,"Meal reaction",88,190,80,70);DrawFoodIcon(mealVisual,mealKind,0,0);
            mealOpacity=mealVisual.gameObject.AddComponent<CanvasGroup>();mealOpacity.blocksRaycasts=false;mealOpacity.interactable=false;mealOpacity.alpha=0;
            var sparks=Rect(hero,"Evolution sparks",0,0,240,250);evolutionOpacity=sparks.gameObject.AddComponent<CanvasGroup>();evolutionOpacity.blocksRaycasts=false;evolutionOpacity.interactable=false;evolutionOpacity.alpha=0;
            for(int n=0;n<6;n++)Mark(sparks,"+",n%2==0?-14:242,38+n/2*62,20,24,24,n%2==0?"55E6C1":"FFD784");
            sleepMark=Label(hero,"Sleep mark","z z",166,-5,70,34,24,"C9BCD8",TextAnchor.MiddleCenter);sleepMark.gameObject.SetActive(false);
        }
        void AnimateHero(float dt){
            if(suspended||platformPaused)return;lifeAge+=dt;mealAge+=dt;evolutionAge+=dt;
            if(!hero||!home.gameObject.activeSelf)return;
            bool calm=Model.S.calm,sleeping=IsSleeping;
            float bounce=calm?0:Mathf.Max(0,1-reactionAge*4)*.035f;
            float size=!calm&&evolutionAge<1.2f?Mathf.Lerp(evolutionFromScale,HeroScale,Mathf.SmoothStep(0,1,evolutionAge/1.2f)):HeroScale;
            float breath=calm?0:Mathf.Sin(lifeAge*(sleeping?1.4f:2f))*.004f;
            hero.localScale=new Vector3(size+bounce,size+bounce+breath,1);
            hero.localEulerAngles=new Vector3(0,0,calm?0:Mathf.Sin(lifeAge*.9f)*(sleeping?.3f:.7f));
            bool blink=sleeping||(!calm&&lifeAge%5.3f<.14f);
            if(nearEye)nearEye.localScale=new Vector3(1,blink?.25f:1,1);if(farEye)farEye.localScale=new Vector3(1,blink?.25f:1,1);
            if(sleepMark)sleepMark.gameObject.SetActive(sleeping);
            if(smile)smile.localScale=new Vector3(1,!calm&&mealAge<1.8f?1+Mathf.Abs(Mathf.Sin(mealAge*13))*.65f:1,1);
            if(mealVisual){mealOpacity.alpha=mealAge<1.8f?Mathf.Min(1,(1.8f-mealAge)*3):0;mealVisual.anchoredPosition=new Vector2(88,-190+(calm?0:Mathf.Sin(Mathf.Clamp01(mealAge/1.8f)*Mathf.PI)*27));}
            if(evolutionOpacity)evolutionOpacity.alpha=calm?0:Mathf.Clamp01(1-evolutionAge/2.4f);
        }
        void VisitorPage(){
            Model.RefreshDay(Date);
            Label(panel,"Guest greeting",Model.VisitorName+" заглянул на чай",24,111,432,64,27,"FFF4D6",TextAnchor.MiddleCenter);
            var guest=Rect(panel,"Guest capybara",139,201,240,250);DrawAvatar(guest,new[]{"sunny","fluffy","sleepy"}[Model.VisitorKind],true);guest.localScale=Vector3.one*.84f;
            Label(panel,"Guest quote",new[]{"«Знаешь секрет? На солнышке\nлюбая подстилка уютнее!»","«Я принёс подарок.\nА обниматься можно бесплатно!»","«Иногда лучший план —\nпросто немного отдохнуть»."}[Model.VisitorKind],32,440,416,114,24,"C9BCD8",TextAnchor.MiddleCenter);
            var gift=Button(panel,"Welcome visitor","",24,591,432,78,()=>{if(Model.WelcomeVisitor()){Save();Sound(feedClip,.2f);Show("visitor");}},"55E6C1",23);
            liveLabels.Add(()=>{gift.interactable=Model.CanVisit;gift.GetComponentInChildren<Text>().text=Model.CanVisit?"ПОЗДОРОВАТЬСЯ · +"+Number(Model.VisitorReward)+" монет":Model.S.feeds<3?"ГОСТИ ПОСЛЕ 3 КОРМЛЕНИЙ":"СПАСИБО, ЧТО ЗАГЛЯНУЛ!";});
            Label(panel,"Guest note","Один подарок в день.\nПропуск дня ничего не отнимает.\nГость — персонаж игры.",24,697,432,115,22,"AEA2C6",TextAnchor.MiddleCenter);
        }
    }
}
