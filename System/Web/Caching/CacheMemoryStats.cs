namespace System.Web.Caching
{
    using System;
    using System.Web.Configuration;

    internal class CacheMemoryStats
    {
        private long _lastTrimCount;
        private long _lastTrimDurationTicks;
        private int _lastTrimGen2Count = -1;
        private int _lastTrimPercent;
        private DateTime _lastTrimTime = DateTime.MinValue;
        private CacheMemorySizePressure _pressureCacheSize;
        private CacheMemoryTotalMemoryPressure _pressureTotalMemory = new CacheMemoryTotalMemoryPressure();
        private long _totalCountBeforeTrim;

        internal CacheMemoryStats(SRef sizedRef)
        {
            this._pressureCacheSize = new CacheMemorySizePressure(sizedRef);
        }

        internal void Dispose()
        {
            this._pressureCacheSize.Dispose();
        }

        internal int GetPercentToTrim()
        {
            if (GC.CollectionCount(2) != this._lastTrimGen2Count)
            {
                return Math.Max(this._pressureTotalMemory.GetPercentToTrim(this._lastTrimTime, this._lastTrimPercent), this._pressureCacheSize.GetPercentToTrim(this._lastTrimTime, this._lastTrimPercent));
            }
            return 0;
        }

        internal bool IsAboveHighPressure()
        {
            if (!this._pressureTotalMemory.IsAboveHighPressure())
            {
                return this._pressureCacheSize.IsAboveHighPressure();
            }
            return true;
        }

        internal bool IsAboveMediumPressure()
        {
            if (!this._pressureTotalMemory.IsAboveMediumPressure())
            {
                return this._pressureCacheSize.IsAboveMediumPressure();
            }
            return true;
        }

        internal void ReadConfig(CacheSection cacheSection)
        {
            this._pressureTotalMemory.ReadConfig(cacheSection);
            this._pressureCacheSize.ReadConfig(cacheSection);
        }

        internal void SetTrimStats(long trimDurationTicks, long totalCountBeforeTrim, long trimCount)
        {
            this._lastTrimDurationTicks = trimDurationTicks;
            int num = GC.CollectionCount(2);
            if (num != this._lastTrimGen2Count)
            {
                this._lastTrimTime = DateTime.UtcNow;
                this._totalCountBeforeTrim = totalCountBeforeTrim;
                this._lastTrimCount = trimCount;
            }
            else
            {
                this._lastTrimCount += trimCount;
            }
            this._lastTrimGen2Count = num;
            this._lastTrimPercent = (int) ((this._lastTrimCount * 100L) / this._totalCountBeforeTrim);
        }

        internal void Update()
        {
            this._pressureTotalMemory.Update();
            this._pressureCacheSize.Update();
        }

        internal CacheMemorySizePressure CacheSizePressure
        {
            get
            {
                return this._pressureCacheSize;
            }
        }

        internal CacheMemoryTotalMemoryPressure TotalMemoryPressure
        {
            get
            {
                return this._pressureTotalMemory;
            }
        }
    }
}

