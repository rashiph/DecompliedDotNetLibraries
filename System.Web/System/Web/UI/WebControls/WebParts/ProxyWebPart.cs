namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [ToolboxItem(false)]
    public abstract class ProxyWebPart : WebPart
    {
        private string _genericWebPartID;
        private string _originalID;
        private string _originalPath;
        private string _originalTypeName;

        protected ProxyWebPart(WebPart webPart)
        {
            if (webPart == null)
            {
                throw new ArgumentNullException("webPart");
            }
            GenericWebPart part = webPart as GenericWebPart;
            if (part != null)
            {
                Type type;
                Control childControl = part.ChildControl;
                if (childControl == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNull", new object[] { "ChildControl" }), "webPart");
                }
                this._originalID = childControl.ID;
                if (string.IsNullOrEmpty(this._originalID))
                {
                    throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNullOrEmptyString", new object[] { "ChildControl.ID" }), "webPart");
                }
                UserControl control2 = childControl as UserControl;
                if (control2 != null)
                {
                    type = typeof(UserControl);
                    this._originalPath = control2.AppRelativeVirtualPath;
                }
                else
                {
                    type = childControl.GetType();
                }
                this._originalTypeName = WebPartUtil.SerializeType(type);
                this._genericWebPartID = part.ID;
                if (string.IsNullOrEmpty(this._genericWebPartID))
                {
                    throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNullOrEmptyString", new object[] { "ID" }), "webPart");
                }
                this.ID = this._genericWebPartID;
            }
            else
            {
                this._originalID = webPart.ID;
                if (string.IsNullOrEmpty(this._originalID))
                {
                    throw new ArgumentException(System.Web.SR.GetString("PropertyCannotBeNullOrEmptyString", new object[] { "ID" }), "webPart");
                }
                this._originalTypeName = WebPartUtil.SerializeType(webPart.GetType());
                this.ID = this._originalID;
            }
        }

        protected ProxyWebPart(string originalID, string originalTypeName, string originalPath, string genericWebPartID)
        {
            if (string.IsNullOrEmpty(originalID))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("originalID");
            }
            if (string.IsNullOrEmpty(originalTypeName))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("originalTypeName");
            }
            if (!string.IsNullOrEmpty(originalPath) && string.IsNullOrEmpty(genericWebPartID))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("genericWebPartID");
            }
            this._originalID = originalID;
            this._originalTypeName = originalTypeName;
            this._originalPath = originalPath;
            this._genericWebPartID = genericWebPartID;
            if (!string.IsNullOrEmpty(genericWebPartID))
            {
                this.ID = this._genericWebPartID;
            }
            else
            {
                this.ID = this._originalID;
            }
        }

        protected internal override void LoadControlState(object savedState)
        {
        }

        protected override void LoadViewState(object savedState)
        {
        }

        protected internal override object SaveControlState()
        {
            base.SaveControlState();
            return null;
        }

        protected override object SaveViewState()
        {
            base.SaveViewState();
            return null;
        }

        public string GenericWebPartID
        {
            get
            {
                if (this._genericWebPartID == null)
                {
                    return string.Empty;
                }
                return this._genericWebPartID;
            }
        }

        public sealed override string ID
        {
            get
            {
                return base.ID;
            }
            set
            {
                base.ID = value;
            }
        }

        public string OriginalID
        {
            get
            {
                if (this._originalID == null)
                {
                    return string.Empty;
                }
                return this._originalID;
            }
        }

        public string OriginalPath
        {
            get
            {
                if (this._originalPath == null)
                {
                    return string.Empty;
                }
                return this._originalPath;
            }
        }

        public string OriginalTypeName
        {
            get
            {
                if (this._originalTypeName == null)
                {
                    return string.Empty;
                }
                return this._originalTypeName;
            }
        }
    }
}

