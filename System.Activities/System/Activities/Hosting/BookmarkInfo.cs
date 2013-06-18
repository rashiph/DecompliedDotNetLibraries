namespace System.Activities.Hosting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class BookmarkInfo
    {
        internal BookmarkInfo(string bookmarkName, string ownerDisplayName, BookmarkScopeInfo scopeInfo)
        {
            this.BookmarkName = bookmarkName;
            this.OwnerDisplayName = ownerDisplayName;
            this.ScopeInfo = scopeInfo;
        }

        [DataMember]
        public string BookmarkName { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        public string OwnerDisplayName { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        public BookmarkScopeInfo ScopeInfo { get; private set; }
    }
}

