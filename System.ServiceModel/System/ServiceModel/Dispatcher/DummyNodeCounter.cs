namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class DummyNodeCounter : INodeCounter
    {
        internal static DummyNodeCounter Dummy = new DummyNodeCounter();

        public int ElapsedCount(int marker)
        {
            return 0;
        }

        public void Increase()
        {
        }

        public void IncreaseBy(int count)
        {
        }

        public int CounterMarker
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public int MaxCounter
        {
            set
            {
            }
        }
    }
}

