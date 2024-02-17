using System.Buffers.Binary;
using System.ComponentModel.DataAnnotations;

namespace Netboot.Network.Client.RBCP
{

    public class BootMenueEntry
    {
        public ushort Id { get; private set; }
        
        public string Description { get; private set; }
        
        public byte Length { get; private set; }

        public BootMenueEntry(ushort id, string desc)
        {
            Id = id;
            Description = desc;
            Length = (byte)Description.Length;
        }
    }
}
