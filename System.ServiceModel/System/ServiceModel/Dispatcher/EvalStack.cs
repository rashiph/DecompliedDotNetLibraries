namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct EvalStack
    {
        internal const int DefaultSize = 2;
        internal QueryBuffer<Value> buffer;
        internal StackRegion frames;
        internal StackRegion stack;
        internal bool contextOnTopOfStack;
        internal EvalStack(int frameCapacity, int stackCapacity)
        {
            this.buffer = new QueryBuffer<Value>(frameCapacity + stackCapacity);
            this.stack = new StackRegion(new QueryRange(0, stackCapacity - 1));
            this.buffer.Reserve(stackCapacity);
            this.frames = new StackRegion(new QueryRange(stackCapacity, (stackCapacity + frameCapacity) - 1));
            this.buffer.Reserve(frameCapacity);
            this.contextOnTopOfStack = false;
        }

        internal Value[] Buffer
        {
            get
            {
                return this.buffer.buffer;
            }
        }
        internal StackFrame this[int frameIndex]
        {
            get
            {
                return this.buffer.buffer[this.frames.stackPtr - frameIndex].Frame;
            }
        }
        internal StackFrame SecondArg
        {
            get
            {
                return this[1];
            }
        }
        internal StackFrame TopArg
        {
            get
            {
                return this[0];
            }
        }
        internal void Clear()
        {
            this.stack.Clear();
            this.frames.Clear();
            this.contextOnTopOfStack = false;
        }

        internal void CopyFrom(ref EvalStack stack)
        {
            this.buffer.CopyFrom(ref stack.buffer);
            this.frames = stack.frames;
            this.stack = stack.stack;
            this.contextOnTopOfStack = stack.contextOnTopOfStack;
        }

        internal int CalculateNodecount()
        {
            if (this.stack.stackPtr < 0)
            {
                return 0;
            }
            StackFrame topArg = this.TopArg;
            int num = 0;
            for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
            {
                Value value2 = this.buffer[i];
                num += value2.NodeCount;
            }
            return num;
        }

        private void GrowFrames()
        {
            int count = this.frames.Count;
            this.buffer.ReserveAt(this.frames.bounds.end + 1, count);
            this.frames.Grow(count);
        }

        private void GrowStack(int growthNeeded)
        {
            int count = this.stack.bounds.Count;
            if (growthNeeded > count)
            {
                count = growthNeeded;
            }
            this.buffer.ReserveAt(this.stack.bounds.end + 1, count);
            this.stack.Grow(count);
            this.frames.Shift(count);
        }

        internal bool InUse
        {
            get
            {
                if (this.contextOnTopOfStack)
                {
                    return (this.frames.Count > 1);
                }
                return (this.frames.Count > 0);
            }
        }
        internal bool PeekBoolean(int index)
        {
            return this.buffer.buffer[index].GetBoolean();
        }

        internal double PeekDouble(int index)
        {
            return this.buffer.buffer[index].GetDouble();
        }

        internal NodeSequence PeekSequence(int index)
        {
            return this.buffer.buffer[index].GetSequence();
        }

        internal string PeekString(int index)
        {
            return this.buffer.buffer[index].GetString();
        }

        internal void PopFrame(ProcessingContext context)
        {
            StackFrame topArg = this.TopArg;
            for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
            {
                this.buffer.buffer[i].Clear(context);
            }
            this.stack.stackPtr = topArg.basePtr - 1;
            this.frames.stackPtr--;
        }

        internal void PushFrame()
        {
            this.frames.stackPtr++;
            if (this.frames.NeedsGrowth)
            {
                this.GrowFrames();
            }
            this.buffer.buffer[this.frames.stackPtr].StartFrame(this.stack.stackPtr);
        }

        internal void PopSequenceFrameTo(ref EvalStack dest)
        {
            StackFrame topArg = this.TopArg;
            dest.PushFrame();
            int count = topArg.Count;
            switch (count)
            {
                case 0:
                    break;

                case 1:
                    dest.Push(this.buffer.buffer[topArg.basePtr].Sequence);
                    break;

                default:
                    dest.Push(this.buffer.buffer, topArg.basePtr, count);
                    break;
            }
            this.stack.stackPtr = topArg.basePtr - 1;
            this.frames.stackPtr--;
        }

        internal void Push(string val)
        {
            this.stack.stackPtr++;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(1);
            }
            this.buffer.buffer[this.stack.stackPtr].String = val;
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(string val, int addCount)
        {
            int stackPtr = this.stack.stackPtr;
            this.stack.stackPtr += addCount;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(addCount);
            }
            int num2 = stackPtr + addCount;
            while (stackPtr < num2)
            {
                this.buffer.buffer[++stackPtr].String = val;
            }
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(bool val)
        {
            this.stack.stackPtr++;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(1);
            }
            this.buffer.buffer[this.stack.stackPtr].Boolean = val;
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(bool val, int addCount)
        {
            int stackPtr = this.stack.stackPtr;
            this.stack.stackPtr += addCount;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(addCount);
            }
            int num2 = stackPtr + addCount;
            while (stackPtr < num2)
            {
                this.buffer.buffer[++stackPtr].Boolean = val;
            }
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(double val)
        {
            this.stack.stackPtr++;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(1);
            }
            this.buffer.buffer[this.stack.stackPtr].Double = val;
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(double val, int addCount)
        {
            int stackPtr = this.stack.stackPtr;
            this.stack.stackPtr += addCount;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(addCount);
            }
            int num2 = stackPtr + addCount;
            while (stackPtr < num2)
            {
                this.buffer.buffer[++stackPtr].Double = val;
            }
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(NodeSequence val)
        {
            this.stack.stackPtr++;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(1);
            }
            this.buffer.buffer[this.stack.stackPtr].Sequence = val;
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(NodeSequence val, int addCount)
        {
            val.refCount += addCount - 1;
            int stackPtr = this.stack.stackPtr;
            this.stack.stackPtr += addCount;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(addCount);
            }
            int num2 = stackPtr + addCount;
            while (stackPtr < num2)
            {
                this.buffer.buffer[++stackPtr].Sequence = val;
            }
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(Value[] buffer, int startAt, int addCount)
        {
            if (addCount > 0)
            {
                int index = this.stack.stackPtr + 1;
                this.stack.stackPtr += addCount;
                if (this.stack.NeedsGrowth)
                {
                    this.GrowStack(addCount);
                }
                if (1 == addCount)
                {
                    this.buffer.buffer[index] = buffer[startAt];
                }
                else
                {
                    Array.Copy(buffer, startAt, this.buffer.buffer, index, addCount);
                }
                this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
            }
        }

        internal void ReplaceAt(int index, NodeSequence seq)
        {
            this.buffer.buffer[index].Sequence = seq;
        }

        internal void SetValue(ProcessingContext context, int index, bool val)
        {
            this.buffer.buffer[index].Update(context, val);
        }

        internal void SetValue(ProcessingContext context, int index, double val)
        {
            this.buffer.buffer[index].Update(context, val);
        }

        internal void SetValue(ProcessingContext context, int index, string val)
        {
            this.buffer.buffer[index].Update(context, val);
        }

        internal void SetValue(ProcessingContext context, int index, NodeSequence val)
        {
            this.buffer.buffer[index].Update(context, val);
        }

        internal void TransferPositionsTo(ref EvalStack stack)
        {
            StackFrame topArg = this.TopArg;
            stack.PushFrame();
            for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
            {
                NodeSequence sequence = this.buffer.buffer[i].Sequence;
                int count = sequence.Count;
                if ((this.stack.stackPtr + count) > this.stack.bounds.end)
                {
                    this.GrowStack(count);
                }
                for (int j = 0; j < count; j++)
                {
                    stack.Push((double) sequence.Items[j].Position);
                }
            }
        }

        internal void TransferSequenceSizeTo(ref EvalStack stack)
        {
            StackFrame topArg = this.TopArg;
            stack.PushFrame();
            for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
            {
                NodeSequence sequence = this.buffer.buffer[i].Sequence;
                int count = sequence.Count;
                if ((this.stack.stackPtr + count) > this.stack.bounds.end)
                {
                    this.GrowStack(count);
                }
                for (int j = 0; j < count; j++)
                {
                    stack.Push((double) NodeSequence.GetContextSize(sequence, j));
                }
            }
        }
    }
}

