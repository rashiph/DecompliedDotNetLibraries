namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Activities.Runtime;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public class Bookmark : IEquatable<Bookmark>
    {
        private static Bookmark asyncOperationCompletionBookmark = new Bookmark(-1L);
        private static IEqualityComparer<Bookmark> comparer;
        [DataMember(EmitDefaultValue=false, Order=2)]
        private ExclusiveHandleList exclusiveHandlesThatReferenceThis;
        [DataMember(EmitDefaultValue=false, Order=1)]
        private string externalName;
        [DataMember(EmitDefaultValue=false, Order=0)]
        private long id;

        private Bookmark(long id)
        {
            this.id = id;
        }

        public Bookmark(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }
            this.externalName = name;
        }

        internal static Bookmark Create(long id)
        {
            return new Bookmark(id);
        }

        public bool Equals(Bookmark other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            if (!this.IsNamed)
            {
                return (this.id == other.id);
            }
            return (other.IsNamed && (this.externalName == other.externalName));
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Bookmark);
        }

        internal BookmarkInfo GenerateBookmarkInfo(BookmarkCallbackWrapper bookmarkCallback)
        {
            BookmarkScopeInfo scopeInfo = null;
            if (this.Scope != null)
            {
                scopeInfo = this.Scope.GenerateScopeInfo();
            }
            return new BookmarkInfo(this.externalName, bookmarkCallback.ActivityInstance.Activity.DisplayName, scopeInfo);
        }

        public override int GetHashCode()
        {
            if (this.IsNamed)
            {
                return this.externalName.GetHashCode();
            }
            return this.id.GetHashCode();
        }

        public override string ToString()
        {
            if (this.IsNamed)
            {
                return this.Name;
            }
            return this.Id.ToString(CultureInfo.InvariantCulture);
        }

        internal static Bookmark AsyncOperationCompletionBookmark
        {
            get
            {
                return asyncOperationCompletionBookmark;
            }
        }

        internal static IEqualityComparer<Bookmark> Comparer
        {
            get
            {
                if (comparer == null)
                {
                    comparer = new BookmarkComparer();
                }
                return comparer;
            }
        }

        internal ExclusiveHandleList ExclusiveHandles
        {
            get
            {
                return this.exclusiveHandlesThatReferenceThis;
            }
            set
            {
                this.exclusiveHandlesThatReferenceThis = value;
            }
        }

        internal long Id
        {
            get
            {
                return this.id;
            }
        }

        internal bool IsNamed
        {
            get
            {
                return (this.id == 0L);
            }
        }

        public string Name
        {
            get
            {
                if (this.IsNamed)
                {
                    return this.externalName;
                }
                return string.Empty;
            }
        }

        [DataMember(EmitDefaultValue=false)]
        internal BookmarkScope Scope { get; set; }

        [DataContract]
        private class BookmarkComparer : IEqualityComparer<Bookmark>
        {
            public bool Equals(Bookmark x, Bookmark y)
            {
                if (object.ReferenceEquals(x, null))
                {
                    return object.ReferenceEquals(y, null);
                }
                return x.Equals(y);
            }

            public int GetHashCode(Bookmark obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}

