namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class StateMachineEventManager
    {
        [DataMember(EmitDefaultValue=false)]
        private Collection<Bookmark> activeBookmarks = new Collection<Bookmark>();
        private const int MaxQueueLength = 0x2000000;
        [DataMember(EmitDefaultValue=false)]
        private Queue<TriggerCompletedEvent> queue = new Queue<TriggerCompletedEvent>();

        public void AddActiveBookmark(Bookmark bookmark)
        {
            this.activeBookmarks.Add(bookmark);
        }

        public TriggerCompletedEvent GetNextCompletedEvent()
        {
            while (this.queue.Count > 0)
            {
                TriggerCompletedEvent event2 = this.queue.Dequeue();
                if (this.activeBookmarks.Contains(event2.Bookmark))
                {
                    this.CurrentBeingProcessedEvent = event2;
                    return event2;
                }
            }
            return null;
        }

        public bool IsReferredByBeingProcessedEvent(Bookmark bookmark)
        {
            return ((this.CurrentBeingProcessedEvent != null) && (this.CurrentBeingProcessedEvent.Bookmark == bookmark));
        }

        public void RegisterCompletedEvent(TriggerCompletedEvent completedEvent, out bool canBeProcessedImmediately)
        {
            canBeProcessedImmediately = this.CanProcessEventImmediately;
            this.queue.Enqueue(completedEvent);
        }

        public void RemoveActiveBookmark(Bookmark bookmark)
        {
            this.activeBookmarks.Remove(bookmark);
        }

        private bool CanProcessEventImmediately
        {
            get
            {
                return (((this.CurrentBeingProcessedEvent == null) && !this.OnTransition) && (this.queue.Count == 0));
            }
        }

        [DataMember(EmitDefaultValue=false)]
        public TriggerCompletedEvent CurrentBeingProcessedEvent { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public int CurrentConditionIndex { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public bool OnTransition { get; set; }
    }
}

