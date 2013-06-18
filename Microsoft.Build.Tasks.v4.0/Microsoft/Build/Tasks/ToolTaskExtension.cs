namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;

    public abstract class ToolTaskExtension : ToolTask
    {
        private Hashtable bag;
        private TaskLoggingHelperExtension logExtension;

        internal ToolTaskExtension() : base(Microsoft.Build.Shared.AssemblyResources.PrimaryResources, "MSBuild.")
        {
            this.bag = new Hashtable();
            this.logExtension = new TaskLoggingHelperExtension(this, Microsoft.Build.Shared.AssemblyResources.PrimaryResources, Microsoft.Build.Shared.AssemblyResources.SharedResources, "MSBuild.");
        }

        protected internal virtual void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
        {
        }

        protected internal virtual void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
        {
        }

        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilderExtension commandLine = new CommandLineBuilderExtension();
            this.AddCommandLineCommands(commandLine);
            return commandLine.ToString();
        }

        protected override string GenerateResponseFileCommands()
        {
            CommandLineBuilderExtension commandLine = new CommandLineBuilderExtension();
            this.AddResponseFileCommands(commandLine);
            return commandLine.ToString();
        }

        protected internal bool GetBoolParameterWithDefault(string parameterName, bool defaultValue)
        {
            object obj2 = this.bag[parameterName];
            if (obj2 != null)
            {
                return (bool) obj2;
            }
            return defaultValue;
        }

        protected internal int GetIntParameterWithDefault(string parameterName, int defaultValue)
        {
            object obj2 = this.bag[parameterName];
            if (obj2 != null)
            {
                return (int) obj2;
            }
            return defaultValue;
        }

        protected internal Hashtable Bag
        {
            get
            {
                return this.bag;
            }
        }

        protected override bool HasLoggedErrors
        {
            get
            {
                if (!this.Log.HasLoggedErrors)
                {
                    return base.HasLoggedErrors;
                }
                return true;
            }
        }

        public TaskLoggingHelper Log
        {
            get
            {
                return this.logExtension;
            }
        }
    }
}

