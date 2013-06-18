namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal class TrackingProvider
    {
        private Hashtable activitySubscriptions;
        private Activity definition;
        private bool filterValuesSetExplicitly;
        private long nextTrackingRecordNumber;
        private IList<TrackingRecord> pendingTrackingRecords;
        private Dictionary<TrackingParticipant, RuntimeTrackingProfile> profileSubscriptions;
        private List<TrackingParticipant> trackingParticipants;

        public TrackingProvider(Activity definition)
        {
            this.definition = definition;
            this.ShouldTrack = true;
            this.ShouldTrackActivityStateRecords = true;
            this.ShouldTrackActivityStateRecordsExecutingState = true;
            this.ShouldTrackActivityStateRecordsClosedState = true;
            this.ShouldTrackBookmarkResumptionRecords = true;
            this.ShouldTrackActivityScheduledRecords = true;
            this.ShouldTrackCancelRequestedRecords = true;
            this.ShouldTrackFaultPropagationRecords = true;
            this.ShouldTrackWorkflowInstanceRecords = true;
        }

        public void AddParticipant(TrackingParticipant participant)
        {
            if (this.trackingParticipants == null)
            {
                this.trackingParticipants = new List<TrackingParticipant>();
                this.profileSubscriptions = new Dictionary<TrackingParticipant, RuntimeTrackingProfile>();
            }
            this.trackingParticipants.Add(participant);
        }

        public void AddRecord(TrackingRecord record)
        {
            if (this.pendingTrackingRecords == null)
            {
                this.pendingTrackingRecords = new List<TrackingRecord>();
            }
            record.RecordNumber = this.GetNextRecordNumber();
            this.pendingTrackingRecords.Add(record);
        }

        public IAsyncResult BeginFlushPendingRecords(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new FlushPendingRecordsAsyncResult(this, timeout, callback, state);
        }

        private void ClearPendingRecords()
        {
            if (this.pendingTrackingRecords != null)
            {
                for (int i = this.pendingTrackingRecords.Count - 1; i >= 0; i--)
                {
                    this.pendingTrackingRecords.RemoveAt(i);
                }
            }
        }

        public void EndFlushPendingRecords(IAsyncResult result)
        {
            FlushPendingRecordsAsyncResult.End(result);
        }

        public void FlushPendingRecords(TimeSpan timeout)
        {
            try
            {
                if (this.HasPendingRecords)
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    for (int i = 0; i < this.trackingParticipants.Count; i++)
                    {
                        TrackingParticipant participant = this.trackingParticipants[i];
                        RuntimeTrackingProfile runtimeTrackingProfile = this.GetRuntimeTrackingProfile(participant);
                        if (this.pendingTrackingRecords != null)
                        {
                            for (int j = 0; j < this.pendingTrackingRecords.Count; j++)
                            {
                                TrackingRecord record = this.pendingTrackingRecords[j];
                                TrackingRecord record2 = null;
                                bool shouldClone = this.trackingParticipants.Count > 1;
                                if (runtimeTrackingProfile == null)
                                {
                                    record2 = shouldClone ? record.Clone() : record;
                                }
                                else
                                {
                                    record2 = runtimeTrackingProfile.Match(record, shouldClone);
                                }
                                if (record2 != null)
                                {
                                    participant.Track(record2, helper.RemainingTime());
                                    if (TD.TrackingRecordRaisedIsEnabled())
                                    {
                                        TD.TrackingRecordRaised(record2.ToString(), participant.GetType().ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.ClearPendingRecords();
            }
        }

        private long GetNextRecordNumber()
        {
            long num;
            this.nextTrackingRecordNumber = (num = this.nextTrackingRecordNumber) + 1L;
            return num;
        }

        private RuntimeTrackingProfile GetRuntimeTrackingProfile(TrackingParticipant participant)
        {
            RuntimeTrackingProfile runtimeTrackingProfile;
            if (!this.profileSubscriptions.TryGetValue(participant, out runtimeTrackingProfile))
            {
                TrackingProfile trackingProfile = participant.TrackingProfile;
                if (trackingProfile != null)
                {
                    runtimeTrackingProfile = RuntimeTrackingProfile.GetRuntimeTrackingProfile(trackingProfile, this.definition);
                    this.Merge(runtimeTrackingProfile.Filter);
                    IEnumerable<string> subscribedActivityNames = runtimeTrackingProfile.GetSubscribedActivityNames();
                    if (subscribedActivityNames != null)
                    {
                        if (this.activitySubscriptions == null)
                        {
                            this.activitySubscriptions = new Hashtable();
                        }
                        foreach (string str in subscribedActivityNames)
                        {
                            if (this.activitySubscriptions[str] == null)
                            {
                                this.activitySubscriptions[str] = str;
                            }
                        }
                    }
                }
                else
                {
                    this.Merge(new TrackingRecordPreFilter(true));
                }
                this.profileSubscriptions.Add(participant, runtimeTrackingProfile);
            }
            return runtimeTrackingProfile;
        }

        private void Merge(TrackingRecordPreFilter filter)
        {
            if (!this.filterValuesSetExplicitly)
            {
                this.filterValuesSetExplicitly = true;
                this.ShouldTrackActivityStateRecordsExecutingState = filter.TrackActivityStateRecordsExecutingState;
                this.ShouldTrackActivityScheduledRecords = filter.TrackActivityScheduledRecords;
                this.ShouldTrackActivityStateRecords = filter.TrackActivityStateRecords;
                this.ShouldTrackActivityStateRecordsClosedState = filter.TrackActivityStateRecordsClosedState;
                this.ShouldTrackBookmarkResumptionRecords = filter.TrackBookmarkResumptionRecords;
                this.ShouldTrackCancelRequestedRecords = filter.TrackCancelRequestedRecords;
                this.ShouldTrackFaultPropagationRecords = filter.TrackFaultPropagationRecords;
                this.ShouldTrackWorkflowInstanceRecords = filter.TrackWorkflowInstanceRecords;
            }
            else
            {
                this.ShouldTrackActivityStateRecordsExecutingState |= filter.TrackActivityStateRecordsExecutingState;
                this.ShouldTrackActivityScheduledRecords |= filter.TrackActivityScheduledRecords;
                this.ShouldTrackActivityStateRecords |= filter.TrackActivityStateRecords;
                this.ShouldTrackActivityStateRecordsClosedState |= filter.TrackActivityStateRecordsClosedState;
                this.ShouldTrackBookmarkResumptionRecords |= filter.TrackBookmarkResumptionRecords;
                this.ShouldTrackCancelRequestedRecords |= filter.TrackCancelRequestedRecords;
                this.ShouldTrackFaultPropagationRecords |= filter.TrackFaultPropagationRecords;
                this.ShouldTrackWorkflowInstanceRecords |= filter.TrackWorkflowInstanceRecords;
            }
        }

        public void OnDeserialized(long nextTrackingRecordNumber)
        {
            this.nextTrackingRecordNumber = nextTrackingRecordNumber;
        }

        public bool ShouldTrackActivity(string name)
        {
            if ((this.activitySubscriptions != null) && !this.activitySubscriptions.ContainsKey(name))
            {
                return this.activitySubscriptions.ContainsKey("*");
            }
            return true;
        }

        public bool HasPendingRecords
        {
            get
            {
                return (((this.pendingTrackingRecords != null) && (this.pendingTrackingRecords.Count > 0)) || !this.filterValuesSetExplicitly);
            }
        }

        public long NextTrackingRecordNumber
        {
            get
            {
                return this.nextTrackingRecordNumber;
            }
        }

        public bool ShouldTrack { get; private set; }

        public bool ShouldTrackActivityScheduledRecords { get; private set; }

        public bool ShouldTrackActivityStateRecords { get; private set; }

        public bool ShouldTrackActivityStateRecordsClosedState { get; private set; }

        public bool ShouldTrackActivityStateRecordsExecutingState { get; private set; }

        public bool ShouldTrackBookmarkResumptionRecords { get; private set; }

        public bool ShouldTrackCancelRequestedRecords { get; private set; }

        public bool ShouldTrackFaultPropagationRecords { get; private set; }

        public bool ShouldTrackWorkflowInstanceRecords { get; private set; }

        private class FlushPendingRecordsAsyncResult : AsyncResult
        {
            private int currentParticipant;
            private int currentRecord;
            private TrackingProvider provider;
            private TimeoutHelper timeoutHelper;
            private static AsyncResult.AsyncCompletion trackingCompleteCallback = new AsyncResult.AsyncCompletion(TrackingProvider.FlushPendingRecordsAsyncResult.OnTrackingComplete);

            public FlushPendingRecordsAsyncResult(TrackingProvider provider, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.provider = provider;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (this.RunLoop())
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<TrackingProvider.FlushPendingRecordsAsyncResult>(result);
            }

            private static bool OnTrackingComplete(IAsyncResult result)
            {
                TrackingProvider.FlushPendingRecordsAsyncResult asyncState = (TrackingProvider.FlushPendingRecordsAsyncResult) result.AsyncState;
                TrackingParticipant participant = asyncState.provider.trackingParticipants[asyncState.currentParticipant];
                bool flag = false;
                try
                {
                    participant.EndTrack(result);
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        asyncState.provider.ClearPendingRecords();
                    }
                }
                return asyncState.RunLoop();
            }

            private bool PostTrackingRecord(TrackingParticipant participant, RuntimeTrackingProfile runtimeProfile)
            {
                TrackingRecord record = this.provider.pendingTrackingRecords[this.currentRecord];
                this.currentRecord++;
                bool flag = false;
                try
                {
                    TrackingRecord record2 = null;
                    bool shouldClone = this.provider.trackingParticipants.Count > 1;
                    if (runtimeProfile == null)
                    {
                        record2 = shouldClone ? record.Clone() : record;
                    }
                    else
                    {
                        record2 = runtimeProfile.Match(record, shouldClone);
                    }
                    if (record2 != null)
                    {
                        IAsyncResult result = participant.BeginTrack(record2, this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(trackingCompleteCallback), this);
                        if (TD.TrackingRecordRaisedIsEnabled())
                        {
                            TD.TrackingRecordRaised(record2.ToString(), participant.GetType().ToString());
                        }
                        if (!result.CompletedSynchronously)
                        {
                            flag = true;
                            return false;
                        }
                        participant.EndTrack(result);
                    }
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        this.provider.ClearPendingRecords();
                    }
                }
                return true;
            }

            private bool RunLoop()
            {
                if (this.provider.HasPendingRecords)
                {
                    while (this.currentParticipant < this.provider.trackingParticipants.Count)
                    {
                        TrackingParticipant participant = this.provider.trackingParticipants[this.currentParticipant];
                        RuntimeTrackingProfile runtimeTrackingProfile = this.provider.GetRuntimeTrackingProfile(participant);
                        if (this.provider.pendingTrackingRecords != null)
                        {
                            while (this.currentRecord < this.provider.pendingTrackingRecords.Count)
                            {
                                if (!this.PostTrackingRecord(participant, runtimeTrackingProfile))
                                {
                                    return false;
                                }
                            }
                        }
                        this.currentRecord = 0;
                        this.currentParticipant++;
                    }
                }
                this.provider.ClearPendingRecords();
                return true;
            }
        }
    }
}

