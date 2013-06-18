namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataListComponentEditor : BaseDataListComponentEditor
    {
        private static Type[] editorPages = new Type[] { typeof(DataListGeneralPage), typeof(FormatPage), typeof(BordersPage) };
        internal static int IDX_BORDERS = 2;
        internal static int IDX_FORMAT = 1;
        internal static int IDX_GENERAL = 0;

        public DataListComponentEditor() : base(IDX_GENERAL)
        {
        }

        public DataListComponentEditor(int initialPage) : base(initialPage)
        {
        }

        protected override Type[] GetComponentEditorPages()
        {
            return editorPages;
        }
    }
}

