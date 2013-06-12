namespace System.Xml.Schema
{
    using System;

    internal class ForwardAxis
    {
        private bool isAttribute;
        private bool isDss;
        private bool isSelfAxis;
        private DoubleLinkAxis rootNode;
        private DoubleLinkAxis topNode;

        public ForwardAxis(DoubleLinkAxis axis, bool isdesorself)
        {
            this.isDss = isdesorself;
            this.isAttribute = Asttree.IsAttribute(axis);
            this.topNode = axis;
            this.rootNode = axis;
            while (this.rootNode.Input != null)
            {
                this.rootNode = (DoubleLinkAxis) this.rootNode.Input;
            }
            this.isSelfAxis = Asttree.IsSelf(this.topNode);
        }

        internal bool IsAttribute
        {
            get
            {
                return this.isAttribute;
            }
        }

        internal bool IsDss
        {
            get
            {
                return this.isDss;
            }
        }

        internal bool IsSelfAxis
        {
            get
            {
                return this.isSelfAxis;
            }
        }

        internal DoubleLinkAxis RootNode
        {
            get
            {
                return this.rootNode;
            }
        }

        internal DoubleLinkAxis TopNode
        {
            get
            {
                return this.topNode;
            }
        }
    }
}

