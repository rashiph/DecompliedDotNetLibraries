namespace System.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration.Internal;
    using System.IO;

    internal class UpdateConfigHost : DelegatingConfigHost
    {
        private HybridDictionary _streams;

        internal UpdateConfigHost(IInternalConfigHost host)
        {
            base.Host = host;
        }

        internal void AddStreamname(string oldStreamname, string newStreamname, bool alwaysIntercept)
        {
            if (!string.IsNullOrEmpty(oldStreamname) && (alwaysIntercept || !StringUtil.EqualsIgnoreCase(oldStreamname, newStreamname)))
            {
                if (this._streams == null)
                {
                    this._streams = new HybridDictionary(true);
                }
                this._streams[oldStreamname] = new StreamUpdate(newStreamname);
            }
        }

        public override void DeleteStream(string streamName)
        {
            StreamUpdate streamUpdate = this.GetStreamUpdate(streamName, false);
            if (streamUpdate != null)
            {
                InternalConfigHost.StaticDeleteStream(streamUpdate.NewStreamname);
            }
            else
            {
                base.Host.DeleteStream(streamName);
            }
        }

        internal string GetNewStreamname(string oldStreamname)
        {
            StreamUpdate streamUpdate = this.GetStreamUpdate(oldStreamname, false);
            if (streamUpdate != null)
            {
                return streamUpdate.NewStreamname;
            }
            return oldStreamname;
        }

        private StreamUpdate GetStreamUpdate(string oldStreamname, bool alwaysIntercept)
        {
            if (this._streams == null)
            {
                return null;
            }
            StreamUpdate update = (StreamUpdate) this._streams[oldStreamname];
            if (((update != null) && !alwaysIntercept) && !update.WriteCompleted)
            {
                update = null;
            }
            return update;
        }

        public override object GetStreamVersion(string streamName)
        {
            StreamUpdate streamUpdate = this.GetStreamUpdate(streamName, false);
            if (streamUpdate != null)
            {
                return InternalConfigHost.StaticGetStreamVersion(streamUpdate.NewStreamname);
            }
            return base.Host.GetStreamVersion(streamName);
        }

        public override bool IsConfigRecordRequired(string configPath)
        {
            return true;
        }

        public override bool IsFile(string streamName)
        {
            StreamUpdate streamUpdate = this.GetStreamUpdate(streamName, false);
            if (streamUpdate != null)
            {
                return InternalConfigHost.StaticIsFile(streamUpdate.NewStreamname);
            }
            return base.Host.IsFile(streamName);
        }

        public override Stream OpenStreamForRead(string streamName)
        {
            StreamUpdate streamUpdate = this.GetStreamUpdate(streamName, false);
            if (streamUpdate != null)
            {
                return InternalConfigHost.StaticOpenStreamForRead(streamUpdate.NewStreamname);
            }
            return base.Host.OpenStreamForRead(streamName);
        }

        public override Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext)
        {
            StreamUpdate streamUpdate = this.GetStreamUpdate(streamName, true);
            if (streamUpdate != null)
            {
                return InternalConfigHost.StaticOpenStreamForWrite(streamUpdate.NewStreamname, templateStreamName, ref writeContext, false);
            }
            return base.Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext);
        }

        public override void WriteCompleted(string streamName, bool success, object writeContext)
        {
            StreamUpdate streamUpdate = this.GetStreamUpdate(streamName, true);
            if (streamUpdate != null)
            {
                InternalConfigHost.StaticWriteCompleted(streamUpdate.NewStreamname, success, writeContext, false);
                if (success)
                {
                    streamUpdate.WriteCompleted = true;
                }
            }
            else
            {
                base.Host.WriteCompleted(streamName, success, writeContext);
            }
        }
    }
}

