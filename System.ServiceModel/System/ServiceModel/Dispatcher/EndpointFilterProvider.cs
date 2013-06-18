namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class EndpointFilterProvider
    {
        private SynchronizedCollection<string> initiatingActions;
        private object mutex = new object();

        public EndpointFilterProvider(params string[] initiatingActions)
        {
            this.initiatingActions = new SynchronizedCollection<string>(this.mutex, initiatingActions);
        }

        public MessageFilter CreateFilter(out int priority)
        {
            lock (this.mutex)
            {
                priority = 1;
                if (this.initiatingActions.Count == 0)
                {
                    return new MatchNoneMessageFilter();
                }
                string[] actions = new string[this.initiatingActions.Count];
                int index = 0;
                for (int i = 0; i < this.initiatingActions.Count; i++)
                {
                    string str = this.initiatingActions[i];
                    if (str == "*")
                    {
                        priority = 0;
                        return new MatchAllMessageFilter();
                    }
                    actions[index] = str;
                    index++;
                }
                return new ActionMessageFilter(actions);
            }
        }

        public SynchronizedCollection<string> InitiatingActions
        {
            get
            {
                return this.initiatingActions;
            }
        }
    }
}

