namespace System.Web.UI.Design
{
    using System;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class TemplateEditingFrame : ITemplateEditingFrame, IDisposable
    {
        private Style controlStyle;
        private string frameContent;
        private string frameName;
        private bool fVisible;
        private System.Design.NativeMethods.IHTMLElement htmlElemContent;
        private System.Design.NativeMethods.IHTMLElement htmlElemControlName;
        private System.Design.NativeMethods.IHTMLElement htmlElemFrame;
        private System.Design.NativeMethods.IHTMLElement htmlElemParent;
        private int initialHeight;
        private int initialWidth;
        private TemplatedControlDesigner owner;
        private object[] templateElements;
        private const string TemplateFrameFooterContent = "</table>";
        private const string TemplateFrameHeaderContent = "<table cellspacing=0 cellpadding=0 border=0 style=\"{4}\">\r\n              <tr>\r\n                <td>\r\n                  <table cellspacing=0 cellpadding=2 border=0 width=100% height=100%>\r\n                    <tr style=\"background-color:buttonshadow\">\r\n                      <td>\r\n                        <table cellspacing=0 cellpadding=0 border=0 width=100% height=100%>\r\n                          <tr>\r\n                            <td valign=middle style=\"font:messagebox;font-weight:bold;color:buttonhighlight\">&nbsp;<span id=\"idControlName\">{0}</span> - <span id=\"idFrameName\">{1}</span>&nbsp;&nbsp;&nbsp;</td>\r\n                            <td align=right valign=middle>&nbsp;<img src=\"{2}\" height=13 width=14 title=\"{3}\">&nbsp;</td>\r\n                          </tr>\r\n                        </table>\r\n                      </td>\r\n                    </tr>\r\n                  </table>\r\n                </td>\r\n              </tr>";
        private const string TemplateFrameSeparatorContent = "<tr style=\"height:1px\"><td style=\"font-size:0pt\"></td></tr>";
        private const string TemplateFrameTemplateContent = "<tr>\r\n                <td>\r\n                  <table cellspacing=0 cellpadding=2 border=0 width=100% height=100% style=\"border:solid 1px buttonface\">\r\n                    <tr style=\"font:messagebox;background-color:buttonface;color:buttonshadow\">\r\n                      <td style=\"border-bottom:solid 1px buttonshadow\">\r\n                        &nbsp;{0}&nbsp;&nbsp;&nbsp;\r\n                      </td>\r\n                    </tr>\r\n                    <tr style=\"{1}\" height=100%>\r\n                      <td style=\"{2}\">\r\n                        <div style=\"width:100%;height:100%\" id=\"{0}\"></div>\r\n                      </td>\r\n                    </tr>\r\n                  </table>\r\n                </td>\r\n              </tr>";
        private static readonly string TemplateInfoIcon = ("res://" + typeof(TemplateEditingFrame).Module.FullyQualifiedName + "//TEMPLATE_TIP");
        private static readonly string TemplateInfoToolTip = System.Design.SR.GetString("TemplateEdit_Tip");
        private string[] templateNames;
        private Style[] templateStyles;
        private TemplateEditingVerb verb;

        public TemplateEditingFrame(TemplatedControlDesigner owner, string frameName, string[] templateNames, Style controlStyle, Style[] templateStyles)
        {
            this.owner = owner;
            this.frameName = frameName;
            this.controlStyle = controlStyle;
            this.templateStyles = templateStyles;
            this.verb = null;
            this.templateNames = (string[]) templateNames.Clone();
            if (owner.BehaviorInternal != null)
            {
                System.Design.NativeMethods.IHTMLElement designTimeElementView = (System.Design.NativeMethods.IHTMLElement) ((IControlDesignerBehavior) owner.BehaviorInternal).DesignTimeElementView;
                this.htmlElemParent = designTimeElementView;
            }
            this.htmlElemControlName = null;
        }

        public void Close(bool saveChanges)
        {
            if (saveChanges)
            {
                this.Save();
            }
            this.ShowInternal(false);
        }

        private string CreateFrameContent()
        {
            StringBuilder builder = new StringBuilder(0x400);
            string str = string.Empty;
            if (this.initialWidth > 0)
            {
                str = "width:" + this.initialWidth + "px;";
            }
            if (this.initialHeight > 0)
            {
                object obj2 = str;
                str = string.Concat(new object[] { obj2, "height:", this.initialHeight, "px;" });
            }
            builder.Append(string.Format(CultureInfo.InvariantCulture, "<table cellspacing=0 cellpadding=0 border=0 style=\"{4}\">\r\n              <tr>\r\n                <td>\r\n                  <table cellspacing=0 cellpadding=2 border=0 width=100% height=100%>\r\n                    <tr style=\"background-color:buttonshadow\">\r\n                      <td>\r\n                        <table cellspacing=0 cellpadding=0 border=0 width=100% height=100%>\r\n                          <tr>\r\n                            <td valign=middle style=\"font:messagebox;font-weight:bold;color:buttonhighlight\">&nbsp;<span id=\"idControlName\">{0}</span> - <span id=\"idFrameName\">{1}</span>&nbsp;&nbsp;&nbsp;</td>\r\n                            <td align=right valign=middle>&nbsp;<img src=\"{2}\" height=13 width=14 title=\"{3}\">&nbsp;</td>\r\n                          </tr>\r\n                        </table>\r\n                      </td>\r\n                    </tr>\r\n                  </table>\r\n                </td>\r\n              </tr>", new object[] { this.owner.Component.GetType().Name, this.Name, TemplateInfoIcon, TemplateInfoToolTip, str }));
            string str2 = string.Empty;
            if (this.controlStyle != null)
            {
                str2 = this.StyleToCss(this.controlStyle);
            }
            string str3 = string.Empty;
            for (int i = 0; i < this.templateNames.Length; i++)
            {
                builder.Append("<tr style=\"height:1px\"><td style=\"font-size:0pt\"></td></tr>");
                if (this.templateStyles != null)
                {
                    str3 = this.StyleToCss(this.templateStyles[i]);
                }
                builder.Append(string.Format(CultureInfo.InvariantCulture, "<tr>\r\n                <td>\r\n                  <table cellspacing=0 cellpadding=2 border=0 width=100% height=100% style=\"border:solid 1px buttonface\">\r\n                    <tr style=\"font:messagebox;background-color:buttonface;color:buttonshadow\">\r\n                      <td style=\"border-bottom:solid 1px buttonshadow\">\r\n                        &nbsp;{0}&nbsp;&nbsp;&nbsp;\r\n                      </td>\r\n                    </tr>\r\n                    <tr style=\"{1}\" height=100%>\r\n                      <td style=\"{2}\">\r\n                        <div style=\"width:100%;height:100%\" id=\"{0}\"></div>\r\n                      </td>\r\n                    </tr>\r\n                  </table>\r\n                </td>\r\n              </tr>", new object[] { this.templateNames[i], str2, str3 }));
            }
            builder.Append("</table>");
            return builder.ToString();
        }

        public void Dispose()
        {
            if ((this.owner != null) && this.owner.InTemplateMode)
            {
                this.owner.ExitTemplateMode(false, false, false);
            }
            this.ReleaseParentElement();
            if (this.verb != null)
            {
                this.verb.Dispose();
                this.verb = null;
            }
        }

        private void Initialize()
        {
            if (this.htmlElemFrame == null)
            {
                try
                {
                    object obj2;
                    this.htmlElemFrame = this.htmlElemParent.GetDocument().CreateElement("SPAN");
                    this.htmlElemFrame.SetInnerHTML(this.Content);
                    System.Design.NativeMethods.IHTMLDOMNode htmlElemFrame = (System.Design.NativeMethods.IHTMLDOMNode) this.htmlElemFrame;
                    if (htmlElemFrame != null)
                    {
                        this.htmlElemContent = (System.Design.NativeMethods.IHTMLElement) htmlElemFrame.GetFirstChild();
                    }
                    System.Design.NativeMethods.IHTMLElement3 element = (System.Design.NativeMethods.IHTMLElement3) this.htmlElemFrame;
                    if (element != null)
                    {
                        element.SetContentEditable("false");
                    }
                    this.templateElements = new object[this.templateNames.Length];
                    object index = 0;
                    System.Design.NativeMethods.IHTMLElementCollection all = (System.Design.NativeMethods.IHTMLElementCollection) this.htmlElemFrame.GetAll();
                    for (int i = 0; i < this.templateNames.Length; i++)
                    {
                        try
                        {
                            obj2 = this.templateNames[i];
                            System.Design.NativeMethods.IHTMLElement element2 = all.Item(obj2, index);
                            element2.SetAttribute("templatename", obj2, 0);
                            string p = "<DIV contentEditable=\"true\" style=\"padding:1;height:100%;width:100%\"></DIV>";
                            element2.SetInnerHTML(p);
                            System.Design.NativeMethods.IHTMLDOMNode node2 = (System.Design.NativeMethods.IHTMLDOMNode) element2;
                            if (node2 != null)
                            {
                                this.templateElements[i] = node2.GetFirstChild();
                            }
                        }
                        catch (Exception)
                        {
                            this.templateElements[i] = null;
                        }
                    }
                    obj2 = "idControlName";
                    this.htmlElemControlName = all.Item(obj2, index);
                    obj2 = "idFrameName";
                    object obj4 = all.Item(obj2, index);
                    if (obj4 != null)
                    {
                        ((System.Design.NativeMethods.IHTMLElement) obj4).SetInnerText(this.frameName);
                    }
                    System.Design.NativeMethods.IHTMLDOMNode htmlElemParent = (System.Design.NativeMethods.IHTMLDOMNode) this.htmlElemParent;
                    if (htmlElemParent != null)
                    {
                        htmlElemParent.AppendChild(htmlElemFrame);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void Open()
        {
            System.Design.NativeMethods.IHTMLElement designTimeElementView = (System.Design.NativeMethods.IHTMLElement) ((IControlDesignerBehavior) this.owner.BehaviorInternal).DesignTimeElementView;
            if (this.htmlElemParent != designTimeElementView)
            {
                this.ReleaseParentElement();
                this.htmlElemParent = designTimeElementView;
            }
            this.Initialize();
            try
            {
                for (int i = 0; i < this.templateNames.Length; i++)
                {
                    if (this.templateElements[i] != null)
                    {
                        bool allowEditing = true;
                        System.Design.NativeMethods.IHTMLElement element2 = (System.Design.NativeMethods.IHTMLElement) this.templateElements[i];
                        string p = this.owner.GetTemplateContent(this, this.templateNames[i], out allowEditing);
                        element2.SetAttribute("contentEditable", allowEditing, 0);
                        if (p != null)
                        {
                            p = "<body contentEditable=true>" + p + "</body>";
                            element2.SetInnerHTML(p);
                        }
                    }
                }
                if (this.htmlElemControlName != null)
                {
                    this.htmlElemControlName.SetInnerText(this.owner.Component.Site.Name);
                }
            }
            catch (Exception)
            {
            }
            this.ShowInternal(true);
        }

        private void ReleaseParentElement()
        {
            this.htmlElemParent = null;
            this.htmlElemFrame = null;
            this.htmlElemContent = null;
            this.htmlElemControlName = null;
            this.templateElements = null;
            this.fVisible = false;
        }

        public void Resize(int width, int height)
        {
            if (this.htmlElemContent != null)
            {
                System.Design.NativeMethods.IHTMLStyle style = this.htmlElemContent.GetStyle();
                if (style != null)
                {
                    style.SetPixelWidth(width);
                    style.SetPixelHeight(height);
                }
            }
        }

        public void Save()
        {
            try
            {
                if (this.templateElements != null)
                {
                    object[] pvars = new object[1];
                    for (int i = 0; i < this.templateNames.Length; i++)
                    {
                        if (this.templateElements[i] != null)
                        {
                            System.Design.NativeMethods.IHTMLElement element = (System.Design.NativeMethods.IHTMLElement) this.templateElements[i];
                            element.GetAttribute("contentEditable", 0, pvars);
                            if (((pvars[0] != null) && (pvars[0] is string)) && (string.Compare((string) pvars[0], "true", StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                string innerHTML = element.GetInnerHTML();
                                this.owner.SetTemplateContent(this, this.templateNames[i], innerHTML);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void Show()
        {
            this.ShowInternal(true);
        }

        private void ShowInternal(bool fShow)
        {
            if ((this.htmlElemFrame != null) && (this.fVisible != fShow))
            {
                try
                {
                    System.Design.NativeMethods.IHTMLDOMNode htmlElemFrame = (System.Design.NativeMethods.IHTMLDOMNode) this.htmlElemFrame;
                    System.Design.NativeMethods.IHTMLStyle style = ((System.Design.NativeMethods.IHTMLElement) htmlElemFrame).GetStyle();
                    if (fShow)
                    {
                        style.SetDisplay(string.Empty);
                    }
                    else
                    {
                        if (this.templateElements != null)
                        {
                            for (int i = 0; i < this.templateElements.Length; i++)
                            {
                                if (this.templateElements[i] != null)
                                {
                                    ((System.Design.NativeMethods.IHTMLElement) this.templateElements[i]).SetInnerHTML(string.Empty);
                                }
                            }
                        }
                        style.SetDisplay("none");
                    }
                }
                catch (Exception)
                {
                }
                this.fVisible = fShow;
            }
        }

        private string StyleToCss(Style style)
        {
            StringBuilder builder = new StringBuilder();
            Color foreColor = style.ForeColor;
            if (!foreColor.IsEmpty)
            {
                builder.Append("color:");
                builder.Append(ColorTranslator.ToHtml(foreColor));
                builder.Append(";");
            }
            foreColor = style.BackColor;
            if (!foreColor.IsEmpty)
            {
                builder.Append("background-color:");
                builder.Append(ColorTranslator.ToHtml(foreColor));
                builder.Append(";");
            }
            FontInfo font = style.Font;
            string name = font.Name;
            if (name.Length != 0)
            {
                builder.Append("font-family:'");
                builder.Append(name);
                builder.Append("';");
            }
            if (font.Bold)
            {
                builder.Append("font-weight:bold;");
            }
            if (font.Italic)
            {
                builder.Append("font-style:italic;");
            }
            name = string.Empty;
            if (font.Underline)
            {
                name = name + "underline";
            }
            if (font.Strikeout)
            {
                name = name + " line-through";
            }
            if (font.Overline)
            {
                name = name + " overline";
            }
            if (name.Length != 0)
            {
                builder.Append("text-decoration:");
                builder.Append(name);
                builder.Append(';');
            }
            FontUnit size = font.Size;
            if (!size.IsEmpty)
            {
                builder.Append("font-size:");
                builder.Append(size.ToString(CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        public void UpdateControlName(string newName)
        {
            if (this.htmlElemControlName != null)
            {
                this.htmlElemControlName.SetInnerText(newName);
            }
        }

        private string Content
        {
            get
            {
                if (this.frameContent == null)
                {
                    this.frameContent = this.CreateFrameContent();
                }
                return this.frameContent;
            }
        }

        public Style ControlStyle
        {
            get
            {
                return this.controlStyle;
            }
        }

        public int InitialHeight
        {
            get
            {
                return this.initialHeight;
            }
            set
            {
                this.initialHeight = value;
            }
        }

        public int InitialWidth
        {
            get
            {
                return this.initialWidth;
            }
            set
            {
                this.initialWidth = value;
            }
        }

        public string Name
        {
            get
            {
                return this.frameName;
            }
        }

        public string[] TemplateNames
        {
            get
            {
                return this.templateNames;
            }
        }

        public Style[] TemplateStyles
        {
            get
            {
                return this.templateStyles;
            }
        }

        public TemplateEditingVerb Verb
        {
            get
            {
                return this.verb;
            }
            set
            {
                this.verb = value;
            }
        }
    }
}

