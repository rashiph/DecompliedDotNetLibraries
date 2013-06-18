namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Data;
    using System.Security.Permissions;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class SqlDataSourceConnectionStringEditor : ConnectionStringEditor
    {
        protected override string GetProviderName(object instance)
        {
            SqlDataSource source = instance as SqlDataSource;
            if (source != null)
            {
                return source.ProviderName;
            }
            return string.Empty;
        }

        protected override void SetProviderName(object instance, DesignerDataConnection connection)
        {
            SqlDataSource component = instance as SqlDataSource;
            if (component != null)
            {
                if (connection.IsConfigured)
                {
                    ExpressionEditor expressionEditor = ExpressionEditor.GetExpressionEditor(typeof(ConnectionStringsExpressionBuilder), component.Site);
                    if (expressionEditor != null)
                    {
                        string expressionPrefix = expressionEditor.ExpressionPrefix;
                        component.Expressions.Add(new ExpressionBinding("ProviderName", typeof(string), expressionPrefix, connection.Name + ".ProviderName"));
                    }
                }
                else
                {
                    TypeDescriptor.GetProperties(component)["ProviderName"].SetValue(component, connection.ProviderName);
                }
            }
        }
    }
}

