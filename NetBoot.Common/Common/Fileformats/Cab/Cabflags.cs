namespace Netboot.Common.FileFormats
{
	public enum Cabflags : ushort
	{
		None = 0x0000,
		Prev = 0x0001,
		Next = 0x0002,
		Reserved = 0x0004
	}
}
