using System;
namespace Capybara67
{
    public enum FlightMood { Normal, Wind, Choice, Rain, Helper }
    public sealed class RoundModel
    {
        public const float Duration=67f, Window=.19f, PerfectWindow=.085f;
        public float Remaining { get; private set; }
        public float FlightTime { get; private set; }
        public float FlightDuration { get; private set; }
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int BestCombo { get; private set; }
        public int Hits { get; private set; }
        public int PerfectHits { get; private set; }
        public int PerfectCombo { get; private set; }
        public int BestPerfectCombo { get; private set; }
        public int Misses { get; private set; }
        public int Lives { get; private set; }
        public int Target { get; private set; }
        public bool Running { get; private set; }
        public FlightMood Mood { get; private set; }
        public int Spark { get; private set; }
        public int LastBonus { get; private set; }
        public bool LastPerfect { get; private set; }
        public bool LastEarly { get; private set; }
        public int Mission { get; private set; }
        public bool MissionDone => expanded && roundNumber>0 && MissionProgress>=MissionGoal;
        public int MissionProgress => Mission==0?Hits:Mission==1?PerfectHits:BestCombo;
        public int MissionGoal => Mission==0?10:Mission==1?3:7;
        public string MissionName => Mission==0?"Поймай 10 мандаринов":Mission==1?"3 идеальные ловли":"Серия из 7 ловель";
        public float Progress => FlightDuration>0?Math.Min(1f,FlightTime/FlightDuration):0;
        public float EarlyCenter => FlightDuration-.6f;
        public float EndWindow => Mood==FlightMood.Choice?PerfectWindow:Window;
        bool expanded, helped, rained;
        int roundNumber, rainLeft, choices;
        float rainDuration;
        public void Begin(int completedRounds=0,bool expansion=false)
        {
            Remaining=Duration;FlightTime=0;FlightDuration=1.35f;
            Score=Combo=BestCombo=Hits=PerfectHits=PerfectCombo=BestPerfectCombo=Misses=Spark=LastBonus=0;
            Lives=3;Target=1;Running=true;Mood=FlightMood.Normal;
            expanded=expansion;roundNumber=completedRounds;Mission=completedRounds>0?(completedRounds-1)%3:0;
            helped=rained=LastPerfect=LastEarly=false;rainLeft=choices=0;
        }
        public int Tick(float seconds)
        {
            if(!Running || seconds<=0 || float.IsNaN(seconds)) return 0;
            Remaining=Math.Max(0,Remaining-seconds);
            if(Remaining<=0){Running=false;return 0;}
            FlightTime+=seconds;
            if(FlightTime>FlightDuration+EndWindow){LastEarly=false;Miss();return -1;}
            return 0;
        }
        public int Tap()
        {
            if(!Running)return 0;
            float error=Math.Abs(FlightTime-FlightDuration);
            bool safe=Mood==FlightMood.Choice && Math.Abs(FlightTime-EarlyCenter)<=Window;
            if(!safe && error>EndWindow){LastEarly=FlightTime<FlightDuration-EndWindow;Miss();return -1;}
            LastPerfect=!safe && error<=PerfectWindow;LastEarly=false;
            if(LastPerfect){PerfectHits++;PerfectCombo++;BestPerfectCombo=Math.Max(BestPerfectCombo,PerfectCombo);}else PerfectCombo=0;
            Hits++;Combo++;BestCombo=Math.Max(BestCombo,Combo);
            LastBonus=Spark>0?5:0;if(Spark>0)Spark--;
            if(Mood==FlightMood.Choice && !safe)LastBonus+=10;
            Score+=(LastPerfect?20:10)+LastBonus;
            bool party=Combo%7==0;
            if(party){Score+=67;if(expanded)Spark=3;}
            NextFlight();return party?7:LastPerfect?2:1;
        }
        void Miss(){LastPerfect=false;LastBonus=0;Lives--;Misses++;Combo=PerfectCombo=Spark=0;if(Lives<=0)Running=false;else NextFlight();}
        void NextFlight()
        {
            Target=1-Target;FlightTime=0;FlightDuration=Math.Max(.72f,1.35f-Hits*.013f);Mood=FlightMood.Normal;
            if(!expanded)return;
            if(Misses==1 && !helped && roundNumber<3){helped=true;Mood=FlightMood.Helper;return;}
            if(Hits>=14 && !rained){rained=true;rainLeft=3;rainDuration=FlightDuration;}
            if(rainLeft>0){rainLeft--;Mood=FlightMood.Rain;FlightDuration=rainDuration;return;}
            if(roundNumber>=2 && Hits>=8 && Hits%13==9 && choices<2){Mood=FlightMood.Choice;FlightDuration=1.45f;choices++;return;}
            if(Hits>=8 && Hits%8==0 && Combo>0)Mood=FlightMood.Wind;
        }
    }
}
