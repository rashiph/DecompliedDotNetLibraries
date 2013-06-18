namespace System.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Runtime;

    public class SelectQuery : WqlObjectQuery
    {
        private string className;
        private string condition;
        private bool isSchemaQuery;
        private StringCollection selectedProperties;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SelectQuery() : this(null)
        {
        }

        public SelectQuery(string queryOrClassName)
        {
            this.selectedProperties = new StringCollection();
            if (queryOrClassName != null)
            {
                if (queryOrClassName.TrimStart(new char[0]).StartsWith(ManagementQuery.tokenSelect, StringComparison.OrdinalIgnoreCase))
                {
                    this.QueryString = queryOrClassName;
                }
                else
                {
                    ManagementPath path = new ManagementPath(queryOrClassName);
                    if (!path.IsClass || (path.NamespacePath.Length != 0))
                    {
                        throw new ArgumentException(RC.GetString("INVALID_QUERY"), "queryOrClassName");
                    }
                    this.ClassName = queryOrClassName;
                }
            }
        }

        public SelectQuery(bool isSchemaQuery, string condition)
        {
            if (!isSchemaQuery)
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"), "isSchemaQuery");
            }
            this.isSchemaQuery = true;
            this.className = null;
            this.condition = condition;
            this.selectedProperties = null;
            this.BuildQuery();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SelectQuery(string className, string condition) : this(className, condition, null)
        {
        }

        public SelectQuery(string className, string condition, string[] selectedProperties)
        {
            this.isSchemaQuery = false;
            this.className = className;
            this.condition = condition;
            this.selectedProperties = new StringCollection();
            if (selectedProperties != null)
            {
                this.selectedProperties.AddRange(selectedProperties);
            }
            this.BuildQuery();
        }

        protected internal void BuildQuery()
        {
            string tokenSelect;
            if (!this.isSchemaQuery)
            {
                if (this.className == null)
                {
                    base.SetQueryString(string.Empty);
                }
                if ((this.className == null) || (this.className.Length == 0))
                {
                    return;
                }
                tokenSelect = ManagementQuery.tokenSelect;
                if ((this.selectedProperties != null) && (0 < this.selectedProperties.Count))
                {
                    int count = this.selectedProperties.Count;
                    for (int i = 0; i < count; i++)
                    {
                        tokenSelect = tokenSelect + this.selectedProperties[i] + ((i == (count - 1)) ? " " : ",");
                    }
                }
                else
                {
                    tokenSelect = tokenSelect + "* ";
                }
                tokenSelect = tokenSelect + "from " + this.className;
            }
            else
            {
                tokenSelect = "select * from meta_class";
            }
            if ((this.Condition != null) && (this.Condition.Length != 0))
            {
                tokenSelect = tokenSelect + " where " + this.condition;
            }
            base.SetQueryString(tokenSelect);
        }

        public override object Clone()
        {
            string[] array = null;
            if (this.selectedProperties != null)
            {
                int count = this.selectedProperties.Count;
                if (0 < count)
                {
                    array = new string[count];
                    this.selectedProperties.CopyTo(array, 0);
                }
            }
            if (!this.isSchemaQuery)
            {
                return new SelectQuery(this.className, this.condition, array);
            }
            return new SelectQuery(true, this.condition);
        }

        protected internal override void ParseQuery(string query)
        {
            this.className = null;
            this.condition = null;
            if (this.selectedProperties != null)
            {
                this.selectedProperties.Clear();
            }
            string strA = query.Trim();
            bool bTokenFound = false;
            if (!this.isSchemaQuery)
            {
                string tokenSelect = ManagementQuery.tokenSelect;
                if ((strA.Length < tokenSelect.Length) || (string.Compare(strA, 0, tokenSelect, 0, tokenSelect.Length, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    throw new ArgumentException(RC.GetString("INVALID_QUERY"));
                }
                ManagementQuery.ParseToken(ref strA, tokenSelect, ref bTokenFound);
                if (strA[0] != '*')
                {
                    string str2;
                    int index;
                    if (this.selectedProperties != null)
                    {
                        this.selectedProperties.Clear();
                    }
                    else
                    {
                        this.selectedProperties = new StringCollection();
                    }
                    while ((index = strA.IndexOf(',')) > 0)
                    {
                        str2 = strA.Substring(0, index);
                        strA = strA.Remove(0, index + 1).TrimStart(null);
                        str2 = str2.Trim();
                        if (str2.Length > 0)
                        {
                            this.selectedProperties.Add(str2);
                        }
                    }
                    index = strA.IndexOf(' ');
                    if (index <= 0)
                    {
                        throw new ArgumentException(RC.GetString("INVALID_QUERY"));
                    }
                    str2 = strA.Substring(0, index);
                    strA = strA.Remove(0, index).TrimStart(null);
                    this.selectedProperties.Add(str2);
                }
                else
                {
                    strA = strA.Remove(0, 1).TrimStart(null);
                }
                tokenSelect = "from ";
                bTokenFound = false;
                if ((strA.Length < tokenSelect.Length) || (string.Compare(strA, 0, tokenSelect, 0, tokenSelect.Length, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    throw new ArgumentException(RC.GetString("INVALID_QUERY"));
                }
                ManagementQuery.ParseToken(ref strA, tokenSelect, null, ref bTokenFound, ref this.className);
                tokenSelect = "where ";
                if ((strA.Length >= tokenSelect.Length) && (string.Compare(strA, 0, tokenSelect, 0, tokenSelect.Length, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    this.condition = strA.Substring(tokenSelect.Length).Trim();
                }
            }
            else
            {
                string strB = "select";
                if ((strA.Length < strB.Length) || (string.Compare(strA, 0, strB, 0, strB.Length, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    throw new ArgumentException(RC.GetString("INVALID_QUERY"), "select");
                }
                strA = strA.Remove(0, strB.Length).TrimStart(null);
                if (strA.IndexOf('*', 0) != 0)
                {
                    throw new ArgumentException(RC.GetString("INVALID_QUERY"), "*");
                }
                strA = strA.Remove(0, 1).TrimStart(null);
                strB = "from";
                if ((strA.Length < strB.Length) || (string.Compare(strA, 0, strB, 0, strB.Length, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    throw new ArgumentException(RC.GetString("INVALID_QUERY"), "from");
                }
                strA = strA.Remove(0, strB.Length).TrimStart(null);
                strB = "meta_class";
                if ((strA.Length < strB.Length) || (string.Compare(strA, 0, strB, 0, strB.Length, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    throw new ArgumentException(RC.GetString("INVALID_QUERY"), "meta_class");
                }
                strA = strA.Remove(0, strB.Length).TrimStart(null);
                if (0 < strA.Length)
                {
                    strB = "where";
                    if ((strA.Length < strB.Length) || (string.Compare(strA, 0, strB, 0, strB.Length, StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        throw new ArgumentException(RC.GetString("INVALID_QUERY"), "where");
                    }
                    strA = strA.Remove(0, strB.Length);
                    if ((strA.Length == 0) || !char.IsWhiteSpace(strA[0]))
                    {
                        throw new ArgumentException(RC.GetString("INVALID_QUERY"));
                    }
                    strA = strA.TrimStart(null);
                    this.condition = strA;
                }
                else
                {
                    this.condition = string.Empty;
                }
                this.className = null;
                this.selectedProperties = null;
            }
        }

        public string ClassName
        {
            get
            {
                if (this.className == null)
                {
                    return string.Empty;
                }
                return this.className;
            }
            set
            {
                this.className = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public string Condition
        {
            get
            {
                if (this.condition == null)
                {
                    return string.Empty;
                }
                return this.condition;
            }
            set
            {
                this.condition = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public bool IsSchemaQuery
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isSchemaQuery;
            }
            set
            {
                this.isSchemaQuery = value;
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }

        public override string QueryString
        {
            get
            {
                this.BuildQuery();
                return base.QueryString;
            }
            set
            {
                base.QueryString = value;
            }
        }

        public StringCollection SelectedProperties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.selectedProperties;
            }
            set
            {
                if (value != null)
                {
                    StringCollection strings = value;
                    StringCollection strings2 = new StringCollection();
                    foreach (string str in strings)
                    {
                        strings2.Add(str);
                    }
                    this.selectedProperties = strings2;
                }
                else
                {
                    this.selectedProperties = new StringCollection();
                }
                this.BuildQuery();
                base.FireIdentifierChanged();
            }
        }
    }
}

