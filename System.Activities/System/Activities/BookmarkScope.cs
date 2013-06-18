namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Globalization;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class BookmarkScope : IEquatable<BookmarkScope>
    {
        private static BookmarkScope defaultBookmarkScope;
        [DataMember(EmitDefaultValue=false)]
        private Guid id;
        [DataMember(EmitDefaultValue=false)]
        private long temporaryId;

        private BookmarkScope()
        {
        }

        public BookmarkScope(Guid id)
        {
            this.id = id;
        }

        internal BookmarkScope(long temporaryId)
        {
            this.temporaryId = temporaryId;
        }

        public bool Equals(BookmarkScope other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (this.IsDefault)
            {
                return other.IsDefault;
            }
            if (this.IsInitialized)
            {
                return (other.id == this.id);
            }
            return (other.temporaryId == this.temporaryId);
        }

        internal BookmarkScopeInfo GenerateScopeInfo()
        {
            if (this.IsInitialized)
            {
                return new BookmarkScopeInfo(this.Id);
            }
            return new BookmarkScopeInfo(this.temporaryId.ToString(CultureInfo.InvariantCulture));
        }

        public override int GetHashCode()
        {
            if (this.IsInitialized)
            {
                return this.id.GetHashCode();
            }
            return this.temporaryId.GetHashCode();
        }

        public void Initialize(NativeActivityContext context, Guid id)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            if (id == Guid.Empty)
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("id");
            }
            if (this.IsInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarkScopeAlreadyInitialized));
            }
            context.InitializeBookmarkScope(this, id);
        }

        public static BookmarkScope Default
        {
            get
            {
                if (defaultBookmarkScope == null)
                {
                    defaultBookmarkScope = new BookmarkScope();
                }
                return defaultBookmarkScope;
            }
        }

        public Guid Id
        {
            get
            {
                return this.id;
            }
            internal set
            {
                this.id = value;
                this.temporaryId = 0L;
            }
        }

        internal bool IsDefault
        {
            get
            {
                return (this.IsInitialized && (this.id == Guid.Empty));
            }
        }

        public bool IsInitialized
        {
            get
            {
                return (this.temporaryId == 0L);
            }
        }

        internal long TemporaryId
        {
            get
            {
                return this.temporaryId;
            }
        }
    }
}

