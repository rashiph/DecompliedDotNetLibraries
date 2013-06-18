namespace System.Activities.Hosting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class BookmarkScopeInfo
    {
        internal BookmarkScopeInfo(Guid id)
        {
            this.Id = id;
        }

        internal BookmarkScopeInfo(string temporaryId)
        {
            this.TemporaryId = temporaryId;
        }

        [DataMember(EmitDefaultValue=false)]
        public Guid Id { get; private set; }

        public bool IsInitialized
        {
            get
            {
                return (this.TemporaryId == null);
            }
        }

        [DataMember(EmitDefaultValue=false)]
        public string TemporaryId { get; private set; }
    }
}

