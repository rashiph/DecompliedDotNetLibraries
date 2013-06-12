namespace System.Windows.Forms.VisualStyles
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    internal struct VisualStyleSystemProperty
    {
        internal static int SupportsFlatMenus;
        internal static int MinimumColorDepth;
        static VisualStyleSystemProperty()
        {
            SupportsFlatMenus = 0x3e9;
            MinimumColorDepth = 0x515;
        }
    }
}

