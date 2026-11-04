using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
    public sealed partial class CapyIdleGame {
        static string GrowthStars(int n)=>n+(n==1?" звезда":n<5?" звезды":" звёзд");
        void GrowthPage(){
            var gift=Button(panel,"Growth gift","",24,90,432,60,()=>{
                double coins=Model.GrowthGiftCoins;int stars=Model.PendingGrowthGifts;
                if(!Model.ClaimGrowthGifts())return;
                Save();Show("home");Sound(evolveClip,.18f);Toast("Подарок за рост! +"+Number(coins)+" монет\n+"+GrowthStars(stars)+" · продолжай расти");
            },"55E6C1",20);
            liveLabels.Add(()=>{gift.interactable=Model.PendingGrowthGifts>0;gift.GetComponentInChildren<Text>().text=Model.PendingGrowthGifts>0?"ЗАБРАТЬ · "+Number(Model.GrowthGiftCoins)+" монет + "+GrowthStars(Model.PendingGrowthGifts):Model.Stage<IdleModel.StageCount?"РОСТ · "+Number(Model.S.growth)+" / "+Number(IdleModel.Growth[Model.Stage]):"Все этапы пройдены · ты хранитель сада";});
            string[] captions={"Новорождённый","Малыш","Любопытный","Молодой\nвзрослый","Хранитель сада"};
            for(int n=0;n<IdleModel.StageCount;n++){int age=n+1;bool last=n==4;float x=last?24:24+n%2*222,y=last?598:154+n/2*222,w=last?432:210;
                InkBox(panel,"Life stage "+age,x,y,w,210,age==Model.Stage?"E2EDC4":"F5E4BE",18,3);
                Shape(panel,"Stage card highlight",x+15,y+7,w-30,3,"FFF9E8",1);
                var preview=Rect(panel,"Age preview "+age,x+(last?21:39),y+10,240,250);DrawAvatar(preview,null,false,age,true);preview.localScale=Vector3.one*(last?.72f:.53f);
                Oval(panel,"Age number badge",x+10,y+10,30,30,age==Model.Stage?"A9C586":"E7CE9D",2);Label(panel,"Age number",age.ToString(),x+10,y+9,30,30,22,align:TextAnchor.MiddleCenter).font=bold;
                Label(panel,"Stage name",captions[n],x+(last?205:9),y+(last?58:140),last?211:192,last?80:65,last?26:22,"513821",TextAnchor.MiddleCenter).font=bold;
                if(last)Label(panel,"Guardian note","Расти в своём темпе",x+211,y+134,202,68,22,"72543A",TextAnchor.MiddleCenter);
            }
        }
    }
}


