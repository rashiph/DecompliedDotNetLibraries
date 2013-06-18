namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.Reflection;
    using System.Xml.Linq;

    internal class XDeferredSingleton<T> where T: XObject
    {
        internal XElement element;
        private Func<XElement, XName, T> func;
        internal XName name;

        public XDeferredSingleton(Func<XElement, XName, T> func, XElement element, XName name)
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

        public T this[string expandedName]
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
                    return default(T);
                }
                return this.func(this.element, this.name);
            }
        }
    }
}

