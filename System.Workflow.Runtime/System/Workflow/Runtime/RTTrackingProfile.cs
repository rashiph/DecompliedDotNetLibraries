namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime.Tracking;

    internal class RTTrackingProfile : ICloneable
    {
        private Dictionary<string, List<ActivityTrackPointCacheItem>> _activities;
        private List<string> _activitiesIgnore;
        private Dictionary<string, List<ActivityTrackPointCacheItem>> _dynamicActivities;
        private List<string> _dynamicActivitiesIgnore;
        private Dictionary<string, List<UserTrackPoint>> _dynamicUser;
        private List<string> _dynamicUserIgnore;
        private bool _isPrivate;
        private IList<WorkflowChangeAction> _pendingChanges;
        private bool _pendingWorkflowChange;
        private TrackingProfile _profile;
        private Type _serviceType;
        private Dictionary<string, List<UserTrackPoint>> _user;
        private List<string> _userIgnore;
        private Type _workflowType;

        protected RTTrackingProfile()
        {
            this._activities = new Dictionary<string, List<ActivityTrackPointCacheItem>>();
            this._activitiesIgnore = new List<string>();
            this._user = new Dictionary<string, List<UserTrackPoint>>();
            this._userIgnore = new List<string>();
        }

        private RTTrackingProfile(RTTrackingProfile runtimeProfile)
        {
            this._activities = new Dictionary<string, List<ActivityTrackPointCacheItem>>();
            this._activitiesIgnore = new List<string>();
            this._user = new Dictionary<string, List<UserTrackPoint>>();
            this._userIgnore = new List<string>();
            this._profile = runtimeProfile._profile;
            this._isPrivate = runtimeProfile._isPrivate;
            this._pendingChanges = runtimeProfile._pendingChanges;
            this._pendingWorkflowChange = runtimeProfile._pendingWorkflowChange;
            this._workflowType = runtimeProfile._workflowType;
            this._activities = new Dictionary<string, List<ActivityTrackPointCacheItem>>(runtimeProfile._activities.Count);
            foreach (KeyValuePair<string, List<ActivityTrackPointCacheItem>> pair in runtimeProfile._activities)
            {
                this._activities.Add(pair.Key, runtimeProfile._activities[pair.Key]);
            }
            this._activitiesIgnore = new List<string>(runtimeProfile._activitiesIgnore);
            if (runtimeProfile._dynamicActivities != null)
            {
                this._dynamicActivities = new Dictionary<string, List<ActivityTrackPointCacheItem>>(runtimeProfile._dynamicActivities.Count);
                foreach (KeyValuePair<string, List<ActivityTrackPointCacheItem>> pair2 in runtimeProfile._dynamicActivities)
                {
                    this._dynamicActivities.Add(pair2.Key, runtimeProfile._dynamicActivities[pair2.Key]);
                }
            }
            if (runtimeProfile._dynamicActivitiesIgnore != null)
            {
                this._dynamicActivitiesIgnore = new List<string>(runtimeProfile._dynamicActivitiesIgnore);
            }
            this._user = new Dictionary<string, List<UserTrackPoint>>(runtimeProfile._user.Count);
            foreach (KeyValuePair<string, List<UserTrackPoint>> pair3 in runtimeProfile._user)
            {
                this._user.Add(pair3.Key, runtimeProfile._user[pair3.Key]);
            }
            this._userIgnore = new List<string>(runtimeProfile._userIgnore);
            if (runtimeProfile._dynamicUser != null)
            {
                this._dynamicUser = new Dictionary<string, List<UserTrackPoint>>(runtimeProfile._dynamicUser.Count);
                foreach (KeyValuePair<string, List<UserTrackPoint>> pair4 in runtimeProfile._dynamicUser)
                {
                    this._dynamicUser.Add(pair4.Key, pair4.Value);
                }
            }
            if (runtimeProfile._dynamicUserIgnore != null)
            {
                this._dynamicUserIgnore = new List<string>(runtimeProfile._dynamicUserIgnore);
            }
        }

        internal RTTrackingProfile(TrackingProfile profile, Activity root, Type serviceType)
        {
            this._activities = new Dictionary<string, List<ActivityTrackPointCacheItem>>();
            this._activitiesIgnore = new List<string>();
            this._user = new Dictionary<string, List<UserTrackPoint>>();
            this._userIgnore = new List<string>();
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }
            if (null == serviceType)
            {
                throw new ArgumentNullException("serviceType");
            }
            this._workflowType = root.GetType();
            this._serviceType = serviceType;
            TrackingProfileSerializer serializer = new TrackingProfileSerializer();
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            StringReader reader = null;
            TrackingProfile profile2 = null;
            try
            {
                serializer.Serialize(writer, profile);
                reader = new StringReader(writer.ToString());
                profile2 = serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (writer != null)
                {
                    writer.Close();
                }
            }
            this._profile = profile2;
            this.CheckAllActivities(root);
        }

        internal bool ActivitySubscriptionNeeded(Activity activity)
        {
            List<ActivityTrackPointCacheItem> list = null;
            if (!this._pendingWorkflowChange || (this._pendingWorkflowChange && !this.IsPendingUpdateActivity(activity, true)))
            {
                bool flag = true;
                while (flag)
                {
                    if (this._activitiesIgnore.Contains(activity.QualifiedName))
                    {
                        return false;
                    }
                    if (this._activities.TryGetValue(activity.QualifiedName, out list))
                    {
                        return true;
                    }
                    this.CheckActivity(activity);
                }
                return false;
            }
            List<UserTrackPoint> includes = null;
            if (this.CreateCacheItems(activity, out includes))
            {
                this.CacheInsertUpdatePending(activity.QualifiedName, includes);
            }
            else
            {
                this._dynamicUserIgnore.Add(activity.QualifiedName);
            }
            if (this.CreateCacheItems(activity, out list))
            {
                this.CacheInsertUpdatePending(activity.QualifiedName, list);
                return true;
            }
            this._dynamicActivitiesIgnore.Add(activity.QualifiedName);
            return false;
        }

        private void CacheInsert(string qualifiedID, List<UserTrackPoint> points)
        {
            if (this._user.ContainsKey(qualifiedID))
            {
                throw new InvalidOperationException(ExecutionStringManager.RTProfileActCacheDupKey);
            }
            foreach (UserTrackPoint point in points)
            {
                this.CacheInsert(qualifiedID, point);
            }
        }

        private void CacheInsert(string qualifiedID, ActivityTrackPointCacheItem point)
        {
            List<ActivityTrackPointCacheItem> list = null;
            if (!this._activities.TryGetValue(qualifiedID, out list))
            {
                list = new List<ActivityTrackPointCacheItem>();
                this._activities.Add(qualifiedID, list);
            }
            list.Add(point);
        }

        private void CacheInsert(string qualifiedID, UserTrackPoint point)
        {
            List<UserTrackPoint> list = null;
            if (!this._user.TryGetValue(qualifiedID, out list))
            {
                list = new List<UserTrackPoint>();
                this._user.Add(qualifiedID, list);
            }
            list.Add(point);
        }

        private void CacheInsert(string qualifiedID, List<ActivityTrackPointCacheItem> points)
        {
            if (this._activities.ContainsKey(qualifiedID))
            {
                throw new InvalidOperationException(ExecutionStringManager.RTProfileActCacheDupKey);
            }
            foreach (ActivityTrackPointCacheItem item in points)
            {
                this.CacheInsert(qualifiedID, item);
            }
        }

        private void CacheInsertUpdatePending(string qualifiedID, List<ActivityTrackPointCacheItem> points)
        {
            if (!this._isPrivate || !this._pendingWorkflowChange)
            {
                throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);
            }
            if (this._dynamicActivities == null)
            {
                throw new InvalidOperationException(ExecutionStringManager.RTProfileDynamicActCacheIsNull);
            }
            List<ActivityTrackPointCacheItem> list = null;
            if (!this._dynamicActivities.TryGetValue(qualifiedID, out list))
            {
                list = new List<ActivityTrackPointCacheItem>();
                this._dynamicActivities.Add(qualifiedID, list);
            }
            foreach (ActivityTrackPointCacheItem item in points)
            {
                list.Add(item);
            }
        }

        private void CacheInsertUpdatePending(string qualifiedID, List<UserTrackPoint> points)
        {
            if (!this._isPrivate || !this._pendingWorkflowChange)
            {
                throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);
            }
            if (this._dynamicUser == null)
            {
                throw new InvalidOperationException(ExecutionStringManager.RTProfileDynamicActCacheIsNull);
            }
            List<UserTrackPoint> list = null;
            if (!this._dynamicUser.TryGetValue(qualifiedID, out list))
            {
                list = new List<UserTrackPoint>();
                this._dynamicUser.Add(qualifiedID, list);
            }
            foreach (UserTrackPoint point in points)
            {
                list.Add(point);
            }
        }

        private void CheckActivity(Activity activity)
        {
            string qualifiedName = activity.QualifiedName;
            List<ActivityTrackPointCacheItem> includes = null;
            if (this.CreateCacheItems(activity, out includes))
            {
                this.CacheInsert(qualifiedName, includes);
            }
            else
            {
                this._activitiesIgnore.Add(qualifiedName);
            }
            List<UserTrackPoint> list2 = null;
            if (this.CreateCacheItems(activity, out list2))
            {
                this.CacheInsert(qualifiedName, list2);
            }
            else
            {
                this._userIgnore.Add(qualifiedName);
            }
        }

        private void CheckAllActivities(Activity activity)
        {
            this.CheckActivity(activity);
            if (activity is CompositeActivity)
            {
                foreach (Activity activity2 in this.GetAllEnabledActivities((CompositeActivity) activity))
                {
                    this.CheckAllActivities(activity2);
                }
            }
        }

        internal RTTrackingProfile Clone()
        {
            return new RTTrackingProfile(this);
        }

        private bool CreateCacheItems(Activity activity, out List<ActivityTrackPointCacheItem> includes)
        {
            includes = new List<ActivityTrackPointCacheItem>();
            foreach (ActivityTrackPoint point in this._profile.ActivityTrackPoints)
            {
                List<ActivityExecutionStatus> list;
                bool hasCondition = false;
                if (point.IsMatch(activity, out list, out hasCondition))
                {
                    includes.Add(new ActivityTrackPointCacheItem(point, list, hasCondition));
                }
            }
            return (includes.Count > 0);
        }

        private bool CreateCacheItems(Activity activity, out List<UserTrackPoint> includes)
        {
            includes = new List<UserTrackPoint>();
            foreach (UserTrackPoint point in this._profile.UserTrackPoints)
            {
                if (point.IsMatch(activity))
                {
                    includes.Add(point);
                }
            }
            return (includes.Count > 0);
        }

        public IList<Activity> GetAllEnabledActivities(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
            {
                throw new ArgumentNullException("compositeActivity");
            }
            List<Activity> list = new List<Activity>(compositeActivity.EnabledActivities);
            foreach (Activity activity in ((ISupportAlternateFlow) compositeActivity).AlternateFlowActivities)
            {
                if (!list.Contains(activity))
                {
                    list.Add(activity);
                }
            }
            return list;
        }

        private bool IsPendingUpdateActivity(Activity activity, bool addedOnly)
        {
            if (!this._isPrivate || !this._pendingWorkflowChange)
            {
                throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);
            }
            if ((this._pendingChanges != null) && (this._pendingChanges.Count > 0))
            {
                foreach (WorkflowChangeAction action in this._pendingChanges)
                {
                    string strB = null;
                    if (action is ActivityChangeAction)
                    {
                        if (action is AddedActivityAction)
                        {
                            strB = ((AddedActivityAction) action).AddedActivity.QualifiedName;
                        }
                        else if ((action is RemovedActivityAction) && !addedOnly)
                        {
                            strB = ((RemovedActivityAction) action).OriginalRemovedActivity.QualifiedName;
                        }
                        if ((strB != null) && (string.Compare(activity.QualifiedName, strB, StringComparison.Ordinal) == 0))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        private bool TryGetCacheItems(Activity activity, out List<ActivityTrackPointCacheItem> points)
        {
            points = null;
            if (this._pendingWorkflowChange && (!this._pendingWorkflowChange || this.IsPendingUpdateActivity(activity, true)))
            {
                return this._dynamicActivities.TryGetValue(activity.QualifiedName, out points);
            }
            return this._activities.TryGetValue(activity.QualifiedName, out points);
        }

        private bool TryGetCacheItems(Activity activity, out List<UserTrackPoint> points)
        {
            points = null;
            if (this._pendingWorkflowChange && (!this._pendingWorkflowChange || this.IsPendingUpdateActivity(activity, true)))
            {
                return this._dynamicUser.TryGetValue(activity.QualifiedName, out points);
            }
            return this._user.TryGetValue(activity.QualifiedName, out points);
        }

        internal bool TryTrackActivityEvent(Activity activity, ActivityExecutionStatus status, IServiceProvider provider, ActivityTrackingRecord record)
        {
            List<ActivityTrackPointCacheItem> list;
            if (!this.TryGetCacheItems(activity, out list))
            {
                return false;
            }
            bool flag = false;
            foreach (ActivityTrackPointCacheItem item in list)
            {
                if ((!item.HasLocationConditions || item.Point.IsMatch(activity, status)) && item.Events.Contains(status))
                {
                    flag = true;
                    item.Point.Track(activity, provider, record.Body);
                    record.Annotations.AddRange(item.Point.Annotations);
                }
            }
            return flag;
        }

        internal bool TryTrackInstanceEvent(TrackingWorkflowEvent status, WorkflowTrackingRecord record)
        {
            bool flag = false;
            foreach (WorkflowTrackPoint point in this._profile.WorkflowTrackPoints)
            {
                if (point.IsMatch(status))
                {
                    record.Annotations.AddRange(point.Annotations);
                    flag = true;
                }
            }
            return flag;
        }

        internal bool TryTrackUserEvent(Activity activity, string keyName, object argument, WorkflowExecutor exec, UserTrackingRecord record)
        {
            List<UserTrackPoint> list;
            if (!this.TryGetCacheItems(activity, out list))
            {
                return false;
            }
            bool flag = false;
            foreach (UserTrackPoint point in list)
            {
                if (point.IsMatch(activity, keyName, argument))
                {
                    flag = true;
                    point.Track(activity, argument, exec, record.Body);
                    record.Annotations.AddRange(point.Annotations);
                }
            }
            return flag;
        }

        public void WorkflowChangeBegin(IList<WorkflowChangeAction> changeActions)
        {
            if (this._pendingWorkflowChange)
            {
                throw new InvalidOperationException(ExecutionStringManager.DynamicUpdateIsNotPending);
            }
            if (!this._isPrivate)
            {
                throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);
            }
            this._dynamicActivities = new Dictionary<string, List<ActivityTrackPointCacheItem>>();
            this._dynamicActivitiesIgnore = new List<string>();
            this._dynamicUser = new Dictionary<string, List<UserTrackPoint>>();
            this._dynamicUserIgnore = new List<string>();
            this._pendingChanges = changeActions;
            this._pendingWorkflowChange = true;
        }

        public void WorkflowChangeCommit()
        {
            if (this._pendingWorkflowChange)
            {
                if (!this._isPrivate)
                {
                    throw new InvalidOperationException(ExecutionStringManager.ProfileIsNotPrivate);
                }
                if (this._pendingChanges != null)
                {
                    foreach (WorkflowChangeAction action in this._pendingChanges)
                    {
                        if (action is RemovedActivityAction)
                        {
                            string qualifiedName = ((RemovedActivityAction) action).OriginalRemovedActivity.QualifiedName;
                            this._activities.Remove(qualifiedName);
                            this._activitiesIgnore.Remove(qualifiedName);
                            this._user.Remove(qualifiedName);
                            this._userIgnore.Remove(qualifiedName);
                        }
                    }
                }
                if ((this._dynamicActivities != null) && (this._dynamicActivities.Count > 0))
                {
                    foreach (KeyValuePair<string, List<ActivityTrackPointCacheItem>> pair in this._dynamicActivities)
                    {
                        this._activities.Add(pair.Key, pair.Value);
                    }
                }
                if ((this._dynamicActivitiesIgnore != null) && (this._dynamicActivitiesIgnore.Count > 0))
                {
                    this._activitiesIgnore.AddRange(this._dynamicActivitiesIgnore);
                }
                if ((this._dynamicUser != null) && (this._dynamicUser.Count > 0))
                {
                    foreach (KeyValuePair<string, List<UserTrackPoint>> pair2 in this._dynamicUser)
                    {
                        this._user.Add(pair2.Key, pair2.Value);
                    }
                }
                if ((this._dynamicUserIgnore != null) && (this._dynamicUserIgnore.Count > 0))
                {
                    this._userIgnore.AddRange(this._dynamicUserIgnore);
                }
                this._dynamicActivities = null;
                this._dynamicActivitiesIgnore = null;
                this._dynamicUser = null;
                this._dynamicUserIgnore = null;
                this._pendingChanges = null;
                this._pendingWorkflowChange = false;
            }
        }

        public void WorkflowChangeRollback()
        {
            this._dynamicActivities = null;
            this._dynamicActivitiesIgnore = null;
            this._dynamicUser = null;
            this._dynamicUserIgnore = null;
            this._pendingChanges = null;
            this._pendingWorkflowChange = false;
        }

        internal bool IsPrivate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._isPrivate;
            }
            set
            {
                if (!value && this._isPrivate)
                {
                    throw new InvalidOperationException(ExecutionStringManager.CannotResetIsPrivate);
                }
                this._isPrivate = value;
            }
        }

        internal System.Version Version
        {
            get
            {
                return this._profile.Version;
            }
        }

        internal Type WorkflowType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._workflowType;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ActivityTrackPointCacheItem
        {
            internal ActivityTrackPoint Point;
            internal List<ActivityExecutionStatus> Events;
            internal bool HasLocationConditions;
            internal ActivityTrackPointCacheItem(ActivityTrackPoint point, List<ActivityExecutionStatus> events, bool hasConditions)
            {
                if (point == null)
                {
                    throw new ArgumentNullException("point");
                }
                if (events == null)
                {
                    throw new ArgumentNullException("events");
                }
                this.Point = point;
                this.Events = events;
                this.HasLocationConditions = hasConditions;
            }
        }
    }
}

