using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.IO;
using osu.Game.Skinning;

namespace osucket.calculations.PPCalculator
{
    public class WorkingBeatmap : osu.Game.Beatmaps.WorkingBeatmap
    {
        private readonly IBeatmap beatmap;
        public int RulesetID => beatmap.BeatmapInfo.RulesetID;
        public double Length => beatmap.HitObjects.Any() ? beatmap.HitObjects.Last().StartTime : 0;

        public string BackgroundFile => beatmap.Metadata.BackgroundFile;

        public WorkingBeatmap(Stream stream, int? beatmapID = null)
            : this(readFromStream(stream), beatmapID)
        {}

        public WorkingBeatmap(IBeatmap beatmap, int? beatmapID = null)
            : base(beatmap.BeatmapInfo, null)
        {
            this.beatmap = beatmap;

            beatmap.BeatmapInfo.Ruleset = GetRulesetFromLegacyID(beatmap.BeatmapInfo.RulesetID).RulesetInfo;
        }


        private static Beatmap readFromStream(Stream stream)
        {
            using var reader = new LineBufferedReader(stream);
            return Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
        }

        public BeatmapDifficulty GetDifficultyWithMods(List<Mod> mods)
        {
            var baseDifficulty = BeatmapInfo.BaseDifficulty;
            var adjustedDifficulty = baseDifficulty.Clone();
            foreach(var mod in mods.OfType<IApplicableToDifficulty>())
            {
                if(RulesetID != 3 || !(mod is ModHardRock || mod is ModEasy))
                    mod.ApplyToDifficulty(adjustedDifficulty);
            }

            return adjustedDifficulty;
        }

        public IBeatmap getBeatmap() => GetBeatmap();

        protected override IBeatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => null;

        protected override Track GetBeatmapTrack() => null;
        public override Stream GetStream(string storagePath)
        {
            throw new NotImplementedException();
        }

        public static Ruleset GetRulesetFromLegacyID(int id)
        {
            switch (id)
            {
                default:
                    throw new ArgumentException("Invalid ruleset ID provided.");
                case 0:
                    return new OsuRuleset();
                case 1:
                    return new TaikoRuleset();
                case 2:
                    return new CatchRuleset();
                case 3:
                    return new ManiaRuleset();
            }
        }

        protected override ISkin GetSkin()
        {
            throw new NotImplementedException();
        }
    }
}