namespace System
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct UriTemplateTableMatchCandidate
    {
        private readonly object data;
        private readonly int segmentsCount;
        private readonly UriTemplate template;
        public UriTemplateTableMatchCandidate(UriTemplate template, int segmentsCount, object data)
        {
            this.template = template;
            this.segmentsCount = segmentsCount;
            this.data = data;
        }

        public object Data
        {
            get
            {
                return this.data;
            }
        }
        public int SegmentsCount
        {
            get
            {
                return this.segmentsCount;
            }
        }
        public UriTemplate Template
        {
            get
            {
                return this.template;
            }
        }
    }
}

