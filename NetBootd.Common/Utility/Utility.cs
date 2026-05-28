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

using Netboot.Common.Utility.Commands;

namespace Netboot.Common.Utility
{
    public class Utility : IDisposable
    {
        static NetbootBase? NetbootBase;

        public Utility(string[] args)
        {


            Initialize(args);
        }

        public void Initialize(string[] args)
        {
            NetbootBase = new NetbootBase(args, true);
            NetbootBase.Bootstrap(null);
        }

        public void RunCommand(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Available Commands:");

                return;
            }


            if (args.First().StartsWith('!'))
            {
                var module = args.First().Substring(1);

                if (!NetbootBase.UtilProviders.ContainsKey(module))
                {
                    Usage();
                    return;
                }
                if (args.Length == 1)
                {
                    Console.WriteLine("add: Add an object");
                    Console.WriteLine("remove: remove an object");
                    Console.WriteLine("edit: edit an object");
                    Console.WriteLine("show: show an object");
                    Console.WriteLine("lsi: list the content for an object");

                }
                else
                {
                    switch (args[1])
                    {
                        case "add":
                            NetbootBase.UtilProviders[module].Add(args.SubArray(2, (args.Length - 2)));
                            break;
                        case "remove":
                            NetbootBase.UtilProviders[module].Remove(args.SubArray(2, (args.Length - 2)));
                            break;
                        case "edit":
                            NetbootBase.UtilProviders[module].Modify(args.SubArray(2, (args.Length - 2)));
                            break;
                        case "list":
                            NetbootBase.UtilProviders[module].List(args.SubArray(2, (args.Length - 2)));
                            break;
                        case "show":
                            NetbootBase.UtilProviders[module].Show(args.SubArray(2, (args.Length - 2)));
                            break;
                        default:
                            Usage();
                            return;
                    }
                }
            }
            else
            {
                Usage();
            }
            
            switch (args.First())
            {
                case "!list":
                    foreach (var module in NetbootBase.UtilProviders)
                    {
                        Console.WriteLine(module);
                    }

                    break;
                case "!dist":


                    switch (args[1])
                    {
                        case "add":
                            if (args.Length == 2)
                                return;

                            switch (args[2])
                            {
                                case "ris":
                                    // https://msfn.org/board/topic/127677-txtsetupsif-layoutinf-reference/
                                    if (args.Length == 3)
                                        return;

                                    using (var nt5dist = new NT5DistShare())
                                    {
                                        nt5dist.Initialize(string.Empty, args[2], args[3]);
                                        nt5dist.Start(args[2], args[3]);
                                    }
                                    break;
                                case "osx":
                                    using (var osxdist = new OSXDistShare())
                                    {
                                        osxdist.Start(args[2], args[3]);
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
                case "!test":
                    Console.WriteLine("!test: Netboot tests!!");
                    Console.WriteLine();
                    Console.WriteLine("Syntax: !test [service]");
                    Console.WriteLine("Send test packet to a service!");

                    switch (args[2])
                    {
                        case "dhcpc":

                            break;

                    }

                    break;
            }
        }

        public void Usage()
        {
            Console.WriteLine();
            Console.WriteLine("Syntax: !dist (mode) (type) (Disk ROOT)");

            foreach (var arg in NetbootBase.UtilProviders)
            {
                Console.WriteLine("!{0}: {1}!", arg.Value.Name, arg.Value.Description);
            }
        }

        public void Dispose()
        {
        }
    }
}
