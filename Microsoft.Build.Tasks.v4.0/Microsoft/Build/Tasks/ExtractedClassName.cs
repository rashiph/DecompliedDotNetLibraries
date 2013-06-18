namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ExtractedClassName
    {
        private bool isInsideConditionalBlock;
        private string name;
        public bool IsInsideConditionalBlock
        {
            get
            {
                return this.isInsideConditionalBlock;
            }
            set
            {
                this.isInsideConditionalBlock = value;
            }
        }
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
    }
}

