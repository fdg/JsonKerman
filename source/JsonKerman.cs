/*
Copyright (c) 2014 David Laurie

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using UnityEngine;
using KSP.IO;

namespace JsonKerman
{
	/**
	 * The main object launches a webserver for the duration of the game.
	 * For each request we'll query the gamestate (or cache things) and 
	 */
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class JsonKerman : MonoBehaviour
	{
		private WebServer server = null;

		private static Dictionary<string, string> MimeTypesForExtensions = new Dictionary<string, string>
		{
			{ ".html", "text/html" },
			{ ".css", "text/css" },
			{ ".js", "application/javascript" },
			{ ".jpg", "image/jpeg" },
			{ ".jpeg", "image/jpeg" },
			{ ".png", "image/png" },
			{ ".gif", "image/gif" },
			{ ".svg", "image/svg+xml" }
		};

		/**
		 * Constructor mostly unused.
		 */
		public JsonKerman()
		{
			Debug.Log("KrimZon JsonKerman init, expects web files in " + IOUtils.GetFilePathFor(this.GetType(), "web/"));
		}

		~JsonKerman()
		{
		}

		/*
		 * Called after the scene is loaded.
		 */
		void Awake()
		{
			if (server == null)
			{
				// TODO: Read this list from config.
				string[] prefixes = new string[1];
				prefixes[0] = "http://*:7001/";
				server = new WebServer(prefixes, this.SendResponse);
				server.Run();
				Debug.Log("KrimZon JsonKerman web server started");
			}
		}

		/*
		 * Called next.
		 */
		void Start()
		{
		}

		/*
		 * Called every frame
		 */
		void Update()
		{
		}

		/*
		 * Called at a fixed time interval determined by the physics time step.
		 */
		void FixedUpdate()
		{
		}

		/*
		 * Called when the game is leaving the scene (or exiting). Perform any clean up work here.
		 */
		void OnDestroy()
		{
			if (server != null)
			{
				server.Stop();
				server = null;
				Debug.Log("KrimZon JsonKerman web server stopped");
			}
		}

		/**
		 * Callback from the webserver.
		 */
		private WebServer.Response SendResponse(HttpListenerRequest request)
		{
			Debug.Log("KrimZon JsonKerman " + request.RemoteEndPoint.Address.ToString() + " " + request.HttpMethod + " " + request.Url.ToString());

			// For security we limit only to requests from private address spaces by default.
			// TODO: Disable this with a config option.
			IPEndPoint endpoint = request.RemoteEndPoint;
			if (!IsPrivateOrLocal(endpoint.Address))
			{
				Debug.Log("KrimZon JsonKerman blocked access from " + endpoint.Address.ToString());
				ServeForbidden();
			}

			if (request.Url.LocalPath.Equals("/jebservice.ksp"))
			{
				return ServeAPI(request.Url.Query);
			}

			if (request.Url.LocalPath.Equals("/"))
			{
				return ServeFile("/index.html");
			}

			return ServeFile(request.Url.LocalPath);
		}

		/**
		 * Returns JSON data for the entire game state.
		 */
		private WebServer.Response ServeAPI(string queryString)
		{
			GameData data = new GameData();
			//NameValueCollection query = HttpUtility.ParseQueryString(queryString);
			// TODO: Read query to determine what data is required from gamedata.
			return new WebServer.Response(data.ToJson(), "application/json");
		}

		private WebServer.Response ServeFile(string path)
		{
			string filename = path.TrimStart(new char[]{'/'});
			if (!Regex.IsMatch(filename, "^[\\w-\\.]+$"))
			{
				return ServeNotFound(path);
			}

			filename = IOUtils.GetFilePathFor(this.GetType(), filename);
			if (!KSP.IO.File.Exists<JsonKerman>(filename))
			{
				return ServeNotFound(path + " (" + filename + ")");
			}

			string extension = Path.GetExtension(filename);
			string mimeType = "text/html";
			MimeTypesForExtensions.TryGetValue(extension.ToLower(), out mimeType);

			WebServer.Response response = new WebServer.Response(filename, mimeType);
			response.StreamFile = true;
			return response;
		}

		private WebServer.Response ServeForbidden()
		{
			WebServer.Response response = new WebServer.Response("Forbidden", "text/html");
			response.StatusCode = 403;
			response.StatusDescription = "Forbidden";
			return response;
		}

		private WebServer.Response ServeNotFound(string path)
		{
			WebServer.Response response = new WebServer.Response("The given path, " + path + ", could not be found" , "text/html");
			response.StatusCode = 404;
			response.StatusDescription = "Not Found";
			return response;
		}

		/**
		 * Returns true if the given IPAddress is in a private or local address range.
		 */
		private bool IsPrivateOrLocal(IPAddress address)
		{
			// TODO: Also account for ipv6.

			byte[] addressBytes = address.GetAddressBytes();
			// 192.168.0.0/16
			if (addressBytes[0] == 192 && addressBytes[1] == 168)
			{
				return true;
			}
			// 127.0.0.0/8 10.0.0.0/8
			if (addressBytes[0] == 127 || addressBytes[0] == 10)
			{
				return true;
			}
			// 172.16.0.0/12
			if (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] < 32)
			{
				return true;
			}
			return false;
		}
	}
}
