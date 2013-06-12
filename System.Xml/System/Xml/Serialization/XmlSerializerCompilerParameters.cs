namespace System.Xml.Serialization
{
    using System;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.Xml.Serialization.Configuration;

    internal sealed class XmlSerializerCompilerParameters
    {
        private bool needTempDirAccess;
        private CompilerParameters parameters;

        private XmlSerializerCompilerParameters(CompilerParameters parameters, bool needTempDirAccess)
        {
            this.needTempDirAccess = needTempDirAccess;
            this.parameters = parameters;
        }

        internal static XmlSerializerCompilerParameters Create(string location)
        {
            CompilerParameters parameters = new CompilerParameters {
                GenerateInMemory = true
            };
            if (string.IsNullOrEmpty(location))
            {
                XmlSerializerSection section = ConfigurationManager.GetSection(ConfigurationStrings.XmlSerializerSectionPath) as XmlSerializerSection;
                location = (section == null) ? location : section.TempFilesLocation;
                if (!string.IsNullOrEmpty(location))
                {
                    location = location.Trim();
                }
            }
            parameters.TempFiles = new TempFileCollection(location);
            return new XmlSerializerCompilerParameters(parameters, string.IsNullOrEmpty(location));
        }

        internal static XmlSerializerCompilerParameters Create(CompilerParameters parameters, bool needTempDirAccess)
        {
            return new XmlSerializerCompilerParameters(parameters, needTempDirAccess);
        }

        internal CompilerParameters CodeDomParameters
        {
            get
            {
                return this.parameters;
            }
        }

        internal bool IsNeedTempDirAccess
        {
            get
            {
                return this.needTempDirAccess;
            }
        }
    }
}

