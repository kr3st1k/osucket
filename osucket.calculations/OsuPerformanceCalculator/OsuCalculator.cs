using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace OsuPerformanceCalculator
{
	public class OsuCalculator : PPCalculator
	{
		public override Ruleset Ruleset { get; } = new OsuRuleset();

		protected override int GetMaxCombo(IReadOnlyList<HitObject> hitObjects)
		{
			return hitObjects.Count + hitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);
		}

		protected override double GetTimeAtHits(IReadOnlyList<HitObject> hitObjects, int hits)
		{
			return hits > hitObjects.Count()
				? hitObjects.Last().GetEndTime()
				: hitObjects[hits - 1].GetEndTime();
		}

		protected override Dictionary<HitResult, int> GenerateHitResults(double accuracy,
			IReadOnlyList<HitObject> hitObjects, int countMiss, int countMeh = 0)
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

			int nObjects = hitObjects.Count;

			int s = nObjects - countMiss - countMeh;
			var countGood = (int) Math.Round(-((accuracy * 6 * nObjects - 6 * s - countMeh) / 4));
			int countGreat = nObjects - countGood;

			return new Dictionary<HitResult, int>
			{
				{HitResult.Great, countGreat},
				{HitResult.Ok, countGood},
				{HitResult.Meh, countMeh},
				{HitResult.Miss, countMiss}
			};
		}

		protected override double GetAccuracy(Dictionary<HitResult, int> statistics)
		{
			int countGreat = statistics[HitResult.Great];
			int countGood = statistics[HitResult.Ok];
			int countMeh = statistics[HitResult.Meh];
			int countMiss = statistics[HitResult.Miss];
			int total = countGreat + countGood + countMeh + countMiss;

			return (double) (6 * countGreat + 2 * countGood + countMeh) / (6 * total);
		}
	}
}