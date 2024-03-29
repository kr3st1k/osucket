﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fleck;

namespace osucket
{
	internal static class Program
	{
		private static List<IWebSocketConnection> sockets = new();

		internal static void Main(string[] args)
		{
			Console.WriteLine(
				"                                                                   \r\n                                                                  \r\n                                       `7MM                 mm    \r\n                                         MM                 MM    \r\n ,pW\"Wq.  ,pP\"Ybd `7MM  `7MM   ,p6\"bo    MM  ,MP' .gP\"Ya  mmMMmm  \r\n6W'   `Wb 8I   `\"   MM    MM  6M'  OO    MM ;Y   ,M'   Yb   MM    \r\n8M     M8 `YMMMa.   MM    MM  8M         MM;Mm   8M\"\"\"\"\"\"   MM    \r\nYA.   ,A9 L.   I8   MM    MM  YM.    ,   MM `Mb. YM.    ,   MM    \r\n `Ybmd9'  M9mmmP'   `Mbod\"YML. YMbmd'  .JMML. YA. `Mbmmd'   `Mbmo \r\n                                                                  \r\n                                                                  ");
			var port = 13371;
			var timer = 500;
			var showErrors = false;
			Environment.GetCommandLineArgs().ToList().ForEach(x =>
			{
				if (x.EndsWith("/?") || x.EndsWith("?") || x.EndsWith("-h") || x.EndsWith("--h") ||
				    x.EndsWith("-help") || x.EndsWith("--help"))
				{
					Console.WriteLine("-showerrors           Output errors\n" +
					                  "-port=13371           WebSocket port\n" +
					                  "-timer=500            Delay in sending data (-delay=500) \n");
					Environment.Exit(0);
				}

				if (x.StartsWith("/showerrors") || x.StartsWith("-showerrors") || x.StartsWith("--showerrors") ||
				    x.StartsWith("-se") || x.StartsWith("--se") || x.StartsWith("/se"))
					showErrors = true;
				try
				{
					if (x.StartsWith("/port") || x.StartsWith("-port") || x.StartsWith("--port") ||
					    x.StartsWith("-p") || x.StartsWith("--p") || x.StartsWith("/p"))
						port = Convert.ToInt32(x.Split("=")[1]);
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
						timer = Convert.ToInt32(x.Split("=")[1]);
				}
				catch
				{
					Console.WriteLine(
						"Invalid timer value, enter an integer value in milliseconds, standard delay(timer) is installed");
				}
			});
			try
			{
				var server = new WebSocketServer($"ws://0.0.0.0:{port}");
				server.Start(socket =>
				{
					socket.OnOpen = () =>
					{
						Console.WriteLine(
							$"Connected. ip:{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}");
						sockets.Add(socket);
					};
					socket.OnClose = () =>
					{
						Console.WriteLine(
							$"Closed connection. ip:{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}");
						sockets.RemoveAt(sockets.IndexOf(socket));
					};
					socket.OnMessage = message => socket.Send(message);
				});
				Task.Run(() => GetMemoryInfo(timer, showErrors)).Wait();
			}
			catch (Exception ex)
			{
				if (ExceptionContainsErrorCode(ex, 10013))
					Console.WriteLine("This port is busy.");
				else if (ex.Source.Contains("System.Private.Uri"))
					Console.WriteLine(
						"You set the port value incorrectly. The number of ports is limited from 0 to 65535.");
				else
					Console.WriteLine(ex);
				Console.WriteLine("");
				Console.WriteLine("The app will close after 15 seconds.");
				Thread.Sleep(15000);
			}
		}

		private static bool ExceptionContainsErrorCode(Exception e, int ErrorCode)
		{
			if (e is Win32Exception winEx && ErrorCode == winEx.ErrorCode)
				return true;

			return e.InnerException != null && ExceptionContainsErrorCode(e.InnerException, ErrorCode);
		}

		private static async Task GetMemoryInfo(int timer, bool showerrors)
		{
			while(true)
			{
				var osuProcesses = Process.GetProcessesByName("osu!");

				if (osuProcesses.Length == 0)
				{
					await Task.Delay(10000);
					continue;
				}
				using Process osuProcess = osuProcesses[0];
				
			while (true)
			{

				if (sockets.Count == 0)
				{
					await Task.Delay(5000);
					continue;
				}

				try
				{
					string data = Calculations.Calculation.GetData(Path.GetDirectoryName(osuProcess.MainModule.FileName));

					foreach (IWebSocketConnection socket in sockets) await socket.Send(data);
					;
				}
				catch (Exception exception)
				{
					if (exception.Message.Contains("ReadProcessMemory")) continue;

					if (showerrors)
						Console.WriteLine(exception);
					else
						Console.WriteLine("An unknown error has occurred. Ignoring...");
				}

				await Task.Delay(timer);
			}
			
			}
		}
	}
}
