namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [PersistChildren(false), Designer("System.Web.UI.Design.WebControls.WebParts.PartDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ParseChildren(true)]
    public abstract class Part : Panel, INamingContainer, ICompositeControlDesignerAccessor
    {
        internal Part()
        {
        }

        public override void DataBind()
        {
            this.OnDataBinding(EventArgs.Empty);
            this.EnsureChildControls();
            this.DataBindChildren();
        }

        void ICompositeControlDesignerAccessor.RecreateChildControls()
        {
            base.ChildControlsCreated = false;
            this.EnsureChildControls();
        }

        [DefaultValue(0), WebCategory("WebPartAppearance"), WebSysDescription("Part_ChromeState")]
        public virtual PartChromeState ChromeState
        {
            get
            {
                object obj2 = this.ViewState["ChromeState"];
                if (obj2 == null)
                {
                    return PartChromeState.Normal;
                }
                return (PartChromeState) obj2;
            }
            set
            {
                if ((value < PartChromeState.Normal) || (value > PartChromeState.Minimized))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["ChromeState"] = value;
            }
        }

        [WebSysDescription("Part_ChromeType"), WebCategory("WebPartAppearance"), DefaultValue(0)]
        public virtual PartChromeType ChromeType
        {
            get
            {
                object obj2 = this.ViewState["ChromeType"];
                if (obj2 == null)
                {
                    return PartChromeType.Default;
                }
                return (PartChromeType) ((int) obj2);
            }
            set
            {
                if ((value < PartChromeType.Default) || (value > PartChromeType.BorderOnly))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["ChromeType"] = (int) value;
            }
        }

        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        [DefaultValue(""), Localizable(true), WebCategory("WebPartAppearance"), WebSysDescription("Part_Description")]
        public virtual string Description
        {
            get
            {
                string str = (string) this.ViewState["Description"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["Description"] = value;
            }
        }

        [Localizable(true), WebSysDefaultValue(""), WebCategory("WebPartAppearance"), WebSysDescription("Part_Title")]
        public virtual string Title
        {
            get
            {
                string str = (string) this.ViewState["Title"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["Title"] = value;
            }
        }
    }
}

