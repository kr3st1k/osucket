using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Objects;
using OsuPerformanceCalculator;

namespace osucket.Calculations.OsuPerformanceCalculator
{
	public class TaikoCalculator : PPCalculator
	{
		public override Ruleset Ruleset { get; } = new TaikoRuleset();

		protected override int GetMaxCombo(IReadOnlyList<HitObject> hitObjects)
		{
			return hitObjects.OfType<Hit>().Count();
		}

		protected override double GetTimeAtHits(IReadOnlyList<HitObject> hitObjects, int hits)
		{
			return hitObjects.OfType<Hit>().ElementAtOrDefault(hits - 1).StartTime;
		}

		protected override Dictionary<HitResult, int> GenerateHitResults(double accuracy,
			IReadOnlyList<HitObject> hitObjects, int countMiss, int countMeh = 0)
		{
			int totalResultCount = GetMaxCombo(hitObjects);

			var targetTotal = (int) Math.Round(accuracy * totalResultCount * 2);

			int great = targetTotal - (totalResultCount - countMiss);
			int good = totalResultCount - great - countMiss;

			return new Dictionary<HitResult, int>
			{
				{HitResult.Great, great},
				{HitResult.Good, good},
				{HitResult.Meh, 0},
				{HitResult.Miss, countMiss}
			};
		}

		protected override double GetAccuracy(Dictionary<HitResult, int> statistics)
		{
			int countGreat = statistics[HitResult.Great];
			int countGood = statistics[HitResult.Good];
			int countMiss = statistics[HitResult.Miss];
			int total = countGreat + countGood + countMiss;

			return (double) (2 * countGreat + countGood) / (2 * total);
		}
	}
}