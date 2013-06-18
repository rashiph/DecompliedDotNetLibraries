namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    public class TableHeaderCell : TableCell
    {
        public TableHeaderCell() : base(HtmlTextWriterTag.Th)
        {
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            TableHeaderScope scope = this.Scope;
            if (scope != TableHeaderScope.NotSet)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Scope, scope.ToString().ToLowerInvariant());
            }
            string abbreviatedText = this.AbbreviatedText;
            if (!string.IsNullOrEmpty(abbreviatedText))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Abbr, abbreviatedText);
            }
            string[] categoryText = this.CategoryText;
            if (categoryText.Length > 0)
            {
                bool flag = true;
                StringBuilder builder = new StringBuilder();
                foreach (string str2 in categoryText)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        builder.Append(",");
                    }
                    builder.Append(str2);
                }
                string str3 = builder.ToString();
                if (!string.IsNullOrEmpty(str3))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Axis, str3);
                }
            }
        }

        [WebCategory("Accessibility"), WebSysDescription("TableHeaderCell_AbbreviatedText"), DefaultValue("")]
        public virtual string AbbreviatedText
        {
            get
            {
                object obj2 = this.ViewState["AbbrText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["AbbrText"] = value;
            }
        }

        [TypeConverter(typeof(StringArrayConverter)), WebCategory("Accessibility"), WebSysDescription("TableHeaderCell_CategoryText"), DefaultValue((string) null)]
        public virtual string[] CategoryText
        {
            get
            {
                object obj2 = this.ViewState["CategoryText"];
                if (obj2 == null)
                {
                    return new string[0];
                }
                return (string[]) ((string[]) obj2).Clone();
            }
            set
            {
                if (value != null)
                {
                    this.ViewState["CategoryText"] = (string[]) value.Clone();
                }
                else
                {
                    this.ViewState["CategoryText"] = null;
                }
            }
        }

        [WebSysDescription("TableHeaderCell_Scope"), WebCategory("Accessibility"), DefaultValue(0)]
        public virtual TableHeaderScope Scope
        {
            get
            {
                object obj2 = this.ViewState["Scope"];
                if (obj2 != null)
                {
                    return (TableHeaderScope) obj2;
                }
                return TableHeaderScope.NotSet;
            }
            set
            {
                this.ViewState["Scope"] = value;
            }
        }
    }
}

