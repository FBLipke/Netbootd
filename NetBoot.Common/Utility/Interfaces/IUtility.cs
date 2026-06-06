using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netboot.Common.Utility
{
	public interface IUtility : IDisposable
	{
		Guid Id { get; set; }

		string Name { get; set; }

		string Description { get; set; }

		void Initialize();

		void Start();

		void Show(string[] args);

		void List(string[] args);

		void Add(string[] args);

		void Remove(string[] args);

		void Modify(string[] args);
	}
}
