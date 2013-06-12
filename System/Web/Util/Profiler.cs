namespace System.Web.Util
{
    using System;
    using System.Collections;
    using System.Web;

    internal class Profiler
    {
        private bool _isEnabled;
        private bool _localOnly = true;
        private bool _mostRecent = false;
        private bool _oldEnabled;
        private TraceMode _outputMode = TraceMode.SortByTime;
        private bool _pageOutput;
        private Queue _requests;
        private int _requestsToProfile = 10;

        internal Profiler()
        {
            this._requests = new Queue(this._requestsToProfile);
        }

        internal void EndProfiling()
        {
            this._isEnabled = false;
        }

        internal void EndRequest(HttpContext context)
        {
            context.Trace.EndRequest();
            if (this.IsEnabled)
            {
                lock (this._requests)
                {
                    this._requests.Enqueue(context.Trace.GetData());
                    if (this.MostRecent && (this._requests.Count > this._requestsToProfile))
                    {
                        this._requests.Dequeue();
                    }
                }
                if (!this.MostRecent && (this._requests.Count >= this._requestsToProfile))
                {
                    this.EndProfiling();
                }
            }
        }

        internal IList GetData()
        {
            return this._requests.ToArray();
        }

        internal void Reset()
        {
            this._requests = new Queue(this._requestsToProfile);
            if (this._requestsToProfile != 0)
            {
                this._isEnabled = this._oldEnabled;
            }
            else
            {
                this._isEnabled = false;
            }
        }

        internal void StartRequest(HttpContext context)
        {
            context.Trace.VerifyStart();
        }

        internal bool IsConfigEnabled
        {
            get
            {
                return this._oldEnabled;
            }
        }

        internal bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }
            set
            {
                this._isEnabled = value;
                this._oldEnabled = value;
            }
        }

        internal bool LocalOnly
        {
            get
            {
                return this._localOnly;
            }
            set
            {
                this._localOnly = value;
            }
        }

        internal bool MostRecent
        {
            get
            {
                return this._mostRecent;
            }
            set
            {
                this._mostRecent = value;
            }
        }

        internal TraceMode OutputMode
        {
            get
            {
                return this._outputMode;
            }
            set
            {
                this._outputMode = value;
            }
        }

        internal bool PageOutput
        {
            get
            {
                if (!this._pageOutput)
                {
                    return false;
                }
                if (this._localOnly)
                {
                    return HttpContext.Current.Request.IsLocal;
                }
                return true;
            }
            set
            {
                this._pageOutput = value;
            }
        }

        internal int RequestsRemaining
        {
            get
            {
                return (this._requestsToProfile - this._requests.Count);
            }
        }

        internal int RequestsToProfile
        {
            get
            {
                return this._requestsToProfile;
            }
            set
            {
                if (value > 0x2710)
                {
                    value = 0x2710;
                }
                this._requestsToProfile = value;
            }
        }
    }
}

