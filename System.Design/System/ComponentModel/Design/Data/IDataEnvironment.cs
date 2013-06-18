namespace System.ComponentModel.Design.Data
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Data.Common;
    using System.Windows.Forms;

    public interface IDataEnvironment
    {
        DesignerDataConnection BuildConnection(IWin32Window owner, DesignerDataConnection initialConnection);
        string BuildQuery(IWin32Window owner, DesignerDataConnection connection, QueryBuilderMode mode, string initialQueryText);
        DesignerDataConnection ConfigureConnection(IWin32Window owner, DesignerDataConnection connection, string name);
        CodeExpression GetCodeExpression(DesignerDataConnection connection);
        IDesignerDataSchema GetConnectionSchema(DesignerDataConnection connection);
        DbConnection GetDesignTimeConnection(DesignerDataConnection connection);

        ICollection Connections { get; }
    }
}

