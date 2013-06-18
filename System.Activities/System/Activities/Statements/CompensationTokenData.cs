namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class CompensationTokenData
    {
        internal CompensationTokenData(long compensationId, long parentCompensationId)
        {
            this.CompensationId = compensationId;
            this.ParentCompensationId = parentCompensationId;
            this.BookmarkTable = new System.Activities.Statements.BookmarkTable();
            this.ExecutionTracker = new System.Activities.Statements.ExecutionTracker();
            this.CompensationState = System.Activities.Statements.CompensationState.Creating;
        }

        internal void RemoveBookmark(NativeActivityContext context, CompensationBookmarkName bookmarkName)
        {
            Bookmark bookmark = this.BookmarkTable[bookmarkName];
            if (bookmark != null)
            {
                context.RemoveBookmark(bookmark);
                this.BookmarkTable[bookmarkName] = null;
            }
        }

        [DataMember]
        internal System.Activities.Statements.BookmarkTable BookmarkTable { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        internal long CompensationId { get; private set; }

        [DefaultValue(1), DataMember(EmitDefaultValue=false)]
        internal System.Activities.Statements.CompensationState CompensationState { get; set; }

        [DataMember(EmitDefaultValue=false)]
        internal string DisplayName { get; set; }

        [DataMember]
        internal System.Activities.Statements.ExecutionTracker ExecutionTracker { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        internal bool IsTokenValidInSecondaryRoot { get; set; }

        [DataMember(EmitDefaultValue=false)]
        internal long ParentCompensationId { get; private set; }
    }
}

