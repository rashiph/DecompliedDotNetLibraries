namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.IO;

    public class ReadLinesFromFile : TaskExtension
    {
        private ITaskItem file;
        private ITaskItem[] lines = new TaskItem[0];

        public override bool Execute()
        {
            bool success = true;
            if ((this.File != null) && System.IO.File.Exists(this.File.ItemSpec))
            {
                string[] strArray = null;
                try
                {
                    strArray = System.IO.File.ReadAllLines(this.File.ItemSpec);
                    ArrayList list = new ArrayList();
                    char[] chArray2 = new char[3];
                    chArray2[1] = ' ';
                    chArray2[2] = '\t';
                    char[] trimChars = chArray2;
                    foreach (string str in strArray)
                    {
                        string unescapedString = str.Trim(trimChars);
                        if (unescapedString.Length > 0)
                        {
                            list.Add(new TaskItem(Microsoft.Build.Shared.EscapingUtilities.Escape(unescapedString)));
                        }
                    }
                    this.Lines = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
                }
                catch (Exception exception)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                    {
                        throw;
                    }
                    this.LogError(this.file, exception, ref success);
                }
            }
            return success;
        }

        private void LogError(ITaskItem fileName, Exception e, ref bool success)
        {
            base.Log.LogErrorWithCodeFromResources("ReadLinesFromFile.ErrorOrWarning", new object[] { fileName.ItemSpec, e.Message });
            success = false;
        }

        [Required]
        public ITaskItem File
        {
            get
            {
                return this.file;
            }
            set
            {
                this.file = value;
            }
        }

        [Output]
        public ITaskItem[] Lines
        {
            get
            {
                return this.lines;
            }
            set
            {
                this.lines = value;
            }
        }
    }
}

