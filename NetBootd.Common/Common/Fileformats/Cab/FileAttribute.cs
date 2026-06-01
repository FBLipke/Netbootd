namespace Netboot.Common.FileFormats
{
    public enum FileAttribute : byte
    {
        ReadOnly = 0x01,
        Hidden = 0x02,
        System = 0x04,
        Archive = 0x20,
        Execute = 0x40,
        NameIsUTF = 0x80
    }
}
