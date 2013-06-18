namespace Microsoft.Compiler.VisualBasic
{
    using System;

    internal sealed class Warning
    {
        private string m_description;
        private Microsoft.Compiler.VisualBasic.SourceLocation m_sourceLocation;
        private int m_warningCode;

        internal Warning(int warningCode, string description, Microsoft.Compiler.VisualBasic.SourceLocation sourceLocation)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            if (sourceLocation == null)
            {
                sourceLocation = new Microsoft.Compiler.VisualBasic.SourceLocation(0, 0, 0, 0);
            }
            this.m_warningCode = warningCode;
            this.m_description = description;
            this.m_sourceLocation = sourceLocation;
        }

        public string Description
        {
            get
            {
                return this.m_description;
            }
        }

        public Microsoft.Compiler.VisualBasic.SourceLocation SourceLocation
        {
            get
            {
                return this.m_sourceLocation;
            }
        }

        public int WarningCode
        {
            get
            {
                return this.m_warningCode;
            }
        }
    }
}

