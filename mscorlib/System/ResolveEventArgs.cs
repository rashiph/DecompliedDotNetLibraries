namespace System
{
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class ResolveEventArgs : EventArgs
    {
        private string _Name;
        private Assembly _RequestingAssembly;

        public ResolveEventArgs(string name)
        {
            this._Name = name;
        }

        public ResolveEventArgs(string name, Assembly requestingAssembly)
        {
            this._Name = name;
            this._RequestingAssembly = requestingAssembly;
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
        }

        public Assembly RequestingAssembly
        {
            get
            {
                return this._RequestingAssembly;
            }
        }
    }
}

