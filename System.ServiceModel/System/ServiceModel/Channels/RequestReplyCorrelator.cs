namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal class RequestReplyCorrelator : IRequestReplyCorrelator
    {
        private Hashtable states = new Hashtable();

        internal RequestReplyCorrelator()
        {
        }

        internal static bool AddressReply(Message reply, Message request)
        {
            ReplyToInfo info = ExtractReplyToInfo(request);
            return AddressReply(reply, info);
        }

        internal static bool AddressReply(Message reply, ReplyToInfo info)
        {
            EndpointAddress faultTo = null;
            if (info.HasFaultTo && reply.IsFault)
            {
                faultTo = info.FaultTo;
            }
            else if (info.HasReplyTo)
            {
                faultTo = info.ReplyTo;
            }
            else if (reply.Version.Addressing == AddressingVersion.WSAddressingAugust2004)
            {
                if (info.HasFrom)
                {
                    faultTo = info.From;
                }
                else
                {
                    faultTo = EndpointAddress.AnonymousAddress;
                }
            }
            if (faultTo != null)
            {
                faultTo.ApplyTo(reply);
                return !faultTo.IsNone;
            }
            return true;
        }

        internal static ReplyToInfo ExtractReplyToInfo(Message message)
        {
            return new ReplyToInfo(message);
        }

        private UniqueId GetRelatesTo(Message reply)
        {
            UniqueId relatesTo = reply.Headers.RelatesTo;
            if (relatesTo == null)
            {
                throw TraceUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("SuppliedMessageIsNotAReplyItHasNoRelatesTo0")), reply);
            }
            return relatesTo;
        }

        internal static void PrepareReply(Message reply, Message request)
        {
            UniqueId messageId = request.Headers.MessageId;
            if (messageId != null)
            {
                MessageHeaders headers = reply.Headers;
                if (object.ReferenceEquals(headers.RelatesTo, null))
                {
                    headers.RelatesTo = messageId;
                }
            }
            if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
            {
                TraceUtility.AddAmbientActivityToMessage(reply);
            }
        }

        internal static void PrepareReply(Message reply, UniqueId messageId)
        {
            if (object.ReferenceEquals(messageId, null))
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MissingMessageID")), reply);
            }
            MessageHeaders headers = reply.Headers;
            if (object.ReferenceEquals(headers.RelatesTo, null))
            {
                headers.RelatesTo = messageId;
            }
            if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
            {
                TraceUtility.AddAmbientActivityToMessage(reply);
            }
        }

        internal static void PrepareRequest(Message request)
        {
            MessageHeaders headers = request.Headers;
            if (headers.MessageId == null)
            {
                headers.MessageId = new UniqueId();
            }
            request.Properties.AllowOutputBatching = false;
            if (TraceUtility.PropagateUserActivity || TraceUtility.ShouldPropagateActivity)
            {
                TraceUtility.AddAmbientActivityToMessage(request);
            }
        }

        void IRequestReplyCorrelator.Add<T>(Message request, T state)
        {
            UniqueId messageId = request.Headers.MessageId;
            System.Type stateType = typeof(T);
            Key key = new Key(messageId, stateType);
            lock (this.states)
            {
                this.states.Add(key, state);
            }
        }

        T IRequestReplyCorrelator.Find<T>(Message reply, bool remove)
        {
            T local;
            UniqueId relatesTo = this.GetRelatesTo(reply);
            System.Type stateType = typeof(T);
            Key key = new Key(relatesTo, stateType);
            lock (this.states)
            {
                local = (T) this.states[key];
                if (remove)
                {
                    this.states.Remove(key);
                }
            }
            return local;
        }

        private class Key
        {
            internal UniqueId MessageId;
            internal System.Type StateType;

            internal Key(UniqueId messageId, System.Type stateType)
            {
                this.MessageId = messageId;
                this.StateType = stateType;
            }

            public override bool Equals(object obj)
            {
                RequestReplyCorrelator.Key key = obj as RequestReplyCorrelator.Key;
                if (key == null)
                {
                    return false;
                }
                return ((key.MessageId == this.MessageId) && (key.StateType == this.StateType));
            }

            public override int GetHashCode()
            {
                return (this.MessageId.GetHashCode() ^ this.StateType.GetHashCode());
            }

            public override string ToString()
            {
                return string.Concat(new object[] { typeof(RequestReplyCorrelator.Key).ToString(), ": {", this.MessageId, ", ", this.StateType.ToString(), "}" });
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ReplyToInfo
        {
            private readonly EndpointAddress faultTo;
            private readonly EndpointAddress from;
            private readonly EndpointAddress replyTo;
            internal ReplyToInfo(Message message)
            {
                this.faultTo = message.Headers.FaultTo;
                this.replyTo = message.Headers.ReplyTo;
                if (message.Version.Addressing == AddressingVersion.WSAddressingAugust2004)
                {
                    this.from = message.Headers.From;
                }
                else
                {
                    this.from = null;
                }
            }

            internal EndpointAddress FaultTo
            {
                get
                {
                    return this.faultTo;
                }
            }
            internal EndpointAddress From
            {
                get
                {
                    return this.from;
                }
            }
            internal bool HasFaultTo
            {
                get
                {
                    return !this.IsTrivial(this.FaultTo);
                }
            }
            internal bool HasFrom
            {
                get
                {
                    return !this.IsTrivial(this.From);
                }
            }
            internal bool HasReplyTo
            {
                get
                {
                    return !this.IsTrivial(this.ReplyTo);
                }
            }
            internal EndpointAddress ReplyTo
            {
                get
                {
                    return this.replyTo;
                }
            }
            private bool IsTrivial(EndpointAddress address)
            {
                if (address != null)
                {
                    return (address == EndpointAddress.AnonymousAddress);
                }
                return true;
            }
        }
    }
}

