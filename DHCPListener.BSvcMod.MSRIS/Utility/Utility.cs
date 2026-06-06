using DHCPListener.BSvcMod.MSRIS.Utility.Commands;
using Netboot.Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCPListener.BSvcMod.MSRIS.Utility
{
	public partial class Utility : IUtility
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public string Name { get; set; } = "ris";

		public string Description { get; set; } = "Provide Utilities for the Remote Installation Services";

		public void Add(string[] args)
		{
			if (args.Length == 0)
				return;

			switch (args.First())
			{
				case "image":

					if (args.Length == 1)
						return;

					var image = new RisImage(args[1]);
					image.Start();
					break;
				default:
					break;
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
			if (args.Length == 0)
				return;

			switch (args.First())
			{
				case "image":

					break;
				default:
					break;
			}

		}

		public void Modify(string[] args)
		{
			if (args.Length == 0)
				return;

			switch (args.First())
			{
				case "image":

					break;
				default:
					break;
			}

		}

		public void Remove(string[] args)
		{
			if (args.Length == 0)
				return;

			switch (args.First())
			{
				case "image":

					break;
				default:
					break;
			}

		}

		public void Show(string[] args)
		{
			if (args.Length == 0)
				return;

			switch (args.First())
			{
				case "image":

					break;
				default:
					break;
			}

		}

		public void Start()
		{
		}
	}
}
