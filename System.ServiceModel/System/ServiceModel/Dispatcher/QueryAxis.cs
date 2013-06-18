namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct QueryAxis
    {
        private AxisDirection direction;
        private QueryNodeType principalNode;
        private QueryAxisType type;
        private QueryNodeType validNodeTypes;
        internal QueryAxis(QueryAxisType type, AxisDirection direction, QueryNodeType principalNode, QueryNodeType validNodeTypes)
        {
            this.direction = direction;
            this.principalNode = principalNode;
            this.type = type;
            this.validNodeTypes = validNodeTypes;
        }

        internal QueryNodeType PrincipalNodeType
        {
            get
            {
                return this.principalNode;
            }
        }
        internal QueryAxisType Type
        {
            get
            {
                return this.type;
            }
        }
        internal QueryNodeType ValidNodeTypes
        {
            get
            {
                return this.validNodeTypes;
            }
        }
        internal bool IsSupported()
        {
            switch (this.type)
            {
                case QueryAxisType.Attribute:
                case QueryAxisType.Child:
                case QueryAxisType.Descendant:
                case QueryAxisType.DescendantOrSelf:
                case QueryAxisType.Self:
                    return true;
            }
            return false;
        }
    }
}

