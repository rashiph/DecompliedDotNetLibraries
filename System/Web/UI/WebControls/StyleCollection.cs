namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.UI;

    public class StyleCollection : StateManagedCollection
    {
        private static readonly Type[] knownTypes = new Type[] { typeof(Style) };

        internal StyleCollection()
        {
        }

        public int Add(Style style)
        {
            return ((IList) this).Add(style);
        }

        public bool Contains(Style style)
        {
            return ((IList) this).Contains(style);
        }

        public void CopyTo(Style[] styleArray, int index)
        {
            base.CopyTo(styleArray, index);
        }

        protected override object CreateKnownType(int index)
        {
            return new Style();
        }

        protected override Type[] GetKnownTypes()
        {
            return knownTypes;
        }

        public int IndexOf(Style style)
        {
            return ((IList) this).IndexOf(style);
        }

        public void Insert(int index, Style style)
        {
            ((IList) this).Insert(index, style);
        }

        public void Remove(Style style)
        {
            ((IList) this).Remove(style);
        }

        public void RemoveAt(int index)
        {
            ((IList) this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o)
        {
            if (o is Style)
            {
                ((Style) o).SetDirty();
            }
        }

        public Style this[int i]
        {
            get
            {
                return (Style) this[i];
            }
            set
            {
                this[i] = value;
            }
        }
    }
}

