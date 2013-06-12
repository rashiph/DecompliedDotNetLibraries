namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web;
    using System.Web.Configuration;

    internal class MainTagNameToTypeMapper
    {
        private ITagNameToTypeMapper _htmlMapper = new HtmlTagNameToTypeMapper();
        private IDictionary _mappedTags;
        private BaseTemplateParser _parser;
        private IDictionary _prefixedMappers;
        private TagNamespaceRegisterEntryTable _tagNamespaceRegisterEntries;
        private List<TagNamespaceRegisterEntry> _tagRegisterEntries;
        private Hashtable _userControlRegisterEntries;

        internal MainTagNameToTypeMapper(BaseTemplateParser parser)
        {
            this._parser = parser;
            if (parser != null)
            {
                PagesSection pagesConfig = parser.PagesConfig;
                if (pagesConfig != null)
                {
                    this._tagNamespaceRegisterEntries = pagesConfig.TagNamespaceRegisterEntriesInternal;
                    if (this._tagNamespaceRegisterEntries != null)
                    {
                        this._tagNamespaceRegisterEntries = (TagNamespaceRegisterEntryTable) this._tagNamespaceRegisterEntries.Clone();
                    }
                    this._userControlRegisterEntries = pagesConfig.UserControlRegisterEntriesInternal;
                    if (this._userControlRegisterEntries != null)
                    {
                        this._userControlRegisterEntries = (Hashtable) this._userControlRegisterEntries.Clone();
                    }
                }
                if (parser.FInDesigner && (this._tagNamespaceRegisterEntries == null))
                {
                    this._tagNamespaceRegisterEntries = new TagNamespaceRegisterEntryTable();
                    foreach (TagNamespaceRegisterEntry entry in PagesSection.DefaultTagNamespaceRegisterEntries)
                    {
                        this._tagNamespaceRegisterEntries[entry.TagPrefix] = new ArrayList(new object[] { entry });
                    }
                }
            }
        }

        internal Type GetControlType(string tagName, IDictionary attribs, bool fAllowHtmlTags)
        {
            Type type = this.GetControlType2(tagName, attribs, fAllowHtmlTags);
            if (((type != null) && (this._parser != null)) && !this._parser.FInDesigner)
            {
                Hashtable tagTypeMappingInternal = this._parser.PagesConfig.TagMapping.TagTypeMappingInternal;
                if (tagTypeMappingInternal != null)
                {
                    Type type2 = (Type) tagTypeMappingInternal[type];
                    if (type2 != null)
                    {
                        type = type2;
                    }
                }
            }
            return type;
        }

        private Type GetControlType2(string tagName, IDictionary attribs, bool fAllowHtmlTags)
        {
            if (this._mappedTags != null)
            {
                Type type = (Type) this._mappedTags[tagName];
                if ((type == null) && this.TryUserControlRegisterDirectives(tagName))
                {
                    type = (Type) this._mappedTags[tagName];
                }
                if (type != null)
                {
                    if (((this._parser != null) && (this._parser._pageParserFilter != null)) && (this._parser._pageParserFilter.GetNoCompileUserControlType() == type))
                    {
                        UserControlRegisterEntry entry = (UserControlRegisterEntry) this._userControlRegisterEntries[tagName];
                        attribs["virtualpath"] = entry.UserControlSource;
                    }
                    return type;
                }
            }
            int index = tagName.IndexOf(':');
            if (index >= 0)
            {
                if (index == (tagName.Length - 1))
                {
                    return null;
                }
                string prefix = tagName.Substring(0, index);
                tagName = tagName.Substring(index + 1);
                ITagNameToTypeMapper mapper = null;
                if (this._prefixedMappers != null)
                {
                    mapper = (ITagNameToTypeMapper) this._prefixedMappers[prefix];
                }
                if (((mapper == null) && this.TryNamespaceRegisterDirectives(prefix)) && (this._prefixedMappers != null))
                {
                    mapper = (ITagNameToTypeMapper) this._prefixedMappers[prefix];
                }
                if (mapper == null)
                {
                    return null;
                }
                return mapper.GetControlType(tagName, attribs);
            }
            if (fAllowHtmlTags)
            {
                return this._htmlMapper.GetControlType(tagName, attribs);
            }
            return null;
        }

        private void ProcessTagNamespaceRegistration(ArrayList nsRegisterEntries)
        {
            foreach (TagNamespaceRegisterEntry entry in nsRegisterEntries)
            {
                try
                {
                    this.ProcessTagNamespaceRegistrationCore(entry);
                }
                catch (Exception exception)
                {
                    throw new HttpParseException(exception.Message, exception, entry.VirtualPath, null, entry.Line);
                }
            }
        }

        internal void ProcessTagNamespaceRegistration(TagNamespaceRegisterEntry nsRegisterEntry)
        {
            string tagPrefix = nsRegisterEntry.TagPrefix;
            ArrayList nsRegisterEntries = null;
            if (this._tagNamespaceRegisterEntries != null)
            {
                nsRegisterEntries = (ArrayList) this._tagNamespaceRegisterEntries[tagPrefix];
            }
            if ((nsRegisterEntries != null) && ((this._prefixedMappers == null) || (this._prefixedMappers[tagPrefix] == null)))
            {
                this.ProcessTagNamespaceRegistration(nsRegisterEntries);
            }
            this.ProcessTagNamespaceRegistrationCore(nsRegisterEntry);
        }

        private void ProcessTagNamespaceRegistrationCore(TagNamespaceRegisterEntry nsRegisterEntry)
        {
            Assembly assembly = null;
            if (!string.IsNullOrEmpty(nsRegisterEntry.AssemblyName))
            {
                assembly = this._parser.AddAssemblyDependency(nsRegisterEntry.AssemblyName);
            }
            if (!string.IsNullOrEmpty(nsRegisterEntry.Namespace))
            {
                this._parser.AddImportEntry(nsRegisterEntry.Namespace);
            }
            NamespaceTagNameToTypeMapper mapper = new NamespaceTagNameToTypeMapper(nsRegisterEntry, assembly, this._parser);
            if (this._prefixedMappers == null)
            {
                this._prefixedMappers = new Hashtable(StringComparer.OrdinalIgnoreCase);
            }
            TagPrefixTagNameToTypeMapper mapper2 = (TagPrefixTagNameToTypeMapper) this._prefixedMappers[nsRegisterEntry.TagPrefix];
            if (mapper2 == null)
            {
                mapper2 = new TagPrefixTagNameToTypeMapper(nsRegisterEntry.TagPrefix);
                this._prefixedMappers[nsRegisterEntry.TagPrefix] = mapper2;
            }
            mapper2.AddNamespaceMapper(mapper);
            this.TagRegisterEntries.Add(nsRegisterEntry);
        }

        internal void ProcessUserControlRegistration(UserControlRegisterEntry ucRegisterEntry)
        {
            Type designTimeUserControlType = null;
            if (this._parser.FInDesigner)
            {
                designTimeUserControlType = this._parser.GetDesignTimeUserControlType(ucRegisterEntry.TagPrefix, ucRegisterEntry.TagName);
            }
            else
            {
                designTimeUserControlType = this._parser.GetUserControlType(ucRegisterEntry.UserControlSource.VirtualPathString);
            }
            if (designTimeUserControlType != null)
            {
                if (this._userControlRegisterEntries == null)
                {
                    this._userControlRegisterEntries = new Hashtable();
                }
                this._userControlRegisterEntries[ucRegisterEntry.TagPrefix + ":" + ucRegisterEntry.TagName] = ucRegisterEntry;
                this.RegisterTag(ucRegisterEntry.TagPrefix + ":" + ucRegisterEntry.TagName, designTimeUserControlType);
            }
        }

        internal void RegisterTag(string tagName, Type type)
        {
            if (this._mappedTags == null)
            {
                this._mappedTags = new Hashtable(StringComparer.OrdinalIgnoreCase);
            }
            try
            {
                this._mappedTags.Add(tagName, type);
            }
            catch (ArgumentException)
            {
                throw new HttpException(System.Web.SR.GetString("Duplicate_registered_tag", new object[] { tagName }));
            }
        }

        private bool TryNamespaceRegisterDirectives(string prefix)
        {
            if (this._tagNamespaceRegisterEntries == null)
            {
                return false;
            }
            ArrayList nsRegisterEntries = (ArrayList) this._tagNamespaceRegisterEntries[prefix];
            if (nsRegisterEntries == null)
            {
                return false;
            }
            this.ProcessTagNamespaceRegistration(nsRegisterEntries);
            return true;
        }

        private bool TryUserControlRegisterDirectives(string tagName)
        {
            if (this._userControlRegisterEntries == null)
            {
                return false;
            }
            UserControlRegisterEntry ucRegisterEntry = (UserControlRegisterEntry) this._userControlRegisterEntries[tagName];
            if (ucRegisterEntry == null)
            {
                return false;
            }
            if (ucRegisterEntry.ComesFromConfig && (ucRegisterEntry.UserControlSource.Parent == this._parser.BaseVirtualDir))
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_use_of_config_uc", new object[] { this._parser.CurrentVirtualPath, ucRegisterEntry.UserControlSource }));
            }
            try
            {
                this.ProcessUserControlRegistration(ucRegisterEntry);
            }
            catch (Exception exception)
            {
                throw new HttpParseException(exception.Message, exception, ucRegisterEntry.VirtualPath, null, ucRegisterEntry.Line);
            }
            return true;
        }

        internal List<TagNamespaceRegisterEntry> TagRegisterEntries
        {
            get
            {
                if (this._tagRegisterEntries == null)
                {
                    this._tagRegisterEntries = new List<TagNamespaceRegisterEntry>();
                }
                return this._tagRegisterEntries;
            }
        }

        internal ICollection UserControlRegisterEntries
        {
            get
            {
                if (this._userControlRegisterEntries != null)
                {
                    return this._userControlRegisterEntries.Values;
                }
                return null;
            }
        }
    }
}

