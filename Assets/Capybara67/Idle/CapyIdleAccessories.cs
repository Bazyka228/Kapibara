using UnityEngine;
using UnityEngine.UI;
namespace Capybara67.Idle {
 public sealed partial class CapyIdleGame {
  static readonly Rect[] HatCrops={
   new Rect(12,111,250,175),
   new Rect(272,108,250,165),
   new Rect(540,117,169,143),
   new Rect(740,98,216,190),
   new Rect(969,113,274,177),
   new Rect(17,330,216,206),
   new Rect(237,326,214,210),
   new Rect(454,349,216,180),
   new Rect(657,368,215,161),
   new Rect(855,339,200,181),
   new Rect(1061,361,185,159),
   new Rect(12,640,249,103),
   new Rect(268,593,241,174),
   new Rect(522,574,225,209),
   new Rect(762,558,216,226),
   new Rect(991,592,257,177),
   new Rect(11,826,285,185),
   new Rect(285,790,232,219),
   new Rect(516,815,238,208),
   new Rect(760,835,221,187),
   new Rect(987,825,258,202),
   new Rect(13,1067,230,154),
   new Rect(278,1034,223,189),
   new Rect(554,1019,194,211),
   new Rect(1007,1066,233,137)};
  static readonly Rect[] ClothCrops={
   new Rect(5,70,254,181),
   new Rect(294,70,225,169),
   new Rect(534,58,248,195),
   new Rect(788,61,260,196),
   new Rect(1056,66,239,189),
   new Rect(16,297,263,194),
   new Rect(287,274,247,216),
   new Rect(543,273,245,222),
   new Rect(790,329,261,157),
   new Rect(1053,297,237,188),
   new Rect(16,562,252,144),
   new Rect(277,527,270,220),
   new Rect(558,517,237,214),
   new Rect(817,515,228,215),
   new Rect(1104,512,178,210),
   new Rect(12,777,290,195),
   new Rect(302,773,251,198),
   new Rect(558,762,248,210),
   new Rect(811,767,255,206),
   new Rect(1088,769,185,201),
   new Rect(17,998,271,177),
   new Rect(300,979,245,208)};
  static readonly Sprite[] HatSprites=new Sprite[25],ClothSprites=new Sprite[22];
  static Sprite AccessorySprite(IdleItem item){
   bool hat=item.slot==0;var sprites=hat?HatSprites:ClothSprites;var crops=hat?HatCrops:ClothCrops;
   if(item.art<0||item.art>=sprites.Length)throw new System.InvalidOperationException("Unmapped wearable: "+item.id);
   if(sprites[item.art])return sprites[item.art];
   var tex=Resources.Load<Texture2D>(hat?"Accessories/Headwear":"Accessories/Clothes");
   if(!tex)throw new System.InvalidOperationException("Missing accessory atlas: "+item.id);
   var r=crops[item.art];float sx=tex.width/(hat?1254f:1300f),sy=tex.height/(hat?1254f:1209f);
   var sprite=Sprite.Create(tex,new Rect(r.x*sx,tex.height-(r.y+r.height)*sy,r.width*sx,r.height*sy),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
   sprite.name="Accessory "+item.id;sprites[item.art]=sprite;return sprite;
  }
  static Image AccessoryImage(Transform parent,IdleItem item,Rect frame,bool preserve=false){
   var image=Rect(parent,"Painted accessory "+item.id,frame.x,frame.y,frame.width,frame.height).gameObject.AddComponent<Image>();
   image.sprite=AccessorySprite(item);image.raycastTarget=false;image.preserveAspect=preserve;
   var mesh=image.gameObject.AddComponent<CapyAccessoryMesh>();mesh.bands=AccessoryMeshBands[(item.slot==0?0:25)+item.art];var crop=item.slot==0?HatCrops[item.art]:ClothCrops[item.art];mesh.sourceSize=crop.size;return image;
  }
  // Actual skull/eye landmarks in the illustrated pose, facing right (including age eight).
  static readonly Vector3[] HatSeats={new Vector3(171,151,78),new Vector3(143,69,110),new Vector3(168,47,102),new Vector3(165,126,106),new Vector3(169,52,106),new Vector3(170,63,112),new Vector3(181,73,86),new Vector3(179,91,88),new Vector3(178,47,96)};
  static readonly Vector2[] EyeSeats={new Vector2(154,189),new Vector2(130,107),new Vector2(151,78),new Vector2(148,148),new Vector2(155,76),new Vector2(148,85),new Vector2(163,91),new Vector2(167,113),new Vector2(164,72)};
  static readonly Vector2[] EarSeats={new Vector2(137,145),new Vector2(77,88),new Vector2(99,81),new Vector2(109,141),new Vector2(105,77),new Vector2(97,84),new Vector2(129,86),new Vector2(137,99),new Vector2(127,55)};
  static Sprite phoneCup;
  static readonly float[] HatAngles={8,9,23,16,12,9,8,7,10};
  static void PaintedHeadphoneBack(Transform p,IdleItem item,Rect head,int age){
   var ear=EarSeats[age-1];var image=AccessoryImage(p,item,new Rect(ear.x-12,head.y-12,head.width*.76f,head.height*.75f));image.name="Headphone band behind head";
  }
  // x/y = the point INSIDE the artwork that rests on the skull; z = width relative to skull.
  static readonly Vector3[] HatMounts={
   new Vector3(.50f,.78f,1.12f),new Vector3(.46f,.77f,1.08f),new Vector3(.50f,.83f,.72f),new Vector3(.52f,.82f,.96f),new Vector3(.50f,.79f,1.12f),
   new Vector3(.50f,.09f,.98f),new Vector3(.50f,.31f,1.04f),new Vector3(.50f,.50f,1.02f),new Vector3(.50f,.82f,1.03f),new Vector3(.50f,.88f,.87f),new Vector3(.50f,.82f,1.03f),
   new Vector3(.32f,.49f,.84f),new Vector3(.54f,.76f,1.02f),new Vector3(.50f,.83f,.96f),new Vector3(.50f,.85f,.86f),new Vector3(.50f,.85f,1.04f),
   new Vector3(.50f,.79f,1.14f),new Vector3(.52f,.87f,.90f),new Vector3(.50f,.40f,1.04f),new Vector3(.50f,.66f,1.02f),new Vector3(.50f,.74f,1.12f),
   new Vector3(.50f,.50f,.42f),new Vector3(.50f,.83f,.91f),new Vector3(.50f,.88f,.73f),new Vector3(.50f,.78f,.98f)};
  static Image MountAccessory(Transform p,IdleItem item,Vector2 seat,Vector2 anchor,float w,float h,float angle=0){
   var image=AccessoryImage(p,item,new Rect(0,0,w,h));var rect=image.rectTransform;
   rect.pivot=new Vector2(anchor.x,1-anchor.y);rect.anchoredPosition=new Vector2(seat.x,-seat.y);rect.localEulerAngles=new Vector3(0,0,angle);return image;
  }
  static void PaintedHat(Transform p,IdleItem item,Rect head,int age){
   if(item.art==5){
    if(!phoneCup){var texture=AccessorySprite(item).texture;float s=texture.width/1254f;phoneCup=Sprite.Create(texture,new Rect(18*s,texture.height-536*s,112*s,110*s),new Vector2(.5f,.5f),100);}
    float size=head.width*.29f;var ear=EarSeats[age-1];var cup=Rect(p,"Painted accessory "+item.id,ear.x-size*.5f,ear.y-size*.5f,size,size).gameObject.AddComponent<Image>();cup.sprite=phoneCup;cup.raycastTarget=false;return;
   }
   int art=item.art;var socket=HatSeats[age-1];var mount=HatMounts[art];var seat=new Vector2(socket.x,socket.y);
   float w=socket.z*mount.z,h=w*HatCrops[art].height/HatCrops[art].width;
   if(art==11)seat=EyeSeats[age-1];
   else if(art==21)seat=EarSeats[age-1];
   else if(art==6||art==18){h=head.height*.65f;seat.y-=10;seat.x-=head.width*.12f;}
   else if(art==7||art==24){h=socket.z*.34f;seat.y+=9;}
   // Resize around the mounting point: limiting height must never lift a hat off the skull.
   if(h*mount.y>seat.y+20){float scale=(seat.y+20)/(h*mount.y);w*=scale;h*=scale;}
   float angle=art==11||art==21||art==6||art==18?0:HatAngles[age-1];
   MountAccessory(p,item,seat,new Vector2(mount.x,mount.y),w,h,angle);
  }
  static bool IsPaintedCape(IdleItem item)=>item!=null&&item.slot==1&&(item.art==3||item.art==15||item.art==16);
  static void PaintedClothes(Transform p,IdleItem item,Rect torso,Rect neck,int age){
   int art=item.art;bool prone=age==1||age==4;neck=ClothingNecks[age-1];var seat=new Vector2(neck.center.x,neck.y);
   if(art==6||art==21){
    var back=BackpackSeats[age-1];float bw=back.z,bh=bw*ClothCrops[art].height/ClothCrops[art].width,bx=back.x,by=back.y;
    Stroke(p,"Bag shoulder piping",bx+bw*.65f,by+8,bx+bw*.95f,by-3,Ink,5);
    Stroke(p,"Bag shoulder strap",bx+bw*.65f,by+8,bx+bw*.95f,by-3,"9E7955",2.5f);
    Stroke(p,"Bag chest piping",bx+bw*.95f,by-3,prone?141:neck.x+neck.width*.25f,prone?194:seat.y+8,Ink,5);
    Stroke(p,"Bag chest strap",bx+bw*.95f,by-3,prone?141:neck.x+neck.width*.25f,prone?194:seat.y+8,"9E7955",2.5f);
    AccessoryImage(p,item,new Rect(bx,by,bw,bh));return;
   }
   if(art==0||art==1||art==10||art==14||art==19||art==20){
    float factor=art==1?.65f:art==14||art==19?.60f:art==10?.9f:.98f;
    float w=(prone?(age==1?54:63):neck.width)*factor,h=w*ClothCrops[art].height/ClothCrops[art].width;
    if(prone)seat=new Vector2(age==1?136:135,age==1?199:195);
    MountAccessory(p,item,seat,new Vector2(.5f,.13f),w,h,prone?-90:0);return;
   }
   if(art==8){AccessoryImage(p,item,new Rect(torso.x+3,torso.y+torso.height*.34f,torso.width-6,prone?27:36));return;}
   if(prone){MountAccessory(p,item,new Vector2(age==1?143:148,201),new Vector2(.5f,.04f),age==1?49:52,age==1?99:108,-90);}
   else {
    Rect fit=new Rect(torso.x-2,neck.y-7,torso.width+4,torso.yMax-neck.y+7);
    if(art==13)fit=new Rect(torso.x+torso.width*.22f,neck.y-3,torso.width*.70f,torso.yMax-neck.y-3);
    float collar=IsPaintedCape(item)?.68f:art==11?.52f:art==13?.52f:.60f;
    var image=AccessoryImage(p,item,fit);image.gameObject.AddComponent<CapyClothDrape>().upperShift=seat.x-(fit.x+collar*fit.width);
   }
   if(item.art==5)Mark(p,"6–7",torso.x+torso.width*.40f,torso.y+torso.height*.45f,torso.width*.40f,25,19,Ink);
  }
  static readonly Rect[] ClothingNecks={new Rect(133,181,22,46),new Rect(92,149,88,0),new Rect(102,132,81,0),new Rect(135,178,26,46),new Rect(101,125,83,0),new Rect(100,133,90,0),new Rect(143,137,70,0),new Rect(144,145,72,0),new Rect(115,125,80,0)};
  static readonly Vector3[] BackpackSeats={new Vector3(55,160,57),new Vector3(58,143,62),new Vector3(52,125,66),new Vector3(47,159,63),new Vector3(48,120,69),new Vector3(52,132,68),new Vector3(73,123,70),new Vector3(70,133,72),new Vector3(63,120,70)};
 }
 public sealed class CapyAccessoryMesh:BaseMeshEffect {
  public int[] bands;public Vector2 sourceSize;
  public override void ModifyMesh(VertexHelper mesh){
   if(!IsActive()||bands==null)return;var image=(Image)graphic;var sprite=image.sprite;var r=image.GetPixelAdjustedRect();var uv=sprite.textureRect;var texture=sprite.texture;
   if(image.preserveAspect){float ratio=sourceSize.x/sourceSize.y;if(r.width/r.height>ratio){float w=r.height*ratio;r.x+=(r.width-w)*.5f;r.width=w;}else{float h=r.width/ratio;r.y+=(r.height-h)*.5f;r.height=h;}}
   mesh.Clear();
   for(int n=0;n<bands.Length;n+=4){
    float x0=bands[n]/sourceSize.x,x1=(bands[n]+bands[n+2])/sourceSize.x,y0=1-bands[n+1]/sourceSize.y,y1=1-(bands[n+1]+bands[n+3])/sourceSize.y;int v=mesh.currentVertCount;
    mesh.AddVert(new Vector3(r.x+x0*r.width,r.y+y0*r.height),image.color,new Vector2((uv.x+x0*uv.width)/texture.width,(uv.y+y0*uv.height)/texture.height));
    mesh.AddVert(new Vector3(r.x+x0*r.width,r.y+y1*r.height),image.color,new Vector2((uv.x+x0*uv.width)/texture.width,(uv.y+y1*uv.height)/texture.height));
    mesh.AddVert(new Vector3(r.x+x1*r.width,r.y+y1*r.height),image.color,new Vector2((uv.x+x1*uv.width)/texture.width,(uv.y+y1*uv.height)/texture.height));
    mesh.AddVert(new Vector3(r.x+x1*r.width,r.y+y0*r.height),image.color,new Vector2((uv.x+x1*uv.width)/texture.width,(uv.y+y0*uv.height)/texture.height));
    mesh.AddTriangle(v,v+1,v+2);mesh.AddTriangle(v+2,v+3,v);
   }
  }
 }
 // Deform only the shoulder area toward the neck; the hem stays attached to the torso.
 public sealed class CapyClothDrape:BaseMeshEffect {
  public float upperShift;
  public override void ModifyMesh(VertexHelper mesh){if(!IsActive())return;var rect=((RectTransform)transform).rect;var vertex=new UIVertex();for(int n=0;n<mesh.currentVertCount;n++){mesh.PopulateUIVertex(ref vertex,n);float t=Mathf.Clamp01((vertex.position.y-rect.yMin)/rect.height);vertex.position.x+=upperShift*t*t;mesh.SetUIVertex(vertex,n);}}
 }
}
