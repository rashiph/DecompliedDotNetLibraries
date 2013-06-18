namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class NumberIntervalOpcode : NumberRelationOpcode
    {
        private Interval interval;

        internal NumberIntervalOpcode(double literal, RelationOperator op) : base(OpcodeID.NumberInterval, literal, op)
        {
        }

        internal override void Add(Opcode op)
        {
            NumberIntervalOpcode opcode = op as NumberIntervalOpcode;
            if (opcode == null)
            {
                base.Add(op);
            }
            else
            {
                NumberIntervalBranchOpcode with = new NumberIntervalBranchOpcode();
                base.prev.Replace(this, with);
                with.Add(this);
                with.Add(opcode);
            }
        }

        internal override object Literal
        {
            get
            {
                if (this.interval == null)
                {
                    this.interval = base.ToInterval();
                }
                return this.interval;
            }
        }
    }
}

