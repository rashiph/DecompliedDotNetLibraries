namespace System.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class RepeatInfo
    {
        private string caption;
        private TableCaptionAlign captionAlign;
        private bool enableLegacyRendering;
        private bool outerTableImplied = false;
        private int repeatColumns = 0;
        private System.Web.UI.WebControls.RepeatDirection repeatDirection = System.Web.UI.WebControls.RepeatDirection.Vertical;
        private System.Web.UI.WebControls.RepeatLayout repeatLayout = System.Web.UI.WebControls.RepeatLayout.Table;
        private bool useAccessibleHeader;

        private void RenderHorizontalRepeater(HtmlTextWriter writer, IRepeatInfoUser user, Style controlStyle, WebControl baseControl)
        {
            int repeatedItemCount = user.RepeatedItemCount;
            int repeatColumns = this.repeatColumns;
            int num3 = 0;
            if (repeatColumns == 0)
            {
                repeatColumns = repeatedItemCount;
            }
            WebControl control = null;
            bool flag = false;
            switch (this.repeatLayout)
            {
                case System.Web.UI.WebControls.RepeatLayout.Table:
                    control = new Table();
                    if (this.Caption.Length != 0)
                    {
                        ((Table) control).Caption = this.Caption;
                        ((Table) control).CaptionAlign = this.CaptionAlign;
                    }
                    flag = true;
                    break;

                case System.Web.UI.WebControls.RepeatLayout.Flow:
                    control = new WebControl(HtmlTextWriterTag.Span);
                    break;
            }
            bool hasSeparators = user.HasSeparators;
            control.ID = baseControl.ClientID;
            control.CopyBaseAttributes(baseControl);
            control.ApplyStyle(controlStyle);
            control.RenderBeginTag(writer);
            if (user.HasHeader)
            {
                if (flag)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    if ((repeatColumns != 1) || hasSeparators)
                    {
                        int num4 = repeatColumns;
                        if (hasSeparators)
                        {
                            num4 += repeatColumns;
                        }
                        writer.AddAttribute(HtmlTextWriterAttribute.Colspan, num4.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    if (this.useAccessibleHeader)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                    }
                    Style itemStyle = user.GetItemStyle(ListItemType.Header, -1);
                    if (itemStyle != null)
                    {
                        itemStyle.AddAttributesToRender(writer);
                    }
                    if (this.useAccessibleHeader)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    }
                    else
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    }
                }
                user.RenderItem(ListItemType.Header, -1, this, writer);
                if (flag)
                {
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                else if (repeatColumns < repeatedItemCount)
                {
                    if (this.EnableLegacyRendering)
                    {
                        writer.WriteObsoleteBreak();
                    }
                    else
                    {
                        writer.WriteBreak();
                    }
                }
            }
            for (int i = 0; i < repeatedItemCount; i++)
            {
                if (flag && (num3 == 0))
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }
                if (flag)
                {
                    Style style2 = user.GetItemStyle(ListItemType.Item, i);
                    if (style2 != null)
                    {
                        style2.AddAttributesToRender(writer);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                }
                user.RenderItem(ListItemType.Item, i, this, writer);
                if (flag)
                {
                    writer.RenderEndTag();
                }
                if (hasSeparators && (i != (repeatedItemCount - 1)))
                {
                    if (flag)
                    {
                        Style style3 = user.GetItemStyle(ListItemType.Separator, i);
                        if (style3 != null)
                        {
                            style3.AddAttributesToRender(writer);
                        }
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    }
                    user.RenderItem(ListItemType.Separator, i, this, writer);
                    if (flag)
                    {
                        writer.RenderEndTag();
                    }
                }
                num3++;
                if (flag && (i == (repeatedItemCount - 1)))
                {
                    int num6 = repeatColumns - num3;
                    if (hasSeparators)
                    {
                        int num7 = (num6 * 2) + 1;
                        if (num7 > num6)
                        {
                            num6 = num7;
                        }
                    }
                    for (int j = 0; j < num6; j++)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.RenderEndTag();
                    }
                }
                if ((num3 == repeatColumns) || (i == (repeatedItemCount - 1)))
                {
                    if (flag)
                    {
                        writer.RenderEndTag();
                    }
                    else if (repeatColumns < repeatedItemCount)
                    {
                        if (this.EnableLegacyRendering)
                        {
                            writer.WriteObsoleteBreak();
                        }
                        else
                        {
                            writer.WriteBreak();
                        }
                    }
                    num3 = 0;
                }
            }
            if (user.HasFooter)
            {
                if (flag)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    if ((repeatColumns != 1) || hasSeparators)
                    {
                        int num9 = repeatColumns;
                        if (hasSeparators)
                        {
                            num9 += repeatColumns;
                        }
                        writer.AddAttribute(HtmlTextWriterAttribute.Colspan, num9.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    Style style4 = user.GetItemStyle(ListItemType.Footer, -1);
                    if (style4 != null)
                    {
                        style4.AddAttributesToRender(writer);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                }
                user.RenderItem(ListItemType.Footer, -1, this, writer);
                if (flag)
                {
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
            }
            control.RenderEndTag(writer);
        }

        public void RenderRepeater(HtmlTextWriter writer, IRepeatInfoUser user, Style controlStyle, WebControl baseControl)
        {
            if (this.IsListLayout)
            {
                if ((user.HasFooter || user.HasHeader) || user.HasSeparators)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("RepeatInfo_ListLayoutDoesNotSupportHeaderFooterSeparator"));
                }
                if (this.RepeatDirection != System.Web.UI.WebControls.RepeatDirection.Vertical)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("RepeatInfo_ListLayoutOnlySupportsVerticalLayout"));
                }
                if ((this.RepeatColumns != 0) && (this.RepeatColumns != 1))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("RepeatInfo_ListLayoutDoesNotSupportMultipleColumn"));
                }
                if (this.OuterTableImplied)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("RepeatInfo_ListLayoutDoesNotSupportImpliedOuterTable"));
                }
            }
            if (this.repeatDirection == System.Web.UI.WebControls.RepeatDirection.Vertical)
            {
                this.RenderVerticalRepeater(writer, user, controlStyle, baseControl);
            }
            else
            {
                this.RenderHorizontalRepeater(writer, user, controlStyle, baseControl);
            }
        }

        private void RenderVerticalRepeater(HtmlTextWriter writer, IRepeatInfoUser user, Style controlStyle, WebControl baseControl)
        {
            int repeatColumns;
            int num3;
            int num4;
            int repeatedItemCount = user.RepeatedItemCount;
            if ((this.repeatColumns == 0) || (this.repeatColumns == 1))
            {
                repeatColumns = 1;
                num4 = 1;
                num3 = repeatedItemCount;
            }
            else
            {
                repeatColumns = this.repeatColumns;
                num3 = ((repeatedItemCount + this.repeatColumns) - 1) / this.repeatColumns;
                if ((num3 == 0) && (repeatedItemCount != 0))
                {
                    num3 = 1;
                }
                num4 = repeatedItemCount % repeatColumns;
                if (num4 == 0)
                {
                    num4 = repeatColumns;
                }
            }
            WebControl control = null;
            bool flag = false;
            if (!this.outerTableImplied)
            {
                switch (this.repeatLayout)
                {
                    case System.Web.UI.WebControls.RepeatLayout.Table:
                        control = new Table();
                        if (this.Caption.Length != 0)
                        {
                            ((Table) control).Caption = this.Caption;
                            ((Table) control).CaptionAlign = this.CaptionAlign;
                        }
                        flag = true;
                        break;

                    case System.Web.UI.WebControls.RepeatLayout.Flow:
                        control = new WebControl(HtmlTextWriterTag.Span);
                        break;

                    case System.Web.UI.WebControls.RepeatLayout.UnorderedList:
                        control = new WebControl(HtmlTextWriterTag.Ul);
                        break;

                    case System.Web.UI.WebControls.RepeatLayout.OrderedList:
                        control = new WebControl(HtmlTextWriterTag.Ol);
                        break;
                }
            }
            bool hasSeparators = user.HasSeparators;
            if (control != null)
            {
                control.ID = baseControl.ClientID;
                control.CopyBaseAttributes(baseControl);
                control.ApplyStyle(controlStyle);
                control.RenderBeginTag(writer);
            }
            if (user.HasHeader)
            {
                if (flag)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    if (repeatColumns != 1)
                    {
                        int num5 = repeatColumns;
                        if (hasSeparators)
                        {
                            num5 += repeatColumns;
                        }
                        writer.AddAttribute(HtmlTextWriterAttribute.Colspan, num5.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    if (this.useAccessibleHeader)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                    }
                    Style itemStyle = user.GetItemStyle(ListItemType.Header, -1);
                    if (itemStyle != null)
                    {
                        itemStyle.AddAttributesToRender(writer);
                    }
                    if (this.useAccessibleHeader)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    }
                    else
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    }
                }
                user.RenderItem(ListItemType.Header, -1, this, writer);
                if (flag)
                {
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                else if (!this.outerTableImplied)
                {
                    if (this.EnableLegacyRendering)
                    {
                        writer.WriteObsoleteBreak();
                    }
                    else
                    {
                        writer.WriteBreak();
                    }
                }
            }
            int num6 = 0;
            for (int i = 0; i < num3; i++)
            {
                if (flag)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }
                int repeatIndex = i;
                for (int j = 0; j < repeatColumns; j++)
                {
                    if (num6 >= repeatedItemCount)
                    {
                        break;
                    }
                    if (j != 0)
                    {
                        repeatIndex += num3;
                        if ((j - 1) >= num4)
                        {
                            repeatIndex--;
                        }
                    }
                    if (repeatIndex < repeatedItemCount)
                    {
                        num6++;
                        if (flag)
                        {
                            Style style2 = user.GetItemStyle(ListItemType.Item, repeatIndex);
                            if (style2 != null)
                            {
                                style2.AddAttributesToRender(writer);
                            }
                            writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        }
                        if (this.IsListLayout)
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Li);
                        }
                        user.RenderItem(ListItemType.Item, repeatIndex, this, writer);
                        if (this.IsListLayout)
                        {
                            writer.RenderEndTag();
                            writer.WriteLine();
                        }
                        if (flag)
                        {
                            writer.RenderEndTag();
                        }
                        if (hasSeparators)
                        {
                            if (repeatIndex != (repeatedItemCount - 1))
                            {
                                if (repeatColumns == 1)
                                {
                                    if (flag)
                                    {
                                        writer.RenderEndTag();
                                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                                    }
                                    else if (!this.outerTableImplied)
                                    {
                                        if (this.EnableLegacyRendering)
                                        {
                                            writer.WriteObsoleteBreak();
                                        }
                                        else
                                        {
                                            writer.WriteBreak();
                                        }
                                    }
                                }
                                if (flag)
                                {
                                    Style style3 = user.GetItemStyle(ListItemType.Separator, repeatIndex);
                                    if (style3 != null)
                                    {
                                        style3.AddAttributesToRender(writer);
                                    }
                                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                }
                                if (repeatIndex < repeatedItemCount)
                                {
                                    user.RenderItem(ListItemType.Separator, repeatIndex, this, writer);
                                }
                                if (flag)
                                {
                                    writer.RenderEndTag();
                                }
                            }
                            else if (flag && (repeatColumns > 1))
                            {
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                writer.RenderEndTag();
                            }
                        }
                    }
                }
                if (flag)
                {
                    if (i == (num3 - 1))
                    {
                        int num10 = repeatColumns - num4;
                        if (hasSeparators)
                        {
                            int num11 = num10 * 2;
                            if (num11 >= num10)
                            {
                                num10 = num11;
                            }
                        }
                        if (num10 != 0)
                        {
                            for (int k = 0; k < num10; k++)
                            {
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                writer.RenderEndTag();
                            }
                        }
                    }
                    writer.RenderEndTag();
                }
                else if (((i != (num3 - 1)) || user.HasFooter) && (!this.outerTableImplied && !this.IsListLayout))
                {
                    if (this.EnableLegacyRendering)
                    {
                        writer.WriteObsoleteBreak();
                    }
                    else
                    {
                        writer.WriteBreak();
                    }
                }
            }
            if (user.HasFooter)
            {
                if (flag)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    if (repeatColumns != 1)
                    {
                        int num13 = repeatColumns;
                        if (hasSeparators)
                        {
                            num13 += repeatColumns;
                        }
                        writer.AddAttribute(HtmlTextWriterAttribute.Colspan, num13.ToString(NumberFormatInfo.InvariantInfo));
                    }
                    Style style4 = user.GetItemStyle(ListItemType.Footer, -1);
                    if (style4 != null)
                    {
                        style4.AddAttributesToRender(writer);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                }
                user.RenderItem(ListItemType.Footer, -1, this, writer);
                if (flag)
                {
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
            }
            if (control != null)
            {
                control.RenderEndTag(writer);
            }
        }

        public string Caption
        {
            get
            {
                if (this.caption != null)
                {
                    return this.caption;
                }
                return string.Empty;
            }
            set
            {
                this.caption = value;
            }
        }

        public TableCaptionAlign CaptionAlign
        {
            get
            {
                return this.captionAlign;
            }
            set
            {
                if ((value < TableCaptionAlign.NotSet) || (value > TableCaptionAlign.Right))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.captionAlign = value;
            }
        }

        internal bool EnableLegacyRendering
        {
            get
            {
                return this.enableLegacyRendering;
            }
            set
            {
                this.enableLegacyRendering = value;
            }
        }

        private bool IsListLayout
        {
            get
            {
                if (this.RepeatLayout != System.Web.UI.WebControls.RepeatLayout.UnorderedList)
                {
                    return (this.RepeatLayout == System.Web.UI.WebControls.RepeatLayout.OrderedList);
                }
                return true;
            }
        }

        public bool OuterTableImplied
        {
            get
            {
                return this.outerTableImplied;
            }
            set
            {
                this.outerTableImplied = value;
            }
        }

        public int RepeatColumns
        {
            get
            {
                return this.repeatColumns;
            }
            set
            {
                this.repeatColumns = value;
            }
        }

        public System.Web.UI.WebControls.RepeatDirection RepeatDirection
        {
            get
            {
                return this.repeatDirection;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.RepeatDirection.Horizontal) || (value > System.Web.UI.WebControls.RepeatDirection.Vertical))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.repeatDirection = value;
            }
        }

        public System.Web.UI.WebControls.RepeatLayout RepeatLayout
        {
            get
            {
                return this.repeatLayout;
            }
            set
            {
                EnumerationRangeValidationUtil.ValidateRepeatLayout(value);
                this.repeatLayout = value;
            }
        }

        public bool UseAccessibleHeader
        {
            get
            {
                return this.useAccessibleHeader;
            }
            set
            {
                this.useAccessibleHeader = value;
            }
        }
    }
}

