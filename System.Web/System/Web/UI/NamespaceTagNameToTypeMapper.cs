namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Reflection;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Web.UI.Design;

    internal class NamespaceTagNameToTypeMapper : ITagNameToTypeMapper
    {
        private Assembly _assembly;
        private System.Web.UI.TagNamespaceRegisterEntry _nsRegisterEntry;
        private TemplateParser _parser;

        internal NamespaceTagNameToTypeMapper(System.Web.UI.TagNamespaceRegisterEntry nsRegisterEntry, Assembly assembly, TemplateParser parser)
        {
            this._nsRegisterEntry = nsRegisterEntry;
            this._assembly = assembly;
            this._parser = parser;
        }

        internal Type GetControlType(string tagName, IDictionary attribs, bool throwOnError)
        {
            string str;
            string str2 = this._nsRegisterEntry.Namespace;
            if (string.IsNullOrEmpty(str2))
            {
                str = tagName;
            }
            else
            {
                str = str2 + "." + tagName;
            }
            if (this._assembly != null)
            {
                Type type = null;
                if (throwOnError)
                {
                    try
                    {
                        return this._assembly.GetType(str, true, true);
                    }
                    catch (FileNotFoundException)
                    {
                        throw;
                    }
                    catch (FileLoadException)
                    {
                        throw;
                    }
                    catch (BadImageFormatException)
                    {
                        throw;
                    }
                    catch
                    {
                        return type;
                    }
                }
                return this._assembly.GetType(str, false, true);
            }
            if (this._parser.FInDesigner && (this._parser.DesignerHost != null))
            {
                if (this._parser.DesignerHost.RootComponent != null)
                {
                    WebFormsRootDesigner designer = this._parser.DesignerHost.GetDesigner(this._parser.DesignerHost.RootComponent) as WebFormsRootDesigner;
                    if (designer != null)
                    {
                        WebFormsReferenceManager referenceManager = designer.ReferenceManager;
                        if (referenceManager != null)
                        {
                            Type type2 = referenceManager.GetType(this._nsRegisterEntry.TagPrefix, tagName);
                            if (type2 != null)
                            {
                                return type2;
                            }
                        }
                    }
                }
                ITypeResolutionService service = (ITypeResolutionService) this._parser.DesignerHost.GetService(typeof(ITypeResolutionService));
                if (service != null)
                {
                    Type type3 = service.GetType(str, false, true);
                    if (type3 != null)
                    {
                        return type3;
                    }
                }
            }
            if (!HostingEnvironment.IsHosted)
            {
                return null;
            }
            return BuildManager.GetTypeFromCodeAssembly(str, true);
        }

        Type ITagNameToTypeMapper.GetControlType(string tagName, IDictionary attribs)
        {
            return this.GetControlType(tagName, attribs, false);
        }

        public System.Web.UI.TagNamespaceRegisterEntry RegisterEntry
        {
            get
            {
                return this._nsRegisterEntry;
            }
        }
    }
}

