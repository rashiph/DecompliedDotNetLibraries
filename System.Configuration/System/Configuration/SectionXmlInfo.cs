namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Runtime;

    internal sealed class SectionXmlInfo : IConfigErrorInfo
    {
        private string _configKey;
        private string _configSource;
        private string _configSourceStreamName;
        private object _configSourceStreamVersion;
        private string _definitionConfigPath;
        private string _filename;
        private int _lineNumber;
        private System.Configuration.OverrideModeSetting _overrideMode;
        private string _protectionProviderName;
        private string _rawXml;
        private bool _skipInChildApps;
        private object _streamVersion;
        private string _subPath;
        private string _targetConfigPath;

        internal SectionXmlInfo(string configKey, string definitionConfigPath, string targetConfigPath, string subPath, string filename, int lineNumber, object streamVersion, string rawXml, string configSource, string configSourceStreamName, object configSourceStreamVersion, string protectionProviderName, System.Configuration.OverrideModeSetting overrideMode, bool skipInChildApps)
        {
            this._configKey = configKey;
            this._definitionConfigPath = definitionConfigPath;
            this._targetConfigPath = targetConfigPath;
            this._subPath = subPath;
            this._filename = filename;
            this._lineNumber = lineNumber;
            this._streamVersion = streamVersion;
            this._rawXml = rawXml;
            this._configSource = configSource;
            this._configSourceStreamName = configSourceStreamName;
            this._configSourceStreamVersion = configSourceStreamVersion;
            this._protectionProviderName = protectionProviderName;
            this._overrideMode = overrideMode;
            this._skipInChildApps = skipInChildApps;
        }

        internal string ConfigKey
        {
            get
            {
                return this._configKey;
            }
        }

        internal string ConfigSource
        {
            get
            {
                return this._configSource;
            }
            set
            {
                this._configSource = value;
            }
        }

        internal string ConfigSourceStreamName
        {
            get
            {
                return this._configSourceStreamName;
            }
            set
            {
                this._configSourceStreamName = value;
            }
        }

        internal object ConfigSourceStreamVersion
        {
            set
            {
                this._configSourceStreamVersion = value;
            }
        }

        internal string DefinitionConfigPath
        {
            get
            {
                return this._definitionConfigPath;
            }
        }

        public string Filename
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._filename;
            }
        }

        public int LineNumber
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._lineNumber;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._lineNumber = value;
            }
        }

        internal System.Configuration.OverrideModeSetting OverrideModeSetting
        {
            get
            {
                return this._overrideMode;
            }
            set
            {
                this._overrideMode = value;
            }
        }

        internal string ProtectionProviderName
        {
            get
            {
                return this._protectionProviderName;
            }
            set
            {
                this._protectionProviderName = value;
            }
        }

        internal string RawXml
        {
            get
            {
                return this._rawXml;
            }
            set
            {
                this._rawXml = value;
            }
        }

        internal bool SkipInChildApps
        {
            get
            {
                return this._skipInChildApps;
            }
            set
            {
                this._skipInChildApps = value;
            }
        }

        internal object StreamVersion
        {
            get
            {
                return this._streamVersion;
            }
            set
            {
                this._streamVersion = value;
            }
        }

        internal string SubPath
        {
            get
            {
                return this._subPath;
            }
        }

        internal string TargetConfigPath
        {
            get
            {
                return this._targetConfigPath;
            }
            set
            {
                this._targetConfigPath = value;
            }
        }
    }
}

