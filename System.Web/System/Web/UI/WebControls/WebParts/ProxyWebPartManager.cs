namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ParseChildren(true), NonVisualControl, Bindable(false), PersistChildren(false), Designer("System.Web.UI.Design.WebControls.WebParts.ProxyWebPartManagerDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ProxyWebPartManager : Control
    {
        private ProxyWebPartConnectionCollection _staticConnections;

        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Focus()
        {
            throw new NotSupportedException(System.Web.SR.GetString("NoFocusSupport", new object[] { base.GetType().Name }));
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page page = this.Page;
            if ((page != null) && !base.DesignMode)
            {
                WebPartManager currentWebPartManager = WebPartManager.GetCurrentWebPartManager(page);
                if (currentWebPartManager == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("WebPartManagerRequired"));
                }
                this.StaticConnections.SetWebPartManager(currentWebPartManager);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ClientID
        {
            get
            {
                return base.ClientID;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override ControlCollection Controls
        {
            get
            {
                return base.Controls;
            }
        }

        [DefaultValue(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool EnableTheming
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("NoThemingSupport", new object[] { base.GetType().Name }));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DefaultValue("")]
        public override string SkinID
        {
            get
            {
                return string.Empty;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("NoThemingSupport", new object[] { base.GetType().Name }));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), WebCategory("Behavior"), WebSysDescription("WebPartManager_StaticConnections"), PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false)]
        public ProxyWebPartConnectionCollection StaticConnections
        {
            get
            {
                if (this._staticConnections == null)
                {
                    this._staticConnections = new ProxyWebPartConnectionCollection();
                }
                return this._staticConnections;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DefaultValue(false)]
        public override bool Visible
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("ControlNonVisual", new object[] { base.GetType().Name }));
            }
        }
    }
}

