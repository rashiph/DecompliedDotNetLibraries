namespace System.Management
{
    using System;
    using System.Runtime;

    public class WqlObjectQuery : ObjectQuery
    {
        public WqlObjectQuery() : base(null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WqlObjectQuery(string query) : base(query)
        {
        }

        public override object Clone()
        {
            return new WqlObjectQuery(this.QueryString);
        }

        public override string QueryLanguage
        {
            get
            {
                return base.QueryLanguage;
            }
        }
    }
}

