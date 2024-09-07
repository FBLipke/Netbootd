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

using Netboot.Common.Definitions;

namespace Netboot
{
    public class NetbootPlatform
    {
        public string ConfigDirectory { get; private set; }

        public string NetbootDirectory { get; private set; }

        public string TFTPRoot { get; private set; }

        public string DirectorySeperatorChar { get; private set; }

        public OSPlatformId OSPlatform { get; private set; }

        public bool Initialize()
        {
            if (OperatingSystem.IsWindows())
                OSPlatform = OSPlatformId.Windows;
            else if (OperatingSystem.IsLinux())
                OSPlatform = OSPlatformId.Linux;
            else if (OperatingSystem.IsIOS())
                OSPlatform = OSPlatformId.Ios;
            else if (OperatingSystem.IsAndroid())
                OSPlatform = OSPlatformId.Android;
            else if (OperatingSystem.IsMacOS())
                OSPlatform = OSPlatformId.MacOS;
            else if (OperatingSystem.IsFreeBSD())
                OSPlatform = OSPlatformId.FreeBSD;
            else
                return false;

            NetbootDirectory = Path.Combine(Directory.GetCurrentDirectory());
            TFTPRoot = Path.Combine(NetbootDirectory, "TFTPRoot");
            ConfigDirectory = Path.Combine(NetbootDirectory, "Config");

            Directory.CreateDirectory(Path.Combine(TFTPRoot,"Setup"));
            Directory.CreateDirectory(Path.Combine(TFTPRoot, "tmp"));

            switch (OSPlatform)
            {
                case OSPlatformId.Windows:
                    DirectorySeperatorChar = "\\";
                    break;
                case OSPlatformId.FreeBSD:
                case OSPlatformId.Android:
                case OSPlatformId.Linux:
                    DirectorySeperatorChar = "/";
                    break;
                case OSPlatformId.MacOS:
                case OSPlatformId.Ios:
                default:
                    return false;
            }

            return true;
        }
    }
}
