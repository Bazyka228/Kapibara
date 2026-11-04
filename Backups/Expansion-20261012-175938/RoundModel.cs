using System;

namespace Capybara67
{
    // Pure game rules: independent of rendering, device input and SDK.
    public sealed class RoundModel
    {
        public const float Duration = 67f;
        public const float Window = .19f;
        public const float PerfectWindow = .085f;
        public float Remaining { get; private set; }
        public float FlightTime { get; private set; }
        public float FlightDuration { get; private set; }
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int BestCombo { get; private set; }
        public int Hits { get; private set; }
        public int PerfectHits { get; private set; }
        public int Misses { get; private set; }
        public int Lives { get; private set; }
        public int Target { get; private set; }
        public bool Running { get; private set; }
        public float Progress => FlightDuration > 0 ? Math.Min(1f, FlightTime / FlightDuration) : 0;

        public void Begin()
        {
            Remaining = Duration; FlightTime = 0; FlightDuration = 1.35f;
            Score = Combo = BestCombo = Hits = PerfectHits = Misses = 0; Lives = 3; Target = 1; Running = true;
        }

        // -1 = a missed landing, 0 = normal update.
        public int Tick(float seconds)
        {
            if (!Running || seconds <= 0) return 0;
            Remaining = Math.Max(0, Remaining - seconds);
            if (Remaining <= 0) { Running = false; return 0; }
            FlightTime += seconds;
            if (FlightTime > FlightDuration + Window) { Miss(); return -1; }
            return 0;
        }

        // 0 = inactive, -1 = early/late, 1 = catch, 2 = perfect, 7 = seven-combo.
        public int Tap()
        {
            if (!Running) return 0;
            float error = Math.Abs(FlightTime - FlightDuration);
            if (error > Window) { Miss(); return -1; }
            bool perfect = error <= PerfectWindow;
            if (perfect) PerfectHits++;
            Hits++; Combo++; BestCombo = Math.Max(BestCombo, Combo);
            Score += perfect ? 20 : 10;
            bool party = Combo % 7 == 0;
            if (party) Score += 67;
            NextFlight();
            return party ? 7 : perfect ? 2 : 1;
        }

        void Miss() { Lives--; Misses++; Combo = 0; if (Lives <= 0) Running = false; else NextFlight(); }
        void NextFlight()
        {
            Target = 1 - Target; FlightTime = 0;
            FlightDuration = Math.Max(.72f, 1.35f - Hits * .013f);
        }
    }
}
