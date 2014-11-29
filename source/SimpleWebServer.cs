using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;

namespace JsonKerman
{
	/**
	 * Simple web server, by some other David, MIT Licensed.
	 * 
	 * https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server.aspx
	 * 
	 * Example Use:
	 *   class Program
	 *   {
	 *       static void Main(string[] args)
	 *       {
	 *           WebServer ws = new WebServer(SendResponse, "http://localhost:8080/test/");
	 *           ws.Run();
	 *           Console.WriteLine("A simple webserver. Press a key to quit.");
	 *           Console.ReadKey();
	 *           ws.Stop();
	 *       }
 	 *   
	 *       public static string SendResponse(HttpListenerRequest request)
	 *       {
	 *           return string.Format("<HTML><BODY>My web page.<br>{0}</BODY></HTML>", DateTime.Now);
	 *       }
	 *   }
	 * 
	 * I've modified this to make the callback return a response object.
	 * It might be better to switch to a delegate that reads the request object and operates on the response directly.
	 */
	public class WebServer
	{
		public class Response
		{
			public int StatusCode;
			public string StatusDescription;
			public string ContentType;
			public string Data;
			public bool StreamFile; // if this is true, Data specifies a filename to stream rather than actual data to output.

			public Response()
			{
				StatusCode = 200;
				StatusDescription = "OK";
				StreamFile = false;
			}

			public Response(string data)
			{
				StatusCode = 200;
				StatusDescription = "OK";
				Data = data;
				StreamFile = false;
			}

			public Response(string data, string contentType)
			{
				StatusCode = 200;
				StatusDescription = "OK";
				Data = data;
				ContentType = contentType;
				StreamFile = false;
			}
		}

		private readonly HttpListener _listener = new HttpListener();
		private readonly Func<HttpListenerRequest, Response> _responderMethod;

		public WebServer(string[] prefixes, Func<HttpListenerRequest, Response> method)
		{
			if (!HttpListener.IsSupported)
				throw new NotSupportedException(
					"Needs Windows XP SP2, Server 2003 or later.");

			// URI prefixes are required, for example 
			// "http://localhost:8080/index/".
			if (prefixes == null || prefixes.Length == 0)
				throw new ArgumentException("prefixes");

			// A responder method is required
			if (method == null)
				throw new ArgumentException("method");

			foreach (string s in prefixes)
				_listener.Prefixes.Add(s);

			_responderMethod = method;
			_listener.Start();
		}

		public WebServer(Func<HttpListenerRequest, Response> method, params string[] prefixes)
			: this(prefixes, method) { }

		public void Run()
		{
			ThreadPool.QueueUserWorkItem((o) =>
			{
				Console.WriteLine("Webserver running...");
				try
				{
					while (_listener.IsListening)
					{
						ThreadPool.QueueUserWorkItem((c) =>
						{
							var ctx = c as HttpListenerContext;
							try
							{
								Response resp = _responderMethod(ctx.Request);

								ctx.Response.ContentType = resp.ContentType;
								ctx.Response.StatusCode = resp.StatusCode;
								ctx.Response.StatusDescription = resp.StatusDescription;

								if (resp.StreamFile)
								{
									FileInfo info = new FileInfo(resp.Data);
									ctx.Response.ContentLength64 = info.Length;

									FileStream file = new FileStream(resp.Data, FileMode.Open);
									int bytes = 256 * 1024;
									byte[] buffer = new byte[bytes];
									for (int read = file.Read(buffer, 0, bytes); read > 0; read = file.Read(buffer, 0, bytes))
									{
										ctx.Response.OutputStream.Write(buffer, 0, read);
									}
									file.Close();
								}
								else
								{
									byte[] buf = Encoding.UTF8.GetBytes(resp.Data);
									ctx.Response.ContentLength64 = buf.Length;
									ctx.Response.OutputStream.Write(buf, 0, buf.Length);
								}
							}
							catch { } // suppress any exceptions
							finally
							{
								// always close the stream
								ctx.Response.OutputStream.Close();
							}
						}, _listener.GetContext());
					}
				}
				catch { } // suppress any exceptions
			});
		}

		public void Stop()
		{
			_listener.Stop();
			_listener.Close();
		}
	}
}

