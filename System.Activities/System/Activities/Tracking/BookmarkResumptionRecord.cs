namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class BookmarkResumptionRecord : TrackingRecord
    {
        private BookmarkResumptionRecord(BookmarkResumptionRecord record) : base(record)
        {
            this.BookmarkScope = record.BookmarkScope;
            this.Owner = record.Owner;
            this.BookmarkName = record.BookmarkName;
            this.Payload = record.Payload;
        }

        internal BookmarkResumptionRecord(Guid instanceId, Bookmark bookmark, System.Activities.ActivityInstance ownerInstance, object payload) : base(instanceId)
        {
            if (bookmark.Scope != null)
            {
                this.BookmarkScope = bookmark.Scope.Id;
            }
            if (bookmark.IsNamed)
            {
                this.BookmarkName = bookmark.Name;
            }
            this.Owner = new ActivityInfo(ownerInstance);
            this.Payload = payload;
        }

        public BookmarkResumptionRecord(Guid instanceId, long recordNumber, Guid bookmarkScope, string bookmarkName, ActivityInfo owner) : base(instanceId, recordNumber)
        {
            if (owner == null)
            {
                throw FxTrace.Exception.ArgumentNull("owner");
            }
            this.BookmarkScope = bookmarkScope;
            this.BookmarkName = bookmarkName;
            this.Owner = owner;
        }

        protected internal override TrackingRecord Clone()
        {
            return new BookmarkResumptionRecord(this);
        }

        public override string ToString()
        {
            object[] args = new object[] { base.ToString(), this.BookmarkName ?? "<null>", this.BookmarkScope, this.Owner.ToString() };
            return string.Format(CultureInfo.CurrentCulture, "BookmarkResumptionRecord {{ {0}, BookmarkName = {1}, BookmarkScope = {2}, OwnerActivity {{ {3} }} }}", args);
        }

        [DataMember(EmitDefaultValue=false)]
        public string BookmarkName { get; private set; }

        [DataMember(EmitDefaultValue=false)]
        public Guid BookmarkScope { get; private set; }

        [DataMember]
        public ActivityInfo Owner { get; private set; }

        [DataMember]
        public object Payload { get; internal set; }
    }
}

