using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  RectTransform sectionHelp;
  string[] SectionGuide(){
   switch(Page){
    case "food":return new[]{"Еда","food","Нажми на еду","Миска — сытость.\nЛисточек — рост.","coin","Смотри на цену","Серую миску пока нельзя взять","paw","Нет монет?","Тапай по капибаре в саду"};
    case "care":return new[]{"Забота","food","Накорми","Листочек утолит голод","bath","Искупай","Води пальцем по пене","coin","Смотри на цену","Выбери оплату перед заботой"};
    case "shop":return new[]{"Гардероб","hat","Выбери вещь","Нажми на её картинку","pet","Посмотри на капибару","Примерка ничего не тратит","coin","Открой и надень","Цена указана на карточке"};
    case "growth":return new[]{"Рост","food","Корми капибару","Еда помогает ей расти","growth","Заполняй полоску","Впереди пять этапов","gift","Забирай подарки","Награда ждёт за каждый этап"};
    case "tap":return new[]{"Сила тапа","paw","Улучшай лапки","Тапы будут давать больше монет","coin","Смотри на цену","Нажми на нужное улучшение","video","Помощник за рекламу","5 видео → автотап на 20 минут"};
    case "activities":return new[]{"Занятия","basket","Выбери одну из шести","Лови, ищи, запоминай и рисуй","pet","Смотри на подсказку","Игра покажет, что делать","coin","Получай награду","Монеты и хорошее настроение"};
    case "surprises":return new[]{"Дерево","ticket","Выбери оплату","Цена — над кнопкой открытия","gift","Открой подарок","Внутри случайная вещь","hat","Найди её в гардеробе","Повтор даст монеты"};
    case "daily":return new[]{"Подарки","calendar","Заглядывай в календарь","Один подарок за день","scene","Третий подарок — сад","Новая сцена останется у тебя","hat","Седьмой — наряд","Пропуск дня не сбросит путь"};
    case "quests":return new[]{"Задания","paw","Посмотри на задание","Кнопка покажет, куда идти","growth","Заполни полоску","Счётчик покажет, сколько осталось","gift","Забери награду","Монеты и билет для дерева"};
    case "mandarins":return new[]{"Магазин","mandarin","Выбери набор","Покупка за настоящие деньги","coin","Один — как тысяча","1 мандарин = 1 000 монет","hat","Плати за вещи","Выбирай монеты или мандарины"};
    case "offline":return new[]{"С возвращением","bed","Капибара отдыхала","Подстилка приносила монеты","coin","Монеты уже твои","Доход учтён за время до 4 часов","pet","Пора в сад","Нажми «К капибаре»"};
    case "settings":return new[]{"Настройки","sound","Звуки и музыка","Включай, если хочется слушать","ball","Меньше движения","Анимации станут спокойнее","pet","Твои вещи останутся","Настройки не сбрасывают игру"};
    default:return null;
   }
  }
  void AddSectionHelpButton(Text title){
   var guide=SectionGuide();if(guide==null)return;
   title.text=guide[0].ToUpperInvariant();title.rectTransform.sizeDelta=new Vector2(275,58);
   Button(panel,"Section help","?",318,20,64,64,OpenSectionHelp,"A7CFD1",30);
  }
  void CloseSectionHelp(){if(!sectionHelp)return;sectionHelp.gameObject.SetActive(false);Destroy(sectionHelp.gameObject);sectionHelp=null;}
  void OpenSectionHelp(){
   var guide=SectionGuide();if(guide==null||sectionHelp)return;Release();Sound(careClip,.12f);
   sectionHelp=Rect(board,"Section picture guide",0,0,480,850);
   Shape(sectionHelp,"Guide backdrop",0,0,480,850,"172B24CF",0).raycastTarget=true;
   PaperCard(sectionHelp,"Guide paper",16,77,448,696);
   Label(sectionHelp,"Guide title","КАК ИГРАТЬ",39,96,400,52,28,align:TextAnchor.MiddleCenter).font=bold;
   for(int n=0;n<3;n++){
    float y=164+n*151;PaperCard(sectionHelp,"Guide step "+n,31,y,418,139);
    var art=Rect(sectionHelp,"Guide picture "+n,44,y+26,80,80);GuideArt(art,guide[1+n*3]);
    Label(sectionHelp,"Step title",guide[2+n*3],137,y+10,297,48,23).font=bold;
    Label(sectionHelp,"Step explanation",guide[3+n*3],137,y+58,297,72,22,"72543A");
   }
   Button(sectionHelp,"Close section help","ПОНЯТНО",41,651,398,84,()=>{Sound(careClip,.12f);CloseSectionHelp();},"55E6C1",26);
  }
  void GuideArt(Transform p,string kind){
   if(kind=="food"){DrawFoodIcon(p,2,4,8);return;}
   if(kind=="leaf"){Leaf(p,13,12,56,55,"8DB95B",-12);return;}
   if(kind=="coin"){Coin(p,7,9,62);return;}
   if(kind=="mandarin"){ActivityMandarin(p,8,9,63);return;}
   if(kind=="gift"){Gift(p,3,4,75);return;}
   if(kind=="pet"){var pet=Rect(p,"Guide capy",1,-2,240,250);pet.localScale=Vector3.one*.33f;DrawAvatar(pet,null,false,2,true);return;}
   if(kind=="bath"||kind=="ball"||kind=="paw"||kind=="bed"){CareIcon(p,kind=="bath"?1:kind=="ball"?2:kind=="bed"?5:3);return;}
   if(kind=="hat"){InkBox(p,"Hat crown",19,10,44,42,"91B68C",8,3);Shape(p,"Hat ribbon",22,35,38,10,"F3CF79",2);InkBox(p,"Hat brim",3,50,75,12,"91B68C",6,3);Leaf(p,46,17,20,15,"5B8C58",-20);return;}
   if(kind=="basket"){ActivityBasketArt(p,0,33,.7f);ActivityMandarin(p,26,0,34);return;}
   if(kind=="search"){ActivityMandarin(p,17,10,43);Leaf(p,4,40,43,32,"8DB95B",-12);Leaf(p,49,28,26,43,"76A15D",10);return;}
   if(kind=="growth"){Leaf(p,4,38,25,25,"8DB95B",-10);Stroke(p,"Arrow",30,41,45,41,Ink,3);Stroke(p,"Arrow tip",39,35,45,41,Ink,3);Stroke(p,"Arrow tip",39,47,45,41,Ink,3);Leaf(p,50,10,30,51,"8DB95B",-5);return;}
   if(kind=="ticket"){InkBox(p,"Ticket",2,18,76,44,"F3CF79",8,3);Stroke(p,"Ticket tear",22,23,22,56,Ink,2);Gift(p,34,24,29);return;}
   if(kind=="scene"){InkBox(p,"Scene frame",2,5,76,64,"A7CFD1",7,3);Oval(p,"Sun",48,13,19,19,"F3CF79",1);Oval(p,"Hill",7,39,65,25,"8DB95B",1);return;}
   if(kind=="heart"){Heart(p,10,10,60,"DD91A4");return;}
   if(kind=="sparkles"){Star(p,5,3,48,"E8BE74");Star(p,46,40,29,"8DBFC5");return;}
   if(kind=="fur"){Oval(p,"Fur swatch",6,5,68,68,"DDA87E",3);Oval(p,"Fur patch",37,21,24,30,"AB775D",0);return;}
   if(kind=="calendar"){InkBox(p,"Calendar",10,9,61,63,"FFF1CE",7,3);Shape(p,"Top",13,12,55,14,"E5A28D",3);for(int n=0;n<6;n++)Shape(p,"Day",20+n%3*15,34+n/3*17,9,9,n==4?"8DB95B":"D6BE8C",2);return;}
   if(kind=="sound"){Mark(p,"♪",6,0,68,77,60,Ink);return;}
   InkBox(p,"Video screen",2,9,76,53,"A7CFD1",8,3);Contour(p,"Play",31,21,25,28,Ink,Rounded(0,0,240,75,0,150));
  }
 }
}
