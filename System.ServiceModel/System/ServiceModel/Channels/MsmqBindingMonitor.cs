namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Messaging;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class MsmqBindingMonitor
    {
        private CommunicationState currentState;
        private static readonly TimeSpan DefaultUpdateInterval = TimeSpan.FromMinutes(10.0);
        private List<MsmqBindingFilter> filters;
        private ManualResetEvent firstRoundComplete;
        private string host;
        private int iteration;
        private Dictionary<string, MatchState> knownPrivateQueues;
        private Dictionary<string, MatchState> knownPublicQueues;
        private object thisLock;
        private IOThreadTimer timer;
        private TimeSpan updateInterval;

        public MsmqBindingMonitor(string host) : this(host, DefaultUpdateInterval)
        {
        }

        public MsmqBindingMonitor(string host, TimeSpan updateInterval)
        {
            this.filters = new List<MsmqBindingFilter>();
            this.knownPublicQueues = new Dictionary<string, MatchState>();
            this.knownPrivateQueues = new Dictionary<string, MatchState>();
            this.thisLock = new object();
            if (string.Compare(host, "localhost", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.host = ".";
            }
            else
            {
                this.host = host;
            }
            this.firstRoundComplete = new ManualResetEvent(false);
            this.updateInterval = updateInterval;
            this.iteration = 1;
        }

        public void AddFilter(MsmqBindingFilter filter)
        {
            lock (this.thisLock)
            {
                this.filters.Add(filter);
                this.MatchFilter(filter, this.knownPublicQueues.Values);
                this.MatchFilter(filter, this.knownPrivateQueues.Values);
            }
        }

        public bool ContainsFilter(MsmqBindingFilter filter)
        {
            lock (this.thisLock)
            {
                return this.filters.Contains(filter);
            }
        }

        private string ExtractQueueName(string name, bool isPrivate)
        {
            if (isPrivate)
            {
                return name.Substring(@"private$\".Length);
            }
            return name;
        }

        private void MatchFilter(MsmqBindingFilter filter, IEnumerable<MatchState> queues)
        {
            foreach (MatchState state in queues)
            {
                int num = filter.Match(state.QueueName);
                if (num > state.LastMatchLength)
                {
                    if (state.LastMatch != null)
                    {
                        state.LastMatch.MatchLost(this.host, state.QueueName, state.IsPrivate, state.CallbackState);
                    }
                    state.LastMatchLength = num;
                    state.LastMatch = filter;
                    state.CallbackState = filter.MatchFound(this.host, state.QueueName, state.IsPrivate);
                }
            }
        }

        private void MatchQueue(MatchState state)
        {
            MsmqBindingFilter lastMatch = state.LastMatch;
            int lastMatchLength = state.LastMatchLength;
            foreach (MsmqBindingFilter filter2 in this.filters)
            {
                int num2 = filter2.Match(state.QueueName);
                if (num2 > lastMatchLength)
                {
                    lastMatchLength = num2;
                    lastMatch = filter2;
                }
            }
            if (lastMatch != state.LastMatch)
            {
                if (state.LastMatch != null)
                {
                    state.LastMatch.MatchLost(this.host, state.QueueName, state.IsPrivate, state.CallbackState);
                }
                state.LastMatchLength = lastMatchLength;
                state.LastMatch = lastMatch;
                state.CallbackState = lastMatch.MatchFound(this.host, state.QueueName, state.IsPrivate);
            }
        }

        private void OnTimer(object state)
        {
            try
            {
                lock (this.thisLock)
                {
                    if (this.currentState == CommunicationState.Opened)
                    {
                        MsmqDiagnostics.ScanStarted();
                        try
                        {
                            MessageQueue[] publicQueuesByMachine = MessageQueue.GetPublicQueuesByMachine(this.host);
                            this.ProcessFoundQueues(publicQueuesByMachine, this.knownPublicQueues, false);
                        }
                        catch (MessageQueueException exception)
                        {
                            MsmqDiagnostics.CannotReadQueues(this.host, true, exception);
                        }
                        try
                        {
                            MessageQueue[] privateQueuesByMachine = MessageQueue.GetPrivateQueuesByMachine(this.host);
                            this.ProcessFoundQueues(privateQueuesByMachine, this.knownPrivateQueues, true);
                        }
                        catch (MessageQueueException exception2)
                        {
                            MsmqDiagnostics.CannotReadQueues(this.host, false, exception2);
                        }
                        this.ProcessLostQueues(this.knownPublicQueues);
                        this.ProcessLostQueues(this.knownPrivateQueues);
                        this.iteration++;
                        this.timer.Set(this.updateInterval);
                    }
                }
            }
            finally
            {
                this.firstRoundComplete.Set();
            }
        }

        public void Open()
        {
            lock (this.thisLock)
            {
                if (this.currentState != CommunicationState.Created)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CommunicationObjectCannotBeModified", new object[] { base.GetType().ToString() })));
                }
                this.currentState = CommunicationState.Opened;
                this.timer = new IOThreadTimer(new Action<object>(this.OnTimer), null, false);
                this.timer.Set(0);
            }
        }

        private void ProcessFoundQueues(MessageQueue[] queues, Dictionary<string, MatchState> knownQueues, bool isPrivate)
        {
            foreach (MessageQueue queue in queues)
            {
                MatchState state;
                string key = this.ExtractQueueName(queue.QueueName, isPrivate);
                if (!knownQueues.TryGetValue(key, out state))
                {
                    state = new MatchState(key, this.iteration, isPrivate);
                    knownQueues.Add(key, state);
                    this.MatchQueue(state);
                }
                else
                {
                    state.DiscoveryIteration = this.iteration;
                }
            }
        }

        private void ProcessLostQueues(Dictionary<string, MatchState> knownQueues)
        {
            List<MatchState> list = new List<MatchState>();
            foreach (MatchState state in knownQueues.Values)
            {
                if (state.DiscoveryIteration != this.iteration)
                {
                    list.Add(state);
                }
            }
            foreach (MatchState state2 in list)
            {
                knownQueues.Remove(state2.QueueName);
                if (state2.LastMatch != null)
                {
                    state2.LastMatch.MatchLost(this.host, state2.QueueName, state2.IsPrivate, state2.CallbackState);
                }
            }
        }

        private void RematchQueues(MsmqBindingFilter filter, IEnumerable<MatchState> queues)
        {
            foreach (MatchState state in queues)
            {
                if (state.LastMatch == filter)
                {
                    state.LastMatch.MatchLost(this.host, state.QueueName, state.IsPrivate, state.CallbackState);
                    state.LastMatch = null;
                    state.LastMatchLength = -1;
                    this.MatchQueue(state);
                }
            }
        }

        public void RemoveFilter(MsmqBindingFilter filter)
        {
            lock (this.thisLock)
            {
                this.filters.Remove(filter);
                this.RematchQueues(filter, this.knownPublicQueues.Values);
                this.RematchQueues(filter, this.knownPrivateQueues.Values);
            }
        }

        public void WaitForFirstRoundComplete()
        {
            this.firstRoundComplete.WaitOne();
        }

        private class MatchState
        {
            private object callbackState;
            private bool isPrivate;
            private int iteration;
            private MsmqBindingFilter lastMatch;
            private int lastMatchLength;
            private string name;

            public MatchState(string name, int iteration, bool isPrivate)
            {
                this.name = name;
                this.iteration = iteration;
                this.isPrivate = isPrivate;
                this.lastMatchLength = -1;
            }

            public object CallbackState
            {
                get
                {
                    return this.callbackState;
                }
                set
                {
                    this.callbackState = value;
                }
            }

            public int DiscoveryIteration
            {
                get
                {
                    return this.iteration;
                }
                set
                {
                    this.iteration = value;
                }
            }

            public bool IsPrivate
            {
                get
                {
                    return this.isPrivate;
                }
            }

            public MsmqBindingFilter LastMatch
            {
                get
                {
                    return this.lastMatch;
                }
                set
                {
                    this.lastMatch = value;
                }
            }

            public int LastMatchLength
            {
                get
                {
                    return this.lastMatchLength;
                }
                set
                {
                    this.lastMatchLength = value;
                }
            }

            public string QueueName
            {
                get
                {
                    return this.name;
                }
            }
        }
    }
}

