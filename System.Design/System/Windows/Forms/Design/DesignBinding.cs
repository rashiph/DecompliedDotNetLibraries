namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;

    [Editor("System.Windows.Forms.Design.DesignBindingEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    internal class DesignBinding
    {
        private string dataMember;
        private object dataSource;
        public static DesignBinding Null = new DesignBinding(null, null);

        public DesignBinding(object dataSource, string dataMember)
        {
            this.dataSource = dataSource;
            this.dataMember = dataMember;
        }

        public bool Equals(object dataSource, string dataMember)
        {
            return ((dataSource == this.dataSource) && string.Equals(dataMember, this.dataMember, StringComparison.OrdinalIgnoreCase));
        }

        public string DataField
        {
            get
            {
                if (string.IsNullOrEmpty(this.dataMember))
                {
                    return string.Empty;
                }
                int num = this.dataMember.LastIndexOf(".");
                if (num == -1)
                {
                    return this.dataMember;
                }
                return this.dataMember.Substring(num + 1);
            }
        }

        public string DataMember
        {
            get
            {
                return this.dataMember;
            }
        }

        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
        }

        public bool IsNull
        {
            get
            {
                return (this.dataSource == null);
            }
        }
    }
}

