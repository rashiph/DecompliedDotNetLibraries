namespace System.Windows.Forms
{
    using System;
    using System.IO;

    internal static class AutomationMessages
    {
        internal const int PGM_GETBUTTONCOUNT = 0x450;
        internal const int PGM_GETBUTTONSTATE = 0x452;
        internal const int PGM_GETBUTTONTEXT = 0x453;
        internal const int PGM_GETBUTTONTOOLTIPTEXT = 0x454;
        internal const int PGM_GETROWCOORDS = 0x455;
        internal const int PGM_GETSELECTEDROW = 0x457;
        internal const int PGM_GETTESTINGINFO = 0x459;
        internal const int PGM_GETVISIBLEROWCOUNT = 0x456;
        internal const int PGM_SETBUTTONSTATE = 0x451;
        internal const int PGM_SETSELECTEDTAB = 0x458;
        private const int WM_USER = 0x400;

        private static string GenerateLogFileName(ref IntPtr fileId)
        {
            string str = null;
            string environmentVariable = Environment.GetEnvironmentVariable("TEMP");
            if (environmentVariable == null)
            {
                return str;
            }
            if (fileId == IntPtr.Zero)
            {
                Random random = new Random(DateTime.Now.Millisecond);
                fileId = new IntPtr(random.Next());
            }
            return string.Concat(new object[] { environmentVariable, @"\Maui", (IntPtr) fileId, ".log" });
        }

        public static string ReadAutomationText(IntPtr fileId)
        {
            string str = null;
            if (fileId != IntPtr.Zero)
            {
                string path = GenerateLogFileName(ref fileId);
                if (!File.Exists(path))
                {
                    return str;
                }
                try
                {
                    FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    StreamReader reader = new StreamReader(stream);
                    str = reader.ReadToEnd();
                    reader.Dispose();
                    stream.Dispose();
                }
                catch
                {
                    str = null;
                }
            }
            return str;
        }

        public static IntPtr WriteAutomationText(string text)
        {
            IntPtr zero = IntPtr.Zero;
            string path = GenerateLogFileName(ref zero);
            if (path != null)
            {
                try
                {
                    FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(stream);
                    writer.WriteLine(text);
                    writer.Dispose();
                    stream.Dispose();
                }
                catch
                {
                    zero = IntPtr.Zero;
                }
            }
            return zero;
        }
    }
}

