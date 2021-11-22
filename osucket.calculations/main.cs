using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;

using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;

using osucket.calculations.PPCalculator;
using Math = System.Math;
using System.Linq;
using System.IO;

namespace osucket.calculations
{
    class MemoryReader
    {
        private StructuredOsuMemoryReader _sreader;
        public StructuredOsuMemoryReader Sreader
        {
            get => _sreader;
            set => _sreader = value;
        }

        public T ReadProperty<T>(object readObj, string propName, T defaultValue = default) where T : struct
        {
            if (_sreader.TryReadProperty(readObj, propName, out var readResult))
                return (T)readResult;

            return defaultValue;
        }

        public T ReadClassProperty<T>(object readObj, string propName, T defaultValue = default) where T : class
        {
            if (_sreader.TryReadProperty(readObj, propName, out var readResult))
                return (T)readResult;

            return defaultValue;
        }

        public bool ReadBool(object readObj, string propName)
        {
            if (_sreader.TryReadProperty(readObj, propName, out var readResult))
            {
                return (bool)readResult;
            }
            return false;
        }
        public int ReadInt(object readObj, string propName) => ReadProperty<int>(readObj, propName, -5);
        public ushort ReadUShort(object readObj, string propName) => ReadProperty<ushort>(readObj, propName);
        public short ReadShort(object readObj, string propName) => ReadProperty<short>(readObj, propName);
        public float ReadFloat(object readObj, string propName) => ReadProperty<float>(readObj, propName, -5f);
        public string ReadString(object readObj, string propName) => ReadClassProperty<string>(readObj, propName, "INVALID READ");

    }

    public class Program
    {
        internal static OsuBaseAddresses baseAddresses = new OsuBaseAddresses();
        internal static Ruleset getRuleset(int rulesetID)
        {
            switch (rulesetID)
            {
                case 0:
                    return new OsuRuleset();
                case 1:
                    return new TaikoRuleset();
                case 2:
                    return new CatchRuleset();
                case 3:
                    return new ManiaRuleset();
                default:
                    throw new ArgumentOutOfRangeException("rulesetID");
            }
        }

        internal static double CalcUR(List<int> HitError)
        {
            if (HitError == null || HitError.Count < 1)
            {
                return 0;
            }

            var totalAll = 0;
            foreach (var item in HitError)
            {
                totalAll += item;
            }
            var average = totalAll / HitError.Count;
            double variance = 0;
            foreach (var item in HitError)
            {
                variance += Math.Pow(item - average, 2);
            }
            variance /= HitError.Count;

            return Math.Sqrt(variance) * 10;
        }

        internal static double GetAccuracy(Dictionary<string, dynamic> gameplay)
        {
            switch (gameplay["mode_int"])
            {
                case 0:
                    return (100.00 * (6 * (double)gameplay["c300"] + 2 * (double)gameplay["c100"] + (double)gameplay["c50"]) / (6 * ((double)gameplay["c50"] + (double)gameplay["c100"] + (double)gameplay["c300"] + (double)gameplay["cMiss"])));
                case 1:
                    return (100.00 * (2 * (double)gameplay["c300"] + (double)gameplay["c100"]) / (2 * ((double)gameplay["c300"] + (double)gameplay["c100"] + (double)gameplay["cMiss"])));
                case 2:
                    return (100.00 * ((double)gameplay["c300"] + (double)gameplay["c100"] + (double)gameplay["c50"]) / ((double)gameplay["c300"] + (double)gameplay["c100"] + (double)gameplay["c50"] + (double)gameplay["cKatu"] + (double)gameplay["cMiss"]));
                case 3:
                    return (100.00 * (6 * (double)gameplay["cGeki"] + 6 * (double)gameplay["c300"] + 4 * (double)gameplay["cKatu"] + 2 * (double)gameplay["c100"] + (double)gameplay["c50"]) / (6 * ((double)gameplay["c50"] + (double)gameplay["c100"] + (double)gameplay["c300"] + (double)gameplay["cMiss"] + (double)gameplay["cGeki"] + (double)gameplay["cKatu"])));
                default:
                    throw new ArgumentOutOfRangeException("gamemode");
            }
        }
        internal static List<Mod> GetMods(string[] Mods, Ruleset ruleset)
        {
            var mods = new List<Mod>();
            var availableMods = ruleset.CreateAllMods().ToList();
            foreach (var modString in Mods)
            {
                Mod newMod = availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase));
                if (newMod == null)
                    continue;

                mods.Add(newMod);
            }

            return mods;
        }

        public static string GetData(string osuDir, string windowTitle)
        {
            StructuredOsuMemoryReader _sreader = StructuredOsuMemoryReader.Instance.GetInstanceForWindowTitleHint(windowTitle);
            MemoryReader _ssreader = new MemoryReader
            {
                Sreader = _sreader
            };

            var res = new Dictionary<string, dynamic> {
                        { "StatusNumber", null },
                        { "Status", null },
                        { "GameModeNumberMenu", null },
                        { "Song", null },
                        { "SongTime", null },
                        { "SongLength", null },
                        { "bg", null},
                        { "beatmap_id", null},
                        { "IsInterface", null },
                        { "StarRate", null},
                        { "ModsMainMenuInt", null},
                        { "SkinName", null },
                        { "GameModeMenu", null },
                        { "SkinDir", null },
                        { "MapDir", null },
                        { "osuFile", null },
                        { "cRetry", null },
                        { "beatmapStatus_int", null },
                        { "beatmapStatus", null },
                        { "Gameplay", null },
                        { "ResultScreen", null }

                    };
            var gameplay = new Dictionary<string, dynamic> {
                        { "mode_int", null },
                        { "keyOverlay", null },
                        { "username", null },
                        { "acc", 100 },
                        { "c300", 0 },
                        { "c100", 0 },
                        { "c50", 0 },
                        { "cMiss", 0 },
                        { "cGeki", 0 },
                        { "cKatu", 0 },
                        { "combo", 0 },
                        { "maxCombo", 0 },
                        { "score", 0 },
                        { "isReplay", null },
                        { "HP", null },
                        { "HPSmooth", null },
                        { "mode", null },
                        { "mods_int", null },
                        { "mods", null },
                        { "pp", null },
                        { "HitErrors", null },
                        { "UR", 0 }
                    };
            var keyOverlay = new Dictionary<string, dynamic>
                    {
                        {"ck1", 0},
                        {"bk1", false},
                        {"ck2", 0},
                        {"bk2", false},
                        {"cm1", 0},
                        {"bm1", false},
                        {"cm2", 0},
                        {"bm2", false}
                    };
            var pp = new Dictionary<string, dynamic> {
                        {"pp", 0},
                        {"fcpp", 0},
                        {"sspp", 0 }
                    };

            res["StatusNumber"] = OsuMemoryReader.Instance.GetCurrentStatus(out int num);
            res["Status"] = Enum.GetName(typeof(OsuMemoryStatus), res["StatusNumber"]);
            res["GameModeNumberMenu"] = OsuMemoryReader.Instance.ReadSongSelectGameMode();
            res["Song"] = OsuMemoryReader.Instance.GetSongString();
            res["SongTime"] = _ssreader.ReadInt(baseAddresses.GeneralData, nameof(GeneralData.AudioTime));
            res["IsInterface"] = _ssreader.ReadBool(baseAddresses.GeneralData, nameof(GeneralData.ShowPlayingInterface));
            res["SkinName"] = OsuMemoryReader.Instance.GetSkinFolderName();
            res["GameModeMenu"] = Enum.GetName(typeof(OsuGameMode), res["GameModeNumberMenu"]);
            res["SkinDir"] = Path.Join(osuDir, "Skins", OsuMemoryReader.Instance.GetSkinFolderName());
            res["MapDir"] = Path.Join(osuDir, "Songs", OsuMemoryReader.Instance.GetMapFolderName());
            res["osuFile"] = Path.Join(res["MapDir"], _ssreader.ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.OsuFileName)));
            var mapinfo = res["osuFile"].Contains("nekodex - circles! (peppy).osu") ? null : MapCache.GetBeatmap(res["osuFile"]);

            var ruleset = mapinfo != null ? getRuleset(mapinfo.RulesetID) : getRuleset(0);
            var playableMap = mapinfo != null ? mapinfo.GetPlayableBeatmap(ruleset.RulesetInfo) : null;
            if (mapinfo != null)
            {
                res["SongLength"] = mapinfo.Length;
                res["bg"] = Path.Join(res["MapDir"], mapinfo.BackgroundFile);
                res["ModsMainMenuInt"] = OsuMemoryReader.Instance.GetMods();
                if (res["GameModeNumberMenu"] != 0 && mapinfo.RulesetID == 0)
                {
                    ruleset = getRuleset(res["GameModeNumberMenu"]);
                    var calculator = PPCalculatorHelpers.GetPPCalculator(res["GameModeNumberMenu"]);
                    var converter = ruleset.CreateBeatmapConverter(playableMap);
                    playableMap = converter.Convert();
                    mapinfo = new PPCalculator.WorkingBeatmap(playableMap);
                }

                var mods_lazer = GetMods(((ModsStr)res["ModsMainMenuInt"]).ToString().Split(","), ruleset);
                res["StarRate"] = osucket.calculations.PPCalculator.DiffCalculator.GetStarRate(mods_lazer, mapinfo, ruleset);

            }
            res["beatmap_id"] = OsuMemoryReader.Instance.GetMapId();

            res["cRetry"] = OsuMemoryReader.Instance.GetRetrys();

            res["beatmapStatus_int"] = _ssreader.ReadShort(baseAddresses.Beatmap, nameof(CurrentBeatmap.Status));
            res["beatmapStatus"] = Enum.GetName(typeof(OsuBeatmapStatus), res["beatmapStatus_int"]);

            if (res["StatusNumber"] == OsuMemoryStatus.Playing)
            {
                gameplay["mode_int"] = _ssreader.ReadInt(baseAddresses.Player, nameof(Player.Mode));

                if (gameplay["mode_int"] == 0 || gameplay["mode_int"] == 2)
                {
                    keyOverlay["ck1"] = _ssreader.ReadInt(baseAddresses.KeyOverlay, nameof(KeyOverlay.K1Count));
                    keyOverlay["ck2"] = _ssreader.ReadInt(baseAddresses.KeyOverlay, nameof(KeyOverlay.K2Count));
                    keyOverlay["cm1"] = _ssreader.ReadInt(baseAddresses.KeyOverlay, nameof(KeyOverlay.M1Count));
                    keyOverlay["cm2"] = _ssreader.ReadInt(baseAddresses.KeyOverlay, nameof(KeyOverlay.M2Count));

                    keyOverlay["bk1"] = _ssreader.ReadBool(baseAddresses.KeyOverlay, nameof(KeyOverlay.K1Pressed));
                    keyOverlay["bk2"] = _ssreader.ReadBool(baseAddresses.KeyOverlay, nameof(KeyOverlay.K2Pressed));
                    keyOverlay["bm1"] = _ssreader.ReadBool(baseAddresses.KeyOverlay, nameof(KeyOverlay.M1Pressed));
                    keyOverlay["bm2"] = _ssreader.ReadBool(baseAddresses.KeyOverlay, nameof(KeyOverlay.M2Pressed));

                    gameplay["keyOverlay"] = keyOverlay;
                }

                gameplay["username"] = _ssreader.ReadString(baseAddresses.Player, nameof(Player.Username));
                gameplay["acc"] = _ssreader.ReadProperty<double>(baseAddresses.Player, nameof(Player.Accuracy));
                gameplay["c300"] = _ssreader.ReadUShort(baseAddresses.Player, nameof(Player.Hit300));
                gameplay["c100"] = _ssreader.ReadUShort(baseAddresses.Player, nameof(Player.Hit100));
                gameplay["c50"] = _ssreader.ReadUShort(baseAddresses.Player, nameof(Player.Hit50));
                gameplay["cMiss"] = _ssreader.ReadUShort(baseAddresses.Player, nameof(Player.HitMiss));
                gameplay["cGeki"] = _ssreader.ReadUShort(baseAddresses.Player, nameof(Player.HitGeki));
                gameplay["cKatu"] = _ssreader.ReadUShort(baseAddresses.Player, nameof(Player.HitKatu));
                gameplay["combo"] = _ssreader.ReadUShort(baseAddresses.Player, nameof(Player.Combo));
                gameplay["maxCombo"] = _ssreader.ReadUShort(baseAddresses.Player, nameof(Player.MaxCombo));
                if (_sreader.TryReadProperty(baseAddresses.Player, nameof(Player.ScoreV2), out var score))
                {
                    gameplay["score"] = score;
                }

                gameplay["isReplay"] = OsuMemoryReader.Instance.IsReplay();

                gameplay["HP"] = _ssreader.ReadProperty<double>(baseAddresses.Player, nameof(Player.HP));
                gameplay["HPSmooth"] = _ssreader.ReadProperty<double>(baseAddresses.Player, nameof(Player.HPSmooth));
                gameplay["mode"] = Enum.GetName(typeof(OsuGameMode), gameplay["mode_int"]);
                if (_sreader.TryReadProperty(baseAddresses.Player, nameof(Player.Mods), out var mods))
                {
                    var _kok = (Mods)mods;
                    var mods_int = _kok.Value;
                    gameplay["mods_int"] = mods_int;
                    gameplay["mods"] = ((ModsStr)mods_int).ToString();
                }
                if (gameplay["maxCombo"] != 0)
                {
                    var calculator = PPCalculatorHelpers.GetPPCalculator(gameplay["mode_int"]);

                    pp["pp"] = calculator.Calculate(mapinfo, res["SongTime"], gameplay["acc"] / 100, gameplay["maxCombo"], gameplay["cMiss"], gameplay["c50"], gameplay["mods"].Split(","), gameplay["score"]);
                    if (gameplay["mode_int"] != 3) pp["fcpp"] = calculator.Calculate(mapinfo, gameplay["acc"] / 100, calculator.GetMaxCombo(playableMap), 0, 0, gameplay["mods"].Split(","), 1000000);
                    pp["sspp"] = calculator.Calculate(mapinfo, 1, calculator.GetMaxCombo(playableMap), 0, 0, gameplay["mods"].Split(","), 1000000);
                }
                gameplay["pp"] = pp;


                gameplay["HitErrors"] = _ssreader.ReadClassProperty<List<int>>(baseAddresses.Player, nameof(Player.HitErrors));
                gameplay["UR"] = CalcUR(gameplay["HitErrors"]);
                res["Gameplay"] = gameplay;
            }
            if (res["StatusNumber"] == OsuMemoryStatus.ResultsScreen)
            {
                gameplay["username"] = _ssreader.ReadString(baseAddresses.ResultsScreen, nameof(ResultsScreen.Username));
                gameplay["c300"] = _ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.Hit300));
                gameplay["c100"] = _ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.Hit100));
                gameplay["c50"] = _ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.Hit50));
                gameplay["cMiss"] = _ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.HitMiss));
                gameplay["cGeki"] = _ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.HitGeki));
                gameplay["cKatu"] = _ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.HitKatu));
                gameplay["maxCombo"] = _ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.MaxCombo));
                if (_sreader.TryReadProperty(baseAddresses.ResultsScreen, nameof(Player.ScoreV2), out var score))
                {
                    if (score is null)
                    {
                        _sreader.TryReadProperty(baseAddresses.ResultsScreen, nameof(Player.Score), out score);

                    }
                    gameplay["score"] = score;
                }
                gameplay["mode_int"] = _ssreader.ReadInt(baseAddresses.ResultsScreen, nameof(ResultsScreen.Mode));
                gameplay["mode"] = Enum.GetName(typeof(OsuGameMode), gameplay["mode_int"]);
                if (_sreader.TryReadProperty(baseAddresses.ResultsScreen, nameof(ResultsScreen.Mods), out var mods))
                {
                    var _kok = (Mods)mods;
                    var mods_int = _kok.Value;
                    gameplay["mods_int"] = mods_int;
                    gameplay["mods"] = $"{(ModsStr)mods_int}";
                }
                var calculator = PPCalculatorHelpers.GetPPCalculator(gameplay["mode_int"]);

                gameplay["acc"] = GetAccuracy(gameplay);


                gameplay["pp"] = calculator.Calculate(mapinfo, gameplay["acc"] / 100, gameplay["maxCombo"], gameplay["cMiss"], gameplay["c50"], gameplay["mods"].Split(","), gameplay["score"]);

                res["ResultScreen"] = gameplay;
            }

            return JsonSerializer.Serialize(res);
        }

    }
}
