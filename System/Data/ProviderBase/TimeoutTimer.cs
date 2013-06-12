namespace System.Data.ProviderBase
{
    using System;
    using System.Data.Common;

    internal class TimeoutTimer
    {
        private bool _isInfiniteTimeout;
        private long _timerExpire;
        internal static readonly long InfiniteTimeout;

        internal void SetTimeoutSeconds(int seconds)
        {
            if (InfiniteTimeout == seconds)
            {
                this._isInfiniteTimeout = true;
            }
            else
            {
                this._timerExpire = ADP.TimerCurrent() + ADP.TimerFromSeconds(seconds);
                this._isInfiniteTimeout = false;
            }
        }

        internal static TimeoutTimer StartMillisecondsTimeout(long milliseconds)
        {
            return new TimeoutTimer { _timerExpire = ADP.TimerCurrent() + (milliseconds * 0x2710L), _isInfiniteTimeout = false };
        }

        internal static TimeoutTimer StartSecondsTimeout(int seconds)
        {
            TimeoutTimer timer = new TimeoutTimer();
            timer.SetTimeoutSeconds(seconds);
            return timer;
        }

        internal bool IsExpired
        {
            get
            {
                return (!this.IsInfinite && ADP.TimerHasExpired(this._timerExpire));
            }
        }

        internal bool IsInfinite
        {
            get
            {
                return this._isInfiniteTimeout;
            }
        }

        internal long LegacyTimerExpire
        {
            get
            {
                if (!this._isInfiniteTimeout)
                {
                    return this._timerExpire;
                }
                return 0x7fffffffffffffffL;
            }
        }

        internal long MillisecondsRemaining
        {
            get
            {
                if (this._isInfiniteTimeout)
                {
                    return 0x7fffffffffffffffL;
                }
                long num = ADP.TimerRemainingMilliseconds(this._timerExpire);
                if (0L > num)
                {
                    num = 0L;
                }
                return num;
            }
        }
    }
}

