namespace System.Deployment.Application
{
    using System;
    using System.Threading;

    internal static class LifetimeManager
    {
        private static bool _immediate;
        private static bool _lifetimeEnded;
        private static ManualResetEvent _lifetimeEndedEvent = new ManualResetEvent(false);
        private static bool _lifetimeExtended;
        private static int _operationsInProgress;
        private static Timer _periodicTimer = new Timer(new TimerCallback(LifetimeManager.PeriodicTimerCallback), null, 0x927c0, 0x927c0);
        private static object _TimerLock = new object();

        private static void CheckAlive()
        {
            if (_lifetimeEnded)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_LifetimeEnded"));
            }
        }

        public static void EndImmediately()
        {
            lock (_TimerLock)
            {
                if (_operationsInProgress != 0)
                {
                    Logger.StartCurrentThreadLogging();
                    Logger.AddPhaseInformation(Resources.GetString("Life_OperationsInProgress"), new object[] { _operationsInProgress });
                    Logger.EndCurrentThreadLogging();
                }
                _lifetimeEndedEvent.Set();
                _lifetimeEnded = true;
                _immediate = true;
            }
        }

        public static void EndOperation()
        {
            lock (_TimerLock)
            {
                CheckAlive();
                _operationsInProgress--;
                _lifetimeExtended = true;
            }
        }

        public static void ExtendLifetime()
        {
            lock (_TimerLock)
            {
                CheckAlive();
                _lifetimeExtended = true;
            }
        }

        private static void PeriodicTimerCallback(object state)
        {
            lock (_TimerLock)
            {
                if ((_operationsInProgress == 0) && !_lifetimeExtended)
                {
                    _lifetimeEndedEvent.Set();
                    _lifetimeEnded = true;
                    _periodicTimer.Dispose();
                }
                else
                {
                    _lifetimeExtended = false;
                }
            }
        }

        public static void StartOperation()
        {
            lock (_TimerLock)
            {
                CheckAlive();
                _operationsInProgress++;
            }
        }

        public static bool WaitForEnd()
        {
            _lifetimeEndedEvent.WaitOne();
            return _immediate;
        }
    }
}

