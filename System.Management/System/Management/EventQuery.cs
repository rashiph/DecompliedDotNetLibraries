namespace System.Management
{
    using System;
    using System.Runtime;

    public class EventQuery : ManagementQuery
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventQuery()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventQuery(string query) : base(query)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventQuery(string language, string query) : base(language, query)
        {
        }

        public override object Clone()
        {
            return new EventQuery(this.QueryLanguage, this.QueryString);
        }
    }
}

