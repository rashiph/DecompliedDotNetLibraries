namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    [DefaultEvent("CollectionChanged")]
    public class BindingsCollection : BaseCollection
    {
        private ArrayList list;

        [System.Windows.Forms.SRDescription("collectionChangedEventDescr")]
        public event CollectionChangeEventHandler CollectionChanged;

        [System.Windows.Forms.SRDescription("collectionChangingEventDescr")]
        public event CollectionChangeEventHandler CollectionChanging;

        internal BindingsCollection()
        {
        }

        protected internal void Add(Binding binding)
        {
            CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Add, binding);
            this.OnCollectionChanging(e);
            this.AddCore(binding);
            this.OnCollectionChanged(e);
        }

        protected virtual void AddCore(Binding dataBinding)
        {
            if (dataBinding == null)
            {
                throw new ArgumentNullException("dataBinding");
            }
            this.List.Add(dataBinding);
        }

        protected internal void Clear()
        {
            CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null);
            this.OnCollectionChanging(e);
            this.ClearCore();
            this.OnCollectionChanged(e);
        }

        protected virtual void ClearCore()
        {
            this.List.Clear();
        }

        protected virtual void OnCollectionChanged(CollectionChangeEventArgs ccevent)
        {
            if (this.onCollectionChanged != null)
            {
                this.onCollectionChanged(this, ccevent);
            }
        }

        protected virtual void OnCollectionChanging(CollectionChangeEventArgs e)
        {
            if (this.onCollectionChanging != null)
            {
                this.onCollectionChanging(this, e);
            }
        }

        protected internal void Remove(Binding binding)
        {
            CollectionChangeEventArgs e = new CollectionChangeEventArgs(CollectionChangeAction.Remove, binding);
            this.OnCollectionChanging(e);
            this.RemoveCore(binding);
            this.OnCollectionChanged(e);
        }

        protected internal void RemoveAt(int index)
        {
            this.Remove(this[index]);
        }

        protected virtual void RemoveCore(Binding dataBinding)
        {
            this.List.Remove(dataBinding);
        }

        protected internal bool ShouldSerializeMyAll()
        {
            return (this.Count > 0);
        }

        public override int Count
        {
            get
            {
                if (this.list == null)
                {
                    return 0;
                }
                return base.Count;
            }
        }

        public Binding this[int index]
        {
            get
            {
                return (Binding) this.List[index];
            }
        }

        protected override ArrayList List
        {
            get
            {
                if (this.list == null)
                {
                    this.list = new ArrayList();
                }
                return this.list;
            }
        }
    }
}

