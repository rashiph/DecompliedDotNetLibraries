namespace System.Windows.Forms.VisualStyles
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    internal struct VisualStyleDocProperty
    {
        internal static string DisplayName;
        internal static string Company;
        internal static string Author;
        internal static string Copyright;
        internal static string Url;
        internal static string Version;
        internal static string Description;
        static VisualStyleDocProperty()
        {
            DisplayName = "DisplayName";
            Company = "Company";
            Author = "Author";
            Copyright = "Copyright";
            Url = "Url";
            Version = "Version";
            Description = "Description";
        }
    }
}

