namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class XmlDesignerDataSourceView : DesignerDataSourceView
    {
        private XmlDataSourceDesigner _owner;

        public XmlDesignerDataSourceView(XmlDataSourceDesigner owner, string viewName) : base(owner, viewName)
        {
            this._owner = owner;
        }

        public override IEnumerable GetDesignTimeData(int minimumRows, out bool isSampleData)
        {
            IEnumerable runtimeEnumerable = this._owner.GetRuntimeEnumerable(base.Name);
            if (runtimeEnumerable != null)
            {
                isSampleData = false;
                return runtimeEnumerable;
            }
            return base.GetDesignTimeData(minimumRows, out isSampleData);
        }

        public override IDataSourceViewSchema Schema
        {
            get
            {
                XmlDataSource designTimeXmlDataSource = this._owner.GetDesignTimeXmlDataSource(string.Empty);
                if (designTimeXmlDataSource != null)
                {
                    string xPath = designTimeXmlDataSource.XPath;
                    if (xPath.Length == 0)
                    {
                        xPath = "/node()/node()";
                    }
                    IDataSourceSchema schema = new XmlDocumentSchema(designTimeXmlDataSource.GetXmlDocument(), xPath);
                    if (schema != null)
                    {
                        IDataSourceViewSchema[] views = schema.GetViews();
                        if ((views != null) && (views.Length > 0))
                        {
                            return views[0];
                        }
                    }
                }
                return null;
            }
        }
    }
}

