namespace System.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Runtime;

    public class WqlEventQuery : EventQuery
    {
        private string condition;
        private string eventClassName;
        private StringCollection groupByPropertyList;
        private TimeSpan groupWithinInterval;
        private string havingCondition;
        private static readonly string tokenSelectAll = "select * ";
        private TimeSpan withinInterval;

        public WqlEventQuery() : this(null, TimeSpan.Zero, null, TimeSpan.Zero, null, null)
        {
        }

        public WqlEventQuery(string queryOrEventClassName)
        {
            this.groupByPropertyList = new StringCollection();
            if (queryOrEventClassName != null)
            {
                if (queryOrEventClassName.TrimStart(new char[0]).StartsWith(tokenSelectAll, StringComparison.OrdinalIgnoreCase))
                {
                    this.QueryString = queryOrEventClassName;
                }
                else
                {
                    ManagementPath path = new ManagementPath(queryOrEventClassName);
                    if (!path.IsClass || (path.NamespacePath.Length != 0))
                    {
                        throw new ArgumentException(RC.GetString("INVALID_QUERY"), "queryOrEventClassName");
                    }
                    this.EventClassName = queryOrEventClassName;
                }
            }
        }

        public WqlEventQuery(string eventClassName, string condition) : this(eventClassName, TimeSpan.Zero, condition, TimeSpan.Zero, null, null)
        {
        }

        public WqlEventQuery(string eventClassName, TimeSpan withinInterval) : this(eventClassName, withinInterval, null, TimeSpan.Zero, null, null)
        {
        }

        public WqlEventQuery(string eventClassName, string condition, TimeSpan groupWithinInterval) : this(eventClassName, TimeSpan.Zero, condition, groupWithinInterval, null, null)
        {
        }

        public WqlEventQuery(string eventClassName, TimeSpan withinInterval, string condition) : this(eventClassName, withinInterval, condition, TimeSpan.Zero, null, null)
        {
        }

        public WqlEventQuery(string eventClassName, string condition, TimeSpan groupWithinInterval, string[] groupByPropertyList) : this(eventClassName, TimeSpan.Zero, condition, groupWithinInterval, groupByPropertyList, null)
        {
        }

        public WqlEventQuery(string eventClassName, TimeSpan withinInterval, string condition, TimeSpan groupWithinInterval, string[] groupByPropertyList, string havingCondition)
        {
            this.eventClassName = eventClassName;
            this.withinInterval = withinInterval;
            this.condition = condition;
            this.groupWithinInterval = groupWithinInterval;
            this.groupByPropertyList = new StringCollection();
            if (groupByPropertyList != null)
            {
                this.groupByPropertyList.AddRange(groupByPropertyList);
            }
            this.havingCondition = havingCondition;
            this.BuildQuery();
        }

        protected internal void BuildQuery()
        {
            if ((this.eventClassName == null) || (this.eventClassName.Length == 0))
            {
                base.SetQueryString(string.Empty);
            }
            else
            {
                string qString = tokenSelectAll + "from " + this.eventClassName;
                if (this.withinInterval != TimeSpan.Zero)
                {
                    qString = qString + " within " + this.withinInterval.TotalSeconds.ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(double)));
                }
                if (this.Condition.Length != 0)
                {
                    qString = qString + " where " + this.condition;
                }
                if (this.groupWithinInterval != TimeSpan.Zero)
                {
                    qString = qString + " group within " + this.groupWithinInterval.TotalSeconds.ToString((IFormatProvider) CultureInfo.InvariantCulture.GetFormat(typeof(double)));
                    if ((this.groupByPropertyList != null) && (0 < this.groupByPropertyList.Count))
                    {
                        int count = this.groupByPropertyList.Count;
                        qString = qString + " by ";
                        for (int i = 0; i < count; i++)
                        {
                            qString = qString + this.groupByPropertyList[i] + ((i == (count - 1)) ? "" : ",");
                        }
                    }
                    if (this.HavingCondition.Length != 0)
                    {
                        qString = qString + " having " + this.havingCondition;
                    }
                }
                base.SetQueryString(qString);
            }
        }

        public override object Clone()
        {
            string[] array = null;
            if (this.groupByPropertyList != null)
            {
                int count = this.groupByPropertyList.Count;
                if (0 < count)
                {
                    array = new string[count];
                    this.groupByPropertyList.CopyTo(array, 0);
                }
            }
            return new WqlEventQuery(this.eventClassName, this.withinInterval, this.condition, this.groupWithinInterval, array, this.havingCondition);
        }

        protected internal override void ParseQuery(string query)
        {
            int index;
            string str2;
            this.eventClassName = null;
            this.withinInterval = TimeSpan.Zero;
            this.condition = null;
            this.groupWithinInterval = TimeSpan.Zero;
            if (this.groupByPropertyList != null)
            {
                this.groupByPropertyList.Clear();
            }
            this.havingCondition = null;
            string strA = query.Trim();
            bool bTokenFound = false;
            string tokenSelect = ManagementQuery.tokenSelect;
            if ((strA.Length < tokenSelect.Length) || (string.Compare(strA, 0, tokenSelect, 0, tokenSelect.Length, StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"));
            }
            strA = strA.Remove(0, tokenSelect.Length).TrimStart(null);
            if (!strA.StartsWith("*", StringComparison.Ordinal))
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"), "*");
            }
            strA = strA.Remove(0, 1).TrimStart(null);
            tokenSelect = "from ";
            if ((strA.Length < tokenSelect.Length) || (string.Compare(strA, 0, tokenSelect, 0, tokenSelect.Length, StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw new ArgumentException(RC.GetString("INVALID_QUERY"), "from");
            }
            ManagementQuery.ParseToken(ref strA, tokenSelect, null, ref bTokenFound, ref this.eventClassName);
            tokenSelect = "within ";
            if ((strA.Length >= tokenSelect.Length) && (string.Compare(strA, 0, tokenSelect, 0, tokenSelect.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                string tokenValue = null;
                bTokenFound = false;
                ManagementQuery.ParseToken(ref strA, tokenSelect, null, ref bTokenFound, ref tokenValue);
                this.withinInterval = TimeSpan.FromSeconds(((IConvertible) tokenValue).ToDouble(null));
            }
            tokenSelect = "group within ";
            if ((strA.Length >= tokenSelect.Length) && ((index = strA.ToLower(CultureInfo.InvariantCulture).IndexOf(tokenSelect, StringComparison.Ordinal)) != -1))
            {
                str2 = strA.Substring(0, index).Trim();
                strA = strA.Remove(0, index);
                string str6 = null;
                bTokenFound = false;
                ManagementQuery.ParseToken(ref strA, tokenSelect, null, ref bTokenFound, ref str6);
                this.groupWithinInterval = TimeSpan.FromSeconds(((IConvertible) str6).ToDouble(null));
                tokenSelect = "by ";
                if ((strA.Length >= tokenSelect.Length) && (string.Compare(strA, 0, tokenSelect, 0, tokenSelect.Length, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    string str3;
                    strA = strA.Remove(0, tokenSelect.Length);
                    if (this.groupByPropertyList != null)
                    {
                        this.groupByPropertyList.Clear();
                    }
                    else
                    {
                        this.groupByPropertyList = new StringCollection();
                    }
                    while ((index = strA.IndexOf(',')) > 0)
                    {
                        str3 = strA.Substring(0, index);
                        strA = strA.Remove(0, index + 1).TrimStart(null);
                        str3 = str3.Trim();
                        if (str3.Length > 0)
                        {
                            this.groupByPropertyList.Add(str3);
                        }
                    }
                    index = strA.IndexOf(' ');
                    if (index > 0)
                    {
                        str3 = strA.Substring(0, index);
                        strA = strA.Remove(0, index).TrimStart(null);
                        this.groupByPropertyList.Add(str3);
                    }
                    else
                    {
                        this.groupByPropertyList.Add(strA);
                        return;
                    }
                }
                tokenSelect = "having ";
                bTokenFound = false;
                if ((strA.Length >= tokenSelect.Length) && (string.Compare(strA, 0, tokenSelect, 0, tokenSelect.Length, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    strA = strA.Remove(0, tokenSelect.Length);
                    if (strA.Length == 0)
                    {
                        throw new ArgumentException(RC.GetString("INVALID_QUERY"), "having");
                    }
                    this.havingCondition = strA;
                }
            }
            else
            {
                str2 = strA.Trim();
            }
            tokenSelect = "where ";
            if ((str2.Length >= tokenSelect.Length) && (string.Compare(str2, 0, tokenSelect, 0, tokenSelect.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                this.condition = str2.Substring(tokenSelect.Length);
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
            }
        }

        public string EventClassName
        {
            get
            {
                if (this.eventClassName == null)
                {
                    return string.Empty;
                }
                return this.eventClassName;
            }
            set
            {
                this.eventClassName = value;
                this.BuildQuery();
            }
        }

        public StringCollection GroupByPropertyList
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.groupByPropertyList;
            }
            set
            {
                StringCollection strings = value;
                StringCollection strings2 = new StringCollection();
                foreach (string str in strings)
                {
                    strings2.Add(str);
                }
                this.groupByPropertyList = strings2;
                this.BuildQuery();
            }
        }

        public TimeSpan GroupWithinInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.groupWithinInterval;
            }
            set
            {
                this.groupWithinInterval = value;
                this.BuildQuery();
            }
        }

        public string HavingCondition
        {
            get
            {
                if (this.havingCondition == null)
                {
                    return string.Empty;
                }
                return this.havingCondition;
            }
            set
            {
                this.havingCondition = value;
                this.BuildQuery();
            }
        }

        public override string QueryLanguage
        {
            get
            {
                return base.QueryLanguage;
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

        public TimeSpan WithinInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.withinInterval;
            }
            set
            {
                this.withinInterval = value;
                this.BuildQuery();
            }
        }
    }
}

