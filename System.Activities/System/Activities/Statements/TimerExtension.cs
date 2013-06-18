namespace System.Activities.Statements
{
    using System;
    using System.Activities;

    public abstract class TimerExtension
    {
        protected TimerExtension()
        {
        }

        public void CancelTimer(Bookmark bookmark)
        {
            this.OnCancelTimer(bookmark);
        }

        protected abstract void OnCancelTimer(Bookmark bookmark);
        protected abstract void OnRegisterTimer(TimeSpan timeout, Bookmark bookmark);
        public void RegisterTimer(TimeSpan timeout, Bookmark bookmark)
        {
            this.OnRegisterTimer(timeout, bookmark);
        }
    }
}

