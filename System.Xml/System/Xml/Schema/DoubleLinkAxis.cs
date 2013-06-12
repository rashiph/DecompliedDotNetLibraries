namespace System.Xml.Schema
{
    using MS.Internal.Xml.XPath;
    using System;

    internal class DoubleLinkAxis : Axis
    {
        internal Axis next;

        internal DoubleLinkAxis(Axis axis, DoubleLinkAxis inputaxis) : base(axis.TypeOfAxis, inputaxis, axis.Prefix, axis.Name, axis.NodeType)
        {
            this.next = null;
            base.Urn = axis.Urn;
            base.abbrAxis = axis.AbbrAxis;
            if (inputaxis != null)
            {
                inputaxis.Next = this;
            }
        }

        internal static DoubleLinkAxis ConvertTree(Axis axis)
        {
            if (axis == null)
            {
                return null;
            }
            return new DoubleLinkAxis(axis, ConvertTree((Axis) axis.Input));
        }

        internal Axis Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }
    }
}

