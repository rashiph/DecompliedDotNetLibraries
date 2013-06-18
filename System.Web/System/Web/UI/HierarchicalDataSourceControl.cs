namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.Web;

    [NonVisualControl, ControlBuilder(typeof(DataSourceControlBuilder)), Designer("System.Web.UI.Design.HierarchicalDataSourceDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Bindable(false)]
    public abstract class HierarchicalDataSourceControl : Control, IHierarchicalDataSource
    {
        private static readonly object EventDataSourceChanged = new object();

        event EventHandler IHierarchicalDataSource.DataSourceChanged
        {
            add
            {
                base.Events.AddHandler(EventDataSourceChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDataSourceChanged, value);
            }
        }

        protected HierarchicalDataSourceControl()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ApplyStyleSheetSkin(Page page)
        {
            base.ApplyStyleSheetSkin(page);
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Control FindControl(string id)
        {
            return base.FindControl(id);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Focus()
        {
            throw new NotSupportedException(System.Web.SR.GetString("NoFocusSupport", new object[] { base.GetType().Name }));
        }

        protected abstract HierarchicalDataSourceView GetHierarchicalView(string viewPath);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool HasControls()
        {
            return base.HasControls();
        }

        protected virtual void OnDataSourceChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventDataSourceChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void RenderControl(HtmlTextWriter writer)
        {
            base.RenderControl(writer);
        }

        HierarchicalDataSourceView IHierarchicalDataSource.GetHierarchicalView(string viewPath)
        {
            return this.GetHierarchicalView(viewPath);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override string ClientID
        {
            get
            {
                return base.ClientID;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override System.Web.UI.ClientIDMode ClientIDMode
        {
            get
            {
                return base.ClientIDMode;
            }
            set
            {
                throw new NotSupportedException();
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

        [Browsable(false), DefaultValue(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [DefaultValue(""), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DefaultValue(false)]
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

