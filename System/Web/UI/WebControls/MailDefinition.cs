namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.IO;
    using System.Net.Configuration;
    using System.Net.Mail;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;

    [TypeConverter(typeof(EmptyStringExpandableObjectConverter)), ParseChildren(true, ""), Bindable(false)]
    public sealed class MailDefinition : IStateManager
    {
        private string _bodyFileName;
        private EmbeddedMailObjectsCollection _embeddedObjects;
        private bool _isTrackingViewState;
        private StateBag _viewState;

        public MailMessage CreateMailMessage(string recipients, IDictionary replacements, Control owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            string body = string.Empty;
            string bodyFileName = this.BodyFileName;
            if (!string.IsNullOrEmpty(bodyFileName))
            {
                string path = bodyFileName;
                if (!UrlPath.IsAbsolutePhysicalPath(path))
                {
                    path = UrlPath.Combine(owner.AppRelativeTemplateSourceDirectory, path);
                }
                TextReader reader = new StreamReader(owner.OpenFile(path));
                try
                {
                    body = reader.ReadToEnd();
                }
                finally
                {
                    reader.Close();
                }
            }
            return this.CreateMailMessage(recipients, replacements, body, owner);
        }

        public MailMessage CreateMailMessage(string recipients, IDictionary replacements, string body, Control owner)
        {
            MailMessage message2;
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            string from = this.From;
            if (string.IsNullOrEmpty(from))
            {
                SmtpSection smtp = RuntimeConfig.GetConfig().Smtp;
                if (((smtp == null) || (smtp.Network == null)) || string.IsNullOrEmpty(smtp.From))
                {
                    throw new HttpException(System.Web.SR.GetString("MailDefinition_NoFromAddressSpecified"));
                }
                from = smtp.From;
            }
            MailMessage message = null;
            try
            {
                message = new MailMessage(from, recipients);
                if (!string.IsNullOrEmpty(this.CC))
                {
                    message.CC.Add(this.CC);
                }
                if (!string.IsNullOrEmpty(this.Subject))
                {
                    message.Subject = this.Subject;
                }
                message.Priority = this.Priority;
                if ((replacements != null) && !string.IsNullOrEmpty(body))
                {
                    foreach (object obj2 in replacements.Keys)
                    {
                        string pattern = obj2 as string;
                        string replacement = replacements[obj2] as string;
                        if ((pattern == null) || (replacement == null))
                        {
                            throw new ArgumentException(System.Web.SR.GetString("MailDefinition_InvalidReplacements"));
                        }
                        replacement = replacement.Replace("$", "$$");
                        body = Regex.Replace(body, pattern, replacement, RegexOptions.IgnoreCase);
                    }
                }
                if (this.EmbeddedObjects.Count > 0)
                {
                    string mediaType = this.IsBodyHtml ? "text/html" : "text/plain";
                    AlternateView item = AlternateView.CreateAlternateViewFromString(body, null, mediaType);
                    foreach (EmbeddedMailObject obj3 in this.EmbeddedObjects)
                    {
                        string path = obj3.Path;
                        if (string.IsNullOrEmpty(path))
                        {
                            throw ExceptionUtil.PropertyNullOrEmpty("EmbeddedMailObject.Path");
                        }
                        if (!UrlPath.IsAbsolutePhysicalPath(path))
                        {
                            path = VirtualPath.Combine(owner.TemplateControlVirtualDirectory, VirtualPath.Create(path)).AppRelativeVirtualPathString;
                        }
                        LinkedResource resource = null;
                        try
                        {
                            Stream contentStream = null;
                            try
                            {
                                contentStream = owner.OpenFile(path);
                                resource = new LinkedResource(contentStream);
                            }
                            catch
                            {
                                if (contentStream != null)
                                {
                                    contentStream.Dispose();
                                }
                                throw;
                            }
                            resource.ContentId = obj3.Name;
                            resource.ContentType.Name = UrlPath.GetFileName(path);
                            item.LinkedResources.Add(resource);
                        }
                        catch
                        {
                            if (resource != null)
                            {
                                resource.Dispose();
                            }
                            throw;
                        }
                    }
                    message.AlternateViews.Add(item);
                }
                else if (!string.IsNullOrEmpty(body))
                {
                    message.Body = body;
                }
                message.IsBodyHtml = this.IsBodyHtml;
                message2 = message;
            }
            catch
            {
                if (message != null)
                {
                    message.Dispose();
                }
                throw;
            }
            return message2;
        }

        void IStateManager.LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                ((IStateManager) this.ViewState).LoadViewState(savedState);
            }
        }

        object IStateManager.SaveViewState()
        {
            if (this._viewState != null)
            {
                return ((IStateManager) this._viewState).SaveViewState();
            }
            return null;
        }

        void IStateManager.TrackViewState()
        {
            this._isTrackingViewState = true;
            if (this._viewState != null)
            {
                ((IStateManager) this._viewState).TrackViewState();
            }
        }

        [DefaultValue(""), WebSysDescription("MailDefinition_BodyFileName"), NotifyParentProperty(true), UrlProperty("*.*"), Editor("System.Web.UI.Design.WebControls.MailDefinitionBodyFileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Behavior")]
        public string BodyFileName
        {
            get
            {
                if (this._bodyFileName != null)
                {
                    return this._bodyFileName;
                }
                return string.Empty;
            }
            set
            {
                this._bodyFileName = value;
            }
        }

        [WebSysDescription("MailDefinition_CC"), WebCategory("Behavior"), NotifyParentProperty(true), DefaultValue("")]
        public string CC
        {
            get
            {
                object obj2 = this.ViewState["CC"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["CC"] = value;
            }
        }

        [WebSysDescription("MailDefinition_EmbeddedObjects"), WebCategory("Behavior"), DefaultValue((string) null), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
        public EmbeddedMailObjectsCollection EmbeddedObjects
        {
            get
            {
                if (this._embeddedObjects == null)
                {
                    this._embeddedObjects = new EmbeddedMailObjectsCollection();
                }
                return this._embeddedObjects;
            }
        }

        [WebSysDescription("MailDefinition_From"), WebCategory("Behavior"), NotifyParentProperty(true), DefaultValue("")]
        public string From
        {
            get
            {
                object obj2 = this.ViewState["From"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["From"] = value;
            }
        }

        [DefaultValue(false), WebSysDescription("MailDefinition_IsBodyHtml"), WebCategory("Behavior"), NotifyParentProperty(true)]
        public bool IsBodyHtml
        {
            get
            {
                object obj2 = this.ViewState["IsBodyHtml"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["IsBodyHtml"] = value;
            }
        }

        [WebCategory("Behavior"), DefaultValue(0), WebSysDescription("MailDefinition_Priority"), NotifyParentProperty(true)]
        public MailPriority Priority
        {
            get
            {
                object obj2 = this.ViewState["Priority"];
                if (obj2 != null)
                {
                    return (MailPriority) obj2;
                }
                return MailPriority.Normal;
            }
            set
            {
                if ((value < MailPriority.Normal) || (value > MailPriority.High))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["Priority"] = value;
            }
        }

        [NotifyParentProperty(true), WebSysDescription("MailDefinition_Subject"), WebCategory("Behavior"), DefaultValue("")]
        public string Subject
        {
            get
            {
                object obj2 = this.ViewState["Subject"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["Subject"] = value;
            }
        }

        internal string SubjectInternal
        {
            get
            {
                return (string) this.ViewState["Subject"];
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this._isTrackingViewState;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private StateBag ViewState
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

