namespace System.DirectoryServices
{
    using System;
    using System.ComponentModel;

    public class DirectoryVirtualListView
    {
        private int afterCount;
        private int approximateTotal;
        private int beforeCount;
        private System.DirectoryServices.DirectoryVirtualListViewContext context;
        private int offset;
        private string target;
        private int targetPercentage;

        public DirectoryVirtualListView()
        {
            this.target = "";
        }

        public DirectoryVirtualListView(int afterCount)
        {
            this.target = "";
            this.AfterCount = afterCount;
        }

        public DirectoryVirtualListView(int beforeCount, int afterCount, int offset)
        {
            this.target = "";
            this.BeforeCount = beforeCount;
            this.AfterCount = afterCount;
            this.Offset = offset;
        }

        public DirectoryVirtualListView(int beforeCount, int afterCount, string target)
        {
            this.target = "";
            this.BeforeCount = beforeCount;
            this.AfterCount = afterCount;
            this.Target = target;
        }

        public DirectoryVirtualListView(int beforeCount, int afterCount, int offset, System.DirectoryServices.DirectoryVirtualListViewContext context)
        {
            this.target = "";
            this.BeforeCount = beforeCount;
            this.AfterCount = afterCount;
            this.Offset = offset;
            this.context = context;
        }

        public DirectoryVirtualListView(int beforeCount, int afterCount, string target, System.DirectoryServices.DirectoryVirtualListViewContext context)
        {
            this.target = "";
            this.BeforeCount = beforeCount;
            this.AfterCount = afterCount;
            this.Target = target;
            this.context = context;
        }

        [DSDescription("DSAfterCount"), DefaultValue(0)]
        public int AfterCount
        {
            get
            {
                return this.afterCount;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DSBadAfterCount"));
                }
                this.afterCount = value;
            }
        }

        [DefaultValue(0), DSDescription("DSApproximateTotal")]
        public int ApproximateTotal
        {
            get
            {
                return this.approximateTotal;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DSBadApproximateTotal"));
                }
                this.approximateTotal = value;
            }
        }

        [DefaultValue(0), DSDescription("DSBeforeCount")]
        public int BeforeCount
        {
            get
            {
                return this.beforeCount;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DSBadBeforeCount"));
                }
                this.beforeCount = value;
            }
        }

        [DSDescription("DSDirectoryVirtualListViewContext"), DefaultValue((string) null)]
        public System.DirectoryServices.DirectoryVirtualListViewContext DirectoryVirtualListViewContext
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
            }
        }

        [DefaultValue(0), DSDescription("DSOffset")]
        public int Offset
        {
            get
            {
                return this.offset;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DSBadOffset"));
                }
                this.offset = value;
                if (this.approximateTotal != 0)
                {
                    this.targetPercentage = (int) ((((double) this.offset) / ((double) this.approximateTotal)) * 100.0);
                }
                else
                {
                    this.targetPercentage = 0;
                }
            }
        }

        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DSDescription("DSTarget"), DefaultValue("")]
        public string Target
        {
            get
            {
                return this.target;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                this.target = value;
            }
        }

        [DSDescription("DSTargetPercentage"), DefaultValue(0)]
        public int TargetPercentage
        {
            get
            {
                return this.targetPercentage;
            }
            set
            {
                if ((value > 100) || (value < 0))
                {
                    throw new ArgumentException(Res.GetString("DSBadTargetPercentage"));
                }
                this.targetPercentage = value;
                this.offset = (this.approximateTotal * this.targetPercentage) / 100;
            }
        }
    }
}

