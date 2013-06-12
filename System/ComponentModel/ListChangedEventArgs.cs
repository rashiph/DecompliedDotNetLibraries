namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class ListChangedEventArgs : EventArgs
    {
        private System.ComponentModel.ListChangedType listChangedType;
        private int newIndex;
        private int oldIndex;
        private System.ComponentModel.PropertyDescriptor propDesc;

        public ListChangedEventArgs(System.ComponentModel.ListChangedType listChangedType, System.ComponentModel.PropertyDescriptor propDesc)
        {
            this.listChangedType = listChangedType;
            this.propDesc = propDesc;
        }

        public ListChangedEventArgs(System.ComponentModel.ListChangedType listChangedType, int newIndex) : this(listChangedType, newIndex, -1)
        {
        }

        public ListChangedEventArgs(System.ComponentModel.ListChangedType listChangedType, int newIndex, System.ComponentModel.PropertyDescriptor propDesc) : this(listChangedType, newIndex)
        {
            this.propDesc = propDesc;
            this.oldIndex = newIndex;
        }

        public ListChangedEventArgs(System.ComponentModel.ListChangedType listChangedType, int newIndex, int oldIndex)
        {
            this.listChangedType = listChangedType;
            this.newIndex = newIndex;
            this.oldIndex = oldIndex;
        }

        public System.ComponentModel.ListChangedType ListChangedType
        {
            get
            {
                return this.listChangedType;
            }
        }

        public int NewIndex
        {
            get
            {
                return this.newIndex;
            }
        }

        public int OldIndex
        {
            get
            {
                return this.oldIndex;
            }
        }

        public System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return this.propDesc;
            }
        }
    }
}

