namespace System.CodeDom.Compiler
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Security.Policy;

    [Serializable, PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class CompilerResults
    {
        private Assembly compiledAssembly;
        private CompilerErrorCollection errors = new CompilerErrorCollection();
        private System.Security.Policy.Evidence evidence;
        private int nativeCompilerReturnValue;
        private StringCollection output = new StringCollection();
        private string pathToAssembly;
        private TempFileCollection tempFiles;

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public CompilerResults(TempFileCollection tempFiles)
        {
            this.tempFiles = tempFiles;
        }

        public Assembly CompiledAssembly
        {
            [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlEvidence)]
            get
            {
                if ((this.compiledAssembly == null) && (this.pathToAssembly != null))
                {
                    AssemblyName assemblyRef = new AssemblyName {
                        CodeBase = this.pathToAssembly
                    };
                    this.compiledAssembly = Assembly.Load(assemblyRef, this.evidence);
                }
                return this.compiledAssembly;
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                this.compiledAssembly = value;
            }
        }

        public CompilerErrorCollection Errors
        {
            get
            {
                return this.errors;
            }
        }

        [Obsolete("CAS policy is obsolete and will be removed in a future release of the .NET Framework. Please see http://go2.microsoft.com/fwlink/?LinkId=131738 for more information.")]
        public System.Security.Policy.Evidence Evidence
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                System.Security.Policy.Evidence evidence = null;
                if (this.evidence != null)
                {
                    evidence = this.evidence.Clone();
                }
                return evidence;
            }
            [SecurityPermission(SecurityAction.Demand, ControlEvidence=true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                if (value != null)
                {
                    this.evidence = value.Clone();
                }
                else
                {
                    this.evidence = null;
                }
            }
        }

        public int NativeCompilerReturnValue
        {
            get
            {
                return this.nativeCompilerReturnValue;
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                this.nativeCompilerReturnValue = value;
            }
        }

        public StringCollection Output
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                return this.output;
            }
        }

        public string PathToAssembly
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                return this.pathToAssembly;
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                this.pathToAssembly = value;
            }
        }

        public TempFileCollection TempFiles
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                return this.tempFiles;
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                this.tempFiles = value;
            }
        }
    }
}

