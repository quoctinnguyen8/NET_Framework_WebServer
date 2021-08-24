using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NET_Framework_WebServer
{
	class HttpServer
	{
		private HttpListener listener;
		private Dictionary<string, string> dicFileType = new Dictionary<string, string>();
		private string serverPath;
		public HttpServer(params string[] prefixes)
		{
			if (!HttpListener.IsSupported)
				throw new Exception("Incompatible device");

			if (prefixes == null || prefixes.Length == 0)
				throw new ArgumentException("prefixes");

			// Khởi tạo HttpListener
			listener = new HttpListener();
			foreach (string prefix in prefixes)
			{
				listener.Prefixes.Add(prefix);
			}

			// Content-type theo extension
			dicFileType.Add("html", "text/html");
			dicFileType.Add("js", "text/javascript");
			dicFileType.Add("json", "application/json");
			dicFileType.Add("txt", "text");
			dicFileType.Add("css", "text/css");
			dicFileType.Add("svg", "image/svg+xml");
			dicFileType.Add("png", "image/png");
			dicFileType.Add("jpg", "image/jpeg");
			dicFileType.Add("ico", "image/x-icon");
			dicFileType.Add("gif", "image/gif");

			serverPath = Directory.GetCurrentDirectory();
		}
		public async Task StartAsync()
		{
			// Bắt đầu lắng nghe kết nối HTTP
			listener.Start();
			do
			{
				try
				{
					Console.WriteLine($"{DateTime.Now.ToLongTimeString()} : waiting a client connect");

					// Một client kết nối đến
					HttpListenerContext context = await listener.GetContextAsync();
					await this.ProcessRequest(context);

				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
				Console.WriteLine(".....");

			}
			while (listener.IsListening);
		}

		// Xử lý trả về nội dung tùy thuộc vào URL truy cập
		async Task ProcessRequest(HttpListenerContext context)
		{
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;
			Console.WriteLine($"{request.HttpMethod} {request.RawUrl} {request.Url.AbsolutePath}");

			// Lấy stream / gửi dữ liệu về cho client
			var outputstream = response.OutputStream;

			var requestPath = request.Url.AbsolutePath;
			if (requestPath == "/" || requestPath == "")
			{
				requestPath = "/index.html";
			}

			var fileType = requestPath.Split('.').Last();

			var fileName = serverPath + requestPath.Replace('/', '\\');

			if (File.Exists(fileName))
			{
				var fileBytesContent = File.ReadAllBytes(fileName);
				// Gửi thông tin về cho Client
				if (dicFileType.ContainsKey(fileType))
				{
					context.Response.Headers.Add("Content-Type", $"{dicFileType[fileType]}; charset=utf-8");
				}
				else
				{
					context.Response.Headers.Add("Content-Type", request.ContentType);
				}
				context.Response.StatusCode = (int)HttpStatusCode.OK;
				response.ContentLength64 = fileBytesContent.Length;
				await outputstream.WriteAsync(fileBytesContent, 0, fileBytesContent.Length);
			}
			else
			{
				response.StatusCode = (int)HttpStatusCode.NotFound;
				byte[] buffer = Encoding.UTF8.GetBytes("NOT FOUND!");
				response.ContentLength64 = buffer.Length;
				await outputstream.WriteAsync(buffer, 0, buffer.Length);
			}

			// Đóng stream để hoàn thành gửi về client
			outputstream.Close();
		}
	}
}
