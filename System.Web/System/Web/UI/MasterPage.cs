namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Web.Util;

    [ParseChildren(false), Designer("Microsoft.VisualStudio.Web.WebForms.MasterPageWebFormDesigner, Microsoft.VisualStudio.Web, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IRootDesigner)), ControlBuilder(typeof(MasterPageControlBuilder))]
    public class MasterPage : UserControl
    {
        private IList _contentPlaceHolders;
        private IDictionary _contentTemplateCollection;
        private IDictionary _contentTemplates;
        private MasterPage _master;
        private bool _masterPageApplied;
        private VirtualPath _masterPageFile;
        internal TemplateControl _ownerControl;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal void AddContentTemplate(string templateName, ITemplate template)
        {
            if (this._contentTemplateCollection == null)
            {
                this._contentTemplateCollection = new Hashtable(10, StringComparer.OrdinalIgnoreCase);
            }
            try
            {
                this._contentTemplateCollection.Add(templateName, template);
            }
            catch (ArgumentException)
            {
                throw new HttpException(System.Web.SR.GetString("MasterPage_Multiple_content", new object[] { templateName }));
            }
        }

        internal static void ApplyMasterRecursive(MasterPage master, IList appliedMasterFilePaths)
        {
            if (master.Master != null)
            {
                string str = master._masterPageFile.VirtualPathString.ToLower(CultureInfo.InvariantCulture);
                if (appliedMasterFilePaths.Contains(str))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("MasterPage_Circular_Master_Not_Allowed", new object[] { master._masterPageFile }));
                }
                appliedMasterFilePaths.Add(str);
                ApplyMasterRecursive(master.Master, appliedMasterFilePaths);
            }
            master._masterPageApplied = true;
        }

        internal static MasterPage CreateMaster(TemplateControl owner, HttpContext context, VirtualPath masterPageFile, IDictionary contentTemplateCollection)
        {
            MasterPage child = null;
            if (masterPageFile == null)
            {
                if ((contentTemplateCollection != null) && (contentTemplateCollection.Count > 0))
                {
                    throw new HttpException(System.Web.SR.GetString("Content_only_allowed_in_content_page"));
                }
                return null;
            }
            VirtualPath virtualPath = VirtualPathProvider.CombineVirtualPathsInternal(owner.TemplateControlVirtualPath, masterPageFile);
            ITypedWebObjectFactory vPathBuildResult = (ITypedWebObjectFactory) BuildManager.GetVPathBuildResult(context, virtualPath);
            if (!typeof(MasterPage).IsAssignableFrom(vPathBuildResult.InstantiatedType))
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_master_base", new object[] { masterPageFile }));
            }
            child = (MasterPage) vPathBuildResult.CreateInstance();
            child.TemplateControlVirtualPath = virtualPath;
            if (owner.HasControls())
            {
                foreach (Control control in owner.Controls)
                {
                    LiteralControl control2 = control as LiteralControl;
                    if ((control2 == null) || (System.Web.UI.Util.FirstNonWhiteSpaceIndex(control2.Text) >= 0))
                    {
                        throw new HttpException(System.Web.SR.GetString("Content_allowed_in_top_level_only"));
                    }
                }
                owner.Controls.Clear();
            }
            if (owner.Controls.IsReadOnly)
            {
                throw new HttpException(System.Web.SR.GetString("MasterPage_Cannot_ApplyTo_ReadOnly_Collection"));
            }
            if (contentTemplateCollection != null)
            {
                foreach (string str in contentTemplateCollection.Keys)
                {
                    if (!child.ContentPlaceHolders.Contains(str.ToLower(CultureInfo.InvariantCulture)))
                    {
                        throw new HttpException(System.Web.SR.GetString("MasterPage_doesnt_have_contentplaceholder", new object[] { str, masterPageFile }));
                    }
                }
                child._contentTemplates = contentTemplateCollection;
            }
            child._ownerControl = owner;
            child.InitializeAsUserControl(owner.Page);
            owner.Controls.Add(child);
            return child;
        }

        public void InstantiateInContentPlaceHolder(Control contentPlaceHolder, ITemplate template)
        {
            HttpContext current = HttpContext.Current;
            TemplateControl templateControl = current.TemplateControl;
            current.TemplateControl = this._ownerControl;
            try
            {
                template.InstantiateIn(contentPlaceHolder);
            }
            finally
            {
                current.TemplateControl = templateControl;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        protected internal IList ContentPlaceHolders
        {
            get
            {
                if (this._contentPlaceHolders == null)
                {
                    this._contentPlaceHolders = new ArrayList();
                }
                return this._contentPlaceHolders;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal IDictionary ContentTemplates
        {
            get
            {
                return this._contentTemplates;
            }
        }

        [Browsable(false), WebSysDescription("MasterPage_MasterPage"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MasterPage Master
        {
            get
            {
                if ((this._master == null) && !this._masterPageApplied)
                {
                    this._master = CreateMaster(this, this.Context, this._masterPageFile, this._contentTemplateCollection);
                }
                return this._master;
            }
        }

        [DefaultValue(""), WebCategory("Behavior"), WebSysDescription("MasterPage_MasterPageFile")]
        public string MasterPageFile
        {
            get
            {
                return VirtualPath.GetVirtualPathString(this._masterPageFile);
            }
            set
            {
                if (this._masterPageApplied)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("PropertySetBeforePageEvent", new object[] { "MasterPageFile", "Page_PreInit" }));
                }
                if (value != VirtualPath.GetVirtualPathString(this._masterPageFile))
                {
                    this._masterPageFile = VirtualPath.CreateAllowNull(value);
                    if ((this._master != null) && this.Controls.Contains(this._master))
                    {
                        this.Controls.Remove(this._master);
                    }
                    this._master = null;
                }
            }
        }
    }
}

