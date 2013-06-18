namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;

    [ControlBuilder(typeof(DataSourceControlBuilder)), NonVisualControl, Bindable(false), Designer("System.Web.UI.Design.DataSourceDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class DataSourceControl : Control, IDataSource, IListSource
    {
        private static readonly object EventDataSourceChanged = new object();
        private static readonly object EventDataSourceChangedInternal = new object();

        internal event EventHandler DataSourceChangedInternal
        {
            add
            {
                base.Events.AddHandler(EventDataSourceChangedInternal, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDataSourceChangedInternal, value);
            }
        }

        event EventHandler IDataSource.DataSourceChanged
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

        protected DataSourceControl()
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

        protected abstract DataSourceView GetView(string viewName);
        protected virtual ICollection GetViewNames()
        {
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool HasControls()
        {
            return base.HasControls();
        }

        private void OnDataSourceChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventDataSourceChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnDataSourceChangedInternal(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventDataSourceChangedInternal];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void RaiseDataSourceChangedEvent(EventArgs e)
        {
            this.OnDataSourceChangedInternal(e);
            this.OnDataSourceChanged(e);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void RenderControl(HtmlTextWriter writer)
        {
            base.RenderControl(writer);
        }

        IList IListSource.GetList()
        {
            if (base.DesignMode)
            {
                return null;
            }
            return ListSourceHelper.GetList(this);
        }

        DataSourceView IDataSource.GetView(string viewName)
        {
            return this.GetView(viewName);
        }

        ICollection IDataSource.GetViewNames()
        {
            return this.GetViewNames();
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override string ClientID
        {
            get
            {
                return base.ClientID;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), DefaultValue(false), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), DefaultValue(""), Browsable(false)]
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

        bool IListSource.ContainsListCollection
        {
            get
            {
                if (base.DesignMode)
                {
                    return false;
                }
                return ListSourceHelper.ContainsListCollection(this);
            }
        }

        [Browsable(false), DefaultValue(false), EditorBrowsable(EditorBrowsableState.Never)]
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

