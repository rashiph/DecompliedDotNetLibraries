namespace System.Data
{
    using System;

    internal sealed class OperatorInfo
    {
        internal int op;
        internal int priority;
        internal Nodes type;

        internal OperatorInfo(Nodes type, int op, int pri)
        {
            this.type = type;
            this.op = op;
            this.priority = pri;
        }
    }
}

