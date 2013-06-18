namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Web.UI;

    public class ConnectionStringsExpressionEditor : ExpressionEditor
    {
        public override object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider)
        {
            Pair pair = (Pair) parseTimeData;
            string first = (string) pair.First;
            bool second = (bool) pair.Second;
            ConnectionStringSettingsCollection connectionStringSettingsCollection = this.GetConnectionStringSettingsCollection(serviceProvider);
            ConnectionStringSettings settings = null;
            foreach (ConnectionStringSettings settings2 in connectionStringSettingsCollection)
            {
                if (string.Equals(first, settings2.Name, StringComparison.OrdinalIgnoreCase))
                {
                    settings = settings2;
                    break;
                }
            }
            if (settings == null)
            {
                return null;
            }
            if (second)
            {
                return settings.ConnectionString;
            }
            return settings.ProviderName;
        }

        private ConnectionStringSettingsCollection GetConnectionStringSettingsCollection(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IWebApplication service = (IWebApplication) serviceProvider.GetService(typeof(IWebApplication));
                if (service != null)
                {
                    System.Configuration.Configuration configuration = service.OpenWebConfiguration(true);
                    if (configuration != null)
                    {
                        ConnectionStringsSection section = (ConnectionStringsSection) configuration.GetSection("connectionStrings");
                        if (section != null)
                        {
                            return section.ConnectionStrings;
                        }
                    }
                }
            }
            return null;
        }

        public override ExpressionEditorSheet GetExpressionEditorSheet(string expression, IServiceProvider serviceProvider)
        {
            return new ConnectionStringsExpressionEditorSheet(expression, this, serviceProvider);
        }

        private static string ParseExpression(string expression, out bool isConnectionString)
        {
            isConnectionString = true;
            expression = expression.Trim();
            if (expression.EndsWith(".connectionstring", StringComparison.OrdinalIgnoreCase))
            {
                return expression.Substring(0, expression.Length - ".connectionstring".Length);
            }
            if (expression.EndsWith(".providername", StringComparison.OrdinalIgnoreCase))
            {
                isConnectionString = false;
                return expression.Substring(0, expression.Length - ".providername".Length);
            }
            return expression;
        }

        private class ConnectionStringsExpressionEditorSheet : ExpressionEditorSheet
        {
            private string _connectionName;
            private ConnectionType _connectionType;
            private ConnectionStringsExpressionEditor _owner;

            public ConnectionStringsExpressionEditorSheet(string expression, ConnectionStringsExpressionEditor owner, IServiceProvider serviceProvider) : base(serviceProvider)
            {
                bool flag;
                this._owner = owner;
                this._connectionName = ConnectionStringsExpressionEditor.ParseExpression(expression, out flag);
                this._connectionType = flag ? ConnectionType.ConnectionString : ConnectionType.ProviderName;
            }

            public override string GetExpression()
            {
                if (string.IsNullOrEmpty(this._connectionName))
                {
                    return string.Empty;
                }
                string str = this._connectionName;
                if (this.Type == ConnectionType.ProviderName)
                {
                    str = str + ".ProviderName";
                }
                return str;
            }

            [DefaultValue(""), TypeConverter(typeof(ConnectionStringsTypeConverter)), System.Design.SRDescription("ConnectionStringsExpressionEditor_ConnectionName")]
            public string ConnectionName
            {
                get
                {
                    return this._connectionName;
                }
                set
                {
                    this._connectionName = value;
                }
            }

            public override bool IsValid
            {
                get
                {
                    return !string.IsNullOrEmpty(this.ConnectionName);
                }
            }

            [DefaultValue(0), System.Design.SRDescription("ConnectionStringsExpressionEditor_ConnectionType")]
            public ConnectionType Type
            {
                get
                {
                    return this._connectionType;
                }
                set
                {
                    this._connectionType = value;
                }
            }

            private class ConnectionStringsTypeConverter : TypeConverter
            {
                private static readonly string NoConnectionName = "(None)";

                public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                {
                    return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
                }

                public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
                {
                    return ((destType == typeof(string)) || base.CanConvertTo(context, destType));
                }

                public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                {
                    if (!(value is string))
                    {
                        return base.ConvertFrom(context, culture, value);
                    }
                    if (string.Equals((string) value, NoConnectionName, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Empty;
                    }
                    return value;
                }

                public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
                {
                    if (!(value is string))
                    {
                        return base.ConvertTo(context, culture, value, destinationType);
                    }
                    if (((string) value).Length == 0)
                    {
                        return NoConnectionName;
                    }
                    return value;
                }

                public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    if (context != null)
                    {
                        ConnectionStringsExpressionEditor.ConnectionStringsExpressionEditorSheet instance = (ConnectionStringsExpressionEditor.ConnectionStringsExpressionEditorSheet) context.Instance;
                        ConnectionStringSettingsCollection connectionStringSettingsCollection = instance._owner.GetConnectionStringSettingsCollection(instance.ServiceProvider);
                        if (connectionStringSettingsCollection != null)
                        {
                            ArrayList values = new ArrayList();
                            foreach (ConnectionStringSettings settings in connectionStringSettingsCollection)
                            {
                                values.Add(settings.Name);
                            }
                            values.Sort();
                            values.Add(string.Empty);
                            return new TypeConverter.StandardValuesCollection(values);
                        }
                    }
                    return base.GetStandardValues(context);
                }

                public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
                {
                    return false;
                }

                public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
                {
                    if (context != null)
                    {
                        ConnectionStringsExpressionEditor.ConnectionStringsExpressionEditorSheet instance = (ConnectionStringsExpressionEditor.ConnectionStringsExpressionEditorSheet) context.Instance;
                        ConnectionStringSettingsCollection connectionStringSettingsCollection = instance._owner.GetConnectionStringSettingsCollection(instance.ServiceProvider);
                        if (connectionStringSettingsCollection != null)
                        {
                            return (connectionStringSettingsCollection.Count > 0);
                        }
                    }
                    return base.GetStandardValuesSupported(context);
                }
            }

            public enum ConnectionType
            {
                ConnectionString,
                ProviderName
            }
        }
    }
}

