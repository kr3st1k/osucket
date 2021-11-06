using Fleck;
using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using OsuMemoryDataProvider;

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

            while (true)
            {

            }
        }

        static async void GetMemoryInfo(IWebSocketConnection socket)
        {
            while (true)
            {
                if(Process.GetProcessesByName("osu!").Length != 0)
                {
                    
                    await socket.Send("{" + $"\"songString\": \"{OsuMemoryReader.Instance.GetSongString()}\"" + "}");
                    
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
