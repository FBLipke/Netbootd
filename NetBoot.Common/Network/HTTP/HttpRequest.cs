using Netboot.Common.Provider.Events;
using System.Net;
using System.Text;

namespace Netboot.Common.Network.HTTP
{
    public class HttpRequest : IDisposable
    {
        public HttpRequest()
        {
            Headers = [];
            Cookies = [];
            Parameters = [];
            RemEndpoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public string Create()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0} {1} {2}\n\r", Method, Path, Version);

            foreach (var header in Headers.ToList())
                stringBuilder.AppendFormat("{0}:{1}\n\r", header.Key, header.Value);

            stringBuilder.Append("\n\r");
            return stringBuilder.ToString();
        }

        public bool HasCookie(string name) => Cookies.Where(c => c.Name == name).FirstOrDefault() != null;

        public void SetCookie(HttpCookie cookie) => Cookies.Add(cookie);

        public HttpCookie? Get_Cookie(string name) => Cookies?.FirstOrDefault(c => c.Name == name);

        public Dictionary<string, string> Headers { get; set; }

        public List<HttpCookie> Cookies { get; set; }

        public IPEndPoint RemEndpoint { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public string Method { get; set; } = null;

        public string Path { get; set; } = null;

        public string Version { get; set; } = null;

        public IMember User { get; set; } = null;

        public MemoryStream Content { get; set; }

        public void Dispose()
        {
            Headers.Clear();
            Cookies.Clear();
            if (Content == null)
                return;

            Content.Dispose();
        }

        public new string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Method: {0}{1}", Method, Environment.NewLine);
            stringBuilder.AppendFormat("Path: {0}{1}", Path, Environment.NewLine);
            stringBuilder.AppendFormat("Version: {0}{1}", Version, Environment.NewLine);
            stringBuilder.AppendFormat("Headers:{0}", Environment.NewLine);

            if (Headers.Count == 0)
                stringBuilder.AppendFormat("No headers in this Request... {0}", Environment.NewLine);

            foreach (var header in Headers.ToList())
                stringBuilder.AppendFormat("Name: {0}{1}Value: {2}{1}", header.Key, Environment.NewLine, header.Value);

            stringBuilder.AppendFormat("Cookies:{0}", Environment.NewLine);

            if (Cookies.Count == 0)
                stringBuilder.AppendFormat("No cookies in this Request... {0}", Environment.NewLine);

            foreach (var cookie in Cookies.ToList())
                stringBuilder.AppendFormat("Name: {0}{1}Value: {2}{1}", cookie.Name, Environment.NewLine, cookie.Value);

            if (Parameters.Count == 0)
                stringBuilder.AppendFormat("No parameters in this Request... {0}", Environment.NewLine);

            foreach (var parameter in Parameters.ToList())
                stringBuilder.AppendFormat("Name: {0}{1}Value: {2}{1}", parameter.Key, Environment.NewLine, parameter.Value);

            return stringBuilder.ToString();
        }
    }
}
