namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Activities.Persistence;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

    public class CompensationExtension : PersistenceParticipant, IWorkflowInstanceExtension
    {
        private static readonly XName compensationExtensionData = compensationNamespace.GetName("Data");
        private static readonly XNamespace compensationNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/compensation");
        private Dictionary<long, CompensationTokenData> compensationTokenTable = new Dictionary<long, CompensationTokenData>();

        internal void Add(long compensationId, CompensationTokenData compensationToken)
        {
            this.CompensationTokenTable[compensationId] = compensationToken;
        }

        protected override void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        {
            writeOnlyValues = null;
            Dictionary<XName, object> dictionary = new Dictionary<XName, object>(1);
            dictionary.Add(compensationExtensionData, new List<object>(6) { this.CompensationTokenTable, this.WorkflowCompensation, this.WorkflowConfirmation, this.WorkflowCompensationScheduled, this.IsWorkflowCompensationBehaviorScheduled, this.Id });
            readWriteValues = dictionary;
        }

        internal Bookmark FindBookmark(long compensationId, CompensationBookmarkName bookmarkName)
        {
            CompensationTokenData data = null;
            Bookmark bookmark = null;
            if (this.CompensationTokenTable.TryGetValue(compensationId, out data))
            {
                bookmark = data.BookmarkTable[bookmarkName];
            }
            return bookmark;
        }

        internal CompensationTokenData Get(long compensationId)
        {
            CompensationTokenData data = null;
            this.CompensationTokenTable.TryGetValue(compensationId, out data);
            return data;
        }

        internal long GetNextId()
        {
            long num;
            this.Id = num = this.Id + 1L;
            return num;
        }

        internal void NotifyMessage(NativeActivityContext context, long compensationId, CompensationBookmarkName compensationBookmark)
        {
            Bookmark bookmark = this.FindBookmark(compensationId, compensationBookmark);
            if (bookmark == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarkNotRegistered(compensationBookmark)));
            }
            context.ResumeBookmark(bookmark, compensationId);
        }

        protected override void PublishValues(IDictionary<XName, object> readWriteValues)
        {
            object obj2;
            if (readWriteValues.TryGetValue(compensationExtensionData, out obj2))
            {
                List<object> list = (List<object>) obj2;
                this.CompensationTokenTable = (Dictionary<long, CompensationTokenData>) list[0];
                this.WorkflowCompensation = (Bookmark) list[1];
                this.WorkflowConfirmation = (Bookmark) list[2];
                this.WorkflowCompensationScheduled = (Bookmark) list[3];
                this.IsWorkflowCompensationBehaviorScheduled = (bool) list[4];
                this.Id = (long) list[5];
            }
        }

        internal void Remove(long compensationId)
        {
            this.CompensationTokenTable.Remove(compensationId);
        }

        internal void SetupWorkflowCompensationBehavior(NativeActivityContext context, BookmarkCallback callback, Activity workflowCompensationBehavior)
        {
            this.WorkflowCompensationScheduled = context.CreateBookmark(callback);
            context.ScheduleSecondaryRoot(workflowCompensationBehavior, null);
            this.Add(0L, new CompensationTokenData(0L, 0L));
            this.IsWorkflowCompensationBehaviorScheduled = true;
        }

        IEnumerable<object> IWorkflowInstanceExtension.GetAdditionalExtensions()
        {
            return null;
        }

        void IWorkflowInstanceExtension.SetInstance(WorkflowInstanceProxy instance)
        {
            this.Instance = instance;
        }

        internal Dictionary<long, CompensationTokenData> CompensationTokenTable
        {
            get
            {
                return this.compensationTokenTable;
            }
            private set
            {
                this.compensationTokenTable = value;
            }
        }

        internal long Id { get; set; }

        internal WorkflowInstanceProxy Instance { get; private set; }

        internal bool IsWorkflowCompensationBehaviorScheduled { get; private set; }

        internal Bookmark WorkflowCompensation { get; set; }

        internal Bookmark WorkflowCompensationScheduled { get; private set; }

        internal Bookmark WorkflowConfirmation { get; set; }
    }
}

