namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web;

    public sealed class WebPartDisplayModeCollection : CollectionBase
    {
        private bool _readOnly;
        private string _readOnlyExceptionMessage;

        internal WebPartDisplayModeCollection()
        {
        }

        public int Add(WebPartDisplayMode value)
        {
            return base.List.Add(value);
        }

        internal int AddInternal(WebPartDisplayMode value)
        {
            int num;
            bool flag = this._readOnly;
            this._readOnly = false;
            try
            {
                try
                {
                    num = base.List.Add(value);
                }
                finally
                {
                    this._readOnly = flag;
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        private void CheckReadOnly()
        {
            if (this._readOnly)
            {
                throw new InvalidOperationException(System.Web.SR.GetString(this._readOnlyExceptionMessage));
            }
        }

        public bool Contains(WebPartDisplayMode value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(WebPartDisplayMode[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(WebPartDisplayMode value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, WebPartDisplayMode value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnClear()
        {
            throw new InvalidOperationException(System.Web.SR.GetString("WebPartDisplayModeCollection_CantRemove"));
        }

        protected override void OnInsert(int index, object value)
        {
            this.CheckReadOnly();
            WebPartDisplayMode mode = (WebPartDisplayMode) value;
            foreach (WebPartDisplayMode mode2 in base.List)
            {
                if (mode.Name == mode2.Name)
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPartDisplayModeCollection_DuplicateName", new object[] { mode.Name }));
                }
            }
            base.OnInsert(index, value);
        }

        protected override void OnRemove(int index, object value)
        {
            throw new InvalidOperationException(System.Web.SR.GetString("WebPartDisplayModeCollection_CantRemove"));
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            throw new InvalidOperationException(System.Web.SR.GetString("WebPartDisplayModeCollection_CantSet"));
        }

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            if (value == null)
            {
                throw new ArgumentNullException("value", System.Web.SR.GetString("Collection_CantAddNull"));
            }
            if (!(value is WebPartDisplayMode))
            {
                throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "WebPartDisplayMode" }), "value");
            }
        }

        internal void SetReadOnly(string exceptionMessage)
        {
            this._readOnlyExceptionMessage = exceptionMessage;
            this._readOnly = true;
        }

        public bool IsReadOnly
        {
            get
            {
                return this._readOnly;
            }
        }

        public WebPartDisplayMode this[int index]
        {
            get
            {
                return (WebPartDisplayMode) base.List[index];
            }
        }

        public WebPartDisplayMode this[string modeName]
        {
            get
            {
                foreach (WebPartDisplayMode mode in base.List)
                {
                    if (string.Equals(mode.Name, modeName, StringComparison.OrdinalIgnoreCase))
                    {
                        return mode;
                    }
                }
                return null;
            }
        }
    }
}

