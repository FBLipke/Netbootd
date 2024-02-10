using Netboot.Network.Server;
using Netboot.Network.Definitions;
using Netboot.Network.Interfaces;
using System.Xml;
using Netboot.Services.Interfaces;
using System.Reflection;
using System.Windows.Input;
using Netboot.Network.Packet;

namespace Netboot
{
    public class NetbootBase : IDisposable
    {
        public static Dictionary<Guid, IServer> Servers = [];
        public static Dictionary<ServerType, IService> Services = [];
        public static Dictionary<Guid, IClient> Clients = [];

        string[] cmdArgs = [];

        public static string WorkingDirectory = Directory.GetCurrentDirectory();

        public NetbootBase(string[] args)
        {
            cmdArgs = args;
        }

        public static void LoadServices()
        {
            var serviceModules = new DirectoryInfo(WorkingDirectory)
                .GetFiles("Netboot.Service.*.dll", SearchOption.AllDirectories);

            foreach (var module in serviceModules)
            {
                var ass = Assembly.LoadFrom(module.FullName);
                foreach (var t in ass.GetTypes())
                {
                    if ((t.IsSubclassOf(typeof(IService)) || t.GetInterfaces().Contains(typeof(IService))) && t.IsAbstract == false)
                    {
                        var b = t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null) as IService;


                        var moduleType = module.Name.Split('.')[2].Trim().ToUpper();

                        if (Enum.TryParse<ServerType>(moduleType, out var serverType))
                            Services.Add(serverType, b);
                    }
                }
            }
        }

        public bool Initialize()
        {
            Console.WriteLine("Netboot 0.1a ({0})", Functions.IsLittleEndian()
                ? "LE (LittleEndian)" : "BE (BigEndian)");

            var ConfigDir = Path.Combine(WorkingDirectory, "Config");
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            if (!Directory.Exists(Path.Combine(WorkingDirectory, "Dump")))
                Directory.CreateDirectory(Path.Combine(WorkingDirectory, "Dump"));

            var tftpRoot = Path.Combine(WorkingDirectory, "TFTPRoot");
            if (!Directory.Exists(Path.Combine(tftpRoot, "Boot")))
                Directory.CreateDirectory(Path.Combine(tftpRoot, "Boot"));

            if (!Directory.Exists(Path.Combine(tftpRoot, "Images")))
                Directory.CreateDirectory(Path.Combine(tftpRoot, "Images"));

            if (!Directory.Exists(Path.Combine(tftpRoot, "OSChooser")))
                Directory.CreateDirectory(Path.Combine(tftpRoot, "OSChooser"));

            #region Parse Config File
            var doc = new XmlDocument();
            doc.Load(Path.Combine(ConfigDir, "Netboot.xml"));

            {
                var serverEntries = doc.SelectNodes("Netboot/Configuration/Network/Server");
                if (serverEntries == null)
                    return false;

                if (serverEntries.Count != 0)
                {
                    foreach (XmlNode serverNode in serverEntries)
                    {
                        if (serverNode == null)
                            continue;

                        var port = serverNode.Attributes?.GetNamedItem("port")?.Value;
                        if (string.IsNullOrEmpty(port))
                        {
                            Console.WriteLine("[E] Server: No Port given!");
                            continue;
                        }

                        if (!ushort.TryParse(port, out var serverPort))
                        {
                            Console.WriteLine("[E] Server: Invalid port given!");
                            continue;
                        }

                        var type = serverNode.Attributes?.GetNamedItem("type")?.Value;
                        if (string.IsNullOrEmpty(type))
                        {
                            Console.WriteLine("[E] Server: No type given!");
                            continue;
                        }

                        if (!Enum.TryParse<ServerType>(type.ToUpper(), out var serverType))
                        {
                            Console.WriteLine("[E] Server: Invalid Argument for type!");
                            continue;
                        }

                        Add(serverType, serverPort);
                    }
                }
            }
            #endregion

            LoadServices();

            return true;
        }

        public void Start()
        {
            foreach (var server in Servers)
                server.Value.Start();
        }

        public void Add(ServerType serverType, ushort port)
        {
            var serverId = Guid.NewGuid();
            var server = new BaseServer(serverId, serverType, port);
            server.DataSent += (sender, e) => { };

            server.DataReceived += (sender, e) =>
            {
                switch (e.ServerType)
                {
                    case ServerType.DHCP:
                    case ServerType.BSDP:
                    case ServerType.BOOTP:
                        var functionName = e.ServerType;
                        Functions.InvokeMethod(Services[e.ServerType], $"Handle_{functionName}_Discover",
                            new object[] { e.ServerType, e.ServerId, e.SocketId, e.Packet });
                        break;
                    case ServerType.TFTP:
                        break;
                    case ServerType.HTTP:
                        break;
                    default:
                        break;
                }

               
            };

            Servers.Add(serverId, server);
        }

        public void Stop()
        {
            foreach (var server in Servers)
                server.Value.Stop();
        }

        public void Dispose()
        {
            foreach (var server in Servers)
                server.Value.Dispose();
        }
    }
}
