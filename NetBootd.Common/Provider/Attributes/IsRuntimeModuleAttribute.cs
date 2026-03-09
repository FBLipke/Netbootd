// Decompiled with JetBrains decompiler
// Type: Netboot.Module.IsRuntimeModuleAttribute
// Assembly: Netboot.Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CE4FCADF-C52D-4962-B4B8-C6D36FAB8FAE
// Assembly location: C:\Users\LipkeGu\Desktop\Netboot___\Netboot.Common.dll

using System;

namespace Netboot.Common.Provider.Attributes
{
	public class IsRuntimeModuleAttribute : Attribute
	{
		public IsRuntimeModuleAttribute()
			=> NetbootBase.Log("I", "Netboot.ModuleInstaller",
				"This Module does not need any Installation Routines!");
	}
}
