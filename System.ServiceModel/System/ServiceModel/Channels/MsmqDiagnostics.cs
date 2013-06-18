namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Xml;

    internal static class MsmqDiagnostics
    {
        private static bool poolFullReported;

        public static ServiceModelActivity BoundDecodeOperation()
        {
            ServiceModelActivity activity = null;
            if (DiagnosticUtility.ShouldUseActivity)
            {
                activity = ServiceModelActivity.CreateBoundedActivity(true);
                ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityProcessingMessage", new object[] { TraceUtility.RetrieveMessageNumber() }), ActivityType.ProcessMessage);
            }
            return activity;
        }

        public static Activity BoundOpenOperation(MsmqReceiveHelper receiver)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80036, System.ServiceModel.SR.GetString("TraceCodeTransportListen", new object[] { receiver.ListenUri.ToString() }), receiver);
            }
            return ServiceModelActivity.BoundOperation(receiver.Activity);
        }

        public static ServiceModelActivity BoundReceiveBytesOperation()
        {
            ServiceModelActivity activity = null;
            if (DiagnosticUtility.ShouldUseActivity)
            {
                activity = ServiceModelActivity.CreateBoundedActivityWithTransferInOnly(Guid.NewGuid());
                ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityReceiveBytes", new object[] { TraceUtility.RetrieveMessageNumber() }), ActivityType.ReceiveBytes);
            }
            return activity;
        }

        public static Activity BoundReceiveOperation(MsmqReceiveHelper receiver)
        {
            if ((DiagnosticUtility.ShouldUseActivity && (ServiceModelActivity.Current != null)) && (ActivityType.ProcessAction != ServiceModelActivity.Current.ActivityType))
            {
                return ServiceModelActivity.BoundOperation(receiver.Activity);
            }
            return null;
        }

        public static void CannotPeekOnQueue(string formatName, Exception ex)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x40052, System.ServiceModel.SR.GetString("TraceCodeMsmqCannotPeekOnQueue"), new StringTraceRecord("QueueFormatName", formatName), null, ex);
            }
        }

        public static void CannotReadQueues(string host, bool publicQueues, Exception ex)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
                dictionary["Host"] = host;
                dictionary["PublicQueues"] = Convert.ToString(publicQueues, CultureInfo.InvariantCulture);
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x40053, System.ServiceModel.SR.GetString("TraceCodeMsmqCannotReadQueues"), new DictionaryTraceRecord(dictionary), null, ex);
            }
        }

        public static void DatagramReceived(NativeMsmqMessage.BufferProperty messageId, Message message)
        {
            DatagramSentOrReceived(messageId, message, 0x40055, System.ServiceModel.SR.GetString("TraceCodeMsmqDatagramReceived"));
        }

        public static void DatagramSent(NativeMsmqMessage.BufferProperty messageId, Message message)
        {
            DatagramSentOrReceived(messageId, message, 0x40054, System.ServiceModel.SR.GetString("TraceCodeMsmqDatagramSent"));
        }

        private static void DatagramSentOrReceived(NativeMsmqMessage.BufferProperty messageId, Message message, int traceCode, string traceDescription)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Guid guid = MessageIdToGuid(messageId);
                UniqueId id = message.Headers.MessageId;
                TraceRecord extendedData = null;
                if (null == id)
                {
                    extendedData = new StringTraceRecord("MSMQMessageId", guid.ToString());
                }
                else
                {
                    Dictionary<string, string> dictionary2 = new Dictionary<string, string>(2);
                    dictionary2.Add("MSMQMessageId", guid.ToString());
                    dictionary2.Add("WCFMessageId", id.ToString());
                    Dictionary<string, string> dictionary = dictionary2;
                    extendedData = new DictionaryTraceRecord(dictionary);
                }
                TraceUtility.TraceEvent(TraceEventType.Verbose, traceCode, traceDescription, extendedData, null, null);
            }
        }

        public static void ExpectedException(Exception ex)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(ex, TraceEventType.Information);
            }
        }

        public static void FoundBaseAddress(Uri uri, string virtualPath)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> dictionary2 = new Dictionary<string, string>(2);
                dictionary2.Add("Uri", uri.ToString());
                dictionary2.Add("VirtualPath", virtualPath);
                Dictionary<string, string> dictionary = dictionary2;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40059, System.ServiceModel.SR.GetString("TraceCodeMsmqFoundBaseAddress"), new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        public static void MatchedApplicationFound(string host, string queueName, bool isPrivate, string canonicalPath)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(4);
                dictionary["Host"] = host;
                dictionary["QueueName"] = queueName;
                dictionary["Private"] = Convert.ToString(isPrivate, CultureInfo.InvariantCulture);
                dictionary["CanonicalPath"] = canonicalPath;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4005b, System.ServiceModel.SR.GetString("TraceCodeMsmqMatchedApplicationFound"), new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        public static void MessageConsumed(string uri, string messageId, bool rejected)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, rejected ? 0x4005e : 0x4005c, rejected ? System.ServiceModel.SR.GetString("TraceCodeMsmqMessageRejected") : System.ServiceModel.SR.GetString("TraceCodeMsmqMessageDropped"), new StringTraceRecord("MSMQMessageId", messageId), null, null);
            }
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                if (rejected)
                {
                    PerformanceCounters.MsmqRejectedMessage(uri);
                }
                else
                {
                    PerformanceCounters.MsmqDroppedMessage(uri);
                }
            }
        }

        private static Guid MessageIdToGuid(NativeMsmqMessage.BufferProperty messageId)
        {
            int length = messageId.Buffer.Length;
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(messageId.Buffer, 4, dst, 0, 0x10);
            return new Guid(dst);
        }

        public static void MessageLockedUnderTheTransaction(long lookupId)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x4005d, System.ServiceModel.SR.GetString("TraceCodeMsmqMessageLockedUnderTheTransaction"), new StringTraceRecord("MSMQMessageLookupId", Convert.ToString(lookupId, CultureInfo.InvariantCulture)), null, null);
            }
        }

        public static void MoveOrDeleteAttemptFailed(long lookupId)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x4005f, System.ServiceModel.SR.GetString("TraceCodeMsmqMoveOrDeleteAttemptFailed"), new StringTraceRecord("MSMQMessageLookupId", Convert.ToString(lookupId, CultureInfo.InvariantCulture)), null, null);
            }
        }

        public static void MsmqDetected(Version version)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40056, System.ServiceModel.SR.GetString("TraceCodeMsmqDetected"), new StringTraceRecord("MSMQVersion", version.ToString()), null, null);
            }
        }

        public static void PoisonMessageMoved(string messageId, bool poisonQueue, string uri)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, poisonQueue ? 0x40060 : 0x40061, poisonQueue ? System.ServiceModel.SR.GetString("TraceCodeMsmqPoisonMessageMovedPoison") : System.ServiceModel.SR.GetString("TraceCodeMsmqPoisonMessageMovedRetry"), new StringTraceRecord("MSMQMessageId", messageId), null, null);
            }
            if (poisonQueue && PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.MsmqPoisonMessage(uri);
            }
        }

        public static void PoisonMessageRejected(string messageId, string uri)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x40062, System.ServiceModel.SR.GetString("TraceCodeMsmqPoisonMessageRejected"), new StringTraceRecord("MSMQMessageId", messageId), null, null);
            }
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.MsmqPoisonMessage(uri);
            }
        }

        public static void PoolFull(int poolSize)
        {
            if (DiagnosticUtility.ShouldTraceInformation && !poolFullReported)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40063, System.ServiceModel.SR.GetString("TraceCodeMsmqPoolFull"), null, null, null);
                poolFullReported = true;
            }
        }

        public static void PotentiallyPoisonMessageDetected(string messageId)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x40064, System.ServiceModel.SR.GetString("TraceCodeMsmqPotentiallyPoisonMessageDetected"), new StringTraceRecord("MSMQMessageId", messageId), null, null);
            }
        }

        public static void QueueClosed(string formatName)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40065, System.ServiceModel.SR.GetString("TraceCodeMsmqQueueClosed"), new StringTraceRecord("FormatName", formatName), null, null);
            }
        }

        public static void QueueOpened(string formatName)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40066, System.ServiceModel.SR.GetString("TraceCodeMsmqQueueOpened"), new StringTraceRecord("FormatName", formatName), null, null);
            }
        }

        public static void QueueTransactionalStatusUnknown(string formatName)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x40067, System.ServiceModel.SR.GetString("TraceCodeMsmqQueueTransactionalStatusUnknown"), new StringTraceRecord("FormatName", formatName), null, null);
            }
        }

        public static void ScanStarted()
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40068, System.ServiceModel.SR.GetString("TraceCodeMsmqScanStarted"), null, null, null);
            }
        }

        public static void SessiongramReceived(string sessionId, NativeMsmqMessage.BufferProperty messageId, int numberOfMessages)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                dictionary["SessionId"] = sessionId;
                dictionary["MSMQMessageId"] = MsmqMessageId.ToString(messageId.Buffer);
                dictionary["NumberOfMessages"] = Convert.ToString(numberOfMessages, CultureInfo.InvariantCulture);
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40069, System.ServiceModel.SR.GetString("TraceCodeMsmqSessiongramReceived"), new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        public static void SessiongramSent(string sessionId, NativeMsmqMessage.BufferProperty messageId, int numberOfMessages)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                dictionary["SessionId"] = sessionId;
                dictionary["MSMQMessageId"] = MsmqMessageId.ToString(messageId.Buffer);
                dictionary["NumberOfMessages"] = Convert.ToString(numberOfMessages, CultureInfo.InvariantCulture);
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x4006a, System.ServiceModel.SR.GetString("TraceCodeMsmqSessiongramSent"), new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        public static void StartingApplication(string application)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4006b, System.ServiceModel.SR.GetString("TraceCodeMsmqStartingApplication"), new StringTraceRecord("Application", application), null, null);
            }
        }

        public static void StartingService(string host, string name, bool isPrivate, string processedVirtualPath)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(4);
                dictionary["Host"] = host;
                dictionary["Name"] = name;
                dictionary["Private"] = Convert.ToString(isPrivate, CultureInfo.InvariantCulture);
                dictionary["VirtualPath"] = processedVirtualPath;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4006c, System.ServiceModel.SR.GetString("TraceCodeMsmqStartingService"), new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        public static ServiceModelActivity StartListenAtActivity(MsmqReceiveHelper receiver)
        {
            ServiceModelActivity activity = receiver.Activity;
            if (DiagnosticUtility.ShouldUseActivity && (activity == null))
            {
                activity = ServiceModelActivity.CreateActivity(true);
                if (FxTrace.Trace != null)
                {
                    FxTrace.Trace.TraceTransfer(activity.Id);
                }
                ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityListenAt", new object[] { receiver.ListenUri.ToString() }), ActivityType.ListenAt);
            }
            return activity;
        }

        public static void TransferFromTransport(Message message)
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                TraceUtility.TransferFromTransport(message);
            }
        }

        public static void UnexpectedAcknowledgment(string messageId, int acknowledgment)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
                dictionary["MSMQMessageId"] = messageId;
                dictionary["Acknowledgment"] = Convert.ToString(acknowledgment, CultureInfo.InvariantCulture);
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x4006d, System.ServiceModel.SR.GetString("TraceCodeMsmqUnexpectedAcknowledgment"), new DictionaryTraceRecord(dictionary), null, null);
            }
        }
    }
}

