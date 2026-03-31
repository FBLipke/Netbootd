using Netboot.Common.Cryptography.Interfaces;
using Netboot.Common.Database.Interfaces;
using Netboot.Common.Network.HTTP;
using Netboot.Common.System;

namespace Netboot.Common.Provider.Events
{
    public interface IProvider : IManager
    {
        bool VolativeModule { get; set; }

        Dictionary<Guid, IMember> Members { get; set; }
        ICrypto Crypt { get; set; }

        IDatabase Database { get; set; }

        Filesystem Filesystem { get; set; }

        bool CanEdit { get; set; }

        string FriendlyName { get; set; }

        string Description { get; set; }

        bool IsPublicModule { get; set; }

        bool CanAdd { get; set; }

        bool CanRemove { get; set; }

        void Remove(Guid id);

        bool Contains(Guid id);

        void Install();

        string Handle_Get_Request(NetbootHttpContext context);

        string Handle_Add_Request(NetbootHttpContext context);

        string Handle_Edit_Request(NetbootHttpContext context);

        string Handle_Remove_Request(NetbootHttpContext context);

        string Handle_Info_Request(NetbootHttpContext context);

        string Handle_Redirect_Request(bool loggedin, string redirectTo, string content = "");

        IMember Request(Guid id);

        bool Active { get; set; }
    }
}
