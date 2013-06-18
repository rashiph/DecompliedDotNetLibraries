namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IntervalTreeTraverser
    {
        private IntervalBoundary currentNode;
        private IntervalBoundary nextNode;
        private IntervalCollection slot;
        private double val;
        internal IntervalTreeTraverser(double val, IntervalBoundary root)
        {
            this.currentNode = null;
            this.slot = null;
            this.nextNode = root;
            this.val = val;
        }

        internal IntervalCollection Slot
        {
            get
            {
                return this.slot;
            }
        }
        internal bool MoveNext()
        {
            while (this.nextNode != null)
            {
                this.currentNode = this.nextNode;
                double num = this.currentNode.Value;
                if (this.val < num)
                {
                    this.slot = this.currentNode.LtSlot;
                    this.nextNode = this.currentNode.Left;
                }
                else if (this.val > num)
                {
                    this.slot = this.currentNode.GtSlot;
                    this.nextNode = this.currentNode.Right;
                }
                else
                {
                    this.slot = this.currentNode.EqSlot;
                    this.nextNode = null;
                }
                if (this.slot != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

