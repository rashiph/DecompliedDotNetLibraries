namespace System.Activities.Tracking
{
    using System;
    using System.Collections.Generic;

    public abstract class TrackingQuery
    {
        private IDictionary<string, string> queryAnnotations;

        protected TrackingQuery()
        {
        }

        internal bool HasAnnotations
        {
            get
            {
                return ((this.queryAnnotations != null) && (this.queryAnnotations.Count > 0));
            }
        }

        public IDictionary<string, string> QueryAnnotations
        {
            get
            {
                if (this.queryAnnotations == null)
                {
                    this.queryAnnotations = new Dictionary<string, string>();
                }
                return this.queryAnnotations;
            }
        }
    }
}

