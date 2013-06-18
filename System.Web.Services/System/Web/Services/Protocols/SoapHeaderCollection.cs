namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class SoapHeaderCollection : CollectionBase
    {
        public int Add(SoapHeader header)
        {
            return base.List.Add(header);
        }

        public bool Contains(SoapHeader header)
        {
            return base.List.Contains(header);
        }

        public void CopyTo(SoapHeader[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(SoapHeader header)
        {
            return base.List.IndexOf(header);
        }

        public void Insert(int index, SoapHeader header)
        {
            base.List.Insert(index, header);
        }

        public void Remove(SoapHeader header)
        {
            base.List.Remove(header);
        }

        public SoapHeader this[int index]
        {
            get
            {
                return (SoapHeader) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

