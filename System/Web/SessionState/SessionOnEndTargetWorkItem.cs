namespace System.Web.SessionState
{
    using System;

    internal class SessionOnEndTargetWorkItem
    {
        private HttpSessionState _sessionState;
        private SessionOnEndTarget _target;

        internal SessionOnEndTargetWorkItem(SessionOnEndTarget target, HttpSessionState sessionState)
        {
            this._target = target;
            this._sessionState = sessionState;
        }

        internal void RaiseOnEndCallback()
        {
            this._target.RaiseOnEnd(this._sessionState);
        }
    }
}

