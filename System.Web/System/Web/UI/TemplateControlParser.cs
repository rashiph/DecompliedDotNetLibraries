namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Util;

    public abstract class TemplateControlParser : BaseTemplateParser
    {
        private IDictionary _outputCacheDirective;
        private System.Web.UI.OutputCacheParameters _outputCacheSettings;

        protected TemplateControlParser()
        {
        }

        private void AddStaticObjectAssemblyDependencies(HttpStaticObjectsCollection staticObjects)
        {
            if ((staticObjects != null) && (staticObjects.Objects != null))
            {
                IDictionaryEnumerator enumerator = staticObjects.Objects.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry) enumerator.Value;
                    base.AddTypeDependency(entry.ObjectType);
                }
            }
        }

        internal Type GetDirectiveType(IDictionary directive, string directiveName)
        {
            string andRemoveNonEmptyNoSpaceAttribute = Util.GetAndRemoveNonEmptyNoSpaceAttribute(directive, "typeName");
            VirtualPath andRemoveVirtualPathAttribute = Util.GetAndRemoveVirtualPathAttribute(directive, "virtualPath");
            Type referencedType = null;
            if ((andRemoveNonEmptyNoSpaceAttribute == null) == (andRemoveVirtualPathAttribute == null))
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_typeNameOrVirtualPath_directive", new object[] { directiveName }));
            }
            if (andRemoveNonEmptyNoSpaceAttribute != null)
            {
                referencedType = base.GetType(andRemoveNonEmptyNoSpaceAttribute);
                base.AddTypeDependency(referencedType);
            }
            else
            {
                referencedType = base.GetReferencedType(andRemoveVirtualPathAttribute);
            }
            Util.CheckUnknownDirectiveAttributes(directiveName, directive);
            return referencedType;
        }

        internal override void HandlePostParse()
        {
            base.HandlePostParse();
            if (!this.FInDesigner)
            {
                if (((base.ScriptList.Count == 0) && (base.BaseType == this.DefaultBaseType)) && (base.CodeFileVirtualPath == null))
                {
                    this.flags[0x20000] = true;
                }
                base._applicationObjects = HttpApplicationFactory.ApplicationState.StaticObjects;
                this.AddStaticObjectAssemblyDependencies(base._applicationObjects);
                base._sessionObjects = HttpApplicationFactory.ApplicationState.SessionStaticObjects;
                this.AddStaticObjectAssemblyDependencies(base._sessionObjects);
            }
        }

        internal override void ProcessConfigSettings()
        {
            base.ProcessConfigSettings();
            if (base.PagesConfig != null)
            {
                this.flags[0x20000] = !base.PagesConfig.AutoEventWireup;
                if (!base.PagesConfig.EnableViewState)
                {
                    base._mainDirectiveConfigSettings["enableviewstate"] = Util.GetStringFromBool(base.PagesConfig.EnableViewState);
                }
                base.CompilationMode = base.PagesConfig.CompilationMode;
            }
            if (base._pageParserFilter != null)
            {
                base.CompilationMode = base._pageParserFilter.GetCompilationMode(base.CompilationMode);
            }
        }

        internal override void ProcessDirective(string directiveName, IDictionary directive)
        {
            if (StringUtil.EqualsIgnoreCase(directiveName, "outputcache"))
            {
                if (!this.FInDesigner)
                {
                    if (this._outputCacheSettings == null)
                    {
                        this._outputCacheSettings = new System.Web.UI.OutputCacheParameters();
                    }
                    if (this._outputCacheDirective != null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Only_one_directive_allowed", new object[] { directiveName }));
                    }
                    this.ProcessOutputCacheDirective(directiveName, directive);
                    this._outputCacheDirective = directive;
                }
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "reference"))
            {
                if (!this.FInDesigner)
                {
                    VirtualPath andRemoveVirtualPathAttribute = Util.GetAndRemoveVirtualPathAttribute(directive, "virtualpath");
                    bool flag = false;
                    bool flag2 = false;
                    VirtualPath path2 = Util.GetAndRemoveVirtualPathAttribute(directive, "page");
                    if (path2 != null)
                    {
                        if (andRemoveVirtualPathAttribute != null)
                        {
                            base.ProcessError(System.Web.SR.GetString("Invalid_reference_directive"));
                            return;
                        }
                        andRemoveVirtualPathAttribute = path2;
                        flag = true;
                    }
                    path2 = Util.GetAndRemoveVirtualPathAttribute(directive, "control");
                    if (path2 != null)
                    {
                        if (andRemoveVirtualPathAttribute != null)
                        {
                            base.ProcessError(System.Web.SR.GetString("Invalid_reference_directive"));
                            return;
                        }
                        andRemoveVirtualPathAttribute = path2;
                        flag2 = true;
                    }
                    if (andRemoveVirtualPathAttribute == null)
                    {
                        base.ProcessError(System.Web.SR.GetString("Invalid_reference_directive"));
                    }
                    else
                    {
                        Type referencedType = base.GetReferencedType(andRemoveVirtualPathAttribute);
                        if (referencedType == null)
                        {
                            base.ProcessError(System.Web.SR.GetString("Invalid_reference_directive_attrib", new object[] { andRemoveVirtualPathAttribute }));
                        }
                        if (flag && !typeof(Page).IsAssignableFrom(referencedType))
                        {
                            base.ProcessError(System.Web.SR.GetString("Invalid_reference_directive_attrib", new object[] { andRemoveVirtualPathAttribute }));
                        }
                        if (flag2 && !typeof(UserControl).IsAssignableFrom(referencedType))
                        {
                            base.ProcessError(System.Web.SR.GetString("Invalid_reference_directive_attrib", new object[] { andRemoveVirtualPathAttribute }));
                        }
                        Util.CheckUnknownDirectiveAttributes(directiveName, directive);
                    }
                }
            }
            else
            {
                base.ProcessDirective(directiveName, directive);
            }
        }

        internal override void ProcessMainDirective(IDictionary mainDirective)
        {
            object obj2 = null;
            try
            {
                obj2 = Util.GetAndRemoveEnumAttribute(mainDirective, typeof(CompilationMode), "compilationmode");
            }
            catch (Exception exception)
            {
                base.ProcessError(exception.Message);
            }
            if (obj2 != null)
            {
                base.CompilationMode = (CompilationMode) obj2;
                if (base._pageParserFilter != null)
                {
                    base.CompilationMode = base._pageParserFilter.GetCompilationMode(base.CompilationMode);
                }
            }
            base.ProcessMainDirective(mainDirective);
        }

        internal override bool ProcessMainDirectiveAttribute(string deviceName, string name, string value, IDictionary parseData)
        {
            string str = name;
            if (str != null)
            {
                if (str != "targetschema")
                {
                    if (!(str == "autoeventwireup"))
                    {
                        if (str == "enabletheming")
                        {
                            return false;
                        }
                        if (str == "codefilebaseclass")
                        {
                            parseData[name] = Util.GetNonEmptyAttribute(name, value);
                            goto Label_007D;
                        }
                        goto Label_0071;
                    }
                    base.OnFoundAttributeRequiringCompilation(name);
                    this.flags[0x20000] = !Util.GetBooleanAttribute(name, value);
                }
                goto Label_007D;
            }
        Label_0071:
            return base.ProcessMainDirectiveAttribute(deviceName, name, value, parseData);
        Label_007D:
            base.ValidateBuiltInAttribute(deviceName, name, value);
            return true;
        }

        internal virtual void ProcessOutputCacheDirective(string directiveName, IDictionary directive)
        {
            int val = 0;
            string str3 = null;
            bool flag = Util.GetAndRemovePositiveIntegerAttribute(directive, "duration", ref val);
            if (flag)
            {
                this.OutputCacheParameters.Duration = val;
            }
            if (this is PageParser)
            {
                str3 = Util.GetAndRemoveNonEmptyAttribute(directive, "cacheProfile");
                if (str3 != null)
                {
                    this.OutputCacheParameters.CacheProfile = str3;
                }
            }
            if ((!flag && ((str3 == null) || (str3.Length == 0))) && this.FDurationRequiredOnOutputCache)
            {
                throw new HttpException(System.Web.SR.GetString("Missing_attr", new object[] { "duration" }));
            }
            string andRemoveNonEmptyAttribute = Util.GetAndRemoveNonEmptyAttribute(directive, "varybycustom");
            if (andRemoveNonEmptyAttribute != null)
            {
                this.OutputCacheParameters.VaryByCustom = andRemoveNonEmptyAttribute;
            }
            string str4 = Util.GetAndRemoveNonEmptyAttribute(directive, "varybycontrol");
            if (str4 != null)
            {
                this.OutputCacheParameters.VaryByControl = str4;
            }
            string str = Util.GetAndRemoveNonEmptyAttribute(directive, "varybyparam");
            if (str != null)
            {
                this.OutputCacheParameters.VaryByParam = str;
            }
            if ((((str == null) && (str4 == null)) && ((str3 == null) || (str3.Length == 0))) && this.FVaryByParamsRequiredOnOutputCache)
            {
                throw new HttpException(System.Web.SR.GetString("Missing_varybyparam_attr"));
            }
            if (StringUtil.EqualsIgnoreCase(str, "none"))
            {
                this.OutputCacheParameters.VaryByParam = null;
            }
            if (StringUtil.EqualsIgnoreCase(str4, "none"))
            {
                this.OutputCacheParameters.VaryByControl = null;
            }
            Util.CheckUnknownDirectiveAttributes(directiveName, directive, this.UnknownOutputCacheAttributeError);
        }

        internal override void ProcessUnknownMainDirectiveAttribute(string filter, string attribName, string value)
        {
            if (attribName == "id")
            {
                base.ProcessUnknownMainDirectiveAttribute(filter, attribName, value);
            }
            else
            {
                try
                {
                    base.RootBuilder.PreprocessAttribute(filter, attribName, value, true);
                }
                catch (Exception exception)
                {
                    base.ProcessError(System.Web.SR.GetString("Attrib_parse_error", new object[] { attribName, exception.Message }));
                }
            }
        }

        internal bool FAutoEventWireup
        {
            get
            {
                return !this.flags[0x20000];
            }
        }

        internal virtual bool FDurationRequiredOnOutputCache
        {
            get
            {
                return true;
            }
        }

        internal virtual bool FVaryByParamsRequiredOnOutputCache
        {
            get
            {
                return true;
            }
        }

        internal System.Web.UI.OutputCacheParameters OutputCacheParameters
        {
            get
            {
                return this._outputCacheSettings;
            }
        }

        internal override bool RequiresCompilation
        {
            get
            {
                if (!this.flags[0x10])
                {
                    return (base.CompilationMode == CompilationMode.Always);
                }
                return true;
            }
        }

        internal abstract string UnknownOutputCacheAttributeError { get; }
    }
}

