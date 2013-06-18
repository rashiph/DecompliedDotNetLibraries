namespace System.Xml.Xsl
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StringPair
    {
        private string left;
        private string right;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StringPair(string left, string right)
        {
            this.left = left;
            this.right = right;
        }

        public string Left
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.left;
            }
        }
        public string Right
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.right;
            }
        }
    }
}

