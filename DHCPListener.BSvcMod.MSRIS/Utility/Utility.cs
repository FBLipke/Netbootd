using Netboot.Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPListener.BSvcMod.MSRIS.Utility
{
    public class Utility : IUtility
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "ris";

        public string Description { get; set; } = "Provide Utilities for the Remote Installation Services";

        public void Add(string[] args)
        {
            foreach (var item in args)
            {
                Console.WriteLine(item);
            }
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public void List(string[] args)
        {
            
        }

        public void Modify(string[] args)
        {
        }

        public void Remove(string[] args)
        {
        }

        public void Show(string[] args)
        {
        }

        public void Start()
        {
        }
    }
}
