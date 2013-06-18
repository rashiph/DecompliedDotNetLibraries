namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ClassInterface(ClassInterfaceType.None), ComVisible(true), Guid("FAD7BA7C-CA00-41e0-A5EF-2DA9A74E58E6")]
    public class BuildResults : IBuildResults
    {
        private ArrayList componentFiles = new ArrayList();
        private string keyFile = string.Empty;
        private ArrayList messages = new ArrayList();
        private bool succeeded = false;

        internal BuildResults()
        {
        }

        internal void AddComponentFiles(string[] filePaths)
        {
            this.componentFiles.AddRange(filePaths);
        }

        internal void AddMessage(BuildMessage message)
        {
            this.messages.Add(message);
        }

        internal void BuildSucceeded()
        {
            this.succeeded = true;
        }

        internal void SetKeyFile(string filePath)
        {
            this.keyFile = filePath;
        }

        public string[] ComponentFiles
        {
            get
            {
                if (this.componentFiles.Count == 0)
                {
                    return null;
                }
                string[] array = new string[this.componentFiles.Count];
                this.componentFiles.CopyTo(array);
                return array;
            }
        }

        public string KeyFile
        {
            get
            {
                return this.keyFile;
            }
        }

        public BuildMessage[] Messages
        {
            get
            {
                if (this.messages.Count == 0)
                {
                    return null;
                }
                BuildMessage[] array = new BuildMessage[this.messages.Count];
                this.messages.CopyTo(array);
                return array;
            }
        }

        public bool Succeeded
        {
            get
            {
                return this.succeeded;
            }
        }
    }
}

