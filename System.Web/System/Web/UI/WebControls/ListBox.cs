namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [ValidationProperty("SelectedItem"), SupportsEventValidation]
    public class ListBox : ListControl, IPostBackDataHandler
    {
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Size, this.Rows.ToString(NumberFormatInfo.InvariantInfo));
            string uniqueID = this.UniqueID;
            if (uniqueID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }
            base.AddAttributesToRender(writer);
        }

        public virtual int[] GetSelectedIndices()
        {
            return (int[]) this.SelectedIndicesInternal.ToArray(typeof(int));
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            if (!base.IsEnabled)
            {
                return false;
            }
            string[] values = postCollection.GetValues(postDataKey);
            bool flag = false;
            this.EnsureDataBound();
            if (values == null)
            {
                if (this.SelectedIndex != -1)
                {
                    base.SetPostDataSelection(-1);
                    flag = true;
                }
                return flag;
            }
            if (this.SelectionMode == ListSelectionMode.Single)
            {
                base.ValidateEvent(postDataKey, values[0]);
                int selectedIndex = this.Items.FindByValueInternal(values[0], false);
                if (this.SelectedIndex != selectedIndex)
                {
                    base.SetPostDataSelection(selectedIndex);
                    flag = true;
                }
                return flag;
            }
            int length = values.Length;
            ArrayList selectedIndicesInternal = this.SelectedIndicesInternal;
            ArrayList selectedIndices = new ArrayList(length);
            for (int i = 0; i < length; i++)
            {
                base.ValidateEvent(postDataKey, values[i]);
                selectedIndices.Add(this.Items.FindByValueInternal(values[i], false));
            }
            int count = 0;
            if (selectedIndicesInternal != null)
            {
                count = selectedIndicesInternal.Count;
            }
            if (count == length)
            {
                for (int j = 0; j < length; j++)
                {
                    if (((int) selectedIndices[j]) != ((int) selectedIndicesInternal[j]))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            else
            {
                flag = true;
            }
            if (flag)
            {
                base.SelectInternal(selectedIndices);
            }
            return flag;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (((this.Page != null) && (this.SelectionMode == ListSelectionMode.Multiple)) && this.Enabled)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
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

        internal override bool IsMultiSelectInternal
        {
            get
            {
                return (this.SelectionMode == ListSelectionMode.Multiple);
            }
        }

        [WebSysDescription("ListBox_Rows"), WebCategory("Appearance"), DefaultValue(4)]
        public virtual int Rows
        {
            get
            {
                object obj2 = this.ViewState["Rows"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 4;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["Rows"] = value;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("ListBox_SelectionMode"), DefaultValue(0)]
        public virtual ListSelectionMode SelectionMode
        {
            get
            {
                object obj2 = this.ViewState["SelectionMode"];
                if (obj2 != null)
                {
                    return (ListSelectionMode) obj2;
                }
                return ListSelectionMode.Single;
            }
            set
            {
                if ((value < ListSelectionMode.Single) || (value > ListSelectionMode.Multiple))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["SelectionMode"] = value;
            }
        }
    }
}

