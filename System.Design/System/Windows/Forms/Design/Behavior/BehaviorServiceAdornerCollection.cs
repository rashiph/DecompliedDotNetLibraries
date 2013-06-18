namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class BehaviorServiceAdornerCollection : CollectionBase
    {
        private BehaviorService behaviorService;

        public BehaviorServiceAdornerCollection(BehaviorService behaviorService)
        {
            this.behaviorService = behaviorService;
        }

        public BehaviorServiceAdornerCollection(BehaviorServiceAdornerCollection value)
        {
            this.AddRange(value);
        }

        public BehaviorServiceAdornerCollection(Adorner[] value)
        {
            this.AddRange(value);
        }

        public int Add(Adorner value)
        {
            value.BehaviorService = this.behaviorService;
            return base.List.Add(value);
        }

        public void AddRange(Adorner[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(BehaviorServiceAdornerCollection value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(Adorner value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(Adorner[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public BehaviorServiceAdornerCollectionEnumerator GetEnumerator()
        {
            return new BehaviorServiceAdornerCollectionEnumerator(this);
        }

        public int IndexOf(Adorner value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, Adorner value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(Adorner value)
        {
            base.List.Remove(value);
        }

        public Adorner this[int index]
        {
            get
            {
                return (Adorner) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

