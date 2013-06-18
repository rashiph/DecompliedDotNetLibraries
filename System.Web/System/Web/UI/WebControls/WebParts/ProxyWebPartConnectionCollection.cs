namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Web;

    [Editor("System.ComponentModel.Design.CollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public sealed class ProxyWebPartConnectionCollection : CollectionBase
    {
        private WebPartManager _webPartManager;

        public int Add(WebPartConnection value)
        {
            return base.List.Add(value);
        }

        private void CheckReadOnly()
        {
            if (this.IsReadOnly)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("ProxyWebPartConnectionCollection_ReadOnly"));
            }
        }

        public bool Contains(WebPartConnection value)
        {
            return base.List.Contains(value);
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
            if (this._webPartManager != null)
            {
                foreach (WebPartConnection connection in this)
                {
                    this._webPartManager.StaticConnections.Remove(connection);
                }
            }
            base.OnClear();
        }

        protected override void OnInsert(int index, object value)
        {
            this.CheckReadOnly();
            if (this._webPartManager != null)
            {
                this._webPartManager.StaticConnections.Insert(index, (WebPartConnection) value);
            }
            base.OnInsert(index, value);
        }

        protected override void OnRemove(int index, object value)
        {
            this.CheckReadOnly();
            if (this._webPartManager != null)
            {
                this._webPartManager.StaticConnections.Remove((WebPartConnection) value);
            }
            base.OnRemove(index, value);
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            this.CheckReadOnly();
            if (this._webPartManager != null)
            {
                int num = this._webPartManager.StaticConnections.IndexOf((WebPartConnection) oldValue);
                this._webPartManager.StaticConnections[num] = (WebPartConnection) newValue;
            }
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
                throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "WebPartConnection" }));
            }
        }

        public void Remove(WebPartConnection value)
        {
            base.List.Remove(value);
        }

        internal void SetWebPartManager(WebPartManager webPartManager)
        {
            this._webPartManager = webPartManager;
            foreach (WebPartConnection connection in this)
            {
                this._webPartManager.StaticConnections.Add(connection);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((this._webPartManager != null) && this._webPartManager.StaticConnections.IsReadOnly);
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
                    if ((connection != null) && string.Equals(connection.ID, id, StringComparison.OrdinalIgnoreCase))
                    {
                        return connection;
                    }
                }
                return null;
            }
        }
    }
}

