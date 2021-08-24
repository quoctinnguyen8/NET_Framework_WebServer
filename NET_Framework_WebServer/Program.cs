using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET_Framework_WebServer
{
	class Program
	{
		static async Task Main(string[] args)
		{
			HttpServer httpServer = new HttpServer(new string[] { "http://*:9988/" });
			Process.Start("http://localhost:9988");
			await httpServer.StartAsync();
			Console.ReadKey();
		}
	}
}
