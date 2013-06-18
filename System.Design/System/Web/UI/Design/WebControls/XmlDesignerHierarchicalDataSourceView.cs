namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class XmlDesignerHierarchicalDataSourceView : DesignerHierarchicalDataSourceView
    {
        private XmlDataSourceDesigner _owner;

        public XmlDesignerHierarchicalDataSourceView(XmlDataSourceDesigner owner, string viewPath) : base(owner, viewPath)
        {
            this._owner = owner;
        }

        public override IHierarchicalEnumerable GetDesignTimeData(out bool isSampleData)
        {
            IHierarchicalEnumerable hierarchicalRuntimeEnumerable = this._owner.GetHierarchicalRuntimeEnumerable(base.Path);
            if (hierarchicalRuntimeEnumerable != null)
            {
                isSampleData = false;
                return hierarchicalRuntimeEnumerable;
            }
            return base.GetDesignTimeData(out isSampleData);
        }

        public override IDataSourceSchema Schema
        {
            get
            {
                XmlDataSource designTimeXmlDataSource = this._owner.GetDesignTimeXmlDataSource(base.Path);
                if (designTimeXmlDataSource == null)
                {
                    return null;
                }
                return new XmlDocumentSchema(designTimeXmlDataSource.GetXmlDocument(), designTimeXmlDataSource.XPath, true);
            }
        }
    }
}

