namespace System.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration.Internal;
    using System.Diagnostics;
    using System.Runtime;

    [DebuggerDisplay("FactoryRecord {ConfigKey}")]
    internal class FactoryRecord : IConfigErrorInfo
    {
        private ConfigurationAllowDefinition _allowDefinition;
        private ConfigurationAllowExeDefinition _allowExeDefinition;
        private string _configKey;
        private List<ConfigurationException> _errors;
        private object _factory;
        private string _factoryTypeName;
        private string _filename;
        private SimpleBitVector32 _flags;
        private string _group;
        private int _lineNumber;
        private string _name;
        private OverrideModeSetting _overrideModeDefault;
        private const int Flag_AllowLocation = 1;
        private const int Flag_IsFactoryTrustedWithoutAptca = 0x20;
        private const int Flag_IsFromTrustedConfigRecord = 0x10;
        private const int Flag_IsGroup = 8;
        private const int Flag_IsUndeclared = 0x40;
        private const int Flag_RequirePermission = 4;
        private const int Flag_RestartOnExternalChanges = 2;

        internal FactoryRecord(string configKey, string group, string name, string factoryTypeName, string filename, int lineNumber)
        {
            this._configKey = configKey;
            this._group = group;
            this._name = name;
            this._factoryTypeName = factoryTypeName;
            this.IsGroup = true;
            this._filename = filename;
            this._lineNumber = lineNumber;
        }

        private FactoryRecord(string configKey, string group, string name, object factory, string factoryTypeName, SimpleBitVector32 flags, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, OverrideModeSetting overrideModeDefault, string filename, int lineNumber, ICollection<ConfigurationException> errors)
        {
            this._configKey = configKey;
            this._group = group;
            this._name = name;
            this._factory = factory;
            this._factoryTypeName = factoryTypeName;
            this._flags = flags;
            this._allowDefinition = allowDefinition;
            this._allowExeDefinition = allowExeDefinition;
            this._overrideModeDefault = overrideModeDefault;
            this._filename = filename;
            this._lineNumber = lineNumber;
            this.AddErrors(errors);
        }

        internal FactoryRecord(string configKey, string group, string name, string factoryTypeName, bool allowLocation, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, OverrideModeSetting overrideModeDefault, bool restartOnExternalChanges, bool requirePermission, bool isFromTrustedConfigRecord, bool isUndeclared, string filename, int lineNumber)
        {
            this._configKey = configKey;
            this._group = group;
            this._name = name;
            this._factoryTypeName = factoryTypeName;
            this._allowDefinition = allowDefinition;
            this._allowExeDefinition = allowExeDefinition;
            this._overrideModeDefault = overrideModeDefault;
            this.AllowLocation = allowLocation;
            this.RestartOnExternalChanges = restartOnExternalChanges;
            this.RequirePermission = requirePermission;
            this.IsFromTrustedConfigRecord = isFromTrustedConfigRecord;
            this.IsUndeclared = isUndeclared;
            this._filename = filename;
            this._lineNumber = lineNumber;
        }

        internal void AddErrors(ICollection<ConfigurationException> coll)
        {
            ErrorsHelper.AddErrors(ref this._errors, coll);
        }

        internal FactoryRecord CloneSection(string filename, int lineNumber)
        {
            return new FactoryRecord(this._configKey, this._group, this._name, this._factory, this._factoryTypeName, this._flags, this._allowDefinition, this._allowExeDefinition, this._overrideModeDefault, filename, lineNumber, this.Errors);
        }

        internal FactoryRecord CloneSectionGroup(string factoryTypeName, string filename, int lineNumber)
        {
            if (this._factoryTypeName != null)
            {
                factoryTypeName = this._factoryTypeName;
            }
            return new FactoryRecord(this._configKey, this._group, this._name, this._factory, factoryTypeName, this._flags, this._allowDefinition, this._allowExeDefinition, this._overrideModeDefault, filename, lineNumber, this.Errors);
        }

        internal bool IsEquivalentSectionFactory(IInternalConfigHost host, string typeName, bool allowLocation, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, bool restartOnExternalChanges, bool requirePermission)
        {
            return ((((allowLocation == this.AllowLocation) && (allowDefinition == this.AllowDefinition)) && (((allowExeDefinition == this.AllowExeDefinition) && (restartOnExternalChanges == this.RestartOnExternalChanges)) && (requirePermission == this.RequirePermission))) && this.IsEquivalentType(host, typeName));
        }

        internal bool IsEquivalentSectionGroupFactory(IInternalConfigHost host, string typeName)
        {
            if ((typeName != null) && (this._factoryTypeName != null))
            {
                return this.IsEquivalentType(host, typeName);
            }
            return true;
        }

        internal bool IsEquivalentType(IInternalConfigHost host, string typeName)
        {
            try
            {
                Type typeWithReflectionPermission;
                Type type2;
                if (this._factoryTypeName == typeName)
                {
                    return true;
                }
                if (host != null)
                {
                    typeWithReflectionPermission = System.Configuration.TypeUtil.GetTypeWithReflectionPermission(host, typeName, false);
                    type2 = System.Configuration.TypeUtil.GetTypeWithReflectionPermission(host, this._factoryTypeName, false);
                }
                else
                {
                    typeWithReflectionPermission = System.Configuration.TypeUtil.GetTypeWithReflectionPermission(typeName, false);
                    type2 = System.Configuration.TypeUtil.GetTypeWithReflectionPermission(this._factoryTypeName, false);
                }
                return ((typeWithReflectionPermission != null) && (typeWithReflectionPermission == type2));
            }
            catch
            {
            }
            return false;
        }

        internal bool IsIgnorable()
        {
            if (this._factory != null)
            {
                return (this._factory is IgnoreSectionHandler);
            }
            return ((this._factoryTypeName != null) && this._factoryTypeName.Contains("System.Configuration.IgnoreSection"));
        }

        internal void ThrowOnErrors()
        {
            ErrorsHelper.ThrowOnErrors(this._errors);
        }

        internal ConfigurationAllowDefinition AllowDefinition
        {
            get
            {
                return this._allowDefinition;
            }
            set
            {
                this._allowDefinition = value;
            }
        }

        internal ConfigurationAllowExeDefinition AllowExeDefinition
        {
            get
            {
                return this._allowExeDefinition;
            }
            set
            {
                this._allowExeDefinition = value;
            }
        }

        internal bool AllowLocation
        {
            get
            {
                return this._flags[1];
            }
            set
            {
                this._flags[1] = value;
            }
        }

        internal string ConfigKey
        {
            get
            {
                return this._configKey;
            }
        }

        internal List<ConfigurationException> Errors
        {
            get
            {
                return this._errors;
            }
        }

        internal object Factory
        {
            get
            {
                return this._factory;
            }
            set
            {
                this._factory = value;
            }
        }

        internal string FactoryTypeName
        {
            get
            {
                return this._factoryTypeName;
            }
            set
            {
                this._factoryTypeName = value;
            }
        }

        public string Filename
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._filename;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._filename = value;
            }
        }

        internal string Group
        {
            get
            {
                return this._group;
            }
        }

        internal bool HasErrors
        {
            get
            {
                return ErrorsHelper.GetHasErrors(this._errors);
            }
        }

        internal bool HasFile
        {
            get
            {
                return (this._lineNumber >= 0);
            }
        }

        internal bool IsFactoryTrustedWithoutAptca
        {
            get
            {
                return this._flags[0x20];
            }
            set
            {
                this._flags[0x20] = value;
            }
        }

        internal bool IsFromTrustedConfigRecord
        {
            get
            {
                return this._flags[0x10];
            }
            set
            {
                this._flags[0x10] = value;
            }
        }

        internal bool IsGroup
        {
            get
            {
                return this._flags[8];
            }
            set
            {
                this._flags[8] = value;
            }
        }

        internal bool IsUndeclared
        {
            get
            {
                return this._flags[0x40];
            }
            set
            {
                this._flags[0x40] = value;
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

        internal string Name
        {
            get
            {
                return this._name;
            }
        }

        internal OverrideModeSetting OverrideModeDefault
        {
            get
            {
                return this._overrideModeDefault;
            }
        }

        internal bool RequirePermission
        {
            get
            {
                return this._flags[4];
            }
            set
            {
                this._flags[4] = value;
            }
        }

        internal bool RestartOnExternalChanges
        {
            get
            {
                return this._flags[2];
            }
            set
            {
                this._flags[2] = value;
            }
        }
    }
}

