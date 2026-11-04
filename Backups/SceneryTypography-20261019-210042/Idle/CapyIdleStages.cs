using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
    public sealed partial class CapyIdleGame {
        void GrowthPage(){
            Label(panel,"Growth explanation","Твой этап: "+Model.Stage+" / "+IdleModel.StageCount+" · расти с заботой и едой",24,95,432,45,22,align:TextAnchor.MiddleCenter);
            string[] captions={"Новорож-\nдённый","Малыш","Любопыт-\nный","Молодой\nвзрослый","Хранитель\nсада"};
            for(int n=0;n<IdleModel.StageCount;n++){int age=n+1;float x=n<3?24+n*148:98+(n-3)*148,y=n<3?171:479;
                InkBox(panel,"Life stage "+age,x,y,136,250,age==Model.Stage?"E9D197":"E8C999",14,3);
                var preview=Rect(panel,"Age preview "+age,x+8,y+27,240,250);DrawAvatar(preview,null,false,age,true);preview.localScale=Vector3.one*.47f;
                Oval(panel,"Age number badge",x+7,y+7,28,28,age==Model.Stage?"A9C586":"E8C999",2);Label(panel,"Age number",age.ToString(),x+7,y+6,28,28,22,align:TextAnchor.MiddleCenter);
                Label(panel,"Stage name",captions[n],x+7,y+163,122,65,22,align:TextAnchor.MiddleCenter);
            }
        }
    }
}

