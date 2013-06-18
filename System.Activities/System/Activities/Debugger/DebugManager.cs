namespace System.Activities.Debugger
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Activities.Runtime;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;

    [DebuggerNonUserCode]
    internal class DebugManager
    {
        private WorkflowInstance host;
        private InstrumentationTracker instrumentationTracker;
        private Dictionary<int, Stack<Activity>> runningThreads;
        private StateManager stateManager;
        private Dictionary<object, System.Activities.Debugger.State> states;
        private List<string> temporaryFiles;

        public DebugManager(Activity root, string moduleNamePrefix, string typeNamePrefix, string auxiliaryThreadName, bool breakOnStartup, WorkflowInstance host, bool debugStartedAtRoot)
        {
            StateManager.Properties properties = new StateManager.Properties {
                ModuleNamePrefix = moduleNamePrefix,
                TypeNamePrefix = typeNamePrefix,
                AuxiliaryThreadName = auxiliaryThreadName,
                BreakOnStartup = breakOnStartup
            };
            this.stateManager = new StateManager(properties, debugStartedAtRoot);
            this.states = new Dictionary<object, System.Activities.Debugger.State>();
            this.runningThreads = new Dictionary<int, Stack<Activity>>();
            this.instrumentationTracker = new InstrumentationTracker(root);
            this.host = host;
        }

        private int CreateLogicalThread(Activity activity, System.Activities.ActivityInstance instance, bool primeCurrentInstance)
        {
            Stack<System.Activities.ActivityInstance> ancestors = null;
            if (!this.DebugStartedAtRoot)
            {
                ancestors = new Stack<System.Activities.ActivityInstance>();
                if (((activity != instance.Activity) || primeCurrentInstance) && (primeCurrentInstance || !IsParallelActivity(instance.Activity)))
                {
                    ancestors.Push(instance);
                }
                System.Activities.ActivityInstance parent = instance.Parent;
                while ((parent != null) && !IsParallelActivity(parent.Activity))
                {
                    ancestors.Push(parent);
                    parent = parent.Parent;
                }
                if (((parent != null) && IsParallelActivity(parent.Activity)) && (this.GetExecutingThreadId(parent.Activity, false) < 0))
                {
                    int num = this.CreateLogicalThread(parent.Activity, parent, true);
                }
            }
            string threadName = string.Empty;
            if (activity.Parent != null)
            {
                threadName = "DebuggerThread:" + activity.Parent.DisplayName;
            }
            int key = this.stateManager.CreateLogicalThread(threadName);
            Stack<Activity> stack2 = new Stack<Activity>();
            this.runningThreads.Add(key, stack2);
            if (!this.DebugStartedAtRoot && (ancestors != null))
            {
                this.PrimeCallStack(key, ancestors);
            }
            return key;
        }

        private bool EnsureInstrumented(Activity activity)
        {
            if (this.states.ContainsKey(activity))
            {
                return true;
            }
            if (this.instrumentationTracker.IsUninstrumentedSubRoot(activity))
            {
                this.Instrument(activity);
                return this.states.ContainsKey(activity);
            }
            return false;
        }

        private void EnterState(int threadId, Activity activity, Dictionary<string, object> locals)
        {
            System.Activities.Debugger.State state;
            this.Push(threadId, activity);
            if (this.states.TryGetValue(activity, out state))
            {
                this.stateManager.EnterState(threadId, state, locals);
            }
        }

        public void Exit()
        {
            if (this.temporaryFiles != null)
            {
                foreach (string str in this.temporaryFiles)
                {
                    try
                    {
                        File.Delete(str);
                    }
                    catch (IOException)
                    {
                    }
                    this.temporaryFiles = null;
                }
            }
            this.stateManager.Dispose();
            this.stateManager = null;
        }

        private static Dictionary<string, object> GenerateLocals(System.Activities.ActivityInstance instance)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("debugInfo", new DebugInfo(instance));
            return dictionary;
        }

        private int GetExecutingThreadId(Activity activity, bool strict)
        {
            int key = -1;
            foreach (KeyValuePair<int, Stack<Activity>> pair in this.runningThreads)
            {
                if (pair.Value.Peek() == activity)
                {
                    key = pair.Key;
                    break;
                }
            }
            if ((key < 0) && !strict)
            {
                foreach (KeyValuePair<int, Stack<Activity>> pair2 in this.runningThreads)
                {
                    Stack<Activity> stack2 = pair2.Value;
                    if (!IsParallelActivity(stack2.Peek()) && IsAncestorOf(stack2.Peek(), activity))
                    {
                        return pair2.Key;
                    }
                }
            }
            return key;
        }

        private int GetOrCreateThreadId(Activity activity, System.Activities.ActivityInstance instance)
        {
            int executingThreadId = -1;
            if ((activity.Parent != null) && !IsParallelActivity(activity.Parent))
            {
                executingThreadId = this.GetExecutingThreadId(activity.Parent, false);
            }
            if (executingThreadId < 0)
            {
                executingThreadId = this.CreateLogicalThread(activity, instance, false);
            }
            return executingThreadId;
        }

        internal void Instrument(Activity activity)
        {
            bool isTemporaryFile = false;
            string sourcePath = null;
            bool flag2 = false;
            try
            {
                Dictionary<object, SourceLocation> sourceLocations = SourceLocationProvider.GetSourceLocations(activity, out sourcePath, out isTemporaryFile);
                this.Instrument(activity, sourceLocations, Path.GetFileNameWithoutExtension(sourcePath));
            }
            catch (Exception exception)
            {
                flag2 = true;
                Debugger.Log(1, "Workflow", System.Activities.SR.DebugInstrumentationFailed(exception.Message));
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
            }
            List<Activity> sameSourceSubRoots = this.instrumentationTracker.GetSameSourceSubRoots(activity);
            this.instrumentationTracker.MarkInstrumented(activity);
            foreach (Activity activity2 in sameSourceSubRoots)
            {
                if (!flag2)
                {
                    this.MapInstrumentationStates(activity, activity2);
                }
                this.instrumentationTracker.MarkInstrumented(activity2);
            }
            if (isTemporaryFile)
            {
                if (this.temporaryFiles == null)
                {
                    this.temporaryFiles = new List<string>();
                }
                this.temporaryFiles.Add(sourcePath);
            }
        }

        private void Instrument(Activity activity, SourceLocation sourceLocation, string name)
        {
            if (this.states.ContainsKey(activity))
            {
                Debugger.Log(1, "Workflow", System.Activities.SR.DuplicateInstrumentation(activity.DisplayName));
            }
            else
            {
                System.Activities.Debugger.State state = this.stateManager.DefineStateWithDebugInfo(sourceLocation, name);
                this.states.Add(activity, state);
            }
        }

        public void Instrument(Activity rootActivity, Dictionary<object, SourceLocation> sourceLocations, string typeNamePrefix)
        {
            Queue<KeyValuePair<Activity, string>> queue = new Queue<KeyValuePair<Activity, string>>();
            Activity key = rootActivity;
            KeyValuePair<Activity, string> item = new KeyValuePair<Activity, string>(key, string.Empty);
            queue.Enqueue(item);
            HashSet<string> set = new HashSet<string>();
            HashSet<Activity> set2 = new HashSet<Activity>();
            while (queue.Count > 0)
            {
                string str;
                SourceLocation location;
                item = queue.Dequeue();
                key = item.Key;
                string str2 = item.Value;
                string displayName = key.DisplayName;
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = key.GetType().Name;
                }
                if (str2 == string.Empty)
                {
                    str = displayName;
                }
                else
                {
                    str = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { str2, displayName });
                }
                int num = 0;
                while (set.Contains(str))
                {
                    num++;
                    str = string.Format(CultureInfo.InvariantCulture, "{0}.{1}{2}", new object[] { str2, displayName, num.ToString(CultureInfo.InvariantCulture) });
                }
                set.Add(str);
                set2.Add(key);
                if (sourceLocations.TryGetValue(key, out location))
                {
                    this.Instrument(key, location, str);
                }
                foreach (Activity activity2 in WorkflowInspectionServices.GetActivities(key))
                {
                    if (!set2.Contains(activity2))
                    {
                        queue.Enqueue(new KeyValuePair<Activity, string>(activity2, str));
                    }
                }
            }
            this.stateManager.Bake(typeNamePrefix);
        }

        private static bool IsAncestorOf(Activity ancestorActivity, Activity activity)
        {
            activity = activity.Parent;
            while (((activity != null) && (activity != ancestorActivity)) && !IsParallelActivity(activity))
            {
                activity = activity.Parent;
            }
            return (activity == ancestorActivity);
        }

        private static bool IsParallelActivity(Activity activity)
        {
            return ((activity is Parallel) || (activity.GetType().IsGenericType && (activity.GetType().GetGenericTypeDefinition() == typeof(ParallelForEach<>))));
        }

        private void LeaveState(Activity activity)
        {
            int executingThreadId = this.GetExecutingThreadId(activity, true);
            if (executingThreadId >= 0)
            {
                System.Activities.Debugger.State state;
                if (this.states.TryGetValue(activity, out state))
                {
                    this.stateManager.LeaveState(executingThreadId, state);
                }
                this.Pop(executingThreadId);
            }
        }

        private void MapInstrumentationStates(Activity rootActivity1, Activity rootActivity2)
        {
            Queue<KeyValuePair<Activity, Activity>> queue = new Queue<KeyValuePair<Activity, Activity>>();
            queue.Enqueue(new KeyValuePair<Activity, Activity>(rootActivity1, rootActivity2));
            HashSet<Activity> set = new HashSet<Activity>();
            while (queue.Count > 0)
            {
                System.Activities.Debugger.State state;
                KeyValuePair<Activity, Activity> pair = queue.Dequeue();
                Activity key = pair.Key;
                Activity activity2 = pair.Value;
                if (this.states.TryGetValue(key, out state))
                {
                    if (this.states.ContainsKey(activity2))
                    {
                        Debugger.Log(1, "Workflow", System.Activities.SR.DuplicateInstrumentation(activity2.DisplayName));
                    }
                    else
                    {
                        this.states.Add(activity2, state);
                    }
                }
                set.Add(key);
                IEnumerator<Activity> enumerator = WorkflowInspectionServices.GetActivities(key).GetEnumerator();
                IEnumerator<Activity> enumerator2 = WorkflowInspectionServices.GetActivities(activity2).GetEnumerator();
                bool flag = enumerator.MoveNext();
                bool flag2 = enumerator2.MoveNext();
                while (flag && flag2)
                {
                    if (!set.Contains(enumerator.Current))
                    {
                        if (enumerator.Current.GetType() != enumerator2.Current.GetType())
                        {
                            Debugger.Log(2, "Workflow", "Unmatched type: " + enumerator.Current.GetType().FullName + " vs " + enumerator2.Current.GetType().FullName + "\n");
                        }
                        queue.Enqueue(new KeyValuePair<Activity, Activity>(enumerator.Current, enumerator2.Current));
                    }
                    flag = enumerator.MoveNext();
                    flag2 = enumerator2.MoveNext();
                }
                if (flag || flag2)
                {
                    Debugger.Log(2, "Workflow", "Unmatched number of children\n");
                }
            }
        }

        public void OnEnterState(System.Activities.ActivityInstance instance)
        {
            Activity activity = instance.Activity;
            if (this.EnsureInstrumented(activity))
            {
                this.EnterState(this.GetOrCreateThreadId(activity, instance), activity, GenerateLocals(instance));
            }
        }

        public void OnEnterState(Activity expression, System.Activities.ActivityInstance instance, LocationEnvironment environment)
        {
            if (this.EnsureInstrumented(expression))
            {
                this.EnterState(this.GetOrCreateThreadId(expression, instance), expression, GenerateLocals(instance));
            }
        }

        public void OnLeaveState(System.Activities.ActivityInstance activityInstance)
        {
            if (this.EnsureInstrumented(activityInstance.Activity))
            {
                this.LeaveState(activityInstance.Activity);
            }
        }

        private void Pop(int threadId)
        {
            Stack<Activity> stack = this.runningThreads[threadId];
            stack.Pop();
            if (stack.Count == 0)
            {
                this.stateManager.Exit(threadId);
                this.runningThreads.Remove(threadId);
            }
        }

        private void PrimeCallStack(int threadId, Stack<System.Activities.ActivityInstance> ancestors)
        {
            bool isPriming = this.stateManager.IsPriming;
            this.stateManager.IsPriming = true;
            while (ancestors.Count > 0)
            {
                System.Activities.ActivityInstance instance = ancestors.Pop();
                if (this.EnsureInstrumented(instance.Activity))
                {
                    this.EnterState(threadId, instance.Activity, GenerateLocals(instance));
                }
            }
            this.stateManager.IsPriming = isPriming;
        }

        private void Push(int threadId, Activity activity)
        {
            this.runningThreads[threadId].Push(activity);
        }

        private bool DebugStartedAtRoot
        {
            get
            {
                return this.stateManager.DebugStartedAtRoot;
            }
        }

        public bool IsPriming
        {
            set
            {
                this.stateManager.IsPriming = value;
            }
        }
    }
}

