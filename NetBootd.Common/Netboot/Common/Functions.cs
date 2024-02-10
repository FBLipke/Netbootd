using System.Net.NetworkInformation;
using System.Net;

namespace Netboot
{
    public static class Functions
    {
        public static void InvokeMethod(object obj, string name, object?[]? args)
        {
            try
            {
                var methods = obj.GetType().GetMethods().Where(m => m.Name == name && m.IsPublic);
                methods.FirstOrDefault().Invoke(obj, args);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static IEnumerable<IPAddress> GetIPAddresses()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    if (!IPAddress.IsLoopback(ip.Address) && ip.Address.GetAddressBytes()[0] != 0xa9)
                        yield return ip.Address;
        }

        public static bool IsLittleEndian() => BitConverter.IsLittleEndian;
    }
}
