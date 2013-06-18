namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;

    public class CheckForUpdateCompletedEventArgs : AsyncCompletedEventArgs
    {
        private readonly Version _availableVersion;
        private readonly bool _isUpdateRequired;
        private readonly Version _minimumRequiredVersion;
        private readonly bool _updateAvailable;
        private readonly long _updateSize;

        internal CheckForUpdateCompletedEventArgs(Exception error, bool cancelled, object userState, bool updateAvailable, Version availableVersion, bool isUpdateRequired, Version minimumRequiredVersion, long updateSize) : base(error, cancelled, userState)
        {
            this._updateAvailable = updateAvailable;
            this._availableVersion = availableVersion;
            this._isUpdateRequired = isUpdateRequired;
            this._minimumRequiredVersion = minimumRequiredVersion;
            this._updateSize = updateSize;
        }

        private void RaiseExceptionIfUpdateNotAvailable()
        {
            if (!this.UpdateAvailable)
            {
                throw new InvalidOperationException(Resources.GetString("Ex_UpdateNotAvailable"));
            }
        }

        public Version AvailableVersion
        {
            get
            {
                this.RaiseExceptionIfUpdateNotAvailable();
                return this._availableVersion;
            }
        }

        public bool IsUpdateRequired
        {
            get
            {
                this.RaiseExceptionIfUpdateNotAvailable();
                return this._isUpdateRequired;
            }
        }

        public Version MinimumRequiredVersion
        {
            get
            {
                this.RaiseExceptionIfUpdateNotAvailable();
                return this._minimumRequiredVersion;
            }
        }

        public bool UpdateAvailable
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._updateAvailable;
            }
        }

        public long UpdateSizeBytes
        {
            get
            {
                this.RaiseExceptionIfUpdateNotAvailable();
                return this._updateSize;
            }
        }
    }
}

