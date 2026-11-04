using System;
namespace Capybara67.Idle {
 [Serializable] public sealed class IdleActivity {
  public int kind,serial,score,mask,resolved,seed,patternVersion,variant;
  public double elapsed,rate,basket=.5,reward;
  public bool complete,newBest;
  public int first=-1,second=-1,round,step,colors,selection;
  public double revealUntil,sequenceAt,feedbackUntil;
  public int deck,attempts,combo,bestCombo,helps,brush,patterns;
  public double readyAt,hintUntil;
 }
 public sealed partial class IdleModel {
  public const int ActivityCount=6;
  public static readonly string[] ActivityNames={"Корзинка","Секреты сада","Уютные пары","Лесной оркестр","Весёлый пикник","Сад художника"};
  public static int ActivityGoal(int kind)=>kind==0?8:kind==2?3:kind==3?9:6;
  public static int ActivityDuration(int kind)=>kind<2?25:kind==4?35:45;
  public bool StartActivity(int kind){
   if(kind<0||kind>=ActivityCount||Stage<2||!S.starter)return false;
   if(S.activity!=null&&!S.activity.complete)return S.activity.kind==kind;
   S.activity=new IdleActivity{kind=kind,serial=S.activitySerial=Math.Max(0,Math.Min(int.MaxValue-1,S.activitySerial))+1,seed=S.activitySerial%7,patternVersion=2,variant=S.activityRuns[kind]%3,rate=2*CareValue*(1+.05*S.tapBonus),sequenceAt=.6};
   var a=S.activity;
   if(kind==2){int[] cards={0,0,1,1,2,2};uint random=(uint)a.serial*747796405u+2891336453u;for(int i=5;i>0;i--){random=unchecked(random*1664525u+1013904223u);int j=(int)(random%(uint)(i+1)),v=cards[i];cards[i]=cards[j];cards[j]=v;}for(int i=0;i<6;i++)a.deck|=cards[i]<<(i*2);a.revealUntil=1.8;}
   return true;
  }
  public static double FruitX(IdleActivity a,int index){
   if(a.patternVersion==0)return .12+((index*37+a.seed*19)%77)/100.0;
   if(index==0)return .5;
   if(a.variant==0)return .5+Math.Sin(index*1.05)*.32;
   if(a.variant==1)return .18+((index+a.seed)%4)*.21;
   return .5+Math.Sin(index*.8+a.seed)*.30;
  }
  public static double FruitFallTime(IdleActivity a)=>a.patternVersion==0?3.2:a.variant==1?3.6:a.variant==2?3.4:3.2;
  public static string ActivityVariantName(int kind,int variant)=>kind==0?
   (variant==0?"Солнечный урожай":variant==1?"Фруктовая дорожка":"Лёгкий ветерок"):
   (variant==0?"Лиственный сад":variant==1?"Цветочная поляна":"Грибная тропинка");
  public static double SearchX(IdleActivity a,int index){
   if(a.patternVersion==0||a.variant==0)return 22+((index+a.seed)%3)*132+((index*7+a.seed*11)%17);
   return a.variant==1?64+((index+a.seed)%2)*207:22+((index+a.seed)%3)*132;
  }
  public static double SearchY(IdleActivity a,int index){
   if(a.patternVersion==0||a.variant==0)return 67+(index/3)*158+((index*23+a.seed*17)%38);
   return a.variant==1?26+(index/2)*112:45+(index/3)*164+((index+a.seed)%3)*19;
  }
  void RepairActivities(){
   if(S.activityRuns==null)S.activityRuns=new int[ActivityCount];else if(S.activityRuns.Length!=ActivityCount)Array.Resize(ref S.activityRuns,ActivityCount);
   if(S.activityBest==null)S.activityBest=new int[ActivityCount];else if(S.activityBest.Length!=ActivityCount)Array.Resize(ref S.activityBest,ActivityCount);
   for(int i=0;i<ActivityCount;i++){S.activityRuns[i]=Math.Max(0,Math.Min(1000000000,S.activityRuns[i]));S.activityBest[i]=Math.Max(0,Math.Min(ActivityGoal(i),S.activityBest[i]));}
   var a=S.activity;if(a==null)return;if(a.kind<0||a.kind>=ActivityCount){S.activity=null;return;}
   a.elapsed=Clean(a.elapsed,ActivityDuration(a.kind));a.rate=Clean(a.rate,1e9);a.reward=Clean(a.reward,1e12);a.basket=Clean(a.basket,1);
   a.variant=Math.Max(0,Math.Min(2,a.variant));a.score=Math.Max(0,Math.Min(ActivityGoal(a.kind),a.score));a.mask&=63;a.resolved&=255;
   a.first=Math.Max(-1,Math.Min(5,a.first));a.second=Math.Max(-1,Math.Min(5,a.second));a.round=Math.Max(0,Math.Min(3,a.round));a.step=Math.Max(0,Math.Min(4,a.step));a.selection=Math.Max(0,Math.Min(4,a.selection));
   a.revealUntil=Clean(a.revealUntil,ActivityDuration(a.kind)+2);a.sequenceAt=Clean(a.sequenceAt,ActivityDuration(a.kind)+4);a.feedbackUntil=Clean(a.feedbackUntil,ActivityDuration(a.kind)+2);
   a.readyAt=Clean(a.readyAt,ActivityDuration(a.kind)+1);a.hintUntil=Clean(a.hintUntil,ActivityDuration(a.kind)+3);a.brush=Math.Max(0,Math.Min(2,a.brush));a.patterns&=4095;
   a.attempts=Math.Max(0,Math.Min(10000,a.attempts));a.helps=Math.Max(0,Math.Min(10000,a.helps));a.combo=Math.Max(0,Math.Min(8,a.combo));a.bestCombo=Math.Max(a.combo,Math.Min(8,a.bestCombo));
  }
  public static bool MemoryPreview(IdleActivity a)=>a.patternVersion>=2&&a.attempts==0&&a.first<0&&a.elapsed<a.revealUntil;
  public static int MemoryFace(IdleActivity a,int index){if(a.patternVersion>=2)return ((a.deck>>(index*2))&3)%3;int[] order={0,1,2,1,0,2};return (order[(index+(a.seed%6+6)%6)%6]+a.variant)%3;}
  public bool ActivityCard(int index){
   var a=S.activity;if(a==null||a.kind!=2||a.complete||MemoryPreview(a)||index<0||index>=6||(a.mask&(1<<index))!=0||a.second>=0||a.first==index)return false;
   if(a.first<0){a.first=index;return true;}
   a.second=index;a.revealUntil=a.elapsed+.95;a.attempts++;
   if(MemoryFace(a,a.first)==MemoryFace(a,index)){a.mask|=(1<<a.first)|(1<<index);a.score++;}
   return true;
  }
  public static int MusicLength(IdleActivity a)=>Math.Min(4,2+a.round);
  public static int MusicNote(IdleActivity a,int index)=>((a.seed%4+4)%4+a.variant*2+a.round+index*(a.round==1?3:1)+index/2)%4;
  public static bool MusicShowing(IdleActivity a)=>a.round<3&&a.elapsed<a.sequenceAt+MusicLength(a)*.7+.25;
  public static int MusicCue(IdleActivity a){double t=a.elapsed-a.sequenceAt;return t>=0&&t<MusicLength(a)*.7&&t%.7<.5?MusicNote(a,(int)(t/.7)):-1;}
  public bool ActivityNote(int note){
   var a=S.activity;if(a==null||a.kind!=3||a.complete||a.round>=3||note<0||note>3||MusicShowing(a))return false;
   if(note!=MusicNote(a,a.step)){a.step=0;a.sequenceAt=a.elapsed+.7;a.feedbackUntil=a.elapsed+.7;return false;}
   a.step++;if(a.step>=MusicLength(a)){a.score+=MusicLength(a);a.round++;a.step=0;a.sequenceAt=a.elapsed+.7;}
   return true;
  }
  public bool RepeatMusic(){var a=S.activity;if(a==null||a.kind!=3||a.complete||a.round>=3||MusicShowing(a))return false;a.step=0;a.sequenceAt=a.elapsed+.4;a.helps++;return true;}
  public static int SearchHint(IdleActivity a){for(int n=0;n<6;n++){int i=(n+(a.seed%6+6)%6)%6;if((a.mask&(1<<i))==0)return i;}return -1;}
  public bool HintActivity(){var a=S.activity;if(a==null||a.kind!=1||a.complete||a.score==6||a.elapsed<a.hintUntil)return false;a.hintUntil=a.elapsed+3;a.helps++;return true;}
  public static int PicnicGroup(IdleActivity a)=>((a.seed%3+3)%3+a.score*2+a.score/3)%3;
  public bool ActivitySort(int group){
   var a=S.activity;if(a==null||a.kind!=4||a.complete||a.score>=6||a.elapsed<a.readyAt||group<0||group>2)return false;
   if(group!=PicnicGroup(a)){a.feedbackUntil=a.elapsed+.8;return false;}a.score++;if(a.patternVersion>=2)a.readyAt=a.elapsed+.42;return true;
  }
  public bool ActivityPaint(int petal,int colour){
   var a=S.activity;if(a==null||a.kind!=5||a.complete||petal<0||petal>=6||colour<0||colour>4)return false;
   a.colors=(a.colors&~(7<<(petal*3)))|((colour+1)<<(petal*3));a.patterns=(a.patterns&~(3<<(petal*2)))|(a.brush<<(petal*2));if((a.mask&(1<<petal))==0){a.mask|=1<<petal;a.score++;}return true;
  }
  public bool FinishPainting(){var a=S.activity;if(a==null||a.kind!=5||a.complete||a.score<6)return false;FinishActivity();return true;}
  public static double FruitAge(IdleActivity a,int index)=>a.elapsed-index*2.7;
  public void ActivityBasket(double x){if(S.activity!=null&&!S.activity.complete&&!double.IsNaN(x))S.activity.basket=Math.Max(0,Math.Min(1,x));}
  public bool ActivityFind(int index){
   var a=S.activity;if(a==null||a.complete||a.kind!=1||index<0||index>=6||(a.mask&(1<<index))!=0)return false;
   a.mask|=1<<index;a.score++;a.feedbackUntil=a.elapsed+6;if(a.score==6)a.revealUntil=a.elapsed+.75;return true;
  }
  public void StepActivity(double seconds){
   var a=S.activity;if(a==null||a.complete||seconds<=0||double.IsNaN(seconds)||double.IsInfinity(seconds))return;
   a.elapsed=Math.Min(ActivityDuration(a.kind),a.elapsed+seconds);
   if(a.kind==2&&a.second>=0&&a.elapsed>=a.revealUntil){a.first=a.second=-1;}
   if(a.kind==0)for(int i=0;i<8;i++)if((a.resolved&(1<<i))==0&&FruitAge(a,i)>=FruitFallTime(a)){a.resolved|=1<<i;if(Math.Abs(a.basket-FruitX(a,i))<=.14){a.score++;a.combo++;a.bestCombo=Math.Max(a.bestCombo,a.combo);}else a.combo=0;}
   if(a.elapsed>=ActivityDuration(a.kind)||(a.kind==1&&a.score==6&&a.elapsed>=a.revealUntil)||(a.kind==2&&a.score==3&&a.second<0)||(a.kind==3&&a.round>=3)||(a.kind==4&&a.score==6&&a.elapsed>=a.readyAt))FinishActivity();
  }
  void FinishActivity(){
   var a=S.activity;if(a==null||a.complete)return;
   int full=a.kind==0?7:ActivityGoal(a.kind);double factor=a.score>=full?1.25:a.score>=3?1.1:a.score>0?.5:.125;
   a.reward=Math.Max(Math.Ceiling(Math.Ceiling(a.rate*a.elapsed*1.25)*.1),Math.Ceiling(a.rate*a.elapsed*factor));a.complete=true;
   S.activityCompleted=true;
   a.newBest=a.score>S.activityBest[a.kind];S.activityBest[a.kind]=Math.Max(S.activityBest[a.kind],a.score);
   S.activityRuns[a.kind]=Math.Min(1000000000,S.activityRuns[a.kind]+1);
   Earn(a.reward);S.mood=Math.Min(100,S.mood+(a.score>0?10:1));
  }
 }
}
