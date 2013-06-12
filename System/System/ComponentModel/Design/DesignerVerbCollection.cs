namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class DesignerVerbCollection : CollectionBase
    {
        public DesignerVerbCollection()
        {
        }

        public DesignerVerbCollection(DesignerVerb[] value)
        {
            this.AddRange(value);
        }

        public int Add(DesignerVerb value)
        {
            return base.List.Add(value);
        }

        public void AddRange(DesignerVerb[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(DesignerVerbCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(DesignerVerb value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(DesignerVerb[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(DesignerVerb value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, DesignerVerb value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnClear()
        {
        }

        protected override void OnInsert(int index, object value)
        {
        }

        protected override void OnRemove(int index, object value)
        {
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
        }

        protected override void OnValidate(object value)
        {
        }

        public void Remove(DesignerVerb value)
        {
            base.List.Remove(value);
        }

        public DesignerVerb this[int index]
        {
            get
            {
                return (DesignerVerb) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

