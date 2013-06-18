namespace System.Management
{
    using System;
    using System.Runtime;

    public class ObjectQuery : ManagementQuery
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ObjectQuery()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ObjectQuery(string query) : base(query)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ObjectQuery(string language, string query) : base(language, query)
        {
        }

        public override object Clone()
        {
            return new ObjectQuery(this.QueryLanguage, this.QueryString);
        }
    }
}

