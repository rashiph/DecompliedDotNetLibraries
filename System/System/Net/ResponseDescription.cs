namespace System.Net
{
    using System;
    using System.Text;

    internal class ResponseDescription
    {
        internal bool Multiline;
        internal const int NoStatus = -1;
        internal int Status = -1;
        internal StringBuilder StatusBuffer = new StringBuilder();
        internal string StatusCodeString;
        internal string StatusDescription;

        internal bool InvalidStatusCode
        {
            get
            {
                if (this.Status >= 100)
                {
                    return (this.Status > 0x257);
                }
                return true;
            }
        }

        internal bool PermanentFailure
        {
            get
            {
                return ((this.Status >= 500) && (this.Status <= 0x257));
            }
        }

        internal bool PositiveCompletion
        {
            get
            {
                return ((this.Status >= 200) && (this.Status <= 0x12b));
            }
        }

        internal bool PositiveIntermediate
        {
            get
            {
                return ((this.Status >= 100) && (this.Status <= 0xc7));
            }
        }

        internal bool TransientFailure
        {
            get
            {
                return ((this.Status >= 400) && (this.Status <= 0x1f3));
            }
        }
    }
}

