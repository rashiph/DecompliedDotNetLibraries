namespace System.Activities.Debugger
{
    using System;
    using System.Activities;
    using System.Collections.Generic;

    internal class InstrumentationTracker
    {
        private Activity root;
        private Dictionary<Activity, string> uninstrumentedSubRoots;

        public InstrumentationTracker(Activity root)
        {
            this.root = root;
        }

        private void CollectSubRoot(Activity activity)
        {
            string fileName = XamlDebuggerXmlReader.GetFileName(activity) as string;
            if (!string.IsNullOrEmpty(fileName))
            {
                this.uninstrumentedSubRoots.Add(activity, fileName);
            }
        }

        public List<Activity> GetSameSourceSubRoots(Activity subRoot)
        {
            string str;
            List<Activity> list = new List<Activity>();
            if (this.UninstrumentedSubRoots.TryGetValue(subRoot, out str))
            {
                foreach (KeyValuePair<Activity, string> pair in this.UninstrumentedSubRoots)
                {
                    if ((pair.Value == str) && (pair.Key != subRoot))
                    {
                        list.Add(pair.Key);
                    }
                }
            }
            return list;
        }

        private void InitializeUninstrumentedSubRoots()
        {
            this.uninstrumentedSubRoots = new Dictionary<Activity, string>();
            Queue<Activity> queue = new Queue<Activity>();
            this.CollectSubRoot(this.root);
            queue.Enqueue(this.root);
            while (queue.Count > 0)
            {
                foreach (Activity activity2 in WorkflowInspectionServices.GetActivities(queue.Dequeue()))
                {
                    this.CollectSubRoot(activity2);
                    queue.Enqueue(activity2);
                }
            }
        }

        public bool IsUninstrumentedSubRoot(Activity subRoot)
        {
            return this.UninstrumentedSubRoots.ContainsKey(subRoot);
        }

        public void MarkInstrumented(Activity subRoot)
        {
            this.UninstrumentedSubRoots.Remove(subRoot);
        }

        private Dictionary<Activity, string> UninstrumentedSubRoots
        {
            get
            {
                if (this.uninstrumentedSubRoots == null)
                {
                    this.InitializeUninstrumentedSubRoots();
                }
                return this.uninstrumentedSubRoots;
            }
        }
    }
}

