using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
    public sealed partial class CapyIdleGame {
        void GrowthPage(){
            Label(panel,"Growth explanation","Твой этап: "+Model.Stage+" / 9 · расти с заботой и едой",24,95,432,45,22,align:TextAnchor.MiddleCenter);
            string[] captions={"Новорож-\nдённый","Малыш","Любопыт-\nный","Игривый","Подросток","Молодой\nвзрослый","Взрослый","Зрелый","Хранитель\nсада"};
            for(int n=0;n<9;n++){int age=n+1;float x=24+n%3*148,y=147+n/3*221;
                InkBox(panel,"Life stage "+age,x,y,136,210,age==Model.Stage?"E9D197":"F6E5BE",14,3);
                var preview=Rect(panel,"Age preview "+age,x+8,y+15,240,250);DrawAvatar(preview,null,false,age,true);preview.localScale=Vector3.one*.47f;
                Oval(panel,"Age number badge",x+7,y+7,28,28,age==Model.Stage?"A9C586":"E8C999",2);Label(panel,"Age number",age.ToString(),x+7,y+6,28,28,22,align:TextAnchor.MiddleCenter);
                Label(panel,"Stage name",captions[n],x+7,y+139,122,65,22,align:TextAnchor.MiddleCenter);
            }
        }
    }
}

