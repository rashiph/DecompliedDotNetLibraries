namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class CodeSubDirectory : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propDirectoryName = new ConfigurationProperty("directoryName", typeof(string), null, StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private const string dirNameAttribName = "directoryName";

        static CodeSubDirectory()
        {
            _properties.Add(_propDirectoryName);
        }

        internal CodeSubDirectory()
        {
        }

        public CodeSubDirectory(string directoryName)
        {
            this.DirectoryName = directoryName;
        }

        internal void DoRuntimeValidation()
        {
            string directoryName = this.DirectoryName;
            if (!BuildManager.IsPrecompiledApp)
            {
                FindFileData data;
                if (!Util.IsValidFileName(directoryName))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_CodeSubDirectory", new object[] { directoryName }), base.ElementInformation.Properties["directoryName"].Source, base.ElementInformation.Properties["directoryName"].LineNumber);
                }
                VirtualPath virtualDir = HttpRuntime.CodeDirectoryVirtualPath.SimpleCombineWithDir(directoryName);
                if (!VirtualPathProvider.DirectoryExistsNoThrow(virtualDir))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_CodeSubDirectory_Not_Exist", new object[] { virtualDir }), base.ElementInformation.Properties["directoryName"].Source, base.ElementInformation.Properties["directoryName"].LineNumber);
                }
                FindFileData.FindFile(virtualDir.MapPathInternal(), out data);
                if (!System.Web.Util.StringUtil.EqualsIgnoreCase(directoryName, data.FileNameLong))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_CodeSubDirectory", new object[] { directoryName }), base.ElementInformation.Properties["directoryName"].Source, base.ElementInformation.Properties["directoryName"].LineNumber);
                }
                if (BuildManager.IsReservedAssemblyName(directoryName))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Reserved_AssemblyName", new object[] { directoryName }), base.ElementInformation.Properties["directoryName"].Source, base.ElementInformation.Properties["directoryName"].LineNumber);
                }
            }
        }

        internal string AssemblyName
        {
            get
            {
                return this.DirectoryName;
            }
        }

        [ConfigurationProperty("directoryName", IsRequired=true, IsKey=true, DefaultValue=""), TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        public string DirectoryName
        {
            get
            {
                return (string) base[_propDirectoryName];
            }
            set
            {
                base[_propDirectoryName] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}

