namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct BindingMemberInfo
    {
        private string dataList;
        private string dataField;
        public BindingMemberInfo(string dataMember)
        {
            if (dataMember == null)
            {
                dataMember = "";
            }
            int length = dataMember.LastIndexOf(".");
            if (length != -1)
            {
                this.dataList = dataMember.Substring(0, length);
                this.dataField = dataMember.Substring(length + 1);
            }
            else
            {
                this.dataList = "";
                this.dataField = dataMember;
            }
        }

        public string BindingPath
        {
            get
            {
                if (this.dataList == null)
                {
                    return "";
                }
                return this.dataList;
            }
        }
        public string BindingField
        {
            get
            {
                if (this.dataField == null)
                {
                    return "";
                }
                return this.dataField;
            }
        }
        public string BindingMember
        {
            get
            {
                if (this.BindingPath.Length <= 0)
                {
                    return this.BindingField;
                }
                return (this.BindingPath + "." + this.BindingField);
            }
        }
        public override bool Equals(object otherObject)
        {
            if (otherObject is BindingMemberInfo)
            {
                BindingMemberInfo info = (BindingMemberInfo) otherObject;
                return string.Equals(this.BindingMember, info.BindingMember, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public static bool operator ==(BindingMemberInfo a, BindingMemberInfo b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BindingMemberInfo a, BindingMemberInfo b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

