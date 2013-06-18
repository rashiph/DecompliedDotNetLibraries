namespace System.Xml.Linq
{
    using System;
    using System.Runtime;

    public class XObjectChangeEventArgs : EventArgs
    {
        public static readonly XObjectChangeEventArgs Add = new XObjectChangeEventArgs(XObjectChange.Add);
        public static readonly XObjectChangeEventArgs Name = new XObjectChangeEventArgs(XObjectChange.Name);
        private XObjectChange objectChange;
        public static readonly XObjectChangeEventArgs Remove = new XObjectChangeEventArgs(XObjectChange.Remove);
        public static readonly XObjectChangeEventArgs Value = new XObjectChangeEventArgs(XObjectChange.Value);

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XObjectChangeEventArgs(XObjectChange objectChange)
        {
            this.objectChange = objectChange;
        }

        public XObjectChange ObjectChange
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.objectChange;
            }
        }
    }
}

