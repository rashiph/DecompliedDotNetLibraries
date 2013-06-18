namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime.Tracking;

    internal class TrackingProfileManager
    {
        private object _cacheLock = new object();
        private Dictionary<Type, Dictionary<Type, ProfileList>> _cacheLookup;
        private bool _init;
        private WorkflowRuntime _runtime;
        private List<TrackingService> _services;

        internal TrackingProfileManager()
        {
        }

        private bool AddToCache(RTTrackingProfile profile, Type serviceType)
        {
            return this.AddToCache(profile, serviceType, false);
        }

        private bool AddToCache(RTTrackingProfile profile, Type serviceType, bool resetNoProfiles)
        {
            if (null == serviceType)
            {
                return false;
            }
            lock (this._cacheLock)
            {
                Dictionary<Type, ProfileList> dictionary = null;
                if (!this._cacheLookup.TryGetValue(serviceType, out dictionary))
                {
                    dictionary = new Dictionary<Type, ProfileList>();
                    this._cacheLookup.Add(serviceType, dictionary);
                }
                ProfileList list = null;
                if (!dictionary.TryGetValue(profile.WorkflowType, out list))
                {
                    list = new ProfileList();
                    dictionary.Add(profile.WorkflowType, list);
                }
                if (resetNoProfiles)
                {
                    list.NoProfile = false;
                }
                return list.Profiles.TryAdd(new CacheItem(profile));
            }
        }

        public static void ClearCache()
        {
            WorkflowRuntime.ClearTrackingProfileCache();
        }

        internal void ClearCacheImpl()
        {
            lock (this._cacheLock)
            {
                this._cacheLookup = new Dictionary<Type, Dictionary<Type, ProfileList>>();
            }
        }

        private RTTrackingProfile CreateProfile(TrackingProfile profile, Type workflowType, Type serviceType)
        {
            return new RTTrackingProfile(profile, this._runtime.GetWorkflowDefinition(workflowType), serviceType);
        }

        private RTTrackingProfile CreateProfile(TrackingProfile profile, Activity schedule, Type serviceType)
        {
            return new RTTrackingProfile(profile, schedule, serviceType);
        }

        internal RTTrackingProfile GetProfile(TrackingService service, Activity schedule)
        {
            if (!this._init)
            {
                throw new ApplicationException(ExecutionStringManager.TrackingProfileManagerNotInitialized);
            }
            if ((service == null) || (schedule == null))
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NullParameters);
                return null;
            }
            Type workflowType = schedule.GetType();
            RTTrackingProfile profile = null;
            if ((service is IProfileNotification) && this.TryGetFromCache(service.GetType(), workflowType, out profile))
            {
                return profile;
            }
            TrackingProfile profile2 = null;
            if (!service.TryGetProfile(workflowType, out profile2))
            {
                this.RemoveProfile(workflowType, service.GetType());
                return null;
            }
            if (this.TryGetFromCache(service.GetType(), workflowType, profile2.Version, out profile))
            {
                return profile;
            }
            string str = schedule.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
            if ((str != null) && (str.Length > 0))
            {
                return this.CreateProfile(profile2, schedule, service.GetType());
            }
            profile = this.CreateProfile(profile2, workflowType, service.GetType());
            lock (this._cacheLock)
            {
                RTTrackingProfile profile3 = null;
                if (this.TryGetFromCache(service.GetType(), workflowType, profile2.Version, out profile3))
                {
                    return profile3;
                }
                if (!this.AddToCache(profile, service.GetType()))
                {
                    throw new ApplicationException(ExecutionStringManager.ProfileCacheInsertFailure);
                }
                return profile;
            }
        }

        internal RTTrackingProfile GetProfile(TrackingService service, Activity workflow, Guid instanceId)
        {
            TrackingProfile profile = service.GetProfile(instanceId);
            if (profile == null)
            {
                return null;
            }
            return new RTTrackingProfile(profile, workflow, service.GetType());
        }

        internal RTTrackingProfile GetProfile(TrackingService service, Activity workflow, Version versionId)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }
            if (!this._init)
            {
                throw new InvalidOperationException(ExecutionStringManager.TrackingProfileManagerNotInitialized);
            }
            Type workflowType = workflow.GetType();
            RTTrackingProfile profile = null;
            if (this.TryGetFromCache(service.GetType(), workflowType, versionId, out profile))
            {
                return profile;
            }
            TrackingProfile profile2 = service.GetProfile(workflowType, versionId);
            string str = workflow.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
            if ((str != null) && (str.Length > 0))
            {
                return this.CreateProfile(profile2, workflow, service.GetType());
            }
            profile = this.CreateProfile(profile2, workflowType, service.GetType());
            lock (this._cacheLock)
            {
                RTTrackingProfile profile3 = null;
                if (this.TryGetFromCache(service.GetType(), workflowType, versionId, out profile3))
                {
                    return profile3;
                }
                if (!this.AddToCache(profile, service.GetType()))
                {
                    throw new ApplicationException(ExecutionStringManager.ProfileCacheInsertFailure);
                }
                return profile;
            }
        }

        internal void Initialize(WorkflowRuntime runtime)
        {
            lock (this._cacheLock)
            {
                if (runtime == null)
                {
                    throw new ArgumentException(ExecutionStringManager.NullEngine);
                }
                this._runtime = runtime;
                this._cacheLookup = new Dictionary<Type, Dictionary<Type, ProfileList>>();
                if (runtime.TrackingServices != null)
                {
                    this._services = runtime.TrackingServices;
                    foreach (TrackingService service in this._services)
                    {
                        if (service is IProfileNotification)
                        {
                            ((IProfileNotification) service).ProfileUpdated += new EventHandler<ProfileUpdatedEventArgs>(this.ProfileUpdated);
                            ((IProfileNotification) service).ProfileRemoved += new EventHandler<ProfileRemovedEventArgs>(this.ProfileRemoved);
                        }
                    }
                }
                this._init = true;
            }
        }

        private void ProfileRemoved(object sender, ProfileRemovedEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (null == e.WorkflowType)
            {
                throw new ArgumentNullException("e");
            }
            this.RemoveProfile(e.WorkflowType, sender.GetType());
        }

        private void ProfileUpdated(object sender, ProfileUpdatedEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            Type serviceType = sender.GetType();
            if (null == e.WorkflowType)
            {
                throw new ArgumentNullException("e");
            }
            if (e.TrackingProfile == null)
            {
                this.RemoveProfile(e.WorkflowType, serviceType);
            }
            else
            {
                RTTrackingProfile profile = this.CreateProfile(e.TrackingProfile, e.WorkflowType, serviceType);
                this.AddToCache(profile, serviceType, true);
            }
        }

        private void RemoveProfile(Type workflowType, Type serviceType)
        {
            lock (this._cacheLock)
            {
                Dictionary<Type, ProfileList> dictionary = null;
                if (!this._cacheLookup.TryGetValue(serviceType, out dictionary))
                {
                    dictionary = new Dictionary<Type, ProfileList>();
                    this._cacheLookup.Add(serviceType, dictionary);
                }
                ProfileList list = null;
                if (!dictionary.TryGetValue(workflowType, out list))
                {
                    list = new ProfileList();
                    dictionary.Add(workflowType, list);
                }
                list.NoProfile = true;
            }
        }

        private bool TryGetFromCache(Type serviceType, Type workflowType, out RTTrackingProfile profile)
        {
            return this.TryGetFromCache(serviceType, workflowType, new Version(0, 0), out profile);
        }

        private bool TryGetFromCache(Type serviceType, Type workflowType, Version versionId, out RTTrackingProfile profile)
        {
            profile = null;
            CacheItem item = null;
            lock (this._cacheLock)
            {
                Dictionary<Type, ProfileList> dictionary = null;
                if (this._cacheLookup.TryGetValue(serviceType, out dictionary))
                {
                    ProfileList list = null;
                    if (!dictionary.TryGetValue(workflowType, out list))
                    {
                        return false;
                    }
                    if (versionId.Major == 0)
                    {
                        if (!list.NoProfile)
                        {
                            if ((list.Profiles == null) || (list.Profiles.Count == 0))
                            {
                                return false;
                            }
                            int num = list.Profiles.Count - 1;
                            if (list.Profiles[num] == null)
                            {
                                return false;
                            }
                            profile = list.Profiles[num].TrackingProfile;
                        }
                        return true;
                    }
                    if (((list.Profiles != null) && (list.Profiles.Count != 0)) && list.Profiles.TryGetValue(new CacheItem(workflowType, versionId), out item))
                    {
                        profile = item.TrackingProfile;
                        return true;
                    }
                }
                return false;
            }
        }

        internal void Uninitialize()
        {
            lock (this._cacheLock)
            {
                if (this._runtime != null)
                {
                    foreach (TrackingService service in this._services)
                    {
                        if (service is IProfileNotification)
                        {
                            ((IProfileNotification) service).ProfileUpdated -= new EventHandler<ProfileUpdatedEventArgs>(this.ProfileUpdated);
                            ((IProfileNotification) service).ProfileRemoved -= new EventHandler<ProfileRemovedEventArgs>(this.ProfileRemoved);
                        }
                    }
                }
                this._runtime = null;
                this._services = null;
                this._init = false;
            }
        }

        private class CacheItem : IComparable
        {
            internal DateTime LastAccess;
            internal Type ScheduleType;
            internal RTTrackingProfile TrackingProfile;
            internal Version VersionId;

            internal CacheItem()
            {
                this.LastAccess = DateTime.UtcNow;
                this.VersionId = new Version(0, 0);
            }

            internal CacheItem(RTTrackingProfile profile)
            {
                this.LastAccess = DateTime.UtcNow;
                this.VersionId = new Version(0, 0);
                if (profile == null)
                {
                    throw new ArgumentNullException("profile");
                }
                this.ScheduleType = profile.WorkflowType;
                this.TrackingProfile = profile;
                this.VersionId = profile.Version;
            }

            internal CacheItem(Type workflowType, Version versionId)
            {
                this.LastAccess = DateTime.UtcNow;
                this.VersionId = new Version(0, 0);
                this.VersionId = versionId;
                this.ScheduleType = workflowType;
            }

            public int CompareTo(object obj)
            {
                if (!(obj is TrackingProfileManager.CacheItem))
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidCacheItem);
                }
                TrackingProfileManager.CacheItem item = (TrackingProfileManager.CacheItem) obj;
                if ((this.VersionId == item.VersionId) && (this.ScheduleType == item.ScheduleType))
                {
                    return 0;
                }
                if (this.VersionId <= item.VersionId)
                {
                    return -1;
                }
                return 1;
            }
        }

        private class ProfileList
        {
            internal bool NoProfile;
            internal Set<TrackingProfileManager.CacheItem> Profiles = new Set<TrackingProfileManager.CacheItem>(5);
        }
    }
}

