namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class XmlAnyElementAttributes : CollectionBase
    {
        public int Add(XmlAnyElementAttribute attribute)
        {
            return base.List.Add(attribute);
        }

        public bool Contains(XmlAnyElementAttribute attribute)
        {
            return base.List.Contains(attribute);
        }

        public void CopyTo(XmlAnyElementAttribute[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(XmlAnyElementAttribute attribute)
        {
            return base.List.IndexOf(attribute);
        }

        public void Insert(int index, XmlAnyElementAttribute attribute)
        {
            base.List.Insert(index, attribute);
        }

        public void Remove(XmlAnyElementAttribute attribute)
        {
            base.List.Remove(attribute);
        }

        public XmlAnyElementAttribute this[int index]
        {
            get
            {
                return (XmlAnyElementAttribute) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

