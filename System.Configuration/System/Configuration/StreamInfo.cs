namespace System.Configuration
{
    using System;

    internal class StreamInfo
    {
        private string _configSource;
        private bool _isMonitored;
        private string _sectionName;
        private string _streamName;
        private object _version;

        private StreamInfo()
        {
        }

        internal StreamInfo(string sectionName, string configSource, string streamName)
        {
            this._sectionName = sectionName;
            this._configSource = configSource;
            this._streamName = streamName;
        }

        internal StreamInfo Clone()
        {
            return new StreamInfo { _sectionName = this._sectionName, _configSource = this._configSource, _streamName = this._streamName, _isMonitored = this._isMonitored, _version = this._version };
        }

        internal string ConfigSource
        {
            get
            {
                return this._configSource;
            }
        }

        internal bool IsMonitored
        {
            get
            {
                return this._isMonitored;
            }
            set
            {
                this._isMonitored = value;
            }
        }

        internal string SectionName
        {
            get
            {
                return this._sectionName;
            }
        }

        internal string StreamName
        {
            get
            {
                return this._streamName;
            }
        }

        internal object Version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }
    }
}

