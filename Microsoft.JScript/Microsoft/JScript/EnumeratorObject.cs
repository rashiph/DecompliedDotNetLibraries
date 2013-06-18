namespace Microsoft.JScript
{
    using System;
    using System.Collections;

    public class EnumeratorObject : JSObject
    {
        private IEnumerable collection;
        protected IEnumerator enumerator;
        private object obj;

        internal EnumeratorObject(EnumeratorPrototype parent) : base(parent)
        {
            this.enumerator = null;
            this.collection = null;
            base.noExpando = false;
        }

        internal EnumeratorObject(EnumeratorPrototype parent, IEnumerable collection) : base(parent)
        {
            this.collection = collection;
            if (collection != null)
            {
                this.enumerator = collection.GetEnumerator();
            }
            this.LoadObject();
            base.noExpando = false;
        }

        internal virtual bool atEnd()
        {
            if (this.enumerator != null)
            {
                return (this.obj == null);
            }
            return true;
        }

        internal virtual object item()
        {
            if (this.enumerator != null)
            {
                return this.obj;
            }
            return null;
        }

        protected void LoadObject()
        {
            if ((this.enumerator != null) && this.enumerator.MoveNext())
            {
                this.obj = this.enumerator.Current;
            }
            else
            {
                this.obj = null;
            }
        }

        internal virtual void moveFirst()
        {
            if (this.collection != null)
            {
                this.enumerator = this.collection.GetEnumerator();
            }
            this.LoadObject();
        }

        internal virtual void moveNext()
        {
            if (this.enumerator != null)
            {
                this.LoadObject();
            }
        }
    }
}

