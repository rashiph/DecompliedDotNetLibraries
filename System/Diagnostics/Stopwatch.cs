namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;

    public class Stopwatch
    {
        private long elapsed;
        public static readonly long Frequency;
        public static readonly bool IsHighResolution;
        private bool isRunning;
        private long startTimeStamp;
        private static readonly double tickFrequency;
        private const long TicksPerMillisecond = 0x2710L;
        private const long TicksPerSecond = 0x989680L;

        static Stopwatch()
        {
            if (!Microsoft.Win32.SafeNativeMethods.QueryPerformanceFrequency(out Frequency))
            {
                IsHighResolution = false;
                Frequency = 0x989680L;
                tickFrequency = 1.0;
            }
            else
            {
                IsHighResolution = true;
                tickFrequency = 10000000.0;
                tickFrequency /= (double) Frequency;
            }
        }

        public Stopwatch()
        {
            this.Reset();
        }

        private long GetElapsedDateTimeTicks()
        {
            long rawElapsedTicks = this.GetRawElapsedTicks();
            if (IsHighResolution)
            {
                double num2 = rawElapsedTicks;
                num2 *= tickFrequency;
                return (long) num2;
            }
            return rawElapsedTicks;
        }

        private long GetRawElapsedTicks()
        {
            long elapsed = this.elapsed;
            if (this.isRunning)
            {
                long num3 = GetTimestamp() - this.startTimeStamp;
                elapsed += num3;
            }
            return elapsed;
        }

        public static long GetTimestamp()
        {
            if (IsHighResolution)
            {
                long num = 0L;
                Microsoft.Win32.SafeNativeMethods.QueryPerformanceCounter(out num);
                return num;
            }
            return DateTime.UtcNow.Ticks;
        }

        public void Reset()
        {
            this.elapsed = 0L;
            this.isRunning = false;
            this.startTimeStamp = 0L;
        }

        public void Restart()
        {
            this.elapsed = 0L;
            this.startTimeStamp = GetTimestamp();
            this.isRunning = true;
        }

        public void Start()
        {
            if (!this.isRunning)
            {
                this.startTimeStamp = GetTimestamp();
                this.isRunning = true;
            }
        }

        public static Stopwatch StartNew()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            return stopwatch;
        }

        public void Stop()
        {
            if (this.isRunning)
            {
                long num2 = GetTimestamp() - this.startTimeStamp;
                this.elapsed += num2;
                this.isRunning = false;
                if (this.elapsed < 0L)
                {
                    this.elapsed = 0L;
                }
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                return new TimeSpan(this.GetElapsedDateTimeTicks());
            }
        }

        public long ElapsedMilliseconds
        {
            get
            {
                return (this.GetElapsedDateTimeTicks() / 0x2710L);
            }
        }

        public long ElapsedTicks
        {
            get
            {
                return this.GetRawElapsedTicks();
            }
        }

        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }
    }
}

