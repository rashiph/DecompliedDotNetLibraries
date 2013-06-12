namespace System.Diagnostics.SymbolStore
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class SymLanguageType
    {
        public static readonly Guid Basic = new Guid(0x3a12d0b8, -15764, 0x11d0, 180, 0x42, 0, 160, 0x24, 0x4a, 0x1d, 210);
        public static readonly Guid C = new Guid(0x63a08714, -969, 0x11d2, 0x90, 0x4c, 0, 0xc0, 0x4f, 0xa3, 2, 0xa1);
        public static readonly Guid Cobol = new Guid(-1358664495, -12063, 0x11d2, 0x97, 0x7c, 0, 160, 0xc9, 180, 0xd5, 12);
        public static readonly Guid CPlusPlus = new Guid(0x3a12d0b7, -15764, 0x11d0, 180, 0x42, 0, 160, 0x24, 0x4a, 0x1d, 210);
        public static readonly Guid CSharp = new Guid(0x3f5162f8, 0x7c6, 0x11d3, 0x90, 0x53, 0, 0xc0, 0x4f, 0xa3, 2, 0xa1);
        public static readonly Guid ILAssembly = new Guid(-1358664493, -12063, 0x11d2, 0x97, 0x7c, 0, 160, 0xc9, 180, 0xd5, 12);
        public static readonly Guid Java = new Guid(0x3a12d0b4, -15764, 0x11d0, 180, 0x42, 0, 160, 0x24, 0x4a, 0x1d, 210);
        public static readonly Guid JScript = new Guid(0x3a12d0b6, -15764, 0x11d0, 180, 0x42, 0, 160, 0x24, 0x4a, 0x1d, 210);
        public static readonly Guid MCPlusPlus = new Guid(0x4b35fde8, 0x7c6, 0x11d3, 0x90, 0x53, 0, 0xc0, 0x4f, 0xa3, 2, 0xa1);
        public static readonly Guid Pascal = new Guid(-1358664494, -12063, 0x11d2, 0x97, 0x7c, 0, 160, 0xc9, 180, 0xd5, 12);
        public static readonly Guid SMC = new Guid(0xd9b9f7b, 0x6611, 0x11d3, 0xbd, 0x2a, 0, 0, 0xf8, 8, 0x49, 0xbd);
    }
}

