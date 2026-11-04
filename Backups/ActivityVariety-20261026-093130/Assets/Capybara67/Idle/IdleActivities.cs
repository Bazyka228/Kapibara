using System;
namespace Capybara67.Idle {
 [Serializable] public sealed class IdleActivity {
  public int kind,serial,score,mask,resolved,seed;
  public double elapsed,rate,basket=.5,reward;
  public bool complete;
 }
 public sealed partial class IdleModel {
  public bool StartActivity(int kind){
   if(kind<0||kind>1||Stage<2||!S.starter)return false;
   if(S.activity!=null&&!S.activity.complete)return S.activity.kind==kind;
   S.activity=new IdleActivity{kind=kind,serial=++S.activitySerial,seed=S.activitySerial%7,rate=2*CareValue*(1+.05*S.tapBonus)};
   return true;
  }
  public static double FruitX(IdleActivity a,int index)=>.12+((index*37+a.seed*19)%77)/100.0;
  public static double FruitAge(IdleActivity a,int index)=>a.elapsed-index*2.7;
  public void ActivityBasket(double x){if(S.activity!=null&&!S.activity.complete&&!double.IsNaN(x))S.activity.basket=Math.Max(0,Math.Min(1,x));}
  public bool ActivityFind(int index){
   var a=S.activity;if(a==null||a.complete||a.kind!=1||index<0||index>=6||(a.mask&(1<<index))!=0)return false;
   a.mask|=1<<index;a.score++;return true;
  }
  public void StepActivity(double seconds){
   var a=S.activity;if(a==null||a.complete||seconds<=0||double.IsNaN(seconds)||double.IsInfinity(seconds))return;
   a.elapsed=Math.Min(25,a.elapsed+seconds);
   if(a.kind==0)for(int i=0;i<8;i++)if((a.resolved&(1<<i))==0&&FruitAge(a,i)>=3.2){a.resolved|=1<<i;if(Math.Abs(a.basket-FruitX(a,i))<=.14)a.score++;}
   if(a.elapsed>=25||(a.kind==1&&a.score==6&&a.elapsed>=15))FinishActivity();
  }
  void FinishActivity(){
   var a=S.activity;if(a==null||a.complete)return;
   int full=a.kind==0?7:6;double factor=a.score>=full?1.25:a.score>=3?1.1:a.score>0?.5:.125;
   a.reward=Math.Max(Math.Ceiling(Math.Ceiling(a.rate*a.elapsed*1.25)*.1),Math.Ceiling(a.rate*a.elapsed*factor));a.complete=true;
   S.activityCompleted=true;
   S.coins+=a.reward;S.totalCoins+=a.reward;S.mood=Math.Min(100,S.mood+(a.score>0?10:1));
  }
 }
}
