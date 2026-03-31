/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Netboot.Common.Network;
using Netboot.Common.Provider.Events;
using Netboot.Common.System;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace Netboot.Common
{
    public class NetbootBase : IDisposable, IManager
    {
        private Thread _heartBeatThread;

        public static NetworkManager NetworkManager { get; private set; }

        public static Dictionary<string, IProvider>? Providers { get; private set; }

        public Filesystem FileSystem { get; set; }

        private string[] cmdArgs = [];

        public bool Running { get; private set; }

        public static NetbootPlatform Platform = new();

        public NetbootBase(string[] args)
        {
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var title = string.Format("NetBoot {0}.{1}", appVersion.Major, appVersion.Minor);
            Console.Title = title;

            cmdArgs = args;

            _heartBeatThread = new Thread(new ThreadStart(HeartBeat));

            Providers = [];
            Provider.Provider.ModuleLoaded += (sender, e) =>
            {
                Log("I", "Common", string.Format("Loading Module \"{0}\"...", e.Module));
                Providers.Add(e.Name, e.Module);

                foreach (XmlNode xmlnode in e.Xml)
                    if (e.Name == xmlnode.Attributes.GetNamedItem("type").Value)
                        Providers[e.Name]?.Bootstrap(xmlnode);

                var funcs = new List<string>
                {
                    "Install",
                    "Start",
                    "HeartBeat"
                };

                foreach (var item in funcs)
                {
                    Log("I", "Common", string.Format("Sending \"{1}\" command  to \"{0}\"", e.Name, item));
                    Provider.Provider.InvokeMethod<IProvider>(Providers[e.Name], item, []);
                }
            };

            NetworkManager = new NetworkManager();
        }

        public void Start()
        {
            NetworkManager.Start();

            Running = Providers.Count != 0;
            _heartBeatThread.Start();
        }

        public void Stop()
        {
            NetworkManager.Stop();

            foreach (var provider in Providers)
            {
                Provider.Provider.InvokeMethod<IProvider>(provider.Value, "Stop");
                Log("I", provider.Key, "stopped!");
            }
        }

        public void Dispose()
        {
            NetworkManager.Dispose();

            if (Providers.Count != 0)
            {
                foreach (var provider in Providers)
                    Provider.Provider.InvokeMethod<IProvider>(provider.Value, "Dispose");

                Providers.Clear();
                Providers = null;
            }

            try
            {
                _heartBeatThread.Abort();
            }
            catch
            {
            }

        }

        public void Bootstrap(XmlNode xml)
        {
            if (!Platform.Initialize())
            {
                Log("E", "Netboot", "Failed to initialize Platform.");
                return;
            }

            FileSystem = new Filesystem(Platform.NetbootDirectory);
            var ConfigFile = Path.Combine(Platform.ConfigDirectory, "Netboot.xml");

            if (!File.Exists(ConfigFile))
                throw new FileNotFoundException(ConfigFile);

            var xmlFile = new XmlDocument();
            xmlFile.Load(ConfigFile);
            var services = xmlFile.SelectNodes("Netboot/Configuration/Services/Service");


            Provider.Provider.LoadModule(FileSystem.Root, services);

            NetworkManager.Bootstrap(xml);
        }

        public void Close()
        {
            NetworkManager.Close();

            foreach (var provider in Providers)
            {
                Provider.Provider.InvokeMethod<IProvider>(provider.Value, "Close");
                Log("I", provider.Key, "closed!");
            }
        }

        public void HeartBeat()
        {
            Thread.Sleep(30000);
            NetworkManager.HeartBeat();

            foreach (var provider in Providers)
                Provider.Provider.InvokeMethod<IProvider>(provider.Value, "HeartBeat");
        }

        public static void Log(string type, string name, string logmessage)
        {
            var str = "\t" + DateTime.Now.ToString("dd.MM.yyyy : HH:mm:ss", CultureInfo.InvariantCulture)
                + "\tNetboot." + name + ": " + logmessage;

            Console.WriteLine("[{0}] {1}", type, str);
        }
    }
}
