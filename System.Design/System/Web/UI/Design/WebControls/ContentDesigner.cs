namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ContentDesigner : ControlDesigner
    {
        private string _content;
        private ContentDefinition _contentDefinition;
        private const string _contentPlaceHolderIDProperty = "ContentPlaceHolderID";
        private IContentResolutionService _contentResolutionService;
        private const string _designtimeHTML = "<table cellspacing=0 cellpadding=0 style=\"border:1px solid black; width:100%; height:200px\">\r\n            <tr>\r\n              <td style=\"width:100%; height:25px; font-family:Tahoma; font-size:{2}pt; color:{3}; background-color:{4}; padding:5px; border-bottom:1px solid black;\">\r\n                &nbsp;{0}\r\n              </td>\r\n            </tr>\r\n            <tr>\r\n              <td style=\"width:100%; height:175px; vertical-align:top;\" {1}=\"0\">\r\n              </td>\r\n            </tr>\r\n          </table>";
        private const string _idProperty = "ID";

        private void ClearRegion()
        {
            if ((this.ContentResolutionService != null) && (this.GetContentDefinition() != null))
            {
                this.ContentResolutionService.SetContentDesignerState(this.GetContentDefinition().ContentPlaceHolderID, ContentDesignerState.ShowDefaultContent);
            }
        }

        private void CreateBlankContent()
        {
            if ((this.ContentResolutionService != null) && (this.GetContentDefinition() != null))
            {
                this.ContentResolutionService.SetContentDesignerState(this.GetContentDefinition().ContentPlaceHolderID, ContentDesignerState.ShowUserContent);
            }
        }

        private ContentDefinition GetContentDefinition()
        {
            if (this._contentDefinition == null)
            {
                try
                {
                    ContentDefinition definition = (ContentDefinition) this.ContentResolutionService.ContentDefinitions[((Content) base.Component).ContentPlaceHolderID];
                    this._contentDefinition = new ContentDefinition(definition.ContentPlaceHolderID, definition.DefaultContent, definition.DefaultDesignTimeHtml);
                }
                catch
                {
                }
            }
            return this._contentDefinition;
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            EditableDesignerRegion region = new EditableDesignerRegion(this, "Content");
            regions.Add(region);
            Font captionFont = SystemFonts.CaptionFont;
            Color controlText = SystemColors.ControlText;
            Color control = SystemColors.Control;
            string str = base.Component.GetType().Name + " - " + base.Component.Site.Name;
            return string.Format(CultureInfo.InvariantCulture, "<table cellspacing=0 cellpadding=0 style=\"border:1px solid black; width:100%; height:200px\">\r\n            <tr>\r\n              <td style=\"width:100%; height:25px; font-family:Tahoma; font-size:{2}pt; color:{3}; background-color:{4}; padding:5px; border-bottom:1px solid black;\">\r\n                &nbsp;{0}\r\n              </td>\r\n            </tr>\r\n            <tr>\r\n              <td style=\"width:100%; height:175px; vertical-align:top;\" {1}=\"0\">\r\n              </td>\r\n            </tr>\r\n          </table>", new object[] { str, DesignerRegion.DesignerRegionAttributeName, captionFont.SizeInPoints, ColorTranslator.ToHtml(controlText), ColorTranslator.ToHtml(control) });
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
            return this._content;
        }

        public override string GetPersistenceContent()
        {
            return this._content;
        }

        protected override void PostFilterProperties(IDictionary properties)
        {
            base.PostFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["ID"];
            PropertyDescriptor descriptor2 = (PropertyDescriptor) properties["ContentPlaceHolderID"];
            properties.Clear();
            ContentDesignerState showDefaultContent = ContentDesignerState.ShowDefaultContent;
            ContentDefinition contentDefinition = this.GetContentDefinition();
            if ((this.ContentResolutionService != null) && (contentDefinition != null))
            {
                showDefaultContent = this.ContentResolutionService.GetContentDesignerState(contentDefinition.ContentPlaceHolderID);
            }
            oldPropertyDescriptor = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { (showDefaultContent == ContentDesignerState.ShowDefaultContent) ? ReadOnlyAttribute.Yes : ReadOnlyAttribute.No });
            properties.Add("ID", oldPropertyDescriptor);
            descriptor2 = TypeDescriptor.CreateProperty(descriptor2.ComponentType, descriptor2, new Attribute[] { ReadOnlyAttribute.Yes });
            properties.Add("ContentPlaceHolderID", descriptor2);
        }

        protected override void PreFilterEvents(IDictionary events)
        {
            events.Clear();
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
            if (string.Compare(this._content, content, StringComparison.Ordinal) != 0)
            {
                this._content = content;
                base.Tag.SetDirty(true);
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new ContentDesignerActionList(this));
                return lists;
            }
        }

        public override bool AllowResize
        {
            get
            {
                return true;
            }
        }

        private IContentResolutionService ContentResolutionService
        {
            get
            {
                if (this._contentResolutionService == null)
                {
                    this._contentResolutionService = (IContentResolutionService) this.GetService(typeof(IContentResolutionService));
                }
                return this._contentResolutionService;
            }
        }

        private class ContentDesignerActionList : DesignerActionList
        {
            private ContentDesigner _parent;

            public ContentDesignerActionList(ContentDesigner parent) : base(parent.Component)
            {
                this._parent = parent;
            }

            public void ClearRegion()
            {
                this._parent.ClearRegion();
            }

            public void CreateBlankContent()
            {
                this._parent.CreateBlankContent();
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                ContentDesignerState showDefaultContent = ContentDesignerState.ShowDefaultContent;
                if ((this._parent.ContentResolutionService != null) && (this._parent.GetContentDefinition() != null))
                {
                    showDefaultContent = this._parent.ContentResolutionService.GetContentDesignerState(this._parent.GetContentDefinition().ContentPlaceHolderID);
                }
                if (showDefaultContent == ContentDesignerState.ShowDefaultContent)
                {
                    items.Add(new DesignerActionMethodItem(this, "CreateBlankContent", System.Design.SR.GetString("Content_CreateBlankContent"), string.Empty, string.Empty, true));
                    return items;
                }
                if (ContentDesignerState.ShowUserContent == showDefaultContent)
                {
                    items.Add(new DesignerActionMethodItem(this, "ClearRegion", System.Design.SR.GetString("Content_ClearRegion"), string.Empty, string.Empty, true));
                }
                return items;
            }

            public override bool AutoShow
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }
        }
    }
}

