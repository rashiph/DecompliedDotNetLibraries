namespace System.Activities.Tracking
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public abstract class TrackingParticipant
    {
        protected TrackingParticipant()
        {
        }

        protected internal virtual IAsyncResult BeginTrack(TrackingRecord record, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new TrackAsyncResult(this, record, timeout, callback, state);
        }

        protected internal virtual void EndTrack(IAsyncResult result)
        {
            TrackAsyncResult.End(result);
        }

        protected internal abstract void Track(TrackingRecord record, TimeSpan timeout);

        public virtual System.Activities.Tracking.TrackingProfile TrackingProfile { get; set; }

        private class TrackAsyncResult : AsyncResult
        {
            private static Action<object> asyncExecuteTrack = new Action<object>(TrackingParticipant.TrackAsyncResult.ExecuteTrack);
            private TrackingParticipant participant;
            private TrackingRecord record;
            private TimeSpan timeout;

            public TrackAsyncResult(TrackingParticipant participant, TrackingRecord record, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.participant = participant;
                this.record = record;
                this.timeout = timeout;
                ActionItem.Schedule(asyncExecuteTrack, this);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<TrackingParticipant.TrackAsyncResult>(result);
            }

            private static void ExecuteTrack(object state)
            {
                ((TrackingParticipant.TrackAsyncResult) state).TrackCore();
            }

            private void TrackCore()
            {
                Exception exception = null;
                try
                {
                    this.participant.Track(this.record, this.timeout);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                base.Complete(false, exception);
            }
        }
    }
}

