namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    internal class XDeferredAxis<T> : IEnumerable<T>, IEnumerable where T: XObject
    {
        internal XElement element;
        private Func<XElement, XName, IEnumerable<T>> func;
        internal XName name;

        public XDeferredAxis(Func<XElement, XName, IEnumerable<T>> func, XElement element, XName name)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            this.func = func;
            this.element = element;
            this.name = name;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.func(this.element, this.name).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerable<T> this[string expandedName]
        {
            get
            {
                if (expandedName == null)
                {
                    throw new ArgumentNullException("expandedName");
                }
                if (this.name == null)
                {
                    this.name = expandedName;
                }
                else if (this.name != expandedName)
                {
                    return Enumerable.Empty<T>();
                }
                return this;
            }
        }
    }
}

