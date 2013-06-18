namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class OutputMessageCollection : IEnumerable
    {
        private int errorCount;
        private readonly List<OutputMessage> list = new List<OutputMessage>();
        private readonly ResourceManager taskResources = AssemblyResources.PrimaryResources;
        private int warningCount;

        internal OutputMessageCollection()
        {
        }

        internal void AddErrorMessage(string taskResourceName, params string[] arguments)
        {
            this.errorCount++;
            string str = this.taskResources.GetString(taskResourceName);
            if (!string.IsNullOrEmpty(str))
            {
                str = string.Format(CultureInfo.CurrentCulture, str, arguments);
            }
            this.list.Add(new OutputMessage(OutputMessageType.Error, taskResourceName, str, arguments));
        }

        internal void AddWarningMessage(string taskResourceName, params string[] arguments)
        {
            this.warningCount++;
            string str = this.taskResources.GetString(taskResourceName);
            if (!string.IsNullOrEmpty(str))
            {
                str = string.Format(CultureInfo.CurrentCulture, str, arguments);
            }
            this.list.Add(new OutputMessage(OutputMessageType.Warning, taskResourceName, str, arguments));
        }

        public void Clear()
        {
            this.list.Clear();
            this.errorCount = 0;
            this.warningCount = 0;
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        internal bool LogTaskMessages(Task task)
        {
            foreach (OutputMessage message in this.list)
            {
                switch (message.Type)
                {
                    case OutputMessageType.Warning:
                        task.Log.LogWarningWithCodeFromResources(message.Name, message.GetArguments());
                        break;

                    case OutputMessageType.Error:
                        task.Log.LogErrorWithCodeFromResources(message.Name, message.GetArguments());
                        break;
                }
            }
            return (this.errorCount <= 0);
        }

        public int ErrorCount
        {
            get
            {
                return this.errorCount;
            }
        }

        public OutputMessage this[int index]
        {
            get
            {
                return this.list[index];
            }
        }

        public int WarningCount
        {
            get
            {
                return this.warningCount;
            }
        }
    }
}

