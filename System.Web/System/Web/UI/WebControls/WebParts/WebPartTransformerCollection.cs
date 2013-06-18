namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web;

    public sealed class WebPartTransformerCollection : CollectionBase
    {
        private bool _readOnly;

        public int Add(WebPartTransformer transformer)
        {
            return base.List.Add(transformer);
        }

        private void CheckReadOnly()
        {
            if (this._readOnly)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartTransformerCollection_ReadOnly"));
            }
        }

        public bool Contains(WebPartTransformer transformer)
        {
            return base.List.Contains(transformer);
        }

        public void CopyTo(WebPartTransformer[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(WebPartTransformer transformer)
        {
            return base.List.IndexOf(transformer);
        }

        public void Insert(int index, WebPartTransformer transformer)
        {
            base.List.Insert(index, transformer);
        }

        protected override void OnClear()
        {
            this.CheckReadOnly();
            base.OnClear();
        }

        protected override void OnInsert(int index, object value)
        {
            this.CheckReadOnly();
            if (base.List.Count > 0)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("WebPartTransformerCollection_NotEmpty"));
            }
            base.OnInsert(index, value);
        }

        protected override void OnRemove(int index, object value)
        {
            this.CheckReadOnly();
            base.OnRemove(index, value);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            this.CheckReadOnly();
            base.OnSet(index, oldValue, newValue);
        }

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            if (value == null)
            {
                throw new ArgumentNullException("value", System.Web.SR.GetString("Collection_CantAddNull"));
            }
            if (!(value is WebPartTransformer))
            {
                throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "WebPartTransformer" }), "value");
            }
        }

        public void Remove(WebPartTransformer transformer)
        {
            base.List.Remove(transformer);
        }

        internal void SetReadOnly()
        {
            this._readOnly = true;
        }

        public bool IsReadOnly
        {
            get
            {
                return this._readOnly;
            }
        }

        public WebPartTransformer this[int index]
        {
            get
            {
                return (WebPartTransformer) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

