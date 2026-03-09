// Decompiled with JetBrains decompiler
// Type: Netboot.Common.ILogin
// Assembly: Netboot.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CE4FCADF-C52D-4962-B4B8-C6D36FAB8FAE
// Assembly location: C:\Users\LipkeGu\Desktop\Netboot___\Netboot.Common.dll

using Netboot.Common.Network;
using Netboot.Common.Network.HTTP;
using Netboot.Common.Provider.Events;

namespace Netboot.Common.System
{
	public interface ILogin
	{
		IMember Handle_Login_Request(NetbootHttpContext request);

		IMember Handle_Logout_Request(NetbootHttpContext request);
	}
}
