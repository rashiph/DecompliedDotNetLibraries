namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Guid("36dcda30-dc3b-4d93-be42-90b2d74c64e7")]
    public class RegistrationConfig
    {
        private string _application;
        private string _approotdir;
        private string _assmfile;
        private System.EnterpriseServices.InstallationFlags _flags;
        private string _partition;
        private string _typelib;

        public string Application
        {
            get
            {
                return this._application;
            }
            set
            {
                this._application = value;
            }
        }

        public string ApplicationRootDirectory
        {
            get
            {
                return this._approotdir;
            }
            set
            {
                this._approotdir = value;
            }
        }

        public string AssemblyFile
        {
            get
            {
                return this._assmfile;
            }
            set
            {
                this._assmfile = value;
            }
        }

        public System.EnterpriseServices.InstallationFlags InstallationFlags
        {
            get
            {
                return this._flags;
            }
            set
            {
                this._flags = value;
            }
        }

        public string Partition
        {
            get
            {
                return this._partition;
            }
            set
            {
                this._partition = value;
            }
        }

        public string TypeLibrary
        {
            get
            {
                return this._typelib;
            }
            set
            {
                this._typelib = value;
            }
        }
    }
}

