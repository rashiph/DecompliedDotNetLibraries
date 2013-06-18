namespace System.Xaml.MS.Impl
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    [DebuggerDisplay("{ClrNamespace} {Assembly.FullName}")]
    internal class AssemblyNamespacePair
    {
        private WeakReference _assembly;
        private string _clrNamespace;

        public AssemblyNamespacePair(System.Reflection.Assembly asm, string clrNamespace)
        {
            this._assembly = new WeakReference(asm);
            this._clrNamespace = clrNamespace;
        }

        public System.Reflection.Assembly Assembly
        {
            get
            {
                return (System.Reflection.Assembly) this._assembly.Target;
            }
        }

        public string ClrNamespace
        {
            get
            {
                return this._clrNamespace;
            }
        }
    }
}

