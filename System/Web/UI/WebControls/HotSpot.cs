namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class HotSpot : IStateManager
    {
        private bool _isTrackingViewState;
        private StateBag _viewState;

        protected HotSpot()
        {
        }

        public abstract string GetCoordinates();
        protected virtual void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                this.ViewState.LoadViewState(savedState);
            }
        }

        protected virtual object SaveViewState()
        {
            if (this._viewState != null)
            {
                return this._viewState.SaveViewState();
            }
            return null;
        }

        internal void SetDirty()
        {
            if (this._viewState != null)
            {
                this._viewState.SetDirty(true);
            }
        }

        void IStateManager.LoadViewState(object savedState)
        {
            this.LoadViewState(savedState);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        public override string ToString()
        {
            return base.GetType().Name;
        }

        protected virtual void TrackViewState()
        {
            this._isTrackingViewState = true;
            if (this._viewState != null)
            {
                this._viewState.TrackViewState();
            }
        }

        [DefaultValue(""), WebSysDescription("HotSpot_AccessKey"), Localizable(true), WebCategory("Accessibility")]
        public virtual string AccessKey
        {
            get
            {
                string str = (string) this.ViewState["AccessKey"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                if ((value != null) && (value.Length > 1))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["AccessKey"] = value;
            }
        }

        [Localizable(true), WebSysDescription("HotSpot_AlternateText"), NotifyParentProperty(true), WebCategory("Behavior"), DefaultValue(""), Bindable(true)]
        public virtual string AlternateText
        {
            get
            {
                object obj2 = this.ViewState["AlternateText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["AlternateText"] = value;
            }
        }

        [NotifyParentProperty(true), WebCategory("Behavior"), DefaultValue(0), WebSysDescription("HotSpot_HotSpotMode")]
        public virtual System.Web.UI.WebControls.HotSpotMode HotSpotMode
        {
            get
            {
                object obj2 = this.ViewState["HotSpotMode"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.HotSpotMode) obj2;
                }
                return System.Web.UI.WebControls.HotSpotMode.NotSet;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.HotSpotMode.NotSet) || (value > System.Web.UI.WebControls.HotSpotMode.Inactive))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["HotSpotMode"] = value;
            }
        }

        protected virtual bool IsTrackingViewState
        {
            get
            {
                return this._isTrackingViewState;
            }
        }

        protected internal abstract string MarkupName { get; }

        [Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Bindable(true), WebCategory("Behavior"), DefaultValue(""), WebSysDescription("HotSpot_NavigateUrl"), NotifyParentProperty(true), UrlProperty]
        public string NavigateUrl
        {
            get
            {
                object obj2 = this.ViewState["NavigateUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["NavigateUrl"] = value;
            }
        }

        [Bindable(true), NotifyParentProperty(true), WebCategory("Behavior"), DefaultValue(""), WebSysDescription("HotSpot_PostBackValue")]
        public string PostBackValue
        {
            get
            {
                object obj2 = this.ViewState["PostBackValue"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["PostBackValue"] = value;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        [WebCategory("Accessibility"), DefaultValue((short) 0), WebSysDescription("HotSpot_TabIndex")]
        public virtual short TabIndex
        {
            get
            {
                object obj2 = this.ViewState["TabIndex"];
                if (obj2 != null)
                {
                    return (short) obj2;
                }
                return 0;
            }
            set
            {
                this.ViewState["TabIndex"] = value;
            }
        }

        [NotifyParentProperty(true), WebCategory("Behavior"), DefaultValue(""), TypeConverter(typeof(TargetConverter)), WebSysDescription("HotSpot_Target")]
        public virtual string Target
        {
            get
            {
                object obj2 = this.ViewState["Target"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["Target"] = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected StateBag ViewState
        {
            get
            {
                if (this._viewState == null)
                {
                    this._viewState = new StateBag(false);
                    if (this._isTrackingViewState)
                    {
                        ((IStateManager) this._viewState).TrackViewState();
                    }
                }
                return this._viewState;
            }
        }
    }
}

