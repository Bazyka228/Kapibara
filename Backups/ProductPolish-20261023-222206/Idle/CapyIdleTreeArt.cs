using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  static void TreeFruit(Transform p,float x,float y,float size,string color){
   var f=Rect(p,"Mandarin fruit",x,y,size,size);var a=Rect(f,"Fruit drawing",0,0,40,44);a.localScale=Vector3.one*(size/40);
   Oval(a,"Fruit outline",1,5,38,33,color,2);Stroke(a,"Peel seam",20,11,20,33,"A76C4040",1.2f);
   Shape(a,"Peel dimple",9,19,2,2,"B57950",0,true);Shape(a,"Peel dimple",13,25,2,2,"B57950",0,true);
   Shape(a,"Soft fruit light",8,11,11,4,"FFE4AC",2);Stroke(a,"Fruit stem",20,5,22,0,Ink,2);
   Leaf(a,20,0,14,8,"93B66E",-20);return;
  }
  static void DrawSurpriseTree(Transform p){
   Shape(p,"Root shadow",11,184,257,19,"65432D22",0,true);
   Shape(p,"Moss bed edge",20,170,238,30,"78915B",0,true);Shape(p,"Moss bed",25,167,226,29,"B7CA8B",0,true);
   var trunk=Rounded(85,0,132,7,149,57,196,35,238,42,191,77,154,90,157,125,221,145,157,150,112,138,62,150,0,144,70,122,93,85,48,60,8,44,33,31,100,59);
   Contour(p,"Branching trunk outline",68,69,143,121,"65432D",trunk);Contour(p,"Warm tree bark",72,71,135,115,"BC895C",trunk);
   Stroke(p,"Trunk light ridge",138,103,132,164,"D9AA74",6);Stroke(p,"Bark groove",149,122,147,166,"956642",2);
   Stroke(p,"Bark groove",127,139,121,168,"956642",2);Stroke(p,"Root ridge",131,168,99,181,"D9AA74",3);
   Stroke(p,"Root ridge",146,168,172,183,"976844",2);Oval(p,"Bark knot",137,130,11,17,"A7754D",1.5f);Stroke(p,"Knot line",142,135,141,142,"69472F",1.5f);
   float[] crownX={41,100,156,9,61,120,184}, crownY={20,5,24,55,38,40,56};
   for(int n=0;n<7;n++){
    float x=crownX[n],y=crownY[n];var cluster=Rect(p,"Leaf crown "+n,x,y,70,73);
    Oval(cluster,"Canopy dark edge",0,0,70,68,"608652",2);Oval(cluster,"Canopy cushion",3,-4,64,58,n%2==0?"8EAD68":"A7BE78",1.5f);
    Shape(cluster,"Leaf mass light",12,3,41,23,"CAD88B",0,true);
    for(int k=0;k<3;k++){Leaf(cluster,8+k*16,32+(k%2)*6,21,13,k%2==0?"B5CD87":"769B5E",k*19-15);}
    AnimateDetail(cluster,4,n*.8f,1.5f);
   }
   for(int n=0;n<4;n++){
    float x=36+n*52,y=80+n%2*11;var fruit=Rect(p,"Swaying gift "+n,x,y,37,44);
    Stroke(fruit,"Hanging stem",18,-12,18,4,"655C37",2);TreeFruit(fruit,0,0,37,n==3?"E3B75E":"EBA466");AnimateDetail(fruit,4,n*1.3f,3);
   }
   Leaf(p,94,12,36,19,"B5CE89",-22);Leaf(p,142,5,41,21,"94B96F",12);
   InkBox(p,"Tree door",125,151,25,31,"815C40",11,2);InkBox(p,"Door inset",129,155,17,24,"D8B783",7,1);Oval(p,"Door handle",139,169,4,4,"F2D38A",1);
   for(int n=0;n<6;n++){float x=n<3?25+n*20:202+(n-3)*17;Leaf(p,x,169+n%2*7,18,12,"83A164",n*17);Flower(p,x,172+n%2*9,n%2==0?"F4DDA0":"E6B2AD",.55f);}
   for(int n=0;n<5;n++){var glint=Rect(p,"Tree firefly",16+n*59,39+n%3*45,12,12);Shape(glint,"Warm glow",-3,-3,17,17,"FFE7B350",0,true);Shape(glint,"Firefly",3,3,4,4,"FFE8B2",0,true);AnimateDetail(glint,0,n*1.7f,5);}
   Rock(p,91,193,26,"CEC399");Rock(p,158,193,19,"DBCEA6");
  }
  void SurpriseOpeningPage(){
   if(Model.S.surpriseResults.Count==0){SurpriseResultsPage();return;}
   int tier=0;foreach(var reward in Model.S.surpriseResults)tier=Mathf.Max(tier,reward.tier);
   string color=IdleModel.RarityColors[tier];
   Label(panel,"Opening headline",Model.S.surpriseResults.Count>1?"Дерево приготовило 10 подарков":"Подарок созрел!",30,110,420,58,27,align:TextAnchor.MiddleCenter);
   Label(panel,"Opening hint","Всё уже сохранено. Можно пропустить.",30,172,420,57,22,"72543A",TextAnchor.MiddleCenter);
   var stage=Rect(panel,"Opening stage",0,241,480,340);
   Shape(stage,"Opening island shadow",90,282,300,25,"65432D24",0,true);Oval(stage,"Opening island",111,264,258,34,"C4D296",2);
   var halo=Rect(stage,"Rarity halo",240,155,0,0);Shape(halo,"Halo",-117,-117,234,234,color+"55",0,true);
   for(int n=0;n<(tier+1)*4;n++){float angle=n*Mathf.PI*2/((tier+1)*4);var ray=Rect(halo,"Rarity ray",Mathf.Cos(angle)*55,Mathf.Sin(angle)*55,6,70);ray.localEulerAngles=new Vector3(0,0,90-angle*Mathf.Rad2Deg);Shape(ray,"Ray",0,0,tier==3?5:3,55,color+"70",2);}
   var fruit=Rect(stage,"Opening fruit",191,65,98,110);TreeFruit(fruit,0,0,98,"F0B169");
   var left=Rect(stage,"Left peel",205,138,40,45);Leaf(left,0,0,43,54,"E7AD65",15);left.gameObject.SetActive(false);
   var right=Rect(stage,"Right peel",236,138,40,45);Leaf(right,0,0,43,54,"F5C882",-15);right.gameObject.SetActive(false);
   var particles=new RectTransform[8+tier*4];
   for(int n=0;n<particles.Length;n++){var q=Rect(stage,"Rarity particle "+n,225,140,30,34);q.localScale=Vector3.zero;TreeEffectMotif(q,new[]{16,14,17,18}[tier]);particles[n]=q;}
   Label(panel,"Opening rarity",IdleModel.RarityNames[tier]+(Model.S.surpriseResults.Count>1?" и другие подарки":" подарок"),30,597,420,71,27,"65432D",TextAnchor.MiddleCenter);
   Button(panel,"Skip opening","ПОКАЗАТЬ ПОДАРКИ",24,740,432,72,()=>Show("surprise-results"),"55E6C1",24);
   if(Model.S.calm){Show("surprise-results");return;}
   float lift=.6f+tier*.12f;
   var sequence=DOTween.Sequence();sequence.Append(fruit.DOAnchorPosY(-114,.28f).SetEase(Ease.InQuad));
   sequence.Append(fruit.DOShakeRotation(lift,new Vector3(0,0,7+tier*3),8,30).SetEase(Ease.InOutSine));
   sequence.Join(halo.DOScale(1.15f+tier*.1f,lift).SetEase(Ease.InOutSine));
   sequence.AppendCallback(()=>{fruit.gameObject.SetActive(false);left.gameObject.SetActive(true);right.gameObject.SetActive(true);Sound(evolveClip,.16f+tier*.02f);});
   sequence.Append(left.DOAnchorPos(new Vector2(122,-228),.65f).SetEase(Ease.OutCubic));sequence.Join(left.DOLocalRotate(new Vector3(0,0,75),.65f));
   sequence.Join(right.DOAnchorPos(new Vector2(315,-228),.65f).SetEase(Ease.OutCubic));sequence.Join(right.DOLocalRotate(new Vector3(0,0,-75),.65f));
   float burst=.28f+lift;
   for(int n=0;n<particles.Length;n++){float a=2*Mathf.PI*n/particles.Length;float radius=91+tier*13+n%2*16;var q=particles[n];sequence.Insert(burst,q.DOScale(.6f+n%3*.13f,.35f).SetEase(Ease.OutBack));sequence.Insert(burst,q.DOAnchorPos(new Vector2(225+Mathf.Cos(a)*radius,-140+Mathf.Sin(a)*radius),.8f).SetEase(Ease.OutCubic));sequence.Insert(burst+.65f,q.DOScale(0,.3f));}
   sequence.AppendInterval(.35f);sequence.OnComplete(()=>{if(Page=="surprise-opening")Show("surprise-results");});surpriseReveal=tweens.Track(sequence,stage.gameObject);
  }
  static void TreeEffectMotif(Transform p,int art){
   switch(art){
    case 13: TreeFruit(p,1,1,28,"F0B16B");Leaf(p,26,17,13,9,"91B36F",18);break;
    case 14:
     Contour(p,"Dew crystal outline",4,0,25,33,Ink,Rounded(120,0,240,65,192,150,44,150,0,64));
     Contour(p,"Dew crystal",6,3,21,27,"ABD6D3",Rounded(120,0,240,65,192,150,44,150,0,64));Stroke(p,"Crystal facet",16,7,16,25,"EFF8DD",2);LifeSpark(p,25,0,10);break;
    case 15:
     Shape(p,"Lantern glow",-6,-6,43,48,"F5D49A35",0,true);InkBox(p,"Paper lantern",3,2,26,29,"E6AA8D",8,1.5f);Stroke(p,"Lantern rib",16,5,16,28,"F8D2AE",2);InkBox(p,"Lantern base",7,29,18,4,"BB8D68",1,1);Shape(p,"Lantern flame",12,23,8,6,"FFF1BF",0,true);break;
    case 16:
     for(int n=0;n<3;n++)Heart(p,3+n%2*13,n==2?14:1,19,n==0?"94B77A":"B1CB87");Stroke(p,"Clover stem",17,20,13,34,"68884D",2);break;
    case 17:
     var ribbon=Rounded(0,5,90,30,180,0,240,45,183,92,110,77,176,150,99,136,40,79);
     Contour(p,"Silk ribbon edge",0,0,32,34,Ink,ribbon);Contour(p,"Silk ribbon",2,2,28,29,"C9A4CF",ribbon);Stroke(p,"Ribbon seam",8,9,21,12,"F4DCEC",2);break;
    default:
     for(int n=0;n<3;n++){Leaf(p,2+n*4,10+n*4,17,10,"E3BE72",-30);Leaf(p,18-n*2,10+n*4,17,10,"F5D795",30);}Star(p,8,0,21,"EDBB56");Star(p,13,5,11,"FFF0BC");break;
   }
  }
  static void TreeEmotionMotif(Transform p,int art){
   if(art==12){Stroke(p,"Delighted eye",8,16,13,12,Ink,2);Stroke(p,"Delighted eye",13,12,17,16,Ink,2);Stroke(p,"Delighted eye",26,16,30,12,Ink,2);Stroke(p,"Delighted eye",30,12,34,16,Ink,2);Oval(p,"Happy mouth",16,21,13,10,"AF7257",1);Shape(p,"Pink tongue",20,26,6,3,"EFAB9E",1);Leaf(p,30,1,12,7,"91AE71");}
   else if(art==13){Stroke(p,"Wish eyelid",9,17,17,19,Ink,2);Stroke(p,"Wish eyelid",26,19,34,17,Ink,2);Star(p,17,21,13,"EBC878");LifeSpark(p,15,1,13);}
   else if(art==14){Shape(p,"Applause eye",10,12,4,5,Ink,0,true);Shape(p,"Applause eye",29,12,4,5,Ink,0,true);Oval(p,"Clapping paw",10,21,12,12,"DFAD87",1.5f);Oval(p,"Clapping paw",22,21,12,12,"DFAD87",1.5f);Stroke(p,"Clap ray",19,6,19,1,"CCA15F",2);Stroke(p,"Clap ray",26,7,30,3,"CCA15F",2);}
   else if(art==15){Stroke(p,"Whistle eye",9,17,17,18,Ink,2);Shape(p,"Whistle eye",29,13,4,5,Ink,0,true);Oval(p,"Whistle mouth",20,24,6,6,"D3A080",1.5f);Stroke(p,"Note",33,2,33,9,Ink,1.5f);Oval(p,"Note pearl",28,7,6,4,"93BDB6",1);}
   else if(art==16){Stroke(p,"Dream eye",9,17,17,18,Ink,2);Stroke(p,"Dream eye",26,18,34,17,Ink,2);Cloud(p,12,23,.18f,"BDB7D7");Star(p,20,2,12,"E8CB8B");}
   else {Star(p,5,9,16,"E9BD65");Star(p,25,9,16,"E9BD65");Stroke(p,"Radiant smile",16,27,22,30,Ink,2);Stroke(p,"Radiant smile",22,30,29,26,Ink,2);Shape(p,"Blush",4,25,7,4,"E6A594",0,true);Shape(p,"Blush",33,25,7,4,"E6A594",0,true);}
  }
 }
}
