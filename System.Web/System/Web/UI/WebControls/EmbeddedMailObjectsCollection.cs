namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Web;

    [Editor("System.Web.UI.Design.WebControls.EmbeddedMailObjectCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public sealed class EmbeddedMailObjectsCollection : CollectionBase
    {
        public int Add(EmbeddedMailObject value)
        {
            return base.List.Add(value);
        }

        public bool Contains(EmbeddedMailObject value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(EmbeddedMailObject[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(EmbeddedMailObject value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, EmbeddedMailObject value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            if (value == null)
            {
                throw new ArgumentNullException("value", System.Web.SR.GetString("Collection_CantAddNull"));
            }
            if (!(value is EmbeddedMailObject))
            {
                throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "EmbeddedMailObject" }), "value");
            }
        }

        public void Remove(EmbeddedMailObject value)
        {
            base.List.Remove(value);
        }

        public EmbeddedMailObject this[int index]
        {
            get
            {
                return (EmbeddedMailObject) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

