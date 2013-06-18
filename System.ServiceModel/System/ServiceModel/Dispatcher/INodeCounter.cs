namespace System.ServiceModel.Dispatcher
{
    using System;

    internal interface INodeCounter
    {
        int ElapsedCount(int marker);
        void Increase();
        void IncreaseBy(int count);

        int CounterMarker { get; set; }

        int MaxCounter { set; }
    }
}

