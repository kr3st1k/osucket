
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using System;
using System.Linq;
using System.Collections.Generic;

namespace osucket.PPCalculator
{
    public abstract class PPCalculator
    {
        public abstract Ruleset Ruleset { get; }

        public int? RulesetID => Ruleset.RulesetInfo.ID;

        private static WorkingBeatmap CropBeatmap(WorkingBeatmap workingBeatmap, double endTime)
        {
            var tempMap = new Beatmap();
            tempMap.HitObjects.AddRange(workingBeatmap.Beatmap.HitObjects.Where(h => h.StartTime <= endTime));
            tempMap.ControlPointInfo = workingBeatmap.Beatmap.ControlPointInfo;
            tempMap.BeatmapInfo = workingBeatmap.BeatmapInfo;

            return new WorkingBeatmap(tempMap);
        }

        public double Calculate(WorkingBeatmap workingBeatmap, double endTime, double accuracy, int combo, int miss = 0, int meh = 0, string[] Mods = null, int score = 0)
        {
            workingBeatmap = CropBeatmap(workingBeatmap, endTime);

            return Calculate(workingBeatmap, accuracy, combo, miss, meh, Mods, score);
        }

        public double Calculate(WorkingBeatmap workingBeatmap, double accuracy, int combo, int miss = 0, int meh = 0, string[] Mods = null, int score = 0)
        {
            var mods = getMods(Mods ?? new string[] { });
            var playableBeatmap = workingBeatmap.GetPlayableBeatmap(Ruleset.RulesetInfo, mods);

            var hits = GenerateHitResults(accuracy, playableBeatmap.HitObjects, miss, meh);

            var scoreInfo = new ScoreInfo()
            {
                Accuracy = GetAccuracy(hits),
                MaxCombo = Math.Min(combo, GetMaxCombo(playableBeatmap.HitObjects)),
                Statistics = hits,
                TotalScore = score,
                Mods = mods.ToArray()
            };

             var performanceCalculator = Ruleset.CreatePerformanceCalculator(workingBeatmap, scoreInfo);

            try
            {
                return performanceCalculator.Calculate(null);
            }
            catch(InvalidOperationException)
            {
                return -1;
            }
        }

        public List<Mod> getMods(string[] Mods)
        {
            var mods = new List<Mod>();
            var availableMods = Ruleset.CreateAllMods().ToList();
            foreach(var modString in Mods)
            {
                Mod newMod = availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase));
                if(newMod == null)
                    continue;

                mods.Add(newMod);
            }

            return mods;
        }

        public int GetMaxCombo(IBeatmap beatmap) => GetMaxCombo(beatmap.HitObjects);
        protected abstract int GetMaxCombo(IReadOnlyList<HitObject> hitObjects);

        public double GetTimeAtHits(IBeatmap beatmap, int hits) => GetTimeAtHits(beatmap.HitObjects, hits);
        protected abstract double GetTimeAtHits(IReadOnlyList<HitObject> hitObjects, int hits);

        protected abstract Dictionary<HitResult, int> GenerateHitResults(double accuracy, IReadOnlyList<HitObject> hitObjects, int countMiss, int countMeh);

        protected abstract double GetAccuracy(Dictionary<HitResult, int> hits);
    }
}