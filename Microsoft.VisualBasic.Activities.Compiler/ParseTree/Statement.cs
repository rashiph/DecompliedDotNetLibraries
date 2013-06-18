namespace ParseTree
{
    using Microsoft.VisualC;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=40), MiscellaneousBits(0x41), DebugInfoInPDB, UnsafeValueType, NativeCppClass]
    internal struct Statement
    {
        [MiscellaneousBits(0x40), DebugInfoInPDB, NativeCppClass, CLSCompliant(false)]
        public enum Opcodes
        {
        }
    }
}

