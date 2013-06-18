namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class HtmlElementCollection : ICollection, IEnumerable
    {
        private HtmlElement[] elementsArray;
        private UnsafeNativeMethods.IHTMLElementCollection htmlElementCollection;
        private HtmlShimManager shimManager;

        internal HtmlElementCollection(HtmlShimManager shimManager)
        {
            this.htmlElementCollection = null;
            this.elementsArray = null;
            this.shimManager = shimManager;
        }

        internal HtmlElementCollection(HtmlShimManager shimManager, UnsafeNativeMethods.IHTMLElementCollection elements)
        {
            this.htmlElementCollection = elements;
            this.elementsArray = null;
            this.shimManager = shimManager;
        }

        internal HtmlElementCollection(HtmlShimManager shimManager, HtmlElement[] array)
        {
            this.htmlElementCollection = null;
            this.elementsArray = array;
            this.shimManager = shimManager;
        }

        public HtmlElementCollection GetElementsByName(string name)
        {
            int count = this.Count;
            HtmlElement[] elementArray = new HtmlElement[count];
            int index = 0;
            for (int i = 0; i < count; i++)
            {
                HtmlElement element = this[i];
                if (element.GetAttribute("name") == name)
                {
                    elementArray[index] = element;
                    index++;
                }
            }
            if (index == 0)
            {
                return new HtmlElementCollection(this.shimManager);
            }
            HtmlElement[] array = new HtmlElement[index];
            for (int j = 0; j < index; j++)
            {
                array[j] = elementArray[j];
            }
            return new HtmlElementCollection(this.shimManager, array);
        }

        public IEnumerator GetEnumerator()
        {
            HtmlElement[] array = new HtmlElement[this.Count];
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
                if (this.NativeHtmlElementCollection != null)
                {
                    return this.NativeHtmlElementCollection.GetLength();
                }
                if (this.elementsArray != null)
                {
                    return this.elementsArray.Length;
                }
                return 0;
            }
        }

        public HtmlElement this[int index]
        {
            get
            {
                if ((index < 0) || (index >= this.Count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "index", index, 0, this.Count - 1 }));
                }
                if (this.NativeHtmlElementCollection != null)
                {
                    UnsafeNativeMethods.IHTMLElement element = this.NativeHtmlElementCollection.Item(index, 0) as UnsafeNativeMethods.IHTMLElement;
                    if (element == null)
                    {
                        return null;
                    }
                    return new HtmlElement(this.shimManager, element);
                }
                if (this.elementsArray != null)
                {
                    return this.elementsArray[index];
                }
                return null;
            }
        }

        public HtmlElement this[string elementId]
        {
            get
            {
                if (this.NativeHtmlElementCollection != null)
                {
                    UnsafeNativeMethods.IHTMLElement element = this.NativeHtmlElementCollection.Item(elementId, 0) as UnsafeNativeMethods.IHTMLElement;
                    if (element == null)
                    {
                        return null;
                    }
                    return new HtmlElement(this.shimManager, element);
                }
                if (this.elementsArray != null)
                {
                    int length = this.elementsArray.Length;
                    for (int i = 0; i < length; i++)
                    {
                        HtmlElement element2 = this.elementsArray[i];
                        if (element2.Id == elementId)
                        {
                            return element2;
                        }
                    }
                }
                return null;
            }
        }

        private UnsafeNativeMethods.IHTMLElementCollection NativeHtmlElementCollection
        {
            get
            {
                return this.htmlElementCollection;
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

