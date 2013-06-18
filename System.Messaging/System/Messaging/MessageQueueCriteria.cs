namespace System.Messaging
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Messaging.Interop;
    using System.Security;
    using System.Security.Permissions;

    public class MessageQueueCriteria
    {
        private Guid category;
        private DateTime createdAfter;
        private DateTime createdBefore;
        private CriteriaPropertyFilter filter = new CriteriaPropertyFilter();
        private string label;
        private string machine;
        private Guid machineId;
        private static DateTime maxDate = new DateTime(0x7f6, 1, 0x13);
        private static DateTime minDate = new DateTime(0x7b2, 1, 1);
        private DateTime modifiedAfter;
        private DateTime modifiedBefore;
        private Restrictions restrictions;

        public void ClearAll()
        {
            this.filter.ClearAll();
        }

        private int ConvertTime(DateTime time)
        {
            time = time.ToUniversalTime();
            TimeSpan span = (TimeSpan) (time - minDate);
            return (int) span.TotalSeconds;
        }

        public Guid Category
        {
            get
            {
                if (!this.filter.Category)
                {
                    throw new InvalidOperationException(Res.GetString("CriteriaNotDefined"));
                }
                return this.category;
            }
            set
            {
                this.category = value;
                this.filter.Category = true;
            }
        }

        public DateTime CreatedAfter
        {
            get
            {
                if (!this.filter.CreatedAfter)
                {
                    throw new InvalidOperationException(Res.GetString("CriteriaNotDefined"));
                }
                return this.createdAfter;
            }
            set
            {
                if ((value < minDate) || (value > maxDate))
                {
                    throw new ArgumentException(Res.GetString("InvalidDateValue", new object[] { minDate.ToString(CultureInfo.CurrentCulture), maxDate.ToString(CultureInfo.CurrentCulture) }));
                }
                this.createdAfter = value;
                if (this.filter.CreatedBefore && (this.createdAfter > this.createdBefore))
                {
                    this.createdBefore = this.createdAfter;
                }
                this.filter.CreatedAfter = true;
            }
        }

        public DateTime CreatedBefore
        {
            get
            {
                if (!this.filter.CreatedBefore)
                {
                    throw new InvalidOperationException(Res.GetString("CriteriaNotDefined"));
                }
                return this.createdBefore;
            }
            set
            {
                if ((value < minDate) || (value > maxDate))
                {
                    throw new ArgumentException(Res.GetString("InvalidDateValue", new object[] { minDate.ToString(CultureInfo.CurrentCulture), maxDate.ToString(CultureInfo.CurrentCulture) }));
                }
                this.createdBefore = value;
                if (this.filter.CreatedAfter && (this.createdAfter > this.createdBefore))
                {
                    this.createdAfter = this.createdBefore;
                }
                this.filter.CreatedBefore = true;
            }
        }

        internal bool FilterMachine
        {
            get
            {
                return this.filter.MachineName;
            }
        }

        public string Label
        {
            get
            {
                if (!this.filter.Label)
                {
                    throw new InvalidOperationException(Res.GetString("CriteriaNotDefined"));
                }
                return this.label;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.label = value;
                this.filter.Label = true;
            }
        }

        public string MachineName
        {
            get
            {
                if (!this.filter.MachineName)
                {
                    throw new InvalidOperationException(Res.GetString("CriteriaNotDefined"));
                }
                return this.machine;
            }
            set
            {
                if (!SyntaxCheck.CheckMachineName(value))
                {
                    throw new ArgumentException(Res.GetString("InvalidProperty", new object[] { "MachineName", value }));
                }
                new MessageQueuePermission(PermissionState.Unrestricted).Assert();
                try
                {
                    this.machineId = MessageQueue.GetMachineId(value);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                this.machine = value;
                this.filter.MachineName = true;
            }
        }

        public DateTime ModifiedAfter
        {
            get
            {
                if (!this.filter.ModifiedAfter)
                {
                    throw new InvalidOperationException(Res.GetString("CriteriaNotDefined"));
                }
                return this.modifiedAfter;
            }
            set
            {
                if ((value < minDate) || (value > maxDate))
                {
                    throw new ArgumentException(Res.GetString("InvalidDateValue", new object[] { minDate.ToString(CultureInfo.CurrentCulture), maxDate.ToString(CultureInfo.CurrentCulture) }));
                }
                this.modifiedAfter = value;
                if (this.filter.ModifiedBefore && (this.modifiedAfter > this.modifiedBefore))
                {
                    this.modifiedBefore = this.modifiedAfter;
                }
                this.filter.ModifiedAfter = true;
            }
        }

        public DateTime ModifiedBefore
        {
            get
            {
                if (!this.filter.ModifiedBefore)
                {
                    throw new InvalidOperationException(Res.GetString("CriteriaNotDefined"));
                }
                return this.modifiedBefore;
            }
            set
            {
                if ((value < minDate) || (value > maxDate))
                {
                    throw new ArgumentException(Res.GetString("InvalidDateValue", new object[] { minDate.ToString(CultureInfo.CurrentCulture), maxDate.ToString(CultureInfo.CurrentCulture) }));
                }
                this.modifiedBefore = value;
                if (this.filter.ModifiedAfter && (this.modifiedAfter > this.modifiedBefore))
                {
                    this.modifiedAfter = this.modifiedBefore;
                }
                this.filter.ModifiedBefore = true;
            }
        }

        internal Restrictions.MQRESTRICTION Reference
        {
            get
            {
                int maxRestrictions = 0;
                if (this.filter.CreatedAfter)
                {
                    maxRestrictions++;
                }
                if (this.filter.CreatedBefore)
                {
                    maxRestrictions++;
                }
                if (this.filter.Label)
                {
                    maxRestrictions++;
                }
                if (this.filter.ModifiedAfter)
                {
                    maxRestrictions++;
                }
                if (this.filter.ModifiedBefore)
                {
                    maxRestrictions++;
                }
                if (this.filter.Category)
                {
                    maxRestrictions++;
                }
                this.restrictions = new Restrictions(maxRestrictions);
                if (this.filter.CreatedAfter)
                {
                    this.restrictions.AddI4(0x6d, 2, this.ConvertTime(this.createdAfter));
                }
                if (this.filter.CreatedBefore)
                {
                    this.restrictions.AddI4(0x6d, 1, this.ConvertTime(this.createdBefore));
                }
                if (this.filter.Label)
                {
                    this.restrictions.AddString(0x6c, 4, this.label);
                }
                if (this.filter.ModifiedAfter)
                {
                    this.restrictions.AddI4(110, 2, this.ConvertTime(this.modifiedAfter));
                }
                if (this.filter.ModifiedBefore)
                {
                    this.restrictions.AddI4(110, 1, this.ConvertTime(this.modifiedBefore));
                }
                if (this.filter.Category)
                {
                    this.restrictions.AddGuid(0x66, 4, this.category);
                }
                return this.restrictions.GetRestrictionsRef();
            }
        }

        private class CriteriaPropertyFilter
        {
            public bool Category;
            public bool CreatedAfter;
            public bool CreatedBefore;
            public bool Label;
            public bool MachineName;
            public bool ModifiedAfter;
            public bool ModifiedBefore;

            public void ClearAll()
            {
                this.CreatedAfter = false;
                this.CreatedBefore = false;
                this.Label = false;
                this.MachineName = false;
                this.ModifiedAfter = false;
                this.ModifiedBefore = false;
                this.Category = false;
            }
        }
    }
}

