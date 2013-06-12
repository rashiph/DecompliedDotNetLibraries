namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class XmlElementAttributes : CollectionBase
    {
        public int Add(XmlElementAttribute attribute)
        {
            return base.List.Add(attribute);
        }

        public bool Contains(XmlElementAttribute attribute)
        {
            return base.List.Contains(attribute);
        }

        public void CopyTo(XmlElementAttribute[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(XmlElementAttribute attribute)
        {
            return base.List.IndexOf(attribute);
        }

        public void Insert(int index, XmlElementAttribute attribute)
        {
            base.List.Insert(index, attribute);
        }

        public void Remove(XmlElementAttribute attribute)
        {
            base.List.Remove(attribute);
        }

        public XmlElementAttribute this[int index]
        {
            get
            {
                return (XmlElementAttribute) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

