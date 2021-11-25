using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace OsuPerformanceCalculator
{
	public class CatchCalculator : PPCalculator
	{
		public override Ruleset Ruleset { get; } = new CatchRuleset();

		protected override int GetMaxCombo(IReadOnlyList<HitObject> hitObjects)
		{
			return hitObjects.Count
			       + hitObjects.OfType<JuiceStream>().Sum(s =>
				       s.NestedHitObjects.Count - s.NestedHitObjects.OfType<TinyDroplet>().Count() - 1)
			       - hitObjects.OfType<BananaShower>().Count();
		}

		protected override double GetTimeAtHits(IReadOnlyList<HitObject> hitObjects, int hits)
		{
			return 0;
		}

		protected override Dictionary<HitResult, int> GenerateHitResults(double accuracy,
			IReadOnlyList<HitObject> hitObjects, int countMiss, int countMeh = 0)
		{
			int combo = GetMaxCombo(hitObjects);
			int fruitsHit = combo - countMiss;
			double tinyTickMiss = fruitsHit / accuracy - countMiss - fruitsHit;

			return new Dictionary<HitResult, int>
			{
				{HitResult.Great, fruitsHit},
				{HitResult.SmallTickMiss, Convert.ToInt32(tinyTickMiss)},
				{HitResult.Miss, countMiss}
			};
		}

		protected override double GetAccuracy(Dictionary<HitResult, int> statistics)
		{
			return 0;
		}
	}
}