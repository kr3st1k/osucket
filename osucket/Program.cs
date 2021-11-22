using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

using osucket.calculations;

using Fleck;


namespace osucket
{

    internal static class Program
    {
        public static List<IWebSocketConnection> sockets = new List<IWebSocketConnection>();
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
                    socket.OnOpen = () => { Console.WriteLine($"Connected. ip:{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}"); sockets.Add(socket); };
                    socket.OnClose = () => { Console.WriteLine($"Closed connection. ip:{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}"); sockets.RemoveAt(sockets.IndexOf(socket)); };
                    socket.OnMessage = message => socket.Send(message);
                });

                GetMemoryInfo(timer, showerrors);
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

        internal static async void GetMemoryInfo(int timer, bool showerrors)
        {
            while (true)
            {
                var osu_processes = Process.GetProcessesByName("osu!");
                
                if (osu_processes.Length == 0)
                {
                    Thread.Sleep(10000);
                    continue;
                } else if (sockets.Count == 0)
                {
                    Thread.Sleep(5000);
                    continue;
                }

                using var osu_process = osu_processes[0];

                try
                {

                    var data = calculations.Program.GetData(Path.GetDirectoryName(osu_process.MainModule.FileName), osu_process.MainWindowTitle);

                    foreach(IWebSocketConnection socket in sockets) {
                        await socket.Send(data);
                    };
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

                Thread.Sleep(timer);
            }
        }
    }
}