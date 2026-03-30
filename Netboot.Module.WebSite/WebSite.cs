using Netboot.Common;
using Netboot.Common.Cryptography;
using Netboot.Common.Cryptography.Interfaces;
using Netboot.Common.Database.Interfaces;
using Netboot.Common.Network.HTTP;
using Netboot.Common.Network.Sockets;
using Netboot.Common.Provider;
using Netboot.Common.Provider.Events;
using Netboot.Common.System;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Netboot.Module
{
	public class Website : IProvider, IManager
	{
		public Dictionary<Guid, IMember> Members { get; set; }

		public bool VolativeModule { get; set; } = true;

		public bool CanEdit { get; set; }

		public string FriendlyName { get; set; } = "WebSite";

		public string Description { get; set; } = "Stellt einen rudimentären ICY Media Stream bereit.";

		public bool CanAdd { get; set; }

		public bool CanRemove { get; set; }

		public static SiteSettings WebSiteSettings { get; set; } = new SiteSettings();

		public bool IsPublicModule { get; set; } = false;

		public bool Active { get; set; }

		public IDatabase Database { get; set; }
		public ICrypto Crypt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public Filesystem Filesystem { get; set; }

		public Website()
		{
			Members = [];
			WebSiteSettings = new SiteSettings();
			Filesystem = new Filesystem("Providers\\WebSite");
		}

		public void Bootstrap(XmlNode xml)
		{
			NetbootBase.NetworkManager.ServerManager
				.Add(ProtoType.Tcp, new List<ushort>(90));

			NetbootBase.NetworkManager.HTTPRequestReceived += (sender, e) =>
			{
				Handle_HTTP_Request(e.Server, e.Socket, e.Client, e.Context);
			};
		}

		public void Close()
		{

		}

		public bool Contains(Guid id) => false;

		public void Dispose()
		{
			Members.Clear();
			Members = null;
		}

		public IMember Get_Member(Guid id) => (IMember)null;

		private string Build_Admin_Navigation(bool loggedin = true)
		{
			var stringBuilder = new StringBuilder("<nav>\n<div class=\"navbar\">\n");
			stringBuilder.Append("<a href=\"/\">Startseite</a>\n");
			foreach (var provider in NetbootBase.Providers)
			{
				if (loggedin)
				{
					stringBuilder.AppendFormat("<div class=\"subnav\">\n<button class=\"subnavbtn\" onclick=\"RequestJSON('{1}', '{2}', 'Get');\">{0}</button>\n", provider.Key, provider.Key.ToLower(), Guid.NewGuid());
					stringBuilder.Append("<div class=\"subnav-content\">\n");

					if (Provider.GetPropertyValue<bool>(provider.Value, "CanAdd"))
						stringBuilder.AppendFormat("<a href=\"#Add\" onclick=\"RequestJSON('{0}', '{1}', 'Add');\">Erstellen</a>\n", provider.Key.ToLower(), Guid.NewGuid());

					if (Provider.GetPropertyValue<bool>(provider.Value, "CanEdit"))
						stringBuilder.AppendFormat("<a href=\"#Edit\" onclick=\"RequestJSON('{0}', '{1}', 'Edit');\">Bearbeiten</a>\n", provider.Key.ToLower(), Guid.NewGuid());

					if (Provider.GetPropertyValue<bool>(provider.Value, "CanRemove"))
						stringBuilder.AppendFormat("<a href=\"#Remove\" onclick=\"RequestJSON('{0}', '{1}', 'Remove');\">Entfernen</a>\n", provider.Key.ToLower(), Guid.NewGuid());

					stringBuilder.AppendFormat("</div>\n</div>\n");
				}
			}
			stringBuilder.Append("</div>\n</nav>\n");
			return stringBuilder.ToString();
		}

		private string Admin_Main(bool loggedin)
		{
			var stringBuilder = new StringBuilder("<main id=\"main\">\n");
			stringBuilder.Append("</main>\n");

			return stringBuilder.ToString();
		}

		public static string Build_Header(bool loggedin, bool redirect)
		{
			var stringBuilder = new StringBuilder("<header>\n");
			stringBuilder.Append("<div id=\"logo\">\n");
			//if (FileSystem.Exists("public/images/logo.png")) stringBuilder.Append("<a href=\"/\">\n<img src=\"/public/images/logo.png\" alt=\"Logo\">\n</a>\n");
			stringBuilder.Append("</div>\n");
			if (!redirect)
				stringBuilder.Append(Build_navigation(loggedin));
			stringBuilder.Append("</header>\n");
			return stringBuilder.ToString();
		}

		private string Handle_Site_Request(bool loggedin, string path, HttpRequest request)
		{
			if (path is null)
				throw new ArgumentNullException(nameof(path));

			var stringBuilder = new StringBuilder("<!DOCTYPE html>\n<html lang=\"de\">\n");
			stringBuilder.Append("<head>\n");
			stringBuilder.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
			stringBuilder.Append("<meta name=\"robots\" content=\"noindex,nofollow\">");
			stringBuilder.Append("<meta charset=\"utf-8\">\n");
			stringBuilder.AppendFormat("<title>{0}</title>\n", WebSiteSettings.SiteTitle);
			stringBuilder.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n");
			stringBuilder.AppendFormat("<link rel='stylesheet' type='text/css' href='/{0}/styles/{1}/style.css' />\n",
				request.Path.EndsWith("/admin/") ? "admin" : "public", WebSiteSettings.DefaultDesign);
			stringBuilder.Append("</head>\n");
			stringBuilder.Append("<body>\n");
			if (!request.Path.EndsWith("admin/"))
			{
				stringBuilder.Append(Build_Header(loggedin, false));
				stringBuilder.Append(Build_Body(loggedin));
			}
			else
			{
				stringBuilder.Append(Build_Admin_Header(loggedin));
				stringBuilder.Append(Admin_Main(loggedin));
			}
			stringBuilder.Append(Build_Footer());
			stringBuilder.Append("</body>\n");
			stringBuilder.Append("<script type=\"text/javascript\" src=\"/scripts/jquery-1.12.4.min.js\"></script>\n");
			stringBuilder.Append("<script type=\"text/javascript\" src=\"/scripts/dropdown.js\"></script>\n");
			stringBuilder.Append("<script type=\"text/javascript\" src=\"/scripts/netboot.js\"></script>\n");
			stringBuilder.Append("</html>\n");
			return stringBuilder.ToString();
		}

		public void Handle_HTTP_Request(Guid server, Guid socket, Guid client, NetbootHttpContext context)
		{
			var num = 200;
			var str1 = "OK";
			var loggedin = context.Request.User != null && context.Request.HasCookie("UserId");
			var dateTime = DateTime.Now;
			var str2 = dateTime.ToString("ddd, MMM dd yyyy HH:mm:ss 'GMT'K", CultureInfo.InvariantCulture);
			byte[] data;
			if (context.Request.Path.StartsWith("/provider/"))
			{
				data = Handle_Provider_Request(context).GetBytes_UTF8();
			}
			else
			{
				switch (context.Request.Path)
				{
					case "/login/":
                        data = Handle_Redirect_Request(Provider.InvokeMethod<Member>(Provider.CanDo("Login").FirstOrDefault(), "Handle_Login_Request",
						[
			  context
						]) != null, "/").GetBytes_UTF8();
						break;
					case "/logout/":
						Provider.InvokeMethod<Member>(Provider.CanDo("Login").FirstOrDefault(), "Handle_Logout_Request",
						[
			  context
						]);
						data = Handle_Redirect_Request(false, "/").GetBytes_UTF8();
						break;
					case "/admin/":
					case "/":
						data = Handle_Site_Request(loggedin, context.Request.Path, context.Request).GetBytes_UTF8();
						break;
					default:
						if (!Filesystem.Exists(context.Request.Path))
						{
							data = new byte[0];
							num = 404;
							str1 = "Not found!";
							break;
						}
						data = Filesystem.Read(context.Request.Path);
						dateTime = Filesystem.GetCreationDate(context.Request.Path);
						str2 = dateTime.ToString("ddd, MMM dd yyyy HH:mm:ss 'GMT'K", CultureInfo.InvariantCulture);
						break;
				}
			}

			var response = new HttpResponse(context.Request);
			if (context.Request.Headers.ContainsKey("Connection"))
				response.KeepAlive = context.Request.Headers["Connection"] == "keep-alive";

			response.Headers.Add("Last-Modified", str2);
			response.Status = num;
			response.Description = str1;
			response.GenerateResponseHeader(ref data, context.Request);


			NetbootBase.NetworkManager.ServerManager.Send(server, socket, client, response.Content, response.KeepAlive);
			response.Dispose();
		}

		private string Handle_Provider_Request(NetbootHttpContext context)
		{
			var strArray = context.Request.Path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var key = string.Empty;
			var str = string.Empty;

			if (strArray.Length > 1)
			{
				key = strArray[1].Captitalize();
				str = strArray[2].Captitalize();
			}
			else
			{
				if (context.Request.Headers.ContainsKey("Provider"))
					key = Base64.FromBase64(context.Request.Headers["Provider"], Encoding.UTF8).Captitalize();
				if (context.Request.Headers.ContainsKey("Action"))
					str = Base64.FromBase64(context.Request.Headers["Action"],Encoding.UTF8).Captitalize();
			}
			if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(str))
				return string.Empty;

			if (!NetbootBase.Providers.ContainsKey(key))
				return "<p class=\"error_text\">Der angegebene Provider ist nicht verfügbar</p>";

			return Provider.InvokeMethod<string>(NetbootBase.Providers[key], "Handle_" + str + "_Request", new object[1]
			{
		 context
			});
		}

		public void HeartBeat()
		{
		}

		public string Handle_Redirect_Request(bool loggedin, string redirectTo, string content = "")
		{
			var stringBuilder = new StringBuilder("<!DOCTYPE html>\n<html lang=\"de\">\n");
			stringBuilder.Append("<head>\n");
			stringBuilder.AppendFormat("<meta http-equiv=\"refresh\" content=\"{0}; URL={1}\">\n", WebSiteSettings.RefreshInterval, redirectTo);
			stringBuilder.Append("<meta charset=\"utf-8\">\n");
			stringBuilder.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n");
			stringBuilder.Append("<head>\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n");
			stringBuilder.AppendFormat("<title>{0}</title>\n", WebSiteSettings.SiteTitle);
			stringBuilder.AppendFormat("<link rel='stylesheet' type='text/css' href='/public/styles/{0}/style.css' />\n", WebSiteSettings.DefaultDesign);
			stringBuilder.AppendFormat("</head>\n");
			stringBuilder.Append("<body>\n");
			stringBuilder.Append(Build_Header(loggedin, true));
			stringBuilder.Append("<main id=\"main\">\n<hr>");
			stringBuilder.Append("<section id=\"home\">");
			stringBuilder.AppendFormat("<p>{0}, Du wirst gleich weitergeleitet.</p>\n", content);
			stringBuilder.Append("</section></main></body>\n");
			stringBuilder.Append(Build_Footer());
			stringBuilder.Append("</html>\n");

			return stringBuilder.ToString();
		}

		public void Install()
		{
		}

		private string Build_Admin_Header(bool loggedin)
		{
			var stringBuilder = new StringBuilder("<header>\n<h3>Allgemein</h3>\n");
			stringBuilder.Append(Build_Admin_Navigation(loggedin));
			stringBuilder.Append("</header>\n");
			return stringBuilder.ToString();
		}

		public static string Build_navigation(bool loggedin)
		{
			var stringBuilder = new StringBuilder("<nav>\n");
			stringBuilder.AppendFormat("<img class=\"nav-icon-bars\" src=\"/public/styles/{0}/images/nav-icon.png\" alt=\"Mobile nav icon\">\n", WebSiteSettings.DefaultDesign);
			stringBuilder.Append("<ul>\n");
			stringBuilder.Append("<a href=\"/\" title=\"Zur Startseite\"><li>Startseite</li></a>\n");
			if (loggedin)
				stringBuilder.Append("<a href=\"/admin/\" title=\"Verwaltungscenter\"><li>Einstellungen</li></a>\n");
			lock (NetbootBase.Providers)
			{
				foreach (var provider in NetbootBase.Providers)
				{
					if (provider.Value.Members.Count != 0)
					{
						var propertyValue1 = Provider.GetPropertyValue<string>(provider.Value, "FriendlyName");
						var propertyValue2 = Provider.GetPropertyValue<string>(provider.Value, "Description");

						if (Provider.GetPropertyValue<bool>(provider.Value, "IsPublicModule"))
							stringBuilder.AppendFormat("<a href=\"#\" onclick=\"RequestHTML('{0}', 'get', '', 'Get');\" title=\"{1}\"><li>{2}</li></a>\n",
								provider.Key.ToLower(), propertyValue2, propertyValue1);
					}
				}
				if (loggedin)
					stringBuilder.AppendFormat("<a href=\"/logout/\" title=\"Die Sitzung beenden\"><li>Abmelden</li></a>\n");
			}
			stringBuilder.Append("</ul>\n");
			stringBuilder.Append("</nav>\n");
			return stringBuilder.ToString();
		}
		private string Build_Login_Form()
		{
			var stringBuilder = new StringBuilder();

			if (NetbootBase.Providers.ContainsKey("User"))
			{
				stringBuilder.Append(NetbootBase.Providers["User"].Filesystem.Read("template/User_Login_Form.tpl").GetString(Encoding.UTF8));

				if (WebSiteSettings.AllowRegistration)
					stringBuilder.Append("<a href=\"#\" onclick=\"RequestHTML('user', 'add', '', 'Get');\"" +
						" title=\"Einen Account erstellen\">Account erstellen</a>\n");
			}

			return stringBuilder.ToString();
		}

		private string Build_Body(bool loggedin)
		{
			var stringBuilder = new StringBuilder("<main id=\"main\">\n");
			stringBuilder.Append("<section id=\"home\">\n");
			stringBuilder.Append("<hr>\n");
			stringBuilder.AppendFormat("<h1>{0}</h1>\n", WebSiteSettings.SiteTitle);
			stringBuilder.AppendFormat("<h2>{0}</h2>\n", WebSiteSettings.SiteSlogan);
			if (!loggedin)
				stringBuilder.Append(Build_Login_Form());
			stringBuilder.Append("</section>\n");
			stringBuilder.Append("<div id=\"disk\"></div>");
			stringBuilder.Append("<div id=\"disktoast\"><p>[#__0x00400000___#]</p></div>");
			stringBuilder.Append("</main>\n");
			return stringBuilder.ToString();
		}

		public static string Build_Footer()
		{
			var stringBuilder = new StringBuilder();
			return stringBuilder.ToString();
		}

		public void Remove(Guid id)
		{
		}

		public IMember Request(Guid id) => (IMember)null;

		public void Start() => HeartBeat();

		public void Stop() => HeartBeat();

		public string Handle_Get_Request(NetbootHttpContext context)
		{
			throw new NotImplementedException();
		}

		public string Handle_Add_Request(NetbootHttpContext context)
		{
			throw new NotImplementedException();
		}

		public string Handle_Edit_Request(NetbootHttpContext context)
		{
			throw new NotImplementedException();
		}

		public string Handle_Remove_Request(NetbootHttpContext context)
		{
			throw new NotImplementedException();
		}

		public string Handle_Info_Request(NetbootHttpContext context)
		{
			throw new NotImplementedException();
		}

		public class SiteSettings
		{
			public string SiteTitle { get; set; } = "Netboot";

			public bool AllowRegistration { get; set; } = true;

			public string SiteSlogan { get; set; } = "Entwickler Version (Server & Socket - Test)";

			public int RefreshInterval { get; } = 5;

			public string DefaultDesign { get; } = "default";
		}
	}
}
