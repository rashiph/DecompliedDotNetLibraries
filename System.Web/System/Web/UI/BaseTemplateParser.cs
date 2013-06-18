namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Util;

    public abstract class BaseTemplateParser : TemplateParser
    {
        private const string _namespaceString = "namespace";
        private const string _sourceString = "src";
        private const string _tagnameString = "tagname";

        protected BaseTemplateParser()
        {
        }

        internal Type GetDesignTimeUserControlType(string tagPrefix, string tagName)
        {
            Type type = typeof(UserControl);
            IDesignerHost designerHost = base.DesignerHost;
            if (designerHost != null)
            {
                IUserControlTypeResolutionService service = (IUserControlTypeResolutionService) designerHost.GetService(typeof(IUserControlTypeResolutionService));
                if (service == null)
                {
                    return type;
                }
                try
                {
                    type = service.GetType(tagPrefix, tagName);
                }
                catch
                {
                }
            }
            return type;
        }

        protected Type GetReferencedType(string virtualPath)
        {
            return this.GetReferencedType(VirtualPath.Create(virtualPath));
        }

        internal Type GetReferencedType(VirtualPath virtualPath)
        {
            return this.GetReferencedType(virtualPath, true);
        }

        internal Type GetReferencedType(VirtualPath virtualPath, bool allowNoCompile)
        {
            virtualPath = base.ResolveVirtualPath(virtualPath);
            if ((base._pageParserFilter != null) && !base._pageParserFilter.AllowVirtualReference(base.CompConfig, virtualPath))
            {
                base.ProcessError(System.Web.SR.GetString("Reference_not_allowed", new object[] { virtualPath }));
            }
            BuildResult vPathBuildResult = null;
            Type baseType = null;
            try
            {
                vPathBuildResult = BuildManager.GetVPathBuildResult(virtualPath);
            }
            catch (HttpCompileException exception)
            {
                if (exception.VirtualPathDependencies != null)
                {
                    foreach (string str in exception.VirtualPathDependencies)
                    {
                        base.AddSourceDependency(VirtualPath.Create(str));
                    }
                }
                throw;
            }
            catch
            {
                if (this.IgnoreParseErrors)
                {
                    base.AddSourceDependency(virtualPath);
                }
                throw;
            }
            BuildResultNoCompileTemplateControl control = vPathBuildResult as BuildResultNoCompileTemplateControl;
            if (control != null)
            {
                if (!allowNoCompile)
                {
                    return null;
                }
                baseType = control.BaseType;
            }
            else
            {
                if (!(vPathBuildResult is BuildResultCompiledType))
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_typeless_reference", new object[] { "src" }));
                }
                BuildResultCompiledType type2 = (BuildResultCompiledType) vPathBuildResult;
                baseType = type2.ResultType;
            }
            base.AddTypeDependency(baseType);
            base.AddBuildResultDependency(vPathBuildResult);
            return baseType;
        }

        protected internal Type GetUserControlType(string virtualPath)
        {
            return this.GetUserControlType(VirtualPath.Create(virtualPath));
        }

        internal Type GetUserControlType(VirtualPath virtualPath)
        {
            Type referencedType = this.GetReferencedType(virtualPath, false);
            if (referencedType == null)
            {
                if (base._pageParserFilter != null)
                {
                    referencedType = base._pageParserFilter.GetNoCompileUserControlType();
                }
                if (referencedType == null)
                {
                    base.ProcessError(System.Web.SR.GetString("Cant_use_nocompile_uc", new object[] { virtualPath }));
                }
                return referencedType;
            }
            System.Web.UI.Util.CheckAssignableType(typeof(UserControl), referencedType);
            return referencedType;
        }

        internal override void ProcessDirective(string directiveName, IDictionary directive)
        {
            if (StringUtil.EqualsIgnoreCase(directiveName, "register"))
            {
                RegisterDirectiveEntry entry;
                string tagPrefix = System.Web.UI.Util.GetAndRemoveNonEmptyIdentifierAttribute(directive, "tagprefix", true);
                string tagName = System.Web.UI.Util.GetAndRemoveNonEmptyIdentifierAttribute(directive, "tagname", false);
                VirtualPath path = System.Web.UI.Util.GetAndRemoveVirtualPathAttribute(directive, "src", false);
                string namespaceName = System.Web.UI.Util.GetAndRemoveNonEmptyNoSpaceAttribute(directive, "namespace", false);
                string assemblyName = System.Web.UI.Util.GetAndRemoveNonEmptyAttribute(directive, "assembly", false);
                if (tagName != null)
                {
                    if (path == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Missing_attr", new object[] { "src" }));
                    }
                    if (namespaceName != null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Invalid_attr", new object[] { "namespace", "tagname" }));
                    }
                    if (assemblyName != null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Invalid_attr", new object[] { "assembly", "tagname" }));
                    }
                    UserControlRegisterEntry ucRegisterEntry = new UserControlRegisterEntry(tagPrefix, tagName) {
                        UserControlSource = path
                    };
                    entry = ucRegisterEntry;
                    base.TypeMapper.ProcessUserControlRegistration(ucRegisterEntry);
                }
                else
                {
                    if (path != null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Missing_attr", new object[] { "tagname" }));
                    }
                    if (namespaceName == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Missing_attr", new object[] { "namespace" }));
                    }
                    TagNamespaceRegisterEntry nsRegisterEntry = new TagNamespaceRegisterEntry(tagPrefix, namespaceName, assemblyName);
                    entry = nsRegisterEntry;
                    base.TypeMapper.ProcessTagNamespaceRegistration(nsRegisterEntry);
                }
                entry.Line = base._lineNumber;
                entry.VirtualPath = base.CurrentVirtualPathString;
                System.Web.UI.Util.CheckUnknownDirectiveAttributes(directiveName, directive);
            }
            else
            {
                base.ProcessDirective(directiveName, directive);
            }
        }
    }
}

