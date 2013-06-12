namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    public interface IDataBoundControl
    {
        string[] DataKeyNames { get; set; }

        string DataMember { get; set; }

        object DataSource { get; set; }

        string DataSourceID { get; set; }

        IDataSource DataSourceObject { get; }
    }
}

