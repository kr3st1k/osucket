using Fleck;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TagLib.NonContainer;

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
                socket.Send("test");
                await Task.Delay(5000);
                if (socket.IsAvailable == false)
                {
                    return;
                }
            }
        }
    }
}
