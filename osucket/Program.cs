using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

using Fleck;

using osucket.PPCalculator;

namespace osucket
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
            if(_sreader.TryReadProperty(readObj, propName, out var readResult))
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


    internal static class Program
    {
        internal static void Main(string[] args)
        { 
            Console.WriteLine("                                                                   \r\n                                                                  \r\n                                       `7MM                 mm    \r\n                                         MM                 MM    \r\n ,pW\"Wq.  ,pP\"Ybd `7MM  `7MM   ,p6\"bo    MM  ,MP' .gP\"Ya  mmMMmm  \r\n6W'   `Wb 8I   `\"   MM    MM  6M'  OO    MM ;Y   ,M'   Yb   MM    \r\n8M     M8 `YMMMa.   MM    MM  8M         MM;Mm   8M\"\"\"\"\"\"   MM    \r\nYA.   ,A9 L.   I8   MM    MM  YM.    ,   MM `Mb. YM.    ,   MM    \r\n `Ybmd9'  M9mmmP'   `Mbod\"YML. YMbmd'  .JMML. YA. `Mbmmd'   `Mbmo \r\n                                                                  \r\n                                                                  ");
            var port = 13371;
            var timer = 500;
            var showerrors = false;
            Environment.GetCommandLineArgs().ToList().ForEach(x =>
            {
                if (x.EndsWith("/?") || x.EndsWith("?") || x.EndsWith("-h") || x.EndsWith("--h") || x.EndsWith("-help") || x.EndsWith("--help"))
                {
                    Console.WriteLine("-showerrors           Output errors\n" + 
                                               "-port=13371           WebSocket port\n" +
                                               "-timer=500            Delay in sending data (-delay=500) \n"); 
                    Environment.Exit(0);
                }
                if (x.StartsWith("/showerrors") || x.StartsWith("-showerrors") || x.StartsWith("--showerrors") ||
                    x.StartsWith("-se") || x.StartsWith("--se") || x.StartsWith("/se"))
                {
                    showerrors = true;
                }
                try
                {
                    if (x.StartsWith("/port") || x.StartsWith("-port") || x.StartsWith("--port") ||
                        x.StartsWith("-p") || x.StartsWith("--p") || x.StartsWith("/p"))
                    {
                        port = Convert.ToInt32(x.Split("=")[1]);
                    }
                }
                catch
                {
                    Console.WriteLine("Invalid port value, enter an integer value, standard port is installed.");
                }
                try
                {
                    if (x.StartsWith("/timer") || x.StartsWith("-timer") || x.StartsWith("--timer") ||
                        x.StartsWith("-t") || x.StartsWith("--t") || x.StartsWith("/t") || 
                        x.StartsWith("/delay") || x.StartsWith("-delay") || x.StartsWith("--delay") ||
                        x.StartsWith("-d") || x.StartsWith("--d") || x.StartsWith("/d"))
                    {
                        timer = Convert.ToInt32(x.Split("=")[1]);
                    }
                }
                catch
                {
                    Console.WriteLine("Invalid timer value, enter an integer value in milliseconds, standard delay(timer) is installed");
                }

            });
            try
            {
                var server = new WebSocketServer($"ws://0.0.0.0:{port}");
                server.Start(socket =>
                {
                    socket.OnOpen = () => GetMemoryInfo(socket, timer, showerrors);
                    socket.OnClose = () => Console.WriteLine("Close!");
                    socket.OnMessage = message => socket.Send(message);
                });
                Thread.Sleep(-1);
            }
            catch (Exception ex)
            {
                if (ExceptionContainsErrorCode(ex, 10013))
                {
                    Console.WriteLine("This port is busy.");
                }
                else if (ex.Source.Contains("System.Private.Uri"))
                {
                    Console.WriteLine("You set the port value incorrectly. The number of ports is limited from 0 to 65535.");
                }
                else
                {
                    Console.WriteLine(ex);
                }
                Console.WriteLine("");
                Console.WriteLine("The app will close after 15 seconds.");
                Thread.Sleep(15000);
            }
        }

        private static bool ExceptionContainsErrorCode(Exception e, int ErrorCode)
        {
            Win32Exception winEx = e as Win32Exception;
            if (winEx != null && ErrorCode == winEx.ErrorCode)
                return true;

            if (e.InnerException != null)
                return ExceptionContainsErrorCode(e.InnerException, ErrorCode);

            return false;
        }

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
        internal static async void GetMemoryInfo(IWebSocketConnection socket, int timer, bool showerrors)
        {
            Console.WriteLine("Connected!");
            var baseAddresses = new OsuBaseAddresses();
            while (true)
            {
                var osu_processes = Process.GetProcessesByName("osu!");
                
                if (osu_processes.Length == 0)
                {
                    await Task.Delay(10000);
                    continue;
                }

                using var osu_process = osu_processes[0];

                try
                {
                    string osu_dir = Path.GetDirectoryName(osu_process.MainModule.FileName);
                    StructuredOsuMemoryReader _sreader = StructuredOsuMemoryReader.Instance.GetInstanceForWindowTitleHint(osu_process.MainWindowTitle);
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
                        { "acc", null },
                        { "c300", null },
                        { "c100", null },
                        { "c50", null },
                        { "cMiss", null },
                        { "cGeki", null },
                        { "cKatu", null },
                        { "combo", null },
                        { "maxCombo", null },
                        { "score", null },
                        { "isReplay", null },
                        { "HP", null },
                        { "HPSmooth", null },
                        { "mode", null },
                        { "mods_int", null },
                        { "mods", null },
                        { "pp", null },
                        { "HitErrors", null }
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
                    res["SkinDir"] = Path.Join(osu_dir, "Skins", OsuMemoryReader.Instance.GetSkinFolderName());
                    res["MapDir"] = Path.Join(osu_dir, "Songs", OsuMemoryReader.Instance.GetMapFolderName());
                    res["osuFile"] = Path.Join(res["MapDir"], _ssreader.ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.OsuFileName)));
                    var mapinfo = res["osuFile"].Contains("nekodex - circles! (peppy).osu") ? null : MapCache.GetBeatmap(res["osuFile"]);
                    
                    
                    if (mapinfo is not null)
                    {
                        res["SongLength"] = mapinfo.Length;
                        res["bg"] = Path.Join(res["MapDir"], mapinfo.BackgroundFile);
                        res["ModsMainMenuInt"] = OsuMemoryReader.Instance.GetMods();
                        var ruleset = getRuleset(mapinfo.RulesetID);
                        var playableMap = mapinfo.GetPlayableBeatmap(ruleset.RulesetInfo);
                        if (res["GameModeNumberMenu"] != 0 && mapinfo.RulesetID == 0)
                        {
                            ruleset = getRuleset(res["GameModeNumberMenu"]);
                            var calculator = PPCalculatorHelpers.GetPPCalculator(res["GameModeNumberMenu"]);
                            var converter = ruleset.CreateBeatmapConverter(mapinfo);
                            playableMap = converter.Convert();
                            mapinfo = new PPCalculator.WorkingBeatmap(playableMap);
                        }

                        var mods_lazer = GetMods(((ModsStr)res["ModsMainMenuInt"]).ToString().Split(","), ruleset);
                        res["StarRate"] = osucket.PPCalculator.DiffCalculator.GetStarRate(mods_lazer, mapinfo, ruleset);
                        
                    }
                    res["beatmap_id"] = OsuMemoryReader.Instance.GetMapId() ;

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
                        gameplay["acc"] =_ssreader.ReadProperty<double>(baseAddresses.Player, nameof(Player.Accuracy));
                        gameplay["c300"] =_ssreader.ReadUShort(baseAddresses.Player, nameof(Player.Hit300));
                        gameplay["c100"] =_ssreader.ReadUShort(baseAddresses.Player, nameof(Player.Hit100));
                        gameplay["c50"] =_ssreader.ReadUShort(baseAddresses.Player, nameof(Player.Hit50));
                        gameplay["cMiss"] =_ssreader.ReadUShort(baseAddresses.Player, nameof(Player.HitMiss));
                        gameplay["cGeki"] =_ssreader.ReadUShort(baseAddresses.Player, nameof(Player.HitGeki));
                        gameplay["cKatu"] =_ssreader.ReadUShort(baseAddresses.Player, nameof(Player.HitKatu));
                        gameplay["combo"] =_ssreader.ReadUShort(baseAddresses.Player, nameof(Player.Combo));
                        gameplay["maxCombo"] =_ssreader.ReadUShort(baseAddresses.Player, nameof(Player.MaxCombo));
                        if (_sreader.TryReadProperty(baseAddresses.Player, nameof(Player.ScoreV2), out var score))
                        {
                            gameplay["score"] =score;
                        }

                        gameplay["isReplay"] =OsuMemoryReader.Instance.IsReplay();

                        gameplay["HP"] =_ssreader.ReadProperty<double>(baseAddresses.Player, nameof(Player.HP));
                        gameplay["HPSmooth"] =_ssreader.ReadProperty<double>(baseAddresses.Player, nameof(Player.HPSmooth));
                        gameplay["mode"] =Enum.GetName(typeof(OsuGameMode), gameplay["mode_int"]);
                         if (_sreader.TryReadProperty(baseAddresses.Player, nameof(Player.Mods), out var mods))
                        {
                            var _kok = (Mods) mods;
                            var mods_int = _kok.Value;
                            gameplay["mods_int"] =mods_int ;
                            gameplay["mods"] =((ModsStr)mods_int).ToString();
                        }
                        if (gameplay["maxCombo"] != 0) {
                            var calculator = PPCalculatorHelpers.GetPPCalculator(gameplay["mode_int"]);
                            var playableMap = mapinfo.GetPlayableBeatmap(calculator.Ruleset.RulesetInfo);

                            if (res["GameModeNumberMenu"] != 0 && mapinfo.RulesetID == 0)
                            {

                                Ruleset ruleset = getRuleset(res["GameModeNumberMenu"]);
                                calculator = PPCalculatorHelpers.GetPPCalculator(res["GameModeNumberMenu"]);
                                var converter = ruleset.CreateBeatmapConverter(playableMap);
                                playableMap = converter.Convert();
                                mapinfo = new PPCalculator.WorkingBeatmap(playableMap);
                            }


                            pp["pp"] = calculator.Calculate(mapinfo, res["SongTime"], gameplay["acc"] / 100, gameplay["maxCombo"], gameplay["cMiss"], gameplay["c50"], gameplay["mods"].Split(","), gameplay["score"]);
                            if (gameplay["mode_int"] != 3) pp["fcpp"] = calculator.Calculate(mapinfo, gameplay["acc"] / 100, calculator.GetMaxCombo(playableMap), 0, 0, gameplay["mods"].Split(","), 1000000);
                            pp["sspp"] = calculator.Calculate(mapinfo, 1, calculator.GetMaxCombo(playableMap), 0, 0, gameplay["mods"].Split(","), 1000000);
                        }
                        gameplay["pp"] =pp;


                        gameplay["HitErrors"] =_ssreader.ReadClassProperty<List<int>>(baseAddresses.Player, nameof(Player.HitErrors));
                        res["Gameplay"] =gameplay;
                    }
                    if (res["StatusNumber"] == OsuMemoryStatus.ResultsScreen)
                    {
                        gameplay["username"] =_ssreader.ReadString(baseAddresses.ResultsScreen, nameof(ResultsScreen.Username));
                        gameplay["c300"] =_ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.Hit300));
                        gameplay["c100"] =_ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.Hit100));
                        gameplay["c50"] =_ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.Hit50));
                        gameplay["cMiss"] =_ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.HitMiss));
                        gameplay["cGeki"] =_ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.HitGeki));
                        gameplay["cKatu"] =_ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.HitKatu));
                        gameplay["maxCombo"] =_ssreader.ReadUShort(baseAddresses.ResultsScreen, nameof(ResultsScreen.MaxCombo));
                        if (_sreader.TryReadProperty(baseAddresses.ResultsScreen, nameof(Player.ScoreV2), out var score))
                        {
                            if (score is null)
                            {
                                _sreader.TryReadProperty(baseAddresses.ResultsScreen, nameof(Player.Score), out score);

                            }
                            gameplay["score"] =score;
                        }
                        gameplay["mode_int"] =_ssreader.ReadInt(baseAddresses.ResultsScreen, nameof(ResultsScreen.Mode));
                        gameplay["mode"] =Enum.GetName(typeof(OsuGameMode), gameplay["mode_int"]);
                        if (_sreader.TryReadProperty(baseAddresses.ResultsScreen, nameof(ResultsScreen.Mods), out var mods))
                        {
                            var _kok = (Mods)mods;
                            var mods_int = _kok.Value;
                            gameplay["mods_int"] =mods_int;
                            gameplay["mods"] =$"{(ModsStr)mods_int}";
                        }
                        var calculator = PPCalculatorHelpers.GetPPCalculator(gameplay["mode_int"]);
                        var playableMap = mapinfo.GetPlayableBeatmap(calculator.Ruleset.RulesetInfo);

                        if (res["GameModeNumberMenu"] != 0 && mapinfo.RulesetID == 0)
                        {

                            Ruleset ruleset = getRuleset(res["GameModeNumberMenu"]);
                            calculator = PPCalculatorHelpers.GetPPCalculator(res["GameModeNumberMenu"]);
                            var converter = ruleset.CreateBeatmapConverter(playableMap);
                            playableMap = converter.Convert();
                            mapinfo = new PPCalculator.WorkingBeatmap(playableMap);
                        }

                        gameplay["acc"] =GetAccuracy(gameplay);


                        gameplay["pp"] =calculator.Calculate(mapinfo,  gameplay["acc"] / 100, gameplay["maxCombo"], gameplay["cMiss"], gameplay["c50"], gameplay["mods"].Split(","), gameplay["score"]);

                        res["ResultScreen"] =gameplay;
                    }

                    string data = JsonSerializer.Serialize(res);

                    await socket.Send(data);
                }
                catch (Exception exception)
                {
                    if (exception.Message.Contains("ReadProcessMemory"))
                    {
                        continue;
                    }
                    else
                    {

                        if (showerrors)
                        {
                            Console.WriteLine(exception);
                        }
                        else
                        {
                            Console.WriteLine("An unknown error has occurred. Ignoring...");
                        }
                    }
                }
                
                await Task.Delay(timer);
                if (socket.IsAvailable == false) return;
            }
        }
    }
}