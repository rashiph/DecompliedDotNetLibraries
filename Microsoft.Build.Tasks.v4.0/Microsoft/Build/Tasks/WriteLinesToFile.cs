namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.IO;
    using System.Text;

    public class WriteLinesToFile : TaskExtension
    {
        private string encoding;
        private ITaskItem file;
        private ITaskItem[] lines;
        private bool overwrite;

        public override bool Execute()
        {
            bool success = true;
            if (this.File != null)
            {
                StringBuilder builder = new StringBuilder();
                if (this.Lines != null)
                {
                    foreach (ITaskItem item in this.Lines)
                    {
                        builder.AppendLine(item.ItemSpec);
                    }
                }
                System.Text.Encoding encoding = null;
                if (this.encoding != null)
                {
                    try
                    {
                        encoding = System.Text.Encoding.GetEncoding(this.encoding);
                    }
                    catch (ArgumentException)
                    {
                        base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "Encoding", "WriteLinesToFile" });
                        return false;
                    }
                }
                try
                {
                    if (this.Overwrite)
                    {
                        if (builder.Length == 0)
                        {
                            System.IO.File.Delete(this.File.ItemSpec);
                            return success;
                        }
                        if (encoding == null)
                        {
                            System.IO.File.WriteAllText(this.File.ItemSpec, builder.ToString());
                            return success;
                        }
                        System.IO.File.WriteAllText(this.File.ItemSpec, builder.ToString(), encoding);
                        return success;
                    }
                    if (encoding == null)
                    {
                        System.IO.File.AppendAllText(this.File.ItemSpec, builder.ToString());
                        return success;
                    }
                    System.IO.File.AppendAllText(this.File.ItemSpec, builder.ToString(), encoding);
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
            base.Log.LogErrorWithCodeFromResources("WriteLinesToFile.ErrorOrWarning", new object[] { fileName.ItemSpec, e.Message });
            success = false;
        }

        public string Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.encoding = value;
            }
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

        public bool Overwrite
        {
            get
            {
                return this.overwrite;
            }
            set
            {
                this.overwrite = value;
            }
        }
    }
}

