namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Web;
    using System.Web.UI;

    [ValidationProperty("SelectedItem"), SupportsEventValidation]
    public class DropDownList : ListControl, IPostBackDataHandler
    {
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            string uniqueID = this.UniqueID;
            if (uniqueID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }
            base.AddAttributesToRender(writer);
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string[] values = postCollection.GetValues(postDataKey);
            this.EnsureDataBound();
            if (values != null)
            {
                base.ValidateEvent(postDataKey, values[0]);
                int selectedIndex = this.Items.FindByValueInternal(values[0], false);
                if (this.SelectedIndex != selectedIndex)
                {
                    base.SetPostDataSelection(selectedIndex);
                    return true;
                }
            }
            return false;
        }

        protected virtual void RaisePostDataChangedEvent()
        {
            if (this.AutoPostBack && !this.Page.IsPostBackEventControlRegistered)
            {
                this.Page.AutoPostBackControl = this;
                if (this.CausesValidation)
                {
                    this.Page.Validate(this.ValidationGroup);
                }
            }
            this.OnSelectedIndexChanged(EventArgs.Empty);
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        protected internal override void VerifyMultiSelect()
        {
            throw new HttpException(System.Web.SR.GetString("Cant_Multiselect", new object[] { "DropDownList" }));
        }

        [Browsable(false)]
        public override Color BorderColor
        {
            get
            {
                return base.BorderColor;
            }
            set
            {
                base.BorderColor = value;
            }
        }

        [Browsable(false)]
        public override System.Web.UI.WebControls.BorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;
            }
        }

        [Browsable(false)]
        public override Unit BorderWidth
        {
            get
            {
                return base.BorderWidth;
            }
            set
            {
                base.BorderWidth = value;
            }
        }

        [WebSysDescription("WebControl_SelectedIndex"), DefaultValue(0), WebCategory("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int SelectedIndex
        {
            get
            {
                int selectedIndex = base.SelectedIndex;
                if ((selectedIndex < 0) && (this.Items.Count > 0))
                {
                    this.Items[0].Selected = true;
                    selectedIndex = 0;
                }
                return selectedIndex;
            }
            set
            {
                base.SelectedIndex = value;
            }
        }

        internal override ArrayList SelectedIndicesInternal
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                return base.SelectedIndicesInternal;
            }
        }

        public override bool SupportsDisabledAttribute
        {
            get
            {
                return true;
            }
        }
    }
}

