namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml;

    public sealed class ImportCatalogPart : CatalogPart
    {
        private WebPart _availableWebPart;
        private WebPartDescriptionCollection _availableWebPartDescriptions;
        private string _importedPartDescription;
        private string _importErrorMessage;
        private FileUpload _upload;
        private Button _uploadButton;
        private const int baseIndex = 0;
        private const int controlStateArrayLength = 2;
        private const string DescriptionPropertyName = "Description";
        private static readonly WebPartDescriptionCollection DesignModeAvailableWebPart = new WebPartDescriptionCollection(new WebPartDescription[] { new WebPartDescription("webpart1", string.Format(CultureInfo.CurrentCulture, System.Web.SR.GetString("CatalogPart_SampleWebPartTitle"), new object[] { "1" }), null, null) });
        private const string IconPropertyName = "CatalogIconImageUrl";
        private const int importedPartDescriptionIndex = 1;
        private const string ImportedWebPartID = "ImportedWebPart";
        private const string TitlePropertyName = "Title";

        private void CreateAvailableWebPartDescriptions()
        {
            if (this._availableWebPartDescriptions == null)
            {
                if ((base.WebPartManager == null) || string.IsNullOrEmpty(this._importedPartDescription))
                {
                    this._availableWebPartDescriptions = new WebPartDescriptionCollection();
                }
                else
                {
                    PermissionSet set = new PermissionSet(PermissionState.None);
                    set.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    set.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal));
                    set.PermitOnly();
                    bool flag = true;
                    string str = null;
                    string description = null;
                    string imageUrl = null;
                    try
                    {
                        try
                        {
                            using (StringReader reader = new StringReader(this._importedPartDescription))
                            {
                                using (XmlTextReader reader2 = new XmlTextReader(reader))
                                {
                                    if (reader2 == null)
                                    {
                                        goto Label_02F7;
                                    }
                                    reader2.MoveToContent();
                                    reader2.MoveToContent();
                                    reader2.ReadStartElement("webParts");
                                    reader2.ReadStartElement("webPart");
                                    reader2.ReadStartElement("metaData");
                                    string str4 = null;
                                    string path = null;
                                    while (reader2.Name != "type")
                                    {
                                        reader2.Skip();
                                        if (reader2.EOF)
                                        {
                                            throw new EndOfStreamException();
                                        }
                                    }
                                    if (reader2.Name == "type")
                                    {
                                        str4 = reader2.GetAttribute("name");
                                        path = reader2.GetAttribute("src");
                                    }
                                    bool isShared = base.WebPartManager.Personalization.Scope == PersonalizationScope.Shared;
                                    if (!string.IsNullOrEmpty(str4))
                                    {
                                        PermissionSet set2 = new PermissionSet(PermissionState.None);
                                        set2.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                                        set2.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Medium));
                                        CodeAccessPermission.RevertPermitOnly();
                                        flag = false;
                                        set2.PermitOnly();
                                        flag = true;
                                        Type type = WebPartUtil.DeserializeType(str4, true);
                                        CodeAccessPermission.RevertPermitOnly();
                                        flag = false;
                                        set.PermitOnly();
                                        flag = true;
                                        if (!base.WebPartManager.IsAuthorized(type, null, null, isShared))
                                        {
                                            this._importErrorMessage = System.Web.SR.GetString("WebPartManager_ForbiddenType");
                                        }
                                        else
                                        {
                                            if (type.IsSubclassOf(typeof(WebPart)) || type.IsSubclassOf(typeof(Control)))
                                            {
                                                goto Label_02DD;
                                            }
                                            this._importErrorMessage = System.Web.SR.GetString("WebPartManager_TypeMustDeriveFromControl");
                                        }
                                    }
                                    else
                                    {
                                        if (base.WebPartManager.IsAuthorized(typeof(UserControl), path, null, isShared))
                                        {
                                            goto Label_02DD;
                                        }
                                        this._importErrorMessage = System.Web.SR.GetString("WebPartManager_ForbiddenType");
                                    }
                                    return;
                                Label_021E:
                                    reader2.Read();
                                Label_0226:
                                    if (!reader2.EOF && ((reader2.NodeType != XmlNodeType.Element) || !(reader2.Name == "property")))
                                    {
                                        goto Label_021E;
                                    }
                                    if (reader2.EOF)
                                    {
                                        goto Label_02F7;
                                    }
                                    string attribute = reader2.GetAttribute("name");
                                    if (attribute == "Title")
                                    {
                                        str = reader2.ReadElementString();
                                    }
                                    else if (attribute == "Description")
                                    {
                                        description = reader2.ReadElementString();
                                    }
                                    else if (attribute == "CatalogIconImageUrl")
                                    {
                                        string s = reader2.ReadElementString().Trim();
                                        if (!CrossSiteScriptingValidation.IsDangerousUrl(s))
                                        {
                                            imageUrl = s;
                                        }
                                    }
                                    else
                                    {
                                        reader2.Read();
                                        goto Label_02DD;
                                    }
                                    if (((str != null) && (description != null)) && (imageUrl != null))
                                    {
                                        goto Label_02F7;
                                    }
                                    reader2.Read();
                                Label_02DD:
                                    if (!reader2.EOF)
                                    {
                                        goto Label_0226;
                                    }
                                }
                            Label_02F7:
                                if (string.IsNullOrEmpty(str))
                                {
                                    str = System.Web.SR.GetString("Part_Untitled");
                                }
                                this._availableWebPartDescriptions = new WebPartDescriptionCollection(new WebPartDescription[] { new WebPartDescription("ImportedWebPart", str, description, imageUrl) });
                            }
                        }
                        catch (XmlException)
                        {
                            this._importErrorMessage = System.Web.SR.GetString("WebPartManager_ImportInvalidFormat");
                        }
                        catch
                        {
                            this._importErrorMessage = !string.IsNullOrEmpty(this._importErrorMessage) ? this._importErrorMessage : System.Web.SR.GetString("WebPart_DefaultImportErrorMessage");
                        }
                        finally
                        {
                            if (flag)
                            {
                                CodeAccessPermission.RevertPermitOnly();
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            this._upload = new FileUpload();
            this.Controls.Add(this._upload);
            this._uploadButton = new Button();
            this._uploadButton.ID = "Upload";
            this._uploadButton.CommandName = "upload";
            this._uploadButton.Click += new EventHandler(this.OnUpload);
            this.Controls.Add(this._uploadButton);
            if (!base.DesignMode && (this.Page != null))
            {
                IScriptManager scriptManager = this.Page.ScriptManager;
                if (scriptManager != null)
                {
                    scriptManager.RegisterPostBackControl(this._uploadButton);
                }
            }
        }

        public override WebPartDescriptionCollection GetAvailableWebPartDescriptions()
        {
            if (base.DesignMode)
            {
                return DesignModeAvailableWebPart;
            }
            this.CreateAvailableWebPartDescriptions();
            return this._availableWebPartDescriptions;
        }

        public override WebPart GetWebPart(WebPartDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }
            if (!this.GetAvailableWebPartDescriptions().Contains(description))
            {
                throw new ArgumentException(System.Web.SR.GetString("CatalogPart_UnknownDescription"), "description");
            }
            if (this._availableWebPart == null)
            {
                using (XmlTextReader reader = new XmlTextReader(new StringReader(this._importedPartDescription)))
                {
                    if ((reader != null) && (base.WebPartManager != null))
                    {
                        this._availableWebPart = base.WebPartManager.ImportWebPart(reader, out this._importErrorMessage);
                    }
                }
                if (this._availableWebPart == null)
                {
                    this._importedPartDescription = null;
                    this._availableWebPartDescriptions = null;
                }
            }
            return this._availableWebPart;
        }

        protected internal override void LoadControlState(object savedState)
        {
            if (savedState == null)
            {
                base.LoadControlState(null);
            }
            else
            {
                object[] objArray = (object[]) savedState;
                if (objArray.Length != 2)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_ControlState"));
                }
                base.LoadControlState(objArray[0]);
                if (objArray[1] != null)
                {
                    this._importedPartDescription = (string) objArray[1];
                    this.GetAvailableWebPartDescriptions();
                }
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Page.RegisterRequiresControlState(this);
        }

        internal void OnUpload(object sender, EventArgs e)
        {
            string fileName = this._upload.FileName;
            Stream fileContent = this._upload.FileContent;
            if (!string.IsNullOrEmpty(fileName) && (fileContent != null))
            {
                using (StreamReader reader = new StreamReader(fileContent, true))
                {
                    this._importedPartDescription = reader.ReadToEnd();
                    this._availableWebPart = null;
                    this._availableWebPartDescriptions = null;
                    this._importErrorMessage = null;
                    if (string.IsNullOrEmpty(this._importedPartDescription))
                    {
                        this._importErrorMessage = System.Web.SR.GetString("ImportCatalogPart_NoFileName");
                    }
                    else
                    {
                        this.GetAvailableWebPartDescriptions();
                    }
                    return;
                }
            }
            this._importErrorMessage = System.Web.SR.GetString("ImportCatalogPart_NoFileName");
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            base.Render(writer);
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            this.EnsureChildControls();
            CatalogZoneBase zone = base.Zone;
            if ((zone != null) && !zone.LabelStyle.IsEmpty)
            {
                zone.LabelStyle.AddAttributesToRender(writer, this);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.For, this._upload.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.Write(this.BrowseHelpText);
            writer.RenderEndTag();
            writer.WriteBreak();
            if ((zone != null) && !zone.EditUIStyle.IsEmpty)
            {
                this._upload.ApplyStyle(zone.EditUIStyle);
            }
            this._upload.RenderControl(writer);
            writer.WriteBreak();
            if ((zone != null) && !zone.LabelStyle.IsEmpty)
            {
                zone.LabelStyle.AddAttributesToRender(writer, this);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(this.UploadHelpText);
            writer.RenderEndTag();
            writer.WriteBreak();
            if ((zone != null) && !zone.EditUIStyle.IsEmpty)
            {
                this._uploadButton.ApplyStyle(zone.EditUIStyle);
            }
            this._uploadButton.Text = this.UploadButtonText;
            this._uploadButton.RenderControl(writer);
            if (((this._importedPartDescription != null) || (this._importErrorMessage != null)) || base.DesignMode)
            {
                writer.WriteBreak();
                if (this._importErrorMessage != null)
                {
                    if ((zone != null) && !zone.ErrorStyle.IsEmpty)
                    {
                        zone.ErrorStyle.AddAttributesToRender(writer, this);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(this.PartImportErrorLabelText);
                    writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                    writer.RenderEndTag();
                    if ((zone != null) && !zone.ErrorStyle.IsEmpty)
                    {
                        zone.ErrorStyle.AddAttributesToRender(writer, this);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.WriteEncodedText(this._importErrorMessage);
                    writer.RenderEndTag();
                }
                else
                {
                    if ((zone != null) && !zone.LabelStyle.IsEmpty)
                    {
                        zone.LabelStyle.AddAttributesToRender(writer, this);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(this.ImportedPartLabelText);
                    writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                    writer.RenderEndTag();
                }
            }
        }

        protected internal override object SaveControlState()
        {
            object[] objArray = new object[] { base.SaveControlState(), this._importedPartDescription };
            for (int i = 0; i < 2; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        [WebCategory("Appearance"), WebSysDescription("ImportCatalogPart_BrowseHelpText"), WebSysDefaultValue("ImportCatalogPart_Browse")]
        public string BrowseHelpText
        {
            get
            {
                object obj2 = this.ViewState["BrowseHelpText"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("ImportCatalogPart_Browse");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["BrowseHelpText"] = value;
            }
        }

        [Themeable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override string DefaultButton
        {
            get
            {
                return base.DefaultButton;
            }
            set
            {
                base.DefaultButton = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("ImportCatalogPart_ImportedPartLabelText"), WebSysDefaultValue("ImportCatalogPart_ImportedPartLabel")]
        public string ImportedPartLabelText
        {
            get
            {
                object obj2 = this.ViewState["ImportedPartLabelText"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("ImportCatalogPart_ImportedPartLabel");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ImportedPartLabelText"] = value;
            }
        }

        [WebSysDescription("ImportCatalogPart_PartImportErrorLabelText"), WebCategory("Appearance"), WebSysDefaultValue("ImportCatalogPart_ImportedPartErrorLabel")]
        public string PartImportErrorLabelText
        {
            get
            {
                object obj2 = this.ViewState["PartImportErrorLabelText"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("ImportCatalogPart_ImportedPartErrorLabel");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["PartImportErrorLabelText"] = value;
            }
        }

        [WebSysDefaultValue("ImportCatalogPart_PartTitle")]
        public override string Title
        {
            get
            {
                string str = (string) this.ViewState["Title"];
                if (str == null)
                {
                    return System.Web.SR.GetString("ImportCatalogPart_PartTitle");
                }
                return str;
            }
            set
            {
                this.ViewState["Title"] = value;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("ImportCatalogPart_UploadButtonText"), WebSysDefaultValue("ImportCatalogPart_UploadButton")]
        public string UploadButtonText
        {
            get
            {
                object obj2 = this.ViewState["UploadButtonText"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("ImportCatalogPart_UploadButton");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["UploadButtonText"] = value;
            }
        }

        [WebSysDescription("ImportCatalogPart_UploadHelpText"), WebCategory("Appearance"), WebSysDefaultValue("ImportCatalogPart_Upload")]
        public string UploadHelpText
        {
            get
            {
                object obj2 = this.ViewState["UploadHelpText"];
                if (obj2 == null)
                {
                    return System.Web.SR.GetString("ImportCatalogPart_Upload");
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["UploadHelpText"] = value;
            }
        }
    }
}

