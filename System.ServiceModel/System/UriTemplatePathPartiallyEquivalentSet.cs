namespace System
{
    using System.Collections.Generic;

    internal class UriTemplatePathPartiallyEquivalentSet
    {
        private List<KeyValuePair<UriTemplate, object>> kvps;
        private int segmentsCount;

        public UriTemplatePathPartiallyEquivalentSet(int segmentsCount)
        {
            this.segmentsCount = segmentsCount;
            this.kvps = new List<KeyValuePair<UriTemplate, object>>();
        }

        public List<KeyValuePair<UriTemplate, object>> Items
        {
            get
            {
                return this.kvps;
            }
        }

        public int SegmentsCount
        {
            get
            {
                return this.segmentsCount;
            }
        }
    }
}

