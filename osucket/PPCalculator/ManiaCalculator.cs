using System.Linq;
using System.Collections.Generic;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osucket.PPCalculator
{
    public class ManiaCalculator : PPCalculator
    {
        public override Ruleset Ruleset { get; } = new ManiaRuleset();

        protected override int GetMaxCombo(IReadOnlyList<HitObject> hitObjects) => 0;

        protected override double GetTimeAtHits(IReadOnlyList<HitObject> hitObjects, int hits)
        {
            foreach(HitObject obj in hitObjects) {
                hits--;

                if(hits <= 0)
                    return obj.StartTime;

                if(obj is HoldNote) hits--;

                if(hits <= 0)
                    return obj.GetEndTime();
            }

            return hitObjects.Last().GetEndTime();
        }

        protected override Dictionary<HitResult, int> GenerateHitResults(double accuracy, IReadOnlyList<HitObject> hitObjects, int countMiss, int countMeh = 0)
        {
            var totalHits = hitObjects.Count;

            return new Dictionary<HitResult, int>
            {
                { HitResult.Perfect, totalHits },
                { HitResult.Great, 0 },
                { HitResult.Ok, 0 },
                { HitResult.Good, 0 },
                { HitResult.Meh, 0 },
                { HitResult.Miss, 0 }
            };
        }

        protected override double GetAccuracy(Dictionary<HitResult, int> statistics) => 0;
    }
}