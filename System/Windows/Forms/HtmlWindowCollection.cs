namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class HtmlWindowCollection : ICollection, IEnumerable
    {
        private UnsafeNativeMethods.IHTMLFramesCollection2 htmlFramesCollection2;
        private HtmlShimManager shimManager;

        internal HtmlWindowCollection(HtmlShimManager shimManager, UnsafeNativeMethods.IHTMLFramesCollection2 collection)
        {
            this.htmlFramesCollection2 = collection;
            this.shimManager = shimManager;
        }

        public IEnumerator GetEnumerator()
        {
            HtmlWindow[] array = new HtmlWindow[this.Count];
            ((ICollection) this).CopyTo(array, 0);
            return array.GetEnumerator();
        }

        void ICollection.CopyTo(Array dest, int index)
        {
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                dest.SetValue(this[i], index++);
            }
        }

        public int Count
        {
            get
            {
                return this.NativeHTMLFramesCollection2.GetLength();
            }
        }

        public HtmlWindow this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "index", index, 0, this.Count - 1 }));
                }
                object idOrName = index;
                UnsafeNativeMethods.IHTMLWindow2 win = this.NativeHTMLFramesCollection2.Item(ref idOrName) as UnsafeNativeMethods.IHTMLWindow2;
                if (win == null)
                {
                    return null;
                }
                return new HtmlWindow(this.shimManager, win);
            }
        }

        public HtmlWindow this[string windowId]
        {
            get
            {
                object idOrName = windowId;
                UnsafeNativeMethods.IHTMLWindow2 win = null;
                try
                {
                    win = this.htmlFramesCollection2.Item(ref idOrName) as UnsafeNativeMethods.IHTMLWindow2;
                }
                catch (COMException)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "windowId", windowId }));
                }
                if (win == null)
                {
                    return null;
                }
                return new HtmlWindow(this.shimManager, win);
            }
        }

        private UnsafeNativeMethods.IHTMLFramesCollection2 NativeHTMLFramesCollection2
        {
            get
            {
                return this.htmlFramesCollection2;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

