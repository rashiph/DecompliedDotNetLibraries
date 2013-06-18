namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Web;

    [Editor("System.ComponentModel.Design.CollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public sealed class WebPartConnectionCollection : CollectionBase
    {
        private bool _readOnly;
        private string _readOnlyExceptionMessage;
        private WebPartManager _webPartManager;

        internal WebPartConnectionCollection(WebPartManager webPartManager)
        {
            this._webPartManager = webPartManager;
        }

        public int Add(WebPartConnection value)
        {
            return base.List.Add(value);
        }

        private void CheckReadOnly()
        {
            if (this._readOnly)
            {
                throw new InvalidOperationException(System.Web.SR.GetString(this._readOnlyExceptionMessage));
            }
        }

        public bool Contains(WebPartConnection value)
        {
            return base.List.Contains(value);
        }

        internal bool ContainsProvider(WebPart provider)
        {
            foreach (WebPartConnection connection in base.List)
            {
                if (connection.Provider == provider)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(WebPartConnection[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(WebPartConnection value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, WebPartConnection value)
        {
            base.List.Insert(index, value);
        }

        protected override void OnClear()
        {
            this.CheckReadOnly();
            base.OnClear();
        }

        protected override void OnInsert(int index, object value)
        {
            this.CheckReadOnly();
            ((WebPartConnection) value).SetWebPartManager(this._webPartManager);
            base.OnInsert(index, value);
        }

        protected override void OnRemove(int index, object value)
        {
            this.CheckReadOnly();
            ((WebPartConnection) value).SetWebPartManager(null);
            base.OnRemove(index, value);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            this.CheckReadOnly();
            ((WebPartConnection) oldValue).SetWebPartManager(null);
            ((WebPartConnection) newValue).SetWebPartManager(this._webPartManager);
            base.OnSet(index, oldValue, newValue);
        }

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            if (value == null)
            {
                throw new ArgumentNullException("value", System.Web.SR.GetString("Collection_CantAddNull"));
            }
            if (!(value is WebPartConnection))
            {
                throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "WebPartConnection" }), "value");
            }
        }

        public void Remove(WebPartConnection value)
        {
            base.List.Remove(value);
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

        public WebPartConnection this[int index]
        {
            get
            {
                return (WebPartConnection) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public WebPartConnection this[string id]
        {
            get
            {
                foreach (WebPartConnection connection in base.List)
                {
                    if (string.Equals(connection.ID, id, StringComparison.OrdinalIgnoreCase))
                    {
                        return connection;
                    }
                }
                return null;
            }
        }
    }
}

