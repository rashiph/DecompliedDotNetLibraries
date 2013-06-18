namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Utilities;
    using System;

    public class AspNetCompiler : ToolTaskExtension
    {
        private bool _aptca;
        private bool _clean;
        private bool _debug;
        private bool _delaySign;
        private bool _fixedNames;
        private bool _force;
        private bool _updateable;

        protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
        {
            commandLine.AppendSwitchIfNotNull("-m ", this.MetabasePath);
            commandLine.AppendSwitchIfNotNull("-v ", this.VirtualPath);
            commandLine.AppendSwitchIfNotNull("-p ", this.PhysicalPath);
            if (this.Updateable)
            {
                commandLine.AppendSwitch("-u");
            }
            if (this.Force)
            {
                commandLine.AppendSwitch("-f");
            }
            if (this.Clean)
            {
                commandLine.AppendSwitch("-c");
            }
            if (this.Debug)
            {
                commandLine.AppendSwitch("-d");
            }
            if (this.FixedNames)
            {
                commandLine.AppendSwitch("-fixednames");
            }
            commandLine.AppendSwitchIfNotNull("", this.TargetPath);
            if (this.AllowPartiallyTrustedCallers)
            {
                commandLine.AppendSwitch("-aptca");
            }
            if (this.DelaySign)
            {
                commandLine.AppendSwitch("-delaysign");
            }
            commandLine.AppendSwitchIfNotNull("-keyfile ", this.KeyFile);
            commandLine.AppendSwitchIfNotNull("-keycontainer ", this.KeyContainer);
        }

        public override bool Execute()
        {
            base.Log.LogExternalProjectStarted(string.Empty, null, this.ProjectName, this.TargetName);
            bool succeeded = false;
            try
            {
                succeeded = base.Execute();
            }
            finally
            {
                base.Log.LogExternalProjectFinished(string.Empty, null, this.ProjectName, succeeded);
            }
            return succeeded;
        }

        protected override string GenerateFullPathToTool()
        {
            string pathToDotNetFrameworkFile = null;
            pathToDotNetFrameworkFile = ToolLocationHelper.GetPathToDotNetFrameworkFile(this.ToolName, TargetDotNetFrameworkVersion.Version40);
            if (pathToDotNetFrameworkFile == null)
            {
                base.Log.LogErrorWithCodeFromResources("General.FrameworksFileNotFound", new object[] { this.ToolName, ToolLocationHelper.GetDotNetFrameworkVersionFolderPrefix(TargetDotNetFrameworkVersion.Version40) });
            }
            return pathToDotNetFrameworkFile;
        }

        protected override bool ValidateParameters()
        {
            if ((this.MetabasePath != null) && ((this.VirtualPath != null) || (this.PhysicalPath != null)))
            {
                base.Log.LogErrorWithCodeFromResources("AspNetCompiler.CannotCombineMetabaseAndVirtualPathOrPhysicalPath", new object[0]);
                return false;
            }
            if ((this.MetabasePath == null) && (this.VirtualPath == null))
            {
                base.Log.LogErrorWithCodeFromResources("AspNetCompiler.MissingMetabasePathAndVirtualPath", new object[0]);
                return false;
            }
            if (this.Updateable && (this.TargetPath == null))
            {
                base.Log.LogErrorWithCodeFromResources("AspNetCompiler.MissingTargetPathForUpdatableApplication", new object[0]);
                return false;
            }
            if (this.Force && (this.TargetPath == null))
            {
                base.Log.LogErrorWithCodeFromResources("AspNetCompiler.MissingTargetPathForOverwrittenApplication", new object[0]);
                return false;
            }
            return true;
        }

        public bool AllowPartiallyTrustedCallers
        {
            get
            {
                return this._aptca;
            }
            set
            {
                this._aptca = value;
            }
        }

        public bool Clean
        {
            get
            {
                return this._clean;
            }
            set
            {
                this._clean = value;
            }
        }

        public bool Debug
        {
            get
            {
                return this._debug;
            }
            set
            {
                this._debug = value;
            }
        }

        public bool DelaySign
        {
            get
            {
                return this._delaySign;
            }
            set
            {
                this._delaySign = value;
            }
        }

        public bool FixedNames
        {
            get
            {
                return this._fixedNames;
            }
            set
            {
                this._fixedNames = value;
            }
        }

        public bool Force
        {
            get
            {
                return this._force;
            }
            set
            {
                this._force = value;
            }
        }

        public string KeyContainer
        {
            get
            {
                return (string) base.Bag["KeyContainer"];
            }
            set
            {
                base.Bag["KeyContainer"] = value;
            }
        }

        public string KeyFile
        {
            get
            {
                return (string) base.Bag["KeyFile"];
            }
            set
            {
                base.Bag["KeyFile"] = value;
            }
        }

        public string MetabasePath
        {
            get
            {
                return (string) base.Bag["MetabasePath"];
            }
            set
            {
                base.Bag["MetabasePath"] = value;
            }
        }

        public string PhysicalPath
        {
            get
            {
                return (string) base.Bag["PhysicalPath"];
            }
            set
            {
                base.Bag["PhysicalPath"] = value;
            }
        }

        private string ProjectName
        {
            get
            {
                if (this.PhysicalPath != null)
                {
                    return this.PhysicalPath;
                }
                if (this.VirtualPath != null)
                {
                    return this.VirtualPath;
                }
                return this.MetabasePath;
            }
        }

        public string TargetFrameworkMoniker
        {
            get
            {
                return (string) base.Bag["TargetFrameworkMoniker"];
            }
            set
            {
                base.Bag["TargetFrameworkMoniker"] = value;
            }
        }

        private string TargetName
        {
            get
            {
                if (this.Clean)
                {
                    return "Clean";
                }
                return null;
            }
        }

        public string TargetPath
        {
            get
            {
                return (string) base.Bag["TargetPath"];
            }
            set
            {
                base.Bag["TargetPath"] = value;
            }
        }

        protected override string ToolName
        {
            get
            {
                return "aspnet_compiler.exe";
            }
        }

        public bool Updateable
        {
            get
            {
                return this._updateable;
            }
            set
            {
                this._updateable = value;
            }
        }

        public string VirtualPath
        {
            get
            {
                return (string) base.Bag["VirtualPath"];
            }
            set
            {
                base.Bag["VirtualPath"] = value;
            }
        }
    }
}

