namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct InterlockedGate
    {
        internal const int Open = 0;
        internal const int Held = 1;
        internal const int Triggered = 2;
        internal const int Closed = 3;
        private int m_State;
        internal void Reset()
        {
            this.m_State = 0;
        }

        internal bool Trigger(bool exclusive)
        {
            int num = Interlocked.CompareExchange(ref this.m_State, 2, 0);
            if (exclusive && ((num == 1) || (num == 2)))
            {
                throw new InternalException();
            }
            return (num == 0);
        }

        internal bool StartTrigger(bool exclusive)
        {
            int num = Interlocked.CompareExchange(ref this.m_State, 1, 0);
            if (exclusive && ((num == 1) || (num == 2)))
            {
                throw new InternalException();
            }
            return (num == 0);
        }

        internal void FinishTrigger()
        {
            if (Interlocked.CompareExchange(ref this.m_State, 2, 1) != 1)
            {
                throw new InternalException();
            }
        }

        internal bool Complete()
        {
            int num;
            while ((num = Interlocked.CompareExchange(ref this.m_State, 3, 2)) != 2)
            {
                switch (num)
                {
                    case 3:
                        return false;

                    case 0:
                    {
                        if (Interlocked.CompareExchange(ref this.m_State, 3, 0) == 0)
                        {
                            return false;
                        }
                        continue;
                    }
                }
                Thread.SpinWait(1);
            }
            return true;
        }
    }
}

