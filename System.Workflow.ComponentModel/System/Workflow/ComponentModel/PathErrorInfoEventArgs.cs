namespace System.Workflow.ComponentModel
{
    using System;

    internal class PathErrorInfoEventArgs : EventArgs
    {
        private string currentPath;
        private SourceValueInfo info;

        public PathErrorInfoEventArgs(SourceValueInfo info, string currentPath)
        {
            if (currentPath == null)
            {
                throw new ArgumentNullException("currentPath");
            }
            this.info = info;
            this.currentPath = currentPath;
        }
    }
}

