namespace System.Xml.Serialization
{
    using System;
    using System.Reflection;
    using System.Threading;

    public abstract class XmlSerializationGeneratedCode
    {
        private ResolveEventHandler assemblyResolver;
        private TempAssembly tempAssembly;
        private int threadCode;

        protected XmlSerializationGeneratedCode()
        {
        }

        internal void Dispose()
        {
            if (this.assemblyResolver != null)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.assemblyResolver;
            }
            this.assemblyResolver = null;
        }

        internal void Init(TempAssembly tempAssembly)
        {
            this.tempAssembly = tempAssembly;
            if ((tempAssembly != null) && tempAssembly.NeedAssembyResolve)
            {
                this.threadCode = Thread.CurrentThread.GetHashCode();
                this.assemblyResolver = new ResolveEventHandler(this.OnAssemblyResolve);
                AppDomain.CurrentDomain.AssemblyResolve += this.assemblyResolver;
            }
        }

        internal Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if ((this.tempAssembly != null) && (Thread.CurrentThread.GetHashCode() == this.threadCode))
            {
                return this.tempAssembly.GetReferencedAssembly(args.Name);
            }
            return null;
        }
    }
}

