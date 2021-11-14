using osu.Game.Rulesets.Mods;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Utils;

namespace osucket.PPCalculator
{
    class DiffCalculator

    {

        public static string GetStarRate(List<Mod> mods, WorkingBeatmap beatmap, Ruleset ruleset)
        {
            var modsShish = TrimNonDifficultyAdjustmentMods(ruleset, mods.ToArray(), beatmap);
            var attributes = ruleset.CreateDifficultyCalculator(beatmap).Calculate(modsShish);

            return attributes.StarRating.ToString("N2");
        }
        
        public static Mod[] TrimNonDifficultyAdjustmentMods(Ruleset ruleset, Mod[] mods, WorkingBeatmap beatmap)
        {
            var difficultyAdjustmentMods = ModUtils.FlattenMods(
                                                       ruleset.CreateDifficultyCalculator(beatmap).CreateDifficultyAdjustmentModCombinations())
                                                   .Select(m => m.GetType())
                                                   .Distinct()
                                                   .ToHashSet();

            // Special case for DT/NC.
            if (mods.Any(m => m is ModDoubleTime))
                difficultyAdjustmentMods.Add(ruleset.CreateAllMods().Single(m => m is ModNightcore).GetType());

            return mods.Where(m => difficultyAdjustmentMods.Contains(m.GetType())).ToArray();
        }
    }
}
