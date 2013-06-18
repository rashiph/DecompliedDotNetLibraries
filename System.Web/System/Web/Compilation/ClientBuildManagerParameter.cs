namespace System.Web.Compilation
{
    using System;

    [Serializable]
    public class ClientBuildManagerParameter
    {
        private System.Web.Compilation.PrecompilationFlags _precompilationFlags;
        private string _strongNameKeyContainer;
        private string _strongNameKeyFile;

        public System.Web.Compilation.PrecompilationFlags PrecompilationFlags
        {
            get
            {
                return this._precompilationFlags;
            }
            set
            {
                this._precompilationFlags = value;
            }
        }

        public string StrongNameKeyContainer
        {
            get
            {
                return this._strongNameKeyContainer;
            }
            set
            {
                this._strongNameKeyContainer = value;
            }
        }

        public string StrongNameKeyFile
        {
            get
            {
                return this._strongNameKeyFile;
            }
            set
            {
                this._strongNameKeyFile = value;
            }
        }
    }
}

