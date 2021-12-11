using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osucket.Calculations.Enums;
using osucket.Calculations.Memory;
using osucket.Calculations.Models;
using osucket.Calculations.OsuPerformanceCalculator;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;
using OsuPerformanceCalculator;
using Math = System.Math;

namespace osucket.Calculations
{
    public static class Calculation
    {
        private static readonly OsuBaseAddresses BaseAddresses = new();

        private static readonly StructuredOsuMemoryReader
            structuredOsuMemoryReader = StructuredOsuMemoryReader.Instance;

        private static OsuBaseAddresses _osuData = structuredOsuMemoryReader.OsuMemoryAddresses;

        private static readonly MemoryReader memoryReader = new()
        {
            StructuredOsuMemoryReader = structuredOsuMemoryReader
        };

        private static Ruleset GetRuleset(RuleSet ruleSet)
        {
            return ruleSet switch
            {
                RuleSet.Osu => new OsuRuleset(),
                RuleSet.Mania => new ManiaRuleset(),
                RuleSet.Taiko => new TaikoRuleset(),
                _ => new CatchRuleset()
            };
        }

        private static double CalculateUnstableRate(IReadOnlyCollection<int> hitError)
        {
            if (hitError == null || hitError.Count < 1) return 0;

            int totalAll = hitError.Sum();
            int average = totalAll / hitError.Count;
            double variance = hitError.Sum(item => Math.Pow(item - average, 2));
            variance /= hitError.Count;

            return Math.Sqrt(variance) * 10;
        }

        private static double GetAccuracy(GamePlay gameplay)
        {
            return gameplay.GameMode switch
            {
                RuleSet.Osu => 100.00 *
                               (6 * (double) gameplay.Count300 + 2 * (double) gameplay.Count100 + gameplay.Count50) /
                               (6 * (gameplay.Count50 + (double) gameplay.Count100 + gameplay.Count300 +
                                     gameplay.CountMiss)),
                RuleSet.Taiko => 100.00 * (2 * (double) gameplay.Count300 + gameplay.Count100) / (2 *
                    (gameplay.Count300 + (double) gameplay.Count100 + gameplay.CountMiss)),
                RuleSet.Fruits => 100.00 * (gameplay.Count300 + (double) gameplay.Count100 + gameplay.Count50) /
                                  (gameplay.Count300 + (double) gameplay.Count100 + gameplay.Count50 +
                                   gameplay.CountKatu + gameplay.CountGeki),
                RuleSet.Mania => 100.00 *
                    (6 * (double) gameplay.CountGeki + 6 * (double) gameplay.Count300 +
                     4 * (double) gameplay.CountKatu +
                     2 * (double) gameplay.Count100 + gameplay.Count50) / (6 * (gameplay.Count50 +
                        (double) gameplay.Count100 + gameplay.Count300 + gameplay.CountMiss +
                        gameplay.CountGeki + gameplay.CountKatu)),
                _ => throw new ArgumentOutOfRangeException("gameplay")
            };
        }

        private static List<Mod> GetMods(IEnumerable<string> mods, Ruleset ruleset)
        {
            var availableMods = ruleset.CreateAllMods().ToList();
            return mods.Select(modString => availableMods.FirstOrDefault(m =>
                    string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase)))
                .Where(newMod => newMod != null).ToList();
        }

        public static bool FirstEventGameplay = true;
        
        //private MapCache mapCache;
        public static string GetData(string osuDir)
        {
            if (!structuredOsuMemoryReader.TryRead(_osuData.GeneralData)) return "{}";
            var general = new General
            {
                OsuMemoryStatus = _osuData.GeneralData.OsuStatus,
                MenuRuleSet = (RuleSet) _osuData.GeneralData.GameMode,
                Song = OsuMemoryReader.Instance.GetSongString(),
                AudioPosition = _osuData.GeneralData.AudioTime,
                IsInterface = _osuData.GeneralData.ShowPlayingInterface,
                SkinName = OsuMemoryReader.Instance.GetSkinFolderName(),
                SkinDirectory = Path.Join(osuDir, "Skins", OsuMemoryReader.Instance.GetSkinFolderName()),
                MapDirectory = Path.Join(osuDir, "Songs", OsuMemoryReader.Instance.GetMapFolderName()),
                MenuMods = OsuMemoryReader.Instance.GetMods(),
                BeatMapId = OsuMemoryReader.Instance.GetMapId(),
                CountRetries = _osuData.GeneralData.Retries,
                SubmissionStatus =
                    (SubmissionStatus) memoryReader.ReadShort(BaseAddresses.Beatmap, nameof(CurrentBeatmap.Status))
            };
            general.MapFile = Path.Join(general.MapDirectory,
                memoryReader.ReadString(BaseAddresses.Beatmap, nameof(CurrentBeatmap.OsuFileName)));

            var gamePlay = new GamePlay();
            var osuKey = new OsuKey();
            var performance = new PerformancePoints();

            var currentBeatMap = general.MapFile.Contains("nekodex - circles! (peppy).osu")
                ? null
                : MapCache.GetBeatMap(general.MapFile);

            var ruleSet = currentBeatMap != null ? GetRuleset((RuleSet) currentBeatMap.RulesetID) : GetRuleset(0);
            var playableBeatMap = currentBeatMap?.GetPlayableBeatmap(ruleSet.RulesetInfo);

            if (currentBeatMap is not null)
            {
                general.SongLength = currentBeatMap.Length;
                general.BackgroundFile = Path.Join(general.MapDirectory, currentBeatMap.BackgroundFile);

                if (general.MenuRuleSet != RuleSet.Osu && currentBeatMap.RulesetID == 0)
                {
                    ruleSet = GetRuleset(general.MenuRuleSet); // TODO: use extension method
                    // PPCalculator calculator = PpCalculatorHelpers.GetPpCalculator(general.MenuRuleSet);
                    currentBeatMap = new WorkingBeatmap(ruleSet.CreateBeatmapConverter(playableBeatMap).Convert());
                }
                general.MapDifficulty = DiffCalculator.GetStarRate(GetMods(((Mods) general.MenuMods).ToString()
                    .Split(","), ruleSet), currentBeatMap, ruleSet);
            }


            switch (general.OsuMemoryStatus)
            {
                case OsuMemoryStatus.Playing:
                {
                    if (structuredOsuMemoryReader.TryRead(_osuData.LeaderBoard))
                    {
                        gamePlay.RawLeaderboard = _osuData.LeaderBoard;
                    }

                    if (structuredOsuMemoryReader.TryRead(_osuData.Player))
                    {
                        if (FirstEventGameplay)
                        {
                            SliderBreaks.ClearValues();
                            FirstEventGameplay = false;
                        }
                        
                        gamePlay.GameMode = (RuleSet) _osuData.Player.Mode;
                            
                        if ((gamePlay.GameMode is RuleSet.Osu or RuleSet.Fruits) &&
                            structuredOsuMemoryReader.TryRead(_osuData.KeyOverlay))
                        {
                            osuKey.IsEnabled = _osuData.KeyOverlay.Enabled;

                            osuKey.CountKeyLeft = _osuData.KeyOverlay.K1Count;
                            osuKey.CountKeyRight = _osuData.KeyOverlay.K2Count;
                            osuKey.CountMouseLeft = _osuData.KeyOverlay.M1Count;
                            osuKey.CountMouseRight = _osuData.KeyOverlay.M2Count;
                            osuKey.PressedKeyLeft = _osuData.KeyOverlay.K1Pressed;
                            osuKey.PressedKeyRight = _osuData.KeyOverlay.K2Pressed;
                            osuKey.PressedMouseLeft = _osuData.KeyOverlay.M1Pressed;
                            osuKey.PressedMouseRight = _osuData.KeyOverlay.M2Pressed;

                            gamePlay.OsuKey = osuKey;
                        }

                        gamePlay.Username = _osuData.Player.Username;
                        gamePlay.Accuracy = _osuData.Player.Accuracy;
                        gamePlay.Count300 = _osuData.Player.Hit300;
                        gamePlay.Count100 = _osuData.Player.Hit100;
                        gamePlay.Count50 = _osuData.Player.Hit50;
                        gamePlay.CountMiss = _osuData.Player.HitMiss;
                        gamePlay.CountGeki = _osuData.Player.HitGeki;
                        gamePlay.CountKatu = _osuData.Player.HitKatu;
                        gamePlay.Combo = _osuData.Player.Combo;
                        gamePlay.MaxCombo = _osuData.Player.MaxCombo;
                        SliderBreaks.GetSliderBreaks((ushort) gamePlay.CountMiss,(ushort) gamePlay.Combo);
                        gamePlay.CountSliderBreaks = SliderBreaks.SliderBreaksCount;
                        gamePlay.IsReplay = _osuData.Player.IsReplay;
                        gamePlay.Health = _osuData.Player.HP;
                        gamePlay.HealthSmooth = _osuData.Player.HPSmooth;
                        int? score = _osuData.Player.ScoreV2;
                        score ??=  _osuData.Player.Score;
                        gamePlay.Score = (int) score;
                            
                            
                        if (structuredOsuMemoryReader.TryReadProperty(BaseAddresses.Player, nameof(Player.Mods),
                            out object mods))
                            gamePlay.Mods = (Mods) ((OsuMemoryDataProvider.OsuMemoryModels.Abstract.Mods) mods)
                                .Value;
                            
                            
                        if (gamePlay.MaxCombo > 0)
                        {
                            var calculator = PpCalculatorHelpers.GetPpCalculator(gamePlay.GameMode);

                            performance.Performance = calculator.Calculate(
                                currentBeatMap,
                                general.AudioPosition,
                                gamePlay.Accuracy / 100,
                                gamePlay.MaxCombo,
                                gamePlay.CountMiss,
                                gamePlay.Count50,
                                string.Join(",",
                                        Enum.GetValues(typeof(Mods)).Cast<Enum>()
                                            .Where(value => gamePlay.Mods.HasFlag(value)))
                                    .Split(","),
                                gamePlay.Score
                            );


                            if (gamePlay.GameMode != RuleSet.Mania)
                                performance.FullComboPerformance = calculator.Calculate(
                                    currentBeatMap,
                                    gamePlay.Accuracy / 100,
                                    calculator.GetMaxCombo(playableBeatMap),
                                    0,
                                    0,
                                    string.Join(",",
                                        Enum.GetValues(typeof(Mods)).Cast<Enum>()
                                            .Where(value => gamePlay.Mods.HasFlag(value))).Split(","),
                                    1000000
                                );
                            performance.SuperSkillPerformance = calculator.Calculate(
                                currentBeatMap,
                                1,
                                calculator.GetMaxCombo(playableBeatMap),
                                0,
                                0,
                                string.Join(",",
                                        Enum.GetValues(typeof(Mods)).Cast<Enum>()
                                            .Where(value => gamePlay.Mods.HasFlag(value)))
                                    .Split(","),
                                1000000);
                        }

                        gamePlay.PerformancePoints = performance;


                        gamePlay.HitError =
                            memoryReader.ReadClassProperty<List<int>>(BaseAddresses.Player,
                                nameof(Player.HitErrors));
                        gamePlay.UnstableRate = CalculateUnstableRate(gamePlay.HitError);
                    }

                    general.Gameplay = gamePlay;
                    break;
                }
                case OsuMemoryStatus.ResultsScreen:
                {
                    if (structuredOsuMemoryReader.TryRead(_osuData.ResultsScreen))
                    {
                        gamePlay.GameMode = (RuleSet) _osuData.ResultsScreen.Mode;


                        gamePlay.Username = _osuData.ResultsScreen.Username;
                        gamePlay.Count300 = _osuData.ResultsScreen.Hit300;
                        gamePlay.Count100 = _osuData.ResultsScreen.Hit100;
                        gamePlay.Count50 = _osuData.ResultsScreen.Hit50;
                        gamePlay.CountMiss = _osuData.ResultsScreen.HitMiss;
                        gamePlay.CountGeki = _osuData.ResultsScreen.HitGeki;
                        gamePlay.CountKatu = _osuData.ResultsScreen.HitKatu;
                        gamePlay.MaxCombo = _osuData.ResultsScreen.MaxCombo;

                        int? score = _osuData.ResultsScreen.ScoreV2;
                        score ??=  _osuData.ResultsScreen.Score;
                        gamePlay.Score = (int) score;

                        if (structuredOsuMemoryReader.TryReadProperty(BaseAddresses.ResultsScreen,
                            nameof(Player.Mods),
                            out object mods))
                            gamePlay.Mods = (Mods) ((OsuMemoryDataProvider.OsuMemoryModels.Abstract.Mods) mods)
                                .Value;

                        var calculator = PpCalculatorHelpers.GetPpCalculator(gamePlay.GameMode);

                        gamePlay.Accuracy =
                            GetAccuracy(gamePlay);


                        performance.Performance = calculator.Calculate(
                            currentBeatMap,
                            gamePlay.Accuracy / 100,
                            gamePlay.MaxCombo,
                            gamePlay.CountMiss,
                            gamePlay.Count50,
                            string.Join(",",
                                    Enum.GetValues(typeof(Mods)).Cast<Enum>()
                                        .Where(value => gamePlay.Mods.HasFlag(value)))
                                .Split(","),
                            gamePlay.Score);
                        gamePlay.PerformancePoints = performance;
                    }

                    general.ResultScreen = gamePlay;
                    break;
                }
                default:
                    if (!FirstEventGameplay) FirstEventGameplay = true;
                    break;
            }

            return JsonSerializer.Serialize(general);
        }
    }
}