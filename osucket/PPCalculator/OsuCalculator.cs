using System;
using System.Linq;
using System.Collections.Generic;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;

namespace osucket.PPCalculator
{
    public class OsuCalculator : PPCalculator
    {
        public override Ruleset Ruleset { get; } = new OsuRuleset();

        protected override int GetMaxCombo(IReadOnlyList<HitObject> hitObjects) =>
            hitObjects.Count + hitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);

        protected override double GetTimeAtHits(IReadOnlyList<HitObject> hitObjects, int hits)
        {
            return hits > hitObjects.Count()
                ? hitObjects.Last().GetEndTime()
                : hitObjects[hits - 1].GetEndTime();
        }

        protected override Dictionary<HitResult, int> GenerateHitResults(double accuracy, IReadOnlyList<HitObject> hitObjects, int countMiss, int countMeh = 0)
        {
            // var countGreat = -1;
            // var countGood = 0;

            // var max300 = nObjects - countMiss;
            // var maxacc = GetAccuracy(new Dictionary<HitResult, int> {
            //     { HitResult.Great, max300 },
            //     { HitResult.Good, countGood },
            //     { HitResult.Meh, countMeh },
            //     { HitResult.Miss, countMiss }
            // }) * 100;

            // countGood = (int)Math.Round(
            //     -3 * ((accuracy - 1) * nObjects + countMiss) * 0.5
            // );

            // if(countGood > max300)
            // {
            //     countGood = 0;
            //     countMeh = (int)Math.Round(
            //         -6 * ((accuracy - 1) * nObjects + countMiss) * 0.5
            //     );
            //     countMeh = Math.Min(max300, countMeh);
            // }

            var nObjects = hitObjects.Count;

            var s = nObjects - countMiss - countMeh;
            var countGood = (int)Math.Round(-((accuracy * 6 * nObjects - 6 * s - countMeh) / 4));
            var countGreat = nObjects - countGood;

            return new Dictionary<HitResult, int>
            {
                { HitResult.Great, countGreat },
                { HitResult.Ok, countGood },
                { HitResult.Meh, countMeh },
                { HitResult.Miss, countMiss }
            };
        }

        protected override double GetAccuracy(Dictionary<HitResult, int> statistics)
        {
            var countGreat = statistics[HitResult.Great];
            var countGood = statistics[HitResult.Ok];
            var countMeh = statistics[HitResult.Meh];
            var countMiss = statistics[HitResult.Miss];
            var total = countGreat + countGood + countMeh + countMiss;

            return (double)((6 * countGreat) + (2 * countGood) + countMeh) / (6 * total);
        }
    }
}