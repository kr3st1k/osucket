using Fleck;
using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using OsuMemoryDataProvider;
using System.Collections.Generic;
using System.Text.Json;
using NUnit.Framework.Internal;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;


namespace osucket
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://0.0.0.0:13371");
            server.Start(socket =>
            {
                socket.OnOpen = () => new Thread(() =>
                {
                    GetMemoryInfo(socket);
                }).Start();
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message => socket.Send(message);
            });
            Thread.Sleep(-1);
        }
        public enum OsuGameMode
        {
            std = 0,
            taiko = 1,
            fruits = 2,
            mania = 3
        }
        
        static async void GetMemoryInfo(IWebSocketConnection socket)
        {
            while (true)
            {
                var baseAddresses = new OsuBaseAddresses();
                Process[] osu_process = Process.GetProcessesByName("osu!");
                if (osu_process.Length != 0)
                {
                    string osu_dir = osu_process[0].MainModule.FileName.Replace("osu!.exe", "");
                    StructuredOsuMemoryReader _sreader = StructuredOsuMemoryReader.Instance.GetInstanceForWindowTitleHint(osu_process[0].MainWindowTitle);
                    T ReadProperty<T>(object readObj, string propName, T defaultValue = default) where T : struct
                    {
                        if (_sreader.TryReadProperty(readObj, propName, out var readResult))
                            return (T)readResult;

                        return defaultValue;
                    }

                    T ReadClassProperty<T>(object readObj, string propName, T defaultValue = default) where T : class
                    {
                        if (_sreader.TryReadProperty(readObj, propName, out var readResult))
                            return (T)readResult;

                        return defaultValue;
                    }
                    int ReadInt(object readObj, string propName)
                        => ReadProperty<int>(readObj, propName, -5);
                    UInt16 ReadShort(object readObj, string propName)
                        => ReadProperty<UInt16>(readObj, propName);

                    float ReadFloat(object readObj, string propName)
                        => ReadProperty<float>(readObj, propName, -5f);

                    string ReadString(object readObj, string propName)
                        => ReadClassProperty<string>(readObj, propName, "INVALID READ");


                    var res = new Dictionary<string, dynamic> { };
                    _sreader.TryRead(baseAddresses.Player);
                    var ro = _sreader.OsuMemoryAddresses.Player;
                    var gameplay = new Dictionary<string, dynamic> { };
                    
                    res.Add("StatusNumber", OsuMemoryReader.Instance.GetCurrentStatus(out int num));
                    res.Add("Status", Enum.GetName(typeof(OsuMemoryStatus), res["StatusNumber"]));
                    res.Add("GameModeNumber", OsuMemoryReader.Instance.ReadSongSelectGameMode());
                    res.Add("Song", OsuMemoryReader.Instance.GetSongString());
                    res.Add("SkinName", OsuMemoryReader.Instance.GetSkinFolderName());
                    res.Add("GameMode", Enum.GetName(typeof(OsuGameMode), res["GameModeNumber"]));
                    res.Add("SkinDir", $"{osu_dir}Skins\\{OsuMemoryReader.Instance.GetSkinFolderName()}");
                    if (res["StatusNumber"] == OsuMemoryStatus.Playing)
                    {
                        gameplay.Add("username", ReadString(baseAddresses.Player, nameof(Player.Username)));
                        gameplay.Add("acc", ReadProperty<double>(baseAddresses.Player, nameof(Player.Accuracy)));
                        gameplay.Add("c300", ReadShort(baseAddresses.Player, nameof(Player.Hit300)));
                        gameplay.Add("c100", ReadShort(baseAddresses.Player, nameof(Player.Hit100)));
                        gameplay.Add("c50", ReadShort(baseAddresses.Player, nameof(Player.Hit50)));
                        gameplay.Add("cMiss", ReadShort(baseAddresses.Player, nameof(Player.HitMiss)));
                        gameplay.Add("cGeki", ReadShort(baseAddresses.Player, nameof(Player.HitGeki)));
                        gameplay.Add("cKatu", ReadShort(baseAddresses.Player, nameof(Player.HitKatu)));
                        gameplay.Add("combo", ReadShort(baseAddresses.Player, nameof(Player.Combo)));
                        gameplay.Add("maxCombo", ReadShort(baseAddresses.Player, nameof(Player.MaxCombo)));
                        if (_sreader.TryReadProperty(baseAddresses.Player, nameof(Player.ScoreV2), out var score))
                        {
                            gameplay.Add("score", score);
                        }
                        gameplay.Add("isReplay", OsuMemoryReader.Instance.IsReplay());
                        gameplay.Add("HP", ReadProperty<double>(baseAddresses.Player, nameof(Player.HP)));
                        gameplay.Add("HPSmooth", ReadProperty<double>(baseAddresses.Player, nameof(Player.HPSmooth)));
                        gameplay.Add("mode_int", ReadInt(baseAddresses.Player, nameof(Player.Mode)));
                        gameplay.Add("mode", Enum.GetName(typeof(OsuGameMode), gameplay["mode_int"]));
                        var mods = OsuMemoryReader.Instance.GetMods();
                        gameplay.Add("mods_int", mods);
                        gameplay.Add("mods", $"{(ModsStr)mods}");
                        gameplay.Add("HitErrors", ReadClassProperty<List<int>>(baseAddresses.Player, nameof(Player.HitErrors)));
                        res.Add("Gameplay", gameplay);

                    }
                    
                    string data = JsonSerializer.Serialize(res);

                    await socket.Send(data);
                    
                }
                await Task.Delay(1000);
                if (socket.IsAvailable == false)
                {
                    GC.Collect();
                    return;
                }
            }
        }
    }
}
