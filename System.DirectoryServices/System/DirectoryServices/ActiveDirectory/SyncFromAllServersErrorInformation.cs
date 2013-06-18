namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    public class SyncFromAllServersErrorInformation
    {
        private SyncFromAllServersErrorCategory category;
        private int errorCode;
        private string errorMessage;
        private string sourceServer;
        private string targetServer;

        internal SyncFromAllServersErrorInformation(SyncFromAllServersErrorCategory category, int errorCode, string errorMessage, string sourceServer, string targetServer)
        {
            this.category = category;
            this.errorCode = errorCode;
            this.errorMessage = errorMessage;
            this.sourceServer = sourceServer;
            this.targetServer = targetServer;
        }

        public SyncFromAllServersErrorCategory ErrorCategory
        {
            get
            {
                return this.category;
            }
        }

        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }
        }

        public string SourceServer
        {
            get
            {
                return this.sourceServer;
            }
        }

        public string TargetServer
        {
            get
            {
                return this.targetServer;
            }
        }
    }
}

