namespace System.Web.UI
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RenderStyle
    {
        public string name;
        public string value;
        public HtmlTextWriterStyle key;
    }
}

