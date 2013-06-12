namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    public class CounterCreationDataCollection : CollectionBase
    {
        public CounterCreationDataCollection()
        {
        }

        public CounterCreationDataCollection(CounterCreationDataCollection value)
        {
            this.AddRange(value);
        }

        public CounterCreationDataCollection(CounterCreationData[] value)
        {
            this.AddRange(value);
        }

        public int Add(CounterCreationData value)
        {
            return base.List.Add(value);
        }

        public void AddRange(CounterCreationData[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(CounterCreationDataCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(CounterCreationData value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(CounterCreationData[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(CounterCreationData value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, CounterCreationData value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(value is CounterCreationData))
            {
                throw new ArgumentException(SR.GetString("MustAddCounterCreationData"));
            }
        }

        public virtual void Remove(CounterCreationData value)
        {
            base.List.Remove(value);
        }

        public CounterCreationData this[int index]
        {
            get
            {
                return (CounterCreationData) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

