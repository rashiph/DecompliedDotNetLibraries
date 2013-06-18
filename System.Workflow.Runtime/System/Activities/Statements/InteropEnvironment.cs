namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Transactions;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Tracking;

    internal class InteropEnvironment : IDisposable, IServiceProvider
    {
        private BookmarkCallback bookmarkCallback;
        private bool canceled;
        private bool completed;
        private bool disposed;
        private static readonly ReadOnlyCollection<IComparable> emptyList = new ReadOnlyCollection<IComparable>(new IComparable[0]);
        private InteropExecutor executor;
        private static MethodInfo getServiceMethod = typeof(NativeActivityContext).GetMethod("GetExtension");
        private IEnumerable<IComparable> initialBookmarks;
        private NativeActivityContext nativeActivityContext;
        private Transaction transaction;
        private Exception uncaughtException;

        public InteropEnvironment(InteropExecutor interopExecutor, NativeActivityContext nativeActivityContext, BookmarkCallback bookmarkCallback, Interop activity, Transaction transaction)
        {
            this.executor = interopExecutor;
            this.nativeActivityContext = nativeActivityContext;
            this.Activity = activity;
            this.executor.ServiceProvider = this;
            this.bookmarkCallback = bookmarkCallback;
            this.transaction = transaction;
            this.OnEnter();
        }

        public void AddResourceManager(VolatileResourceManager resourceManager)
        {
            this.Activity.AddResourceManager(this.nativeActivityContext, resourceManager);
        }

        public void Cancel()
        {
            try
            {
                this.ProcessExecutionStatus(this.executor.Cancel());
                this.canceled = true;
            }
            catch (Exception exception)
            {
                this.uncaughtException = exception;
                throw;
            }
        }

        public void CommitTransaction()
        {
            this.Activity.CommitTransaction(this.nativeActivityContext);
        }

        public void CreateTransaction(TransactionOptions transactionOptions)
        {
            this.Activity.CreateTransaction(this.nativeActivityContext, transactionOptions);
        }

        public void EnqueueEvent(IComparable queueName, object item)
        {
            try
            {
                this.ProcessExecutionStatus(this.executor.EnqueueEvent(queueName, item));
            }
            catch (Exception exception)
            {
                this.uncaughtException = exception;
                throw;
            }
        }

        public void Execute(System.Workflow.ComponentModel.Activity definition, NativeActivityContext context)
        {
            try
            {
                this.executor.Initialize(definition, this.Activity.GetInputArgumentValues(context), this.Activity.HasNameCollision);
                this.ProcessExecutionStatus(this.executor.Execute());
            }
            catch (Exception exception)
            {
                this.uncaughtException = exception;
                throw;
            }
        }

        private void OnEnter()
        {
            this.initialBookmarks = this.executor.Queues;
            this.executor.SetAmbientTransactionAndServiceEnvironment(this.transaction);
        }

        private void OnExit()
        {
            if ((this.uncaughtException == null) || !WorkflowExecutor.IsIrrecoverableException(this.uncaughtException))
            {
                this.executor.ClearAmbientTransactionAndServiceEnvironment();
                IEnumerable<IComparable> queues = this.executor.Queues;
                if (this.completed || (this.uncaughtException != null))
                {
                    this.Activity.OnClose(this.nativeActivityContext, this.uncaughtException);
                    this.Activity.SetOutputArgumentValues(this.executor.Outputs, this.nativeActivityContext);
                    this.nativeActivityContext.RemoveAllBookmarks();
                    this.executor.BookmarkQueueMap.Clear();
                    if (this.canceled)
                    {
                        this.nativeActivityContext.MarkCanceled();
                    }
                }
                else
                {
                    IList<IComparable> list = new List<IComparable>();
                    foreach (IComparable comparable in this.initialBookmarks)
                    {
                        list.Add(comparable);
                    }
                    IList<IComparable> list2 = null;
                    foreach (IComparable comparable2 in queues)
                    {
                        if (!list.Remove(comparable2))
                        {
                            if (list2 == null)
                            {
                                list2 = new List<IComparable>();
                            }
                            list2.Add(comparable2);
                        }
                    }
                    if (list2 != null)
                    {
                        foreach (IComparable comparable3 in list2)
                        {
                            Bookmark key = this.nativeActivityContext.CreateBookmark(comparable3.ToString(), this.bookmarkCallback, BookmarkOptions.MultipleResume);
                            this.executor.BookmarkQueueMap.Add(key, comparable3);
                        }
                    }
                    foreach (IComparable comparable4 in list)
                    {
                        this.nativeActivityContext.RemoveBookmark(comparable4.ToString());
                        List<Bookmark> list3 = new List<Bookmark>();
                        foreach (KeyValuePair<Bookmark, IComparable> pair in this.executor.BookmarkQueueMap)
                        {
                            if (pair.Value == comparable4)
                            {
                                list3.Add(pair.Key);
                            }
                        }
                        foreach (Bookmark bookmark2 in list3)
                        {
                            this.executor.BookmarkQueueMap.Remove(bookmark2);
                        }
                    }
                }
            }
        }

        public void Persist()
        {
            this.Activity.Persist(this.nativeActivityContext);
        }

        private void ProcessExecutionStatus(ActivityExecutionStatus executionStatus)
        {
            this.completed = executionStatus == ActivityExecutionStatus.Closed;
        }

        public void Resume()
        {
            try
            {
                this.ProcessExecutionStatus(this.executor.Resume());
            }
            catch (Exception exception)
            {
                this.uncaughtException = exception;
                throw;
            }
        }

        void IDisposable.Dispose()
        {
            if (!this.disposed)
            {
                this.OnExit();
                this.disposed = true;
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return getServiceMethod.MakeGenericMethod(new Type[] { serviceType }).Invoke(this.nativeActivityContext, null);
        }

        public void TrackActivityStatusChange(System.Workflow.ComponentModel.Activity activity, int eventCounter)
        {
            this.nativeActivityContext.Track(new InteropTrackingRecord(this.Activity.DisplayName, new ActivityTrackingRecord(activity.GetType(), activity.QualifiedName, activity.ContextGuid, (activity.Parent == null) ? Guid.Empty : activity.Parent.ContextGuid, activity.ExecutionStatus, DateTime.UtcNow, eventCounter, null)));
        }

        public void TrackData(System.Workflow.ComponentModel.Activity activity, int eventCounter, string key, object data)
        {
            this.nativeActivityContext.Track(new InteropTrackingRecord(this.Activity.DisplayName, new UserTrackingRecord(activity.GetType(), activity.QualifiedName, activity.ContextGuid, (activity.Parent == null) ? Guid.Empty : activity.Parent.ContextGuid, DateTime.UtcNow, eventCounter, key, data)));
        }

        public Interop Activity
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<Activity>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<Activity>k__BackingField = value;
            }
        }

        public static class ParameterHelper
        {
            private static readonly Type activityConditionType = typeof(ActivityCondition);
            internal const string activityNameMetaProperty = "Name";
            private static readonly Type activityType = typeof(System.Workflow.ComponentModel.Activity);
            private static readonly Type compositeActivityType = typeof(CompositeActivity);
            private static readonly Type dependencyObjectType = typeof(DependencyObject);
            internal const string interopPropertyActivityMetaProperties = "ActivityMetaProperties";
            internal const string interopPropertyActivityProperties = "ActivityProperties";
            internal const string interopPropertyActivityType = "ActivityType";

            public static bool HasPropertyNameCollision(IList<PropertyInfo> properties)
            {
                HashSet<string> set = new HashSet<string>();
                foreach (PropertyInfo info in properties)
                {
                    set.Add(info.Name);
                }
                if ((set.Contains("ActivityType") || set.Contains("ActivityProperties")) || set.Contains("ActivityMetaProperties"))
                {
                    return true;
                }
                foreach (PropertyInfo info2 in properties)
                {
                    if (set.Contains(info2.Name + "Out"))
                    {
                        return true;
                    }
                }
                return false;
            }

            public static bool IsBindable(PropertyInfo propertyInfo)
            {
                bool flag;
                if (!IsBindableOrMetaProperty(propertyInfo, out flag))
                {
                    return false;
                }
                return !flag;
            }

            public static bool IsBindableOrMetaProperty(PropertyInfo propertyInfo, out bool isMetaProperty)
            {
                isMetaProperty = false;
                if (propertyInfo.DeclaringType.Equals(compositeActivityType) || propertyInfo.DeclaringType.Equals(dependencyObjectType))
                {
                    return false;
                }
                if (propertyInfo.DeclaringType.Equals(activityType) && !string.Equals(propertyInfo.Name, "Name", StringComparison.Ordinal))
                {
                    return false;
                }
                if (activityConditionType.IsAssignableFrom(propertyInfo.PropertyType))
                {
                    return false;
                }
                DependencyProperty property = DependencyProperty.FromName(propertyInfo.Name, propertyInfo.DeclaringType);
                if ((property != null) && property.DefaultMetadata.IsMetaProperty)
                {
                    isMetaProperty = true;
                }
                return true;
            }
        }
    }
}

