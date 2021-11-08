using Fleck;
using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using OsuMemoryDataProvider;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NUnit.Framework.Internal;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;


namespace osucket
{
    class MemoryReader
    {
        private StructuredOsuMemoryReader _sreader;
        public StructuredOsuMemoryReader Sreader
        {
            get { return _sreader; }

            set { _sreader = value; }
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
        public int ReadInt(object readObj, string propName)
            => ReadProperty<int>(readObj, propName, -5);
        public UInt16 ReadShort(object readObj, string propName) => ReadProperty<UInt16>(readObj, propName);

        public float ReadFloat(object readObj, string propName) => ReadProperty<float>(readObj, propName, -5f);

        public string ReadString(object readObj, string propName) => ReadClassProperty<string>(readObj, propName, "INVALID READ");

    }


    class Program
    {
        public enum OsuGameMode
        {
            std = 0,
            taiko = 1,
            fruits = 2,
            mania = 3
        }
        static void Main(string[] args)
        {
            Int32 timer = 600;
            bool showerrors = false;
            Environment.GetCommandLineArgs().ToList().ForEach(x =>
            {
                if (x.EndsWith("/?") || x.EndsWith("-h") || x.EndsWith("--h") || x.EndsWith("-help") ||
                    x.EndsWith("--help"))
                {
                    Console.WriteLine(@"help");
                    Environment.Exit(0);
                }
                if (x.StartsWith("/showerrors") || x.StartsWith("-showerrors") || x.StartsWith("--showerrors") ||
                    x.StartsWith("-se") || x.StartsWith("--se") || x.StartsWith("/se"))
                {
                    showerrors = true;
                }
                try
                {
                    if (x.StartsWith("/timer") || x.StartsWith("-timer") || x.StartsWith("--timer") ||
                        x.StartsWith("-t") || x.StartsWith("--t") || x.StartsWith("/t"))
                    {
                        timer = Convert.ToInt32(x.Split("=")[1]);
                    }
                }
                catch
                {
                    Console.WriteLine("Invalid value, enter an integer value in milliseconds");
                    Environment.Exit(0);
                }

            });
            var server = new WebSocketServer("ws://0.0.0.0:13371");
            server.Start(socket =>
            {
                socket.OnOpen = () => GetMemoryInfo(socket, timer, showerrors);
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message => socket.Send(message);
            });
            Thread.Sleep(-1);
        }
        
        static async void GetMemoryInfo(IWebSocketConnection socket, int timer, bool showerrors)
        {
            Console.WriteLine("Connected!");
            while (true)
            {
                Process[] osu_process = Process.GetProcessesByName("osu!");
                if (osu_process.Length != 0)
                {
                    try
                    {
                        var baseAddresses = new OsuBaseAddresses();
                        string osu_dir = osu_process[0].MainModule.FileName.Replace("osu!.exe", "");
                        StructuredOsuMemoryReader _sreader = StructuredOsuMemoryReader.Instance.GetInstanceForWindowTitleHint(osu_process[0].MainWindowTitle);
                        MemoryReader _ssreader = new MemoryReader
                        {
                            Sreader = _sreader
                        };


                        var res = new Dictionary<string, dynamic> { };
                        var gameplay = new Dictionary<string, dynamic> { };
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

                        res.Add("StatusNumber", OsuMemoryReader.Instance.GetCurrentStatus(out int num));
                        res.Add("Status", Enum.GetName(typeof(OsuMemoryStatus), res["StatusNumber"]));
                        res.Add("GameModeNumberMenu", OsuMemoryReader.Instance.ReadSongSelectGameMode());
                        res.Add("Song", OsuMemoryReader.Instance.GetSongString());
                        res.Add("SkinName", OsuMemoryReader.Instance.GetSkinFolderName());
                        res.Add("GameModeMenu", Enum.GetName(typeof(OsuGameMode), res["GameModeNumberMenu"]));
                        res.Add("SkinDir", $"{osu_dir}Skins\\{OsuMemoryReader.Instance.GetSkinFolderName()}\\");
                        res.Add("MapDir", $"{osu_dir}{OsuMemoryReader.Instance.GetMapFolderName()}\\");
                        res.Add("cRetry", OsuMemoryReader.Instance.GetRetrys());
                        res.Add("beatmapStatus", _ssreader.ReadProperty<Int16>(baseAddresses.Beatmap, nameof(CurrentBeatmap.Status)));
                        if (res["StatusNumber"] == OsuMemoryStatus.Playing)
                        {
                            keyOverlay["ck1"] = _ssreader.ReadInt(baseAddresses.KeyOverlay, nameof(KeyOverlay.K1Count));
                            keyOverlay["ck2"] = _ssreader.ReadInt(baseAddresses.KeyOverlay, nameof(KeyOverlay.K2Count));
                            keyOverlay["cm1"] = _ssreader.ReadInt(baseAddresses.KeyOverlay, nameof(KeyOverlay.M1Count));
                            keyOverlay["cm2"] = _ssreader.ReadInt(baseAddresses.KeyOverlay, nameof(KeyOverlay.M2Count));

                            keyOverlay["bk1"] = _ssreader.ReadBool(baseAddresses.KeyOverlay, nameof(KeyOverlay.K1Pressed));
                            keyOverlay["bk2"] = _ssreader.ReadBool(baseAddresses.KeyOverlay, nameof(KeyOverlay.K2Pressed));
                            keyOverlay["bm1"] = _ssreader.ReadBool(baseAddresses.KeyOverlay, nameof(KeyOverlay.M1Pressed));
                            keyOverlay["bm2"] = _ssreader.ReadBool(baseAddresses.KeyOverlay, nameof(KeyOverlay.M2Pressed));

                            gameplay.Add("keyOverlay", keyOverlay);

                            gameplay.Add("username", _ssreader.ReadString(baseAddresses.Player, nameof(Player.Username)));
                            gameplay.Add("acc", _ssreader.ReadProperty<double>(baseAddresses.Player, nameof(Player.Accuracy)));
                            gameplay.Add("c300", _ssreader.ReadShort(baseAddresses.Player, nameof(Player.Hit300)));
                            gameplay.Add("c100", _ssreader.ReadShort(baseAddresses.Player, nameof(Player.Hit100)));
                            gameplay.Add("c50", _ssreader.ReadShort(baseAddresses.Player, nameof(Player.Hit50)));
                            gameplay.Add("cMiss", _ssreader.ReadShort(baseAddresses.Player, nameof(Player.HitMiss)));
                            gameplay.Add("cGeki", _ssreader.ReadShort(baseAddresses.Player, nameof(Player.HitGeki)));
                            gameplay.Add("cKatu", _ssreader.ReadShort(baseAddresses.Player, nameof(Player.HitKatu)));
                            gameplay.Add("combo", _ssreader.ReadShort(baseAddresses.Player, nameof(Player.Combo)));
                            gameplay.Add("maxCombo", _ssreader.ReadShort(baseAddresses.Player, nameof(Player.MaxCombo)));
                            if (_sreader.TryReadProperty(baseAddresses.Player, nameof(Player.ScoreV2), out var score))
                            {
                                gameplay.Add("score", score);
                            }

                            gameplay.Add("isReplay", OsuMemoryReader.Instance.IsReplay());
                            gameplay.Add("HP", _ssreader.ReadProperty<double>(baseAddresses.Player, nameof(Player.HP)));
                            gameplay.Add("HPSmooth", _ssreader.ReadProperty<double>(baseAddresses.Player, nameof(Player.HPSmooth)));
                            gameplay.Add("mode_int", _ssreader.ReadInt(baseAddresses.Player, nameof(Player.Mode)));
                            gameplay.Add("mode", Enum.GetName(typeof(OsuGameMode), gameplay["mode_int"]));
                            if (_sreader.TryReadProperty(baseAddresses.Player, nameof(Player.Mods), out var mods))
                            {
                                var _kok = (Mods) mods;
                                var mods_int = _kok.Value;
                                gameplay.Add("mods_int", mods_int);
                                gameplay.Add("mods", $"{(ModsStr) mods_int}");
                            }

                            gameplay.Add("HitErrors", _ssreader.ReadClassProperty<List<int>>(baseAddresses.Player, nameof(Player.HitErrors)));
                            res.Add("Gameplay", gameplay);
                        }

                        string data = JsonSerializer.Serialize(res);

                        await socket.Send(data);

                        res.Clear();
                        gameplay.Clear();
                        keyOverlay.Clear();
                        osu_process[0].Dispose();
                        GC.Collect();
                    }
                    catch (Exception exception)
                    {
                        if (showerrors)
                        {
                            Console.WriteLine(exception);
                        }
                        else
                        {
                            Console.WriteLine("An unknown error has occurred. Ignoring...");
                        }
                        GC.Collect();
                    }
                }
                else
                {
                    await Task.Delay(10000);
                    GC.Collect();
                }
                await Task.Delay(timer);
                if (socket.IsAvailable == false)
                {
                    GC.Collect();
                    return;
                }
            }
        }
    }
}
