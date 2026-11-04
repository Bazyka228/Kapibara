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
   int tier=0,heroIndex=0;
   for(int n=0;n<Model.S.surpriseResults.Count;n++)if(Model.S.surpriseResults[n].tier>tier){tier=Model.S.surpriseResults[n].tier;heroIndex=n;}
   tier=Mathf.Clamp(tier,0,3);string color=IdleModel.RarityColors[tier];
   Label(panel,"Opening headline",Model.S.surpriseResults.Count>1?"Дерево приготовило 10 подарков":"Подарок созрел!",30,110,420,58,27,align:TextAnchor.MiddleCenter);
   Label(panel,"Opening hint","Что спряталось под бантом?",30,172,420,57,22,"72543A",TextAnchor.MiddleCenter);
   var stage=Rect(panel,"Opening stage",0,241,480,340);
   Shape(stage,"Opening island shadow",90,282,300,25,"65432D24",0,true);Oval(stage,"Opening island",111,264,258,34,"C4D296",2);
   var halo=Rect(stage,"Gift halo",240,171,0,0);var haloOpacity=GiftOpacity(halo,0);
   Shape(halo,"Gift soft halo",-110,-110,220,220,color+"45",0,true);
   for(int n=0;n<4+tier*2;n++){
    float angle=n*Mathf.PI*2/(4+tier*2);var ray=Rect(halo,"Gift light ray "+n,Mathf.Cos(angle)*69,Mathf.Sin(angle)*69,0,0);
    ray.localEulerAngles=new Vector3(0,0,-angle*Mathf.Rad2Deg);Shape(ray,"Gift ray",0,-2,27+tier*4,4,color+"85",2);
   }
   // Prebuilt once for this opening. Nothing is instantiated or destroyed during the burst.
   var particles=new RectTransform[8+tier*4];var particleOpacity=new CanvasGroup[particles.Length];
   for(int n=0;n<particles.Length;n++){
    var q=Rect(stage,"Gift particle "+n,240,161,0,0);q.localScale=Vector3.zero;particleOpacity[n]=GiftOpacity(q,0);
    if(n%3==0)Star(q,-7,-7,14,color);else if(n%3==1)Leaf(q,-8,-5,16,10,n%2==0?"A9C780":"E5C67D",n*17);
    else Shape(q,"Gift confetti",-3,-5,6,10,color,2);
    particles[n]=q;
   }
   var reward=Rect(stage,"Gift reward medallion",240,211,0,0);var rewardOpacity=GiftOpacity(reward,0);reward.localScale=Vector3.one*.85f;
   Oval(reward,"Gift medallion edge",-59,-59,118,118,"FFF1CE",3);Oval(reward,"Gift medallion inset",-54,-54,108,108,color,2);
   var result=Model.S.surpriseResults[heroIndex];var item=IdleCatalog.Find(result.item);
   if(item!=null){var preview=Rect(reward,"Gift reward picture",-61.6f,-66.4f,78,78);preview.localScale=Vector3.one*1.6f;ItemPicture(preview,item);}
   else if(result.coins>0)Coin(reward,-31,-31,62);
   else {var motif=Rect(reward,"Gift reward symbol",-28,-30,40,40);motif.localScale=Vector3.one*1.5f;TreeEffectMotif(motif,13);}
   var box=Rect(stage,"Gift box motion",240,285,0,0);
   var body=Rect(box,"Gift body",0,0,0,0);
   // A dark inner rim becomes visible as the independent lid lifts away.
   Oval(body,"Gift inner rim",-90,-130,180,30,"6A6545",2);
   InkBox(body,"Gift wrapping",-90,-119,180,119,"91AC7E",12,3);
   Shape(body,"Gift side fold",66,-115,19,108,"759363",5);Shape(body,"Gift lid cast shadow",-84,-114,166,8,"55704D45",2);
   Stroke(body,"Gift lower piping",-75,-9,71,-9,"CBD8A6",3);
   Shape(body,"Gift ribbon shadow",-12,-114,31,110,"B49B5F",1);Shape(body,"Gift gold ribbon",-15,-116,27,112,"E8C17C",2);
   Stroke(body,"Gift ribbon seam",-10,-109,-10,-11,"FFF0BD",2);Stroke(body,"Gift ribbon seam",7,-109,7,-11,"BC9555",1.5f);
   Leaf(body,-69,-77,28,18,"C9D8A4",-18);Leaf(body,-54,-63,22,14,"ADC58A",22);
   Stroke(body,"Gift tag thread",31,-108,52,-78,Ink,2);InkBox(body,"Gift paper tag",40,-78,29,37,"FFF1CE",5,2);
   Shape(body,"Gift tag eyelet",52,-73,4,4,Ink,0,true);Leaf(body,47,-63,17,14,"94AE72",-14);
   Oval(body,"Gift seal",-17,-52,33,33,"D6A16C",2);Leaf(body,-9,-44,18,14,"F2D394",-10);
   var lid=Rect(box,"Gift lid",0,-124,0,0);var lidOpacity=GiftOpacity(lid,1);
   InkBox(lid,"Gift lid edge",-100,-19,200,35,"BFD097",9,3);Shape(lid,"Gift lid upper seam",-90,-14,178,4,"E4EBC3",2);
   Shape(lid,"Gift lid ribbon",-15,-16,27,28,"EDC985",2);Stroke(lid,"Gift lid ribbon seam",-10,-12,-10,8,"FFF0BD",2);
   var bow=Rect(lid,"Gift bow",0,-20,0,0);
   var leftBow=Rounded(238,132,167,20,69,0,9,23,0,85,55,138,144,150);
   var rightBow=Rounded(2,132,73,20,171,0,231,23,240,85,185,138,96,150);
   Contour(bow,"Gift left bow outline",-64,-42,66,42,Ink,leftBow);Contour(bow,"Gift left bow",-60,-38,58,34,"ECC580",leftBow);
   Contour(bow,"Gift right bow outline",-2,-42,66,42,Ink,rightBow);Contour(bow,"Gift right bow",2,-38,58,34,"F2D398",rightBow);
   Stroke(bow,"Gift bow left fold",-47,-28,-10,-7,"FFF0BE",3);Stroke(bow,"Gift bow right fold",11,-7,47,-28,"FFF0BE",3);
   InkBox(bow,"Gift bow knot",-14,-16,28,25,"DEB46E",7,2);Shape(bow,"Gift knot seam",-8,-11,16,3,"FFE8B0",1);
   Label(panel,"Opening rarity",IdleModel.RarityNames[tier]+(Model.S.surpriseResults.Count>1?" и другие подарки":" подарок"),30,597,420,71,27,"65432D",TextAnchor.MiddleCenter);
   Label(panel,"Gift saved note","Подарок уже твой",30,675,420,40,22,"72543A",TextAnchor.MiddleCenter);
   Button(panel,"Skip opening","ПОКАЗАТЬ ПОДАРКИ",24,740,432,72,()=>Show("surprise-results"),"55E6C1",24);
   if(Model.S.calm){Show("surprise-results");return;}
   const float pop=.82f;
   var sequence=DOTween.Sequence();sequence.AppendInterval(2.45f+tier*.12f);
   // Two readable, deterministic lid wiggles; no random shake or cumulative scale changes.
   sequence.Insert(.08f,lid.DOLocalRotate(new Vector3(0,0,-6),.12f).SetEase(Ease.OutSine));
   sequence.Insert(.20f,lid.DOLocalRotate(new Vector3(0,0,6),.16f).SetEase(Ease.InOutSine));
   sequence.Insert(.36f,lid.DOLocalRotate(new Vector3(0,0,-4),.14f).SetEase(Ease.InOutSine));
   sequence.Insert(.50f,lid.DOLocalRotate(Vector3.zero,.15f).SetEase(Ease.OutSine));
   sequence.Insert(.12f,lid.DOAnchorPosY(132,.18f).SetEase(Ease.OutQuad));sequence.Insert(.30f,lid.DOAnchorPosY(124,.20f).SetEase(Ease.InQuad));
   sequence.Insert(.59f,box.DOScale(new Vector3(1.035f,.94f,1),.23f).SetEase(Ease.InQuad));
   sequence.Insert(pop,box.DOScale(Vector3.one,.23f).SetEase(Ease.OutCubic));
   sequence.Insert(pop,lid.DOAnchorPos(new Vector2(116,279),.72f).SetEase(Ease.OutCubic));
   sequence.Insert(pop,lid.DOLocalRotate(new Vector3(0,0,-29),.72f).SetEase(Ease.OutCubic));
   sequence.Insert(pop+.35f,lidOpacity.DOFade(0,.37f).SetEase(Ease.InQuad));
   sequence.InsertCallback(pop,()=>Sound(evolveClip,.16f+tier*.02f));
   sequence.Insert(pop,haloOpacity.DOFade(1,.28f));sequence.Insert(pop,halo.DOScale(1.08f+tier*.025f,.65f).SetEase(Ease.OutCubic));
   sequence.Insert(1.04f,rewardOpacity.DOFade(1,.22f));sequence.Insert(1.04f,reward.DOAnchorPosY(-125,.52f).SetEase(Ease.OutCubic));
   sequence.Insert(1.04f,reward.DOScale(Vector3.one,.45f).SetEase(Ease.OutBack));
   sequence.Insert(1.39f,body.DOScale(new Vector3(1.035f,.96f,1),.12f).SetEase(Ease.InSine));sequence.Insert(1.51f,body.DOScale(Vector3.one,.18f).SetEase(Ease.OutSine));
   for(int n=0;n<particles.Length;n++){
    float a=(25+n*130f)*Mathf.Deg2Rad, radius=102+tier*9+n%3*10;var q=particles[n];float t=pop+.04f+n%3*.025f;
    sequence.Insert(t,particleOpacity[n].DOFade(1,.08f));sequence.Insert(t,q.DOScale(.65f+n%3*.13f,.20f).SetEase(Ease.OutCubic));
    sequence.Insert(t,q.DOAnchorPos(new Vector2(240+Mathf.Cos(a)*radius,-161+Mathf.Sin(a)*radius*.78f),.70f).SetEase(Ease.OutCubic));
    sequence.Insert(t,q.DOLocalRotate(new Vector3(0,0,(n%2==0?1:-1)*75),.70f).SetEase(Ease.OutSine));
    sequence.Insert(t+.51f,particleOpacity[n].DOFade(0,.27f));
   }
   // Reward was committed before this page. Completion only navigates; skip/close kills it.
   sequence.OnComplete(()=>{if(Page=="surprise-opening")Show("surprise-results");});surpriseReveal=tweens.Track(sequence,stage.gameObject);
  }
  static CanvasGroup GiftOpacity(RectTransform root,float alpha){
   var group=root.gameObject.AddComponent<CanvasGroup>();group.alpha=alpha;group.interactable=false;group.blocksRaycasts=false;return group;
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
