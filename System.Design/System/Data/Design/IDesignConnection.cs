namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Data;

    internal interface IDesignConnection : IDataSourceNamedObject, INamedObject, ICloneable, IDataSourceInitAfterLoading, IDataSourceXmlSpecialOwner
    {
        IDbConnection CreateEmptyDbConnection();

        string AppSettingsObjectName { get; set; }

        string ConnectionString { get; set; }

        System.Data.Design.ConnectionString ConnectionStringObject { get; set; }

        bool IsAppSettingsProperty { get; set; }

        IDictionary Properties { get; }

        CodePropertyReferenceExpression PropertyReference { get; set; }

        string Provider { get; set; }
    }
}

