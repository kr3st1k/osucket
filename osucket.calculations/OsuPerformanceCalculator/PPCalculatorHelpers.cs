using osucket.Calculations.Enums;
using OsuPerformanceCalculator;

namespace osucket.Calculations.OsuPerformanceCalculator
{
	internal static class PpCalculatorHelpers
	{
		internal static PPCalculator GetPpCalculator(RuleSet ruleSet)
		{
			return ruleSet switch
			{
				RuleSet.Osu => new OsuCalculator(),
				RuleSet.Mania => new ManiaCalculator(),
				RuleSet.Taiko => new TaikoCalculator(),
				_ => new CatchCalculator()
			};
		}
	}
}