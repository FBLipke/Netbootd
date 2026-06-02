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

namespace Netboot.Common.Common.Definitions
{


    public enum Architecture : ushort
    {
        X86PC = 0,
        NECPC98,
        EFIItanium,
        DECAlpha,
        Arcx86,
        IntelLeanClient,
        EFI_IA32,
        EFIByteCode,
        EFI_xScale,
        EFI_x8664,
        ARM32_EFI,
        ARM64_EFI,
        PowerPCOpenFW,
        PowerPCePAPR,
        PowerOpalV3,
        X86EfiHttp,
        X64EfiHttp,
        EfiHttp,
        Arm32EfiHttp,
        Arm64EfiHttp,
        PCBiosHttp,
        Arm32Uboot,
        Arm64UBoot,
        Arm32UbootHttp,
        Arm64UbootHttp,
        RiscV32EFi,
        RiscV32EFiHttp,
        RiscV64EFi,
        RiscV64EFiHttp,
        RiscV128Efi,
        RiscV128EfiHttp,
        S390Basic,
        S390Extended,
        MIPS32Efi,
        MIPS64Efi,
        SunWay32Efi,
        SunWay64Efi,
        LoongArch32Efi,
        LoongArch32EfiHttp,
        LoongArch64Efi,
        LoongArch64EfiHttp,
        ArmRPIBoot
    }
    

    public enum OSPlatformId : byte
    {
        Windows = 0,
        Linux = 1,
        MacOS = 2,
        Ios = 3,
        Android = 4,
        FreeBSD = 5
    }

    public enum EndianessBehavier : byte
    {
        LittleEndian,
        BigEndian
    }

}
