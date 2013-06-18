namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataGridComponentEditor : BaseDataListComponentEditor
    {
        private static Type[] editorPages = new Type[] { typeof(DataGridGeneralPage), typeof(DataGridColumnsPage), typeof(DataGridPagingPage), typeof(FormatPage), typeof(BordersPage) };
        internal static int IDX_BORDERS = 4;
        internal static int IDX_COLUMNS = 1;
        internal static int IDX_FORMAT = 3;
        internal static int IDX_GENERAL = 0;
        internal static int IDX_PAGING = 2;

        public DataGridComponentEditor() : base(IDX_GENERAL)
        {
        }

        public DataGridComponentEditor(int initialPage) : base(initialPage)
        {
        }

        protected override Type[] GetComponentEditorPages()
        {
            return editorPages;
        }
    }
}

