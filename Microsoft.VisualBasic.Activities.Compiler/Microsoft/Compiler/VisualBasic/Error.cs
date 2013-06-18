namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Error", Justification="For Internal Partners Only")]
    internal sealed class Error
    {
        private string m_description;
        private int m_errorCode;
        private Microsoft.Compiler.VisualBasic.SourceLocation m_sourceLocation;

        internal Error(int errorCode, string description, Microsoft.Compiler.VisualBasic.SourceLocation sourceLocation)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            if (sourceLocation == null)
            {
                sourceLocation = new Microsoft.Compiler.VisualBasic.SourceLocation(0, 0, 0, 0);
            }
            this.m_errorCode = errorCode;
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

        public int ErrorCode
        {
            get
            {
                return this.m_errorCode;
            }
        }

        public Microsoft.Compiler.VisualBasic.SourceLocation SourceLocation
        {
            get
            {
                return this.m_sourceLocation;
            }
        }
    }
}

