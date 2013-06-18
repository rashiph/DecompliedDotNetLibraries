namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Reflection;
    using System.Runtime.Serialization;

    [DataContract]
    internal class BookmarkTable
    {
        [DataMember]
        private Bookmark[] bookmarkTable = new Bookmark[tableSize];
        private static int tableSize = Enum.GetValues(typeof(CompensationBookmarkName)).Length;

        public Bookmark this[CompensationBookmarkName bookmarkName]
        {
            get
            {
                return this.bookmarkTable[(int) bookmarkName];
            }
            set
            {
                this.bookmarkTable[(int) bookmarkName] = value;
            }
        }
    }
}

