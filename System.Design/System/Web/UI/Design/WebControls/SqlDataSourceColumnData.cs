namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Data;
    using System.Data.Common;
    using System.Data.OracleClient;
    using System.Globalization;
    using System.Text;

    internal sealed class SqlDataSourceColumnData
    {
        private string _cachedAliasedName;
        private string _cachedEscapedName;
        private string _cachedParameterPlaceholder;
        private string _cachedWebParameterName;
        private DesignerDataColumn _column;
        private DesignerDataConnection _connection;
        private StringCollection _usedNames;

        public SqlDataSourceColumnData(DesignerDataConnection connection, DesignerDataColumn column) : this(connection, column, null)
        {
        }

        public SqlDataSourceColumnData(DesignerDataConnection connection, DesignerDataColumn column, StringCollection usedNames)
        {
            this._connection = connection;
            this._column = column;
            this._usedNames = usedNames;
        }

        private string CreateAliasedName()
        {
            int num;
            string str2;
            string str3;
            string name = this._column.Name;
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            bool flag2 = false;
            foreach (char ch in name)
            {
                if (char.IsWhiteSpace(ch) || (ch == '_'))
                {
                    if (!flag2)
                    {
                        builder.Append('_');
                        flag2 = true;
                    }
                }
                else if (char.IsLetterOrDigit(ch))
                {
                    builder.Append(ch);
                    flag2 = false;
                }
                else
                {
                    flag = true;
                    break;
                }
            }
            if ((builder.Length == 0) || !char.IsLetter(builder[0]))
            {
                flag = true;
            }
            if (flag)
            {
                str2 = "column";
                num = 1;
                str3 = str2 + '1';
            }
            else
            {
                num = 2;
                str2 = builder.ToString();
                str3 = str2;
            }
            if (this._usedNames != null)
            {
                if (this._usedNames.Contains(str3))
                {
                    do
                    {
                        str3 = str2 + num.ToString(CultureInfo.InvariantCulture);
                        num++;
                    }
                    while (this._usedNames.Contains(str3));
                }
                this._usedNames.Add(str3);
            }
            return str3;
        }

        private string CreateEscapedName()
        {
            StringBuilder builder = new StringBuilder();
            if (this._column == null)
            {
                builder.Append("*");
            }
            else
            {
                builder.Append(EscapeObjectName(this._connection, this._column.Name));
            }
            return builder.ToString();
        }

        private string CreateParameterPlaceholder(string oldValueFormatString)
        {
            DbProviderFactory dbProviderFactory = SqlDataSourceDesigner.GetDbProviderFactory(this._connection.ProviderName);
            string parameterPlaceholderPrefix = SqlDataSourceDesigner.GetParameterPlaceholderPrefix(dbProviderFactory);
            if (!SqlDataSourceDesigner.SupportsNamedParameters(dbProviderFactory))
            {
                return parameterPlaceholderPrefix;
            }
            if (oldValueFormatString == null)
            {
                return (parameterPlaceholderPrefix + this.AliasedName);
            }
            return (parameterPlaceholderPrefix + string.Format(CultureInfo.InvariantCulture, oldValueFormatString, new object[] { this.AliasedName }));
        }

        private string CreateWebParameterName(string oldValueFormatString)
        {
            if (oldValueFormatString == null)
            {
                return this.AliasedName;
            }
            return string.Format(CultureInfo.InvariantCulture, oldValueFormatString, new object[] { this.AliasedName });
        }

        internal static string EscapeObjectName(DesignerDataConnection connection, string objectName)
        {
            string str = "[";
            string str2 = "]";
            try
            {
                DbProviderFactory dbProviderFactory = SqlDataSourceDesigner.GetDbProviderFactory(connection.ProviderName);
                DbCommandBuilder builder = dbProviderFactory.CreateCommandBuilder();
                if (dbProviderFactory == OracleClientFactory.Instance)
                {
                    str = str2 = "\"";
                }
                builder.QuotePrefix = str;
                builder.QuoteSuffix = str2;
                return builder.QuoteIdentifier(objectName);
            }
            catch (Exception)
            {
                return (str + objectName + str2);
            }
        }

        public string GetOldValueParameterPlaceHolder(string oldValueFormatString)
        {
            return this.CreateParameterPlaceholder(oldValueFormatString);
        }

        public string GetOldValueWebParameterName(string oldValueFormatString)
        {
            return this.CreateWebParameterName(oldValueFormatString);
        }

        public string AliasedName
        {
            get
            {
                if (this._cachedAliasedName == null)
                {
                    this._cachedAliasedName = this.CreateAliasedName();
                }
                return this._cachedAliasedName;
            }
        }

        public DesignerDataColumn Column
        {
            get
            {
                return this._column;
            }
        }

        public string EscapedName
        {
            get
            {
                if (this._cachedEscapedName == null)
                {
                    this._cachedEscapedName = this.CreateEscapedName();
                }
                return this._cachedEscapedName;
            }
        }

        public string ParameterPlaceholder
        {
            get
            {
                if (this._cachedParameterPlaceholder == null)
                {
                    this._cachedParameterPlaceholder = this.CreateParameterPlaceholder(null);
                }
                return this._cachedParameterPlaceholder;
            }
        }

        public string SelectName
        {
            get
            {
                if ((this._column != null) && (this.AliasedName != this._column.Name))
                {
                    return (this.EscapedName + " AS " + this.AliasedName);
                }
                return this.EscapedName;
            }
        }

        public string WebParameterName
        {
            get
            {
                if (this._cachedWebParameterName == null)
                {
                    this._cachedWebParameterName = this.CreateWebParameterName(null);
                }
                return this._cachedWebParameterName;
            }
        }
    }
}

