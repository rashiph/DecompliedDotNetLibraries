namespace System.Drawing.Design
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string bitmapFileDescription = "bitmapFileDescription";
        internal const string ColorEditorAccName = "ColorEditorAccName";
        internal const string ColorEditorPaletteTab = "ColorEditorPaletteTab";
        internal const string ColorEditorStandardTab = "ColorEditorStandardTab";
        internal const string ColorEditorSystemTab = "ColorEditorSystemTab";
        internal const string ContentAlignmentEditorAccName = "ContentAlignmentEditorAccName";
        internal const string ContentAlignmentEditorBottomCenterAccName = "ContentAlignmentEditorBottomCenterAccName";
        internal const string ContentAlignmentEditorBottomLeftAccName = "ContentAlignmentEditorBottomLeftAccName";
        internal const string ContentAlignmentEditorBottomRightAccName = "ContentAlignmentEditorBottomRightAccName";
        internal const string ContentAlignmentEditorMiddleCenterAccName = "ContentAlignmentEditorMiddleCenterAccName";
        internal const string ContentAlignmentEditorMiddleLeftAccName = "ContentAlignmentEditorMiddleLeftAccName";
        internal const string ContentAlignmentEditorMiddleRightAccName = "ContentAlignmentEditorMiddleRightAccName";
        internal const string ContentAlignmentEditorTopCenterAccName = "ContentAlignmentEditorTopCenterAccName";
        internal const string ContentAlignmentEditorTopLeftAccName = "ContentAlignmentEditorTopLeftAccName";
        internal const string ContentAlignmentEditorTopRightAccName = "ContentAlignmentEditorTopRightAccName";
        internal const string iconFileDescription = "iconFileDescription";
        internal const string imageFileDescription = "imageFileDescription";
        private static System.Drawing.Design.SR loader;
        internal const string metafileFileDescription = "metafileFileDescription";
        private ResourceManager resources;
        internal const string ToolboxServiceAssemblyNotFound = "ToolboxServiceAssemblyNotFound";
        internal const string ToolboxServiceBadToolboxItem = "ToolboxServiceBadToolboxItem";
        internal const string ToolboxServiceBadToolboxItemWithException = "ToolboxServiceBadToolboxItemWithException";
        internal const string ToolboxServiceToolboxItemSerializerNotFound = "ToolboxServiceToolboxItemSerializerNotFound";

        internal SR()
        {
            this.resources = new ResourceManager("System.Drawing.Design", base.GetType().Assembly);
        }

        private static System.Drawing.Design.SR GetLoader()
        {
            if (loader == null)
            {
                System.Drawing.Design.SR sr = new System.Drawing.Design.SR();
                Interlocked.CompareExchange<System.Drawing.Design.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            System.Drawing.Design.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            System.Drawing.Design.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            System.Drawing.Design.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

