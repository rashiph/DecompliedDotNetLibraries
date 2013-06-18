namespace System.Activities.Tracking
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal class RuntimeTrackingProfile
    {
        private List<string> activityNames;
        private List<ActivityScheduledQuery> activityScheduledSubscriptions;
        private Dictionary<string, HybridCollection<ActivityStateQuery>> activitySubscriptions;
        private TrackingProfile associatedProfile;
        private Dictionary<string, BookmarkResumptionQuery> bookmarkSubscriptions;
        private List<CancelRequestedQuery> cancelRequestedSubscriptions;
        private List<CustomTrackingQuery> customTrackingQuerySubscriptions;
        private List<FaultPropagationQuery> faultPropagationSubscriptions;
        private bool isRootNativeActivity;
        private static RuntimeTrackingProfileCache profileCache;
        private TrackingRecordPreFilter trackingRecordPreFilter;
        private Dictionary<string, WorkflowInstanceQuery> workflowEventSubscriptions;

        internal RuntimeTrackingProfile(TrackingProfile profile, Activity rootElement)
        {
            this.associatedProfile = profile;
            this.isRootNativeActivity = rootElement is NativeActivity;
            this.trackingRecordPreFilter = new TrackingRecordPreFilter();
            foreach (TrackingQuery query in this.associatedProfile.Queries)
            {
                if (query is ActivityStateQuery)
                {
                    this.AddActivitySubscription((ActivityStateQuery) query);
                }
                else if (query is WorkflowInstanceQuery)
                {
                    this.AddWorkflowSubscription((WorkflowInstanceQuery) query);
                }
                else if (query is BookmarkResumptionQuery)
                {
                    this.AddBookmarkSubscription((BookmarkResumptionQuery) query);
                }
                else if (query is CustomTrackingQuery)
                {
                    this.AddCustomTrackingSubscription((CustomTrackingQuery) query);
                }
                else if (query is ActivityScheduledQuery)
                {
                    this.AddActivityScheduledSubscription((ActivityScheduledQuery) query);
                }
                else if (query is CancelRequestedQuery)
                {
                    this.AddCancelRequestedSubscription((CancelRequestedQuery) query);
                }
                else if (query is FaultPropagationQuery)
                {
                    this.AddFaultPropagationSubscription((FaultPropagationQuery) query);
                }
            }
        }

        private void AddActivityName(string name)
        {
            if (this.activityNames == null)
            {
                this.activityNames = new List<string>();
            }
            this.activityNames.Add(name);
        }

        private void AddActivityScheduledSubscription(ActivityScheduledQuery activityScheduledQuery)
        {
            this.trackingRecordPreFilter.TrackActivityScheduledRecords = true;
            if (this.activityScheduledSubscriptions == null)
            {
                this.activityScheduledSubscriptions = new List<ActivityScheduledQuery>();
            }
            this.activityScheduledSubscriptions.Add(activityScheduledQuery);
        }

        private void AddActivitySubscription(ActivityStateQuery query)
        {
            HybridCollection<ActivityStateQuery> hybrids;
            this.trackingRecordPreFilter.TrackActivityStateRecords = true;
            foreach (string str in query.States)
            {
                if (string.CompareOrdinal(str, "*") == 0)
                {
                    this.trackingRecordPreFilter.TrackActivityStateRecordsClosedState = true;
                    this.trackingRecordPreFilter.TrackActivityStateRecordsExecutingState = true;
                    break;
                }
                if (string.CompareOrdinal(str, "Closed") == 0)
                {
                    this.trackingRecordPreFilter.TrackActivityStateRecordsClosedState = true;
                }
                else if (string.CompareOrdinal(str, "Executing") == 0)
                {
                    this.trackingRecordPreFilter.TrackActivityStateRecordsExecutingState = true;
                }
            }
            if (this.activitySubscriptions == null)
            {
                this.activitySubscriptions = new Dictionary<string, HybridCollection<ActivityStateQuery>>();
            }
            if (!this.activitySubscriptions.TryGetValue(query.ActivityName, out hybrids))
            {
                hybrids = new HybridCollection<ActivityStateQuery>();
                this.activitySubscriptions[query.ActivityName] = hybrids;
            }
            hybrids.Add(query);
            this.AddActivityName(query.ActivityName);
        }

        private void AddBookmarkSubscription(BookmarkResumptionQuery bookmarkTrackingQuery)
        {
            this.trackingRecordPreFilter.TrackBookmarkResumptionRecords = true;
            if (this.bookmarkSubscriptions == null)
            {
                this.bookmarkSubscriptions = new Dictionary<string, BookmarkResumptionQuery>();
            }
            if (!this.bookmarkSubscriptions.ContainsKey(bookmarkTrackingQuery.Name))
            {
                this.bookmarkSubscriptions.Add(bookmarkTrackingQuery.Name, bookmarkTrackingQuery);
            }
        }

        private void AddCancelRequestedSubscription(CancelRequestedQuery cancelQuery)
        {
            this.trackingRecordPreFilter.TrackCancelRequestedRecords = true;
            if (this.cancelRequestedSubscriptions == null)
            {
                this.cancelRequestedSubscriptions = new List<CancelRequestedQuery>();
            }
            this.cancelRequestedSubscriptions.Add(cancelQuery);
        }

        private void AddCustomTrackingSubscription(CustomTrackingQuery customQuery)
        {
            if (this.customTrackingQuerySubscriptions == null)
            {
                this.customTrackingQuerySubscriptions = new List<CustomTrackingQuery>();
            }
            this.customTrackingQuerySubscriptions.Add(customQuery);
        }

        private void AddFaultPropagationSubscription(FaultPropagationQuery faultQuery)
        {
            this.trackingRecordPreFilter.TrackFaultPropagationRecords = true;
            if (this.faultPropagationSubscriptions == null)
            {
                this.faultPropagationSubscriptions = new List<FaultPropagationQuery>();
            }
            this.faultPropagationSubscriptions.Add(faultQuery);
        }

        private void AddWorkflowSubscription(WorkflowInstanceQuery workflowTrackingQuery)
        {
            this.trackingRecordPreFilter.TrackWorkflowInstanceRecords = true;
            if (this.workflowEventSubscriptions == null)
            {
                this.workflowEventSubscriptions = new Dictionary<string, WorkflowInstanceQuery>();
            }
            if (workflowTrackingQuery.HasStates)
            {
                foreach (string str in workflowTrackingQuery.States)
                {
                    if (!this.workflowEventSubscriptions.ContainsKey(str))
                    {
                        this.workflowEventSubscriptions.Add(str, workflowTrackingQuery);
                    }
                }
            }
        }

        private static bool CheckSubscription(string name, string value)
        {
            if (string.CompareOrdinal(name, value) != 0)
            {
                return (string.CompareOrdinal(name, "*") == 0);
            }
            return true;
        }

        private static void ExtractArguments(ActivityStateRecord activityStateRecord, ActivityStateQuery activityStateQuery)
        {
            if (activityStateQuery.HasArguments)
            {
                activityStateRecord.Arguments = activityStateRecord.GetArguments(activityStateQuery.Arguments);
            }
            else
            {
                activityStateRecord.Arguments = ActivityUtilities.EmptyParameters;
            }
        }

        private static void ExtractVariables(ActivityStateRecord activityStateRecord, ActivityStateQuery activityStateQuery)
        {
            if (activityStateQuery.HasVariables)
            {
                activityStateRecord.Variables = activityStateRecord.GetVariables(activityStateQuery.Variables);
            }
            else
            {
                activityStateRecord.Variables = ActivityUtilities.EmptyParameters;
            }
        }

        internal static RuntimeTrackingProfile GetRuntimeTrackingProfile(TrackingProfile profile, Activity rootElement)
        {
            return Cache.GetRuntimeTrackingProfile(profile, rootElement);
        }

        internal IEnumerable<string> GetSubscribedActivityNames()
        {
            return this.activityNames;
        }

        private ActivityScheduledQuery Match(ActivityScheduledRecord activityScheduledRecord)
        {
            ActivityScheduledQuery query = null;
            if (this.activityScheduledSubscriptions != null)
            {
                for (int i = 0; i < this.activityScheduledSubscriptions.Count; i++)
                {
                    string strB = (activityScheduledRecord.Activity == null) ? null : activityScheduledRecord.Activity.Name;
                    if (string.CompareOrdinal(this.activityScheduledSubscriptions[i].ActivityName, strB) == 0)
                    {
                        if (!CheckSubscription(this.activityScheduledSubscriptions[i].ChildActivityName, activityScheduledRecord.Child.Name))
                        {
                            continue;
                        }
                        query = this.activityScheduledSubscriptions[i];
                        break;
                    }
                    if ((string.CompareOrdinal(this.activityScheduledSubscriptions[i].ActivityName, "*") == 0) && CheckSubscription(this.activityScheduledSubscriptions[i].ChildActivityName, activityScheduledRecord.Child.Name))
                    {
                        query = this.activityScheduledSubscriptions[i];
                        break;
                    }
                }
            }
            if (((query == null) || (this.associatedProfile.ImplementationVisibility != ImplementationVisibility.RootScope)) || (this.ShouldTrackActivity(activityScheduledRecord.Activity, query.ActivityName) && this.ShouldTrackActivity(activityScheduledRecord.Child, query.ChildActivityName)))
            {
                return query;
            }
            return null;
        }

        private ActivityStateQuery Match(ActivityStateRecord activityStateRecord)
        {
            ActivityStateQuery query = null;
            if (this.activitySubscriptions != null)
            {
                HybridCollection<ActivityStateQuery> hybrids;
                if (this.activitySubscriptions.TryGetValue(activityStateRecord.Activity.Name, out hybrids))
                {
                    query = MatchActivityState(activityStateRecord, hybrids.AsReadOnly());
                }
                if ((query == null) && this.activitySubscriptions.TryGetValue("*", out hybrids))
                {
                    query = MatchActivityState(activityStateRecord, hybrids.AsReadOnly());
                    if (((query != null) && (this.associatedProfile.ImplementationVisibility == ImplementationVisibility.RootScope)) && !this.ShouldTrackActivity(activityStateRecord.Activity, "*"))
                    {
                        return null;
                    }
                }
            }
            return query;
        }

        private BookmarkResumptionQuery Match(BookmarkResumptionRecord bookmarkRecord)
        {
            BookmarkResumptionQuery query = null;
            if (this.bookmarkSubscriptions != null)
            {
                if (bookmarkRecord.BookmarkName != null)
                {
                    this.bookmarkSubscriptions.TryGetValue(bookmarkRecord.BookmarkName, out query);
                }
                if (query == null)
                {
                    this.bookmarkSubscriptions.TryGetValue("*", out query);
                }
            }
            return query;
        }

        private CancelRequestedQuery Match(CancelRequestedRecord cancelRecord)
        {
            CancelRequestedQuery query = null;
            if (this.cancelRequestedSubscriptions != null)
            {
                for (int i = 0; i < this.cancelRequestedSubscriptions.Count; i++)
                {
                    string strB = (cancelRecord.Activity == null) ? null : cancelRecord.Activity.Name;
                    if (string.CompareOrdinal(this.cancelRequestedSubscriptions[i].ActivityName, strB) == 0)
                    {
                        if (!CheckSubscription(this.cancelRequestedSubscriptions[i].ChildActivityName, cancelRecord.Child.Name))
                        {
                            continue;
                        }
                        query = this.cancelRequestedSubscriptions[i];
                        break;
                    }
                    if ((string.CompareOrdinal(this.cancelRequestedSubscriptions[i].ActivityName, "*") == 0) && CheckSubscription(this.cancelRequestedSubscriptions[i].ChildActivityName, cancelRecord.Child.Name))
                    {
                        query = this.cancelRequestedSubscriptions[i];
                        break;
                    }
                }
            }
            if (((query == null) || (this.associatedProfile.ImplementationVisibility != ImplementationVisibility.RootScope)) || (this.ShouldTrackActivity(cancelRecord.Activity, query.ActivityName) && this.ShouldTrackActivity(cancelRecord.Child, query.ChildActivityName)))
            {
                return query;
            }
            return null;
        }

        private CustomTrackingQuery Match(CustomTrackingRecord customRecord)
        {
            if (this.customTrackingQuerySubscriptions != null)
            {
                for (int i = 0; i < this.customTrackingQuerySubscriptions.Count; i++)
                {
                    if (string.CompareOrdinal(this.customTrackingQuerySubscriptions[i].Name, customRecord.Name) == 0)
                    {
                        if (CheckSubscription(this.customTrackingQuerySubscriptions[i].ActivityName, customRecord.Activity.Name))
                        {
                            return this.customTrackingQuerySubscriptions[i];
                        }
                    }
                    else if ((string.CompareOrdinal(this.customTrackingQuerySubscriptions[i].Name, "*") == 0) && CheckSubscription(this.customTrackingQuerySubscriptions[i].ActivityName, customRecord.Activity.Name))
                    {
                        return this.customTrackingQuerySubscriptions[i];
                    }
                }
            }
            return null;
        }

        private FaultPropagationQuery Match(FaultPropagationRecord faultRecord)
        {
            FaultPropagationQuery query = null;
            if (this.faultPropagationSubscriptions != null)
            {
                for (int i = 0; i < this.faultPropagationSubscriptions.Count; i++)
                {
                    string str = (faultRecord.FaultHandler == null) ? null : faultRecord.FaultHandler.Name;
                    if (string.CompareOrdinal(this.faultPropagationSubscriptions[i].FaultSourceActivityName, faultRecord.FaultSource.Name) == 0)
                    {
                        if (!CheckSubscription(this.faultPropagationSubscriptions[i].FaultHandlerActivityName, str))
                        {
                            continue;
                        }
                        query = this.faultPropagationSubscriptions[i];
                        break;
                    }
                    if ((string.CompareOrdinal(this.faultPropagationSubscriptions[i].FaultSourceActivityName, "*") == 0) && CheckSubscription(this.faultPropagationSubscriptions[i].FaultHandlerActivityName, str))
                    {
                        query = this.faultPropagationSubscriptions[i];
                        break;
                    }
                }
            }
            if (((query == null) || (this.associatedProfile.ImplementationVisibility != ImplementationVisibility.RootScope)) || (this.ShouldTrackActivity(faultRecord.FaultHandler, query.FaultHandlerActivityName) && this.ShouldTrackActivity(faultRecord.FaultSource, query.FaultSourceActivityName)))
            {
                return query;
            }
            return null;
        }

        private WorkflowInstanceQuery Match(WorkflowInstanceRecord workflowRecord)
        {
            WorkflowInstanceQuery query = null;
            if ((this.workflowEventSubscriptions != null) && !this.workflowEventSubscriptions.TryGetValue(workflowRecord.State, out query))
            {
                this.workflowEventSubscriptions.TryGetValue("*", out query);
            }
            return query;
        }

        internal TrackingRecord Match(TrackingRecord record, bool shouldClone)
        {
            TrackingQuery query = null;
            if (record is WorkflowInstanceRecord)
            {
                query = this.Match((WorkflowInstanceRecord) record);
            }
            else if (record is ActivityStateRecord)
            {
                query = this.Match((ActivityStateRecord) record);
            }
            else if (record is BookmarkResumptionRecord)
            {
                query = this.Match((BookmarkResumptionRecord) record);
            }
            else if (record is CustomTrackingRecord)
            {
                query = this.Match((CustomTrackingRecord) record);
            }
            else if (record is ActivityScheduledRecord)
            {
                query = this.Match((ActivityScheduledRecord) record);
            }
            else if (record is CancelRequestedRecord)
            {
                query = this.Match((CancelRequestedRecord) record);
            }
            else if (record is FaultPropagationRecord)
            {
                query = this.Match((FaultPropagationRecord) record);
            }
            if (query != null)
            {
                return PrepareRecord(record, query, shouldClone);
            }
            return null;
        }

        private static ActivityStateQuery MatchActivityState(ActivityStateRecord activityRecord, ReadOnlyCollection<ActivityStateQuery> subscriptions)
        {
            ActivityStateQuery query = null;
            for (int i = 0; i < subscriptions.Count; i++)
            {
                if (subscriptions[i].States.Contains(activityRecord.State))
                {
                    return subscriptions[i];
                }
                if (subscriptions[i].States.Contains("*") && (query == null))
                {
                    query = subscriptions[i];
                }
            }
            return query;
        }

        private static TrackingRecord PrepareRecord(TrackingRecord record, TrackingQuery query, bool shouldClone)
        {
            TrackingRecord record2 = shouldClone ? record.Clone() : record;
            if (query.HasAnnotations)
            {
                record2.Annotations = new ReadOnlyDictionary<string, string>(query.QueryAnnotations, false);
            }
            if (query is ActivityStateQuery)
            {
                ExtractArguments((ActivityStateRecord) record2, (ActivityStateQuery) query);
                ExtractVariables((ActivityStateRecord) record2, (ActivityStateQuery) query);
            }
            return record2;
        }

        private bool ShouldTrackActivity(ActivityInfo activityInfo, string queryName)
        {
            if ((activityInfo != null) && (queryName == "*"))
            {
                if (this.isRootNativeActivity)
                {
                    if (activityInfo.Instance.Activity.MemberOf.ParentId != 0)
                    {
                        return false;
                    }
                }
                else if ((activityInfo.Instance.Activity.MemberOf.ParentId != 0) && (activityInfo.Instance.Activity.MemberOf.Parent.ParentId != 0))
                {
                    return false;
                }
            }
            return true;
        }

        private static RuntimeTrackingProfileCache Cache
        {
            get
            {
                if (profileCache == null)
                {
                    profileCache = new RuntimeTrackingProfileCache();
                }
                return profileCache;
            }
        }

        internal TrackingRecordPreFilter Filter
        {
            get
            {
                return this.trackingRecordPreFilter;
            }
        }

        private class RuntimeTrackingProfileCache
        {
            private ConditionalWeakTable<Activity, HybridCollection<RuntimeTrackingProfile>> cache = new ConditionalWeakTable<Activity, HybridCollection<RuntimeTrackingProfile>>();

            public RuntimeTrackingProfile GetRuntimeTrackingProfile(TrackingProfile profile, Activity rootElement)
            {
                RuntimeTrackingProfile item = null;
                HybridCollection<RuntimeTrackingProfile> hybrids = null;
                lock (this.cache)
                {
                    if (!this.cache.TryGetValue(rootElement, out hybrids))
                    {
                        item = new RuntimeTrackingProfile(profile, rootElement);
                        hybrids = new HybridCollection<RuntimeTrackingProfile>();
                        hybrids.Add(item);
                        this.cache.Add(rootElement, hybrids);
                        return item;
                    }
                    foreach (RuntimeTrackingProfile profile3 in hybrids.AsReadOnly())
                    {
                        if ((string.CompareOrdinal(profile.Name, profile3.associatedProfile.Name) == 0) && (string.CompareOrdinal(profile.ActivityDefinitionId, profile3.associatedProfile.ActivityDefinitionId) == 0))
                        {
                            item = profile3;
                            break;
                        }
                    }
                    if (item == null)
                    {
                        item = new RuntimeTrackingProfile(profile, rootElement);
                        hybrids.Add(item);
                    }
                }
                return item;
            }
        }
    }
}

