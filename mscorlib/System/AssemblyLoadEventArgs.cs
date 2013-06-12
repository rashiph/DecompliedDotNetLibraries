namespace System
{
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class AssemblyLoadEventArgs : EventArgs
    {
        private Assembly _LoadedAssembly;

        public AssemblyLoadEventArgs(Assembly loadedAssembly)
        {
            this._LoadedAssembly = loadedAssembly;
        }

        public Assembly LoadedAssembly
        {
            get
            {
                return this._LoadedAssembly;
            }
        }
    }
}

