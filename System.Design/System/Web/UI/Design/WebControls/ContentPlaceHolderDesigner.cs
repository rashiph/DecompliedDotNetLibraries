namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ContentPlaceHolderDesigner : ControlDesigner
    {
        private string _content;
        private const string designtimeHTML = "<table cellspacing=0 cellpadding=0 style=\"border:1px solid black; width:100%; height:200px;\">\r\n            <tr>\r\n              <td style=\"width:100%; height:25px; font-family:Tahoma; font-size:{2}pt; color:{3}; background-color:{4}; padding:5px; border-bottom:1px solid black;\">\r\n                &nbsp;{0}\r\n              </td>\r\n            </tr>\r\n            <tr>\r\n              <td style=\"width:100%; height:175px; vertical-align:top;\" {1}=\"0\">\r\n              </td>\r\n            </tr>\r\n          </table>";

        private string CreateDesignTimeHTML()
        {
            StringBuilder builder = new StringBuilder(0x400);
            Font captionFont = SystemFonts.CaptionFont;
            Color controlText = SystemColors.ControlText;
            Color control = SystemColors.Control;
            string str = base.Component.GetType().Name + " - " + base.Component.Site.Name;
            builder.Append(string.Format(CultureInfo.InvariantCulture, "<table cellspacing=0 cellpadding=0 style=\"border:1px solid black; width:100%; height:200px;\">\r\n            <tr>\r\n              <td style=\"width:100%; height:25px; font-family:Tahoma; font-size:{2}pt; color:{3}; background-color:{4}; padding:5px; border-bottom:1px solid black;\">\r\n                &nbsp;{0}\r\n              </td>\r\n            </tr>\r\n            <tr>\r\n              <td style=\"width:100%; height:175px; vertical-align:top;\" {1}=\"0\">\r\n              </td>\r\n            </tr>\r\n          </table>", new object[] { str, DesignerRegion.DesignerRegionAttributeName, captionFont.SizeInPoints, ColorTranslator.ToHtml(controlText), ColorTranslator.ToHtml(control) }));
            return builder.ToString();
        }

        public override string GetDesignTimeHtml()
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (!(service.RootComponent is MasterPage))
            {
                throw new InvalidOperationException(System.Design.SR.GetString("ContentPlaceHolder_Invalid_RootComponent"));
            }
            return base.GetDesignTimeHtml();
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (!(service.RootComponent is MasterPage))
            {
                throw new InvalidOperationException(System.Design.SR.GetString("ContentPlaceHolder_Invalid_RootComponent"));
            }
            regions.Add(new EditableDesignerRegion(this, "Content"));
            return this.CreateDesignTimeHTML();
        }

        public override string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            if (this._content == null)
            {
                this._content = base.Tag.GetContent();
            }
            if (this._content == null)
            {
                return string.Empty;
            }
            return this._content.Trim();
        }

        public override string GetPersistenceContent()
        {
            return this._content;
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
            this._content = content;
            base.Tag.SetDirty(true);
        }

        public override bool AllowResize
        {
            get
            {
                return true;
            }
        }
    }
}

