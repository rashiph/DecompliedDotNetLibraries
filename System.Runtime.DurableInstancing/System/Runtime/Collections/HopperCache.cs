namespace System.Runtime.Collections
{
    using System;
    using System.Collections;
    using System.Threading;

    internal class HopperCache
    {
        private readonly int hopperSize;
        private Hashtable limitedHopper;
        private LastHolder mruEntry;
        private Hashtable outstandingHopper;
        private int promoting;
        private Hashtable strongHopper;
        private readonly bool weak;

        public HopperCache(int hopperSize, bool weak)
        {
            this.hopperSize = hopperSize;
            this.weak = weak;
            this.outstandingHopper = new Hashtable(hopperSize * 2);
            this.strongHopper = new Hashtable(hopperSize * 2);
            this.limitedHopper = new Hashtable(hopperSize * 2);
        }

        public void Add(object key, object value)
        {
            if (this.weak && !object.ReferenceEquals(value, DBNull.Value))
            {
                value = new WeakReference(value);
            }
            if (this.strongHopper.Count >= (this.hopperSize * 2))
            {
                Hashtable limitedHopper = this.limitedHopper;
                limitedHopper.Clear();
                limitedHopper.Add(key, value);
                try
                {
                }
                finally
                {
                    this.limitedHopper = this.strongHopper;
                    this.strongHopper = limitedHopper;
                }
            }
            else
            {
                this.strongHopper[key] = value;
            }
        }

        public object GetValue(object syncObject, object key)
        {
            WeakReference reference;
            object target;
            LastHolder mruEntry = this.mruEntry;
            if ((mruEntry != null) && key.Equals(mruEntry.Key))
            {
                if (!this.weak || ((reference = mruEntry.Value as WeakReference) == null))
                {
                    return mruEntry.Value;
                }
                target = reference.Target;
                if (target != null)
                {
                    return target;
                }
                this.mruEntry = null;
            }
            object obj3 = this.outstandingHopper[key];
            target = (this.weak && ((reference = obj3 as WeakReference) != null)) ? reference.Target : obj3;
            if (target != null)
            {
                this.mruEntry = new LastHolder(key, obj3);
                return target;
            }
            obj3 = this.strongHopper[key];
            target = (this.weak && ((reference = obj3 as WeakReference) != null)) ? reference.Target : obj3;
            if (target == null)
            {
                obj3 = this.limitedHopper[key];
                target = (this.weak && ((reference = obj3 as WeakReference) != null)) ? reference.Target : obj3;
                if (target == null)
                {
                    return null;
                }
            }
            this.mruEntry = new LastHolder(key, obj3);
            int num = 1;
            try
            {
                try
                {
                }
                finally
                {
                    num = Interlocked.CompareExchange(ref this.promoting, 1, 0);
                }
                if (num != 0)
                {
                    return target;
                }
                if (this.outstandingHopper.Count >= this.hopperSize)
                {
                    lock (syncObject)
                    {
                        Hashtable limitedHopper = this.limitedHopper;
                        limitedHopper.Clear();
                        limitedHopper.Add(key, obj3);
                        try
                        {
                        }
                        finally
                        {
                            this.limitedHopper = this.strongHopper;
                            this.strongHopper = this.outstandingHopper;
                            this.outstandingHopper = limitedHopper;
                        }
                        return target;
                    }
                }
                this.outstandingHopper[key] = obj3;
            }
            finally
            {
                if (num == 0)
                {
                    this.promoting = 0;
                }
            }
            return target;
        }

        private class LastHolder
        {
            private readonly object key;
            private readonly object value;

            internal LastHolder(object key, object value)
            {
                this.key = key;
                this.value = value;
            }

            internal object Key
            {
                get
                {
                    return this.key;
                }
            }

            internal object Value
            {
                get
                {
                    return this.value;
                }
            }
        }
    }
}

