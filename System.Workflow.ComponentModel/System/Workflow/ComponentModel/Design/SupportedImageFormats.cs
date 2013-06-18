namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing.Imaging;
    using System.Runtime;

    internal class SupportedImageFormats
    {
        public string Description;
        public ImageFormat Format;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SupportedImageFormats(string description, ImageFormat imageFormat)
        {
            this.Description = description;
            this.Format = imageFormat;
        }
    }
}

