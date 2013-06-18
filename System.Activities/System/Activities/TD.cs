namespace System.Activities
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime;
    using System.Security;

    internal class TD
    {
        [SecurityCritical]
        private static EventDescriptor[] eventDescriptors;
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private TD()
        {
        }

        internal static void ActivityCompleted(string param0, string param1, string param2, string param3)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x2a))
            {
                WriteEtwEvent(0x2a, param0, param1, param2, param3, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ActivityCompleted", Culture), new object[] { param0, param1, param2, param3 });
                WriteTraceSource(0x2a, description, payload);
            }
        }

        internal static bool ActivityCompletedIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x2a);
            }
            return true;
        }

        internal static void ActivityScheduled(string param0, string param1, string param2, string param3, string param4, string param5)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x2b))
            {
                WriteEtwEvent(0x2b, param0, param1, param2, param3, param4, param5, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ActivityScheduled", Culture), new object[] { param0, param1, param2, param3, param4, param5 });
                WriteTraceSource(0x2b, description, payload);
            }
        }

        internal static bool ActivityScheduledIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x2b);
            }
            return true;
        }

        internal static void ArrayItemValueResult(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x44))
            {
                WriteEtwEvent(0x44, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ArrayItemValueResult", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x44, description, payload);
            }
        }

        internal static bool ArrayItemValueResultIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x44);
            }
            return true;
        }

        internal static void BinaryExpressionResult(string param0, string param1, string param2, string param3)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x41))
            {
                WriteEtwEvent(0x41, param0, param1, param2, param3, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("BinaryExpressionResult", Culture), new object[] { param0, param1, param2, param3 });
                WriteTraceSource(0x41, description, payload);
            }
        }

        internal static bool BinaryExpressionResultIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x41);
            }
            return true;
        }

        internal static void BookmarkScopeInitialized(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x25))
            {
                WriteEtwEvent(0x25, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("BookmarkScopeInitialized", Culture), new object[] { param0, param1 });
                WriteTraceSource(0x25, description, payload);
            }
        }

        internal static bool BookmarkScopeInitializedIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x25);
            }
            return true;
        }

        internal static void CompensationState(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x40))
            {
                WriteEtwEvent(0x40, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CompensationState", Culture), new object[] { param0, param1 });
                WriteTraceSource(0x40, description, payload);
            }
        }

        internal static bool CompensationStateIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x40);
            }
            return true;
        }

        internal static void CompleteBookmarkWorkItem(string param0, string param1, string param2, string param3, string param4)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(8))
            {
                WriteEtwEvent(8, param0, param1, param2, param3, param4, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CompleteBookmarkWorkItem", Culture), new object[] { param0, param1, param2, param3, param4 });
                WriteTraceSource(8, description, payload);
            }
        }

        internal static bool CompleteBookmarkWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(8);
            }
            return true;
        }

        internal static void CompleteCancelActivityWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(9))
            {
                WriteEtwEvent(9, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CompleteCancelActivityWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(9, description, payload);
            }
        }

        internal static bool CompleteCancelActivityWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(9);
            }
            return true;
        }

        internal static void CompleteCompletionWorkItem(string param0, string param1, string param2, string param3, string param4, string param5)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(10))
            {
                WriteEtwEvent(10, param0, param1, param2, param3, param4, param5, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CompleteCompletionWorkItem", Culture), new object[] { param0, param1, param2, param3, param4, param5 });
                WriteTraceSource(10, description, payload);
            }
        }

        internal static bool CompleteCompletionWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(10);
            }
            return true;
        }

        internal static void CompleteExecuteActivityWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(11))
            {
                WriteEtwEvent(11, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CompleteExecuteActivityWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(11, description, payload);
            }
        }

        internal static bool CompleteExecuteActivityWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(11);
            }
            return true;
        }

        internal static void CompleteFaultWorkItem(string param0, string param1, string param2, string param3, string param4, string param5, Exception exception)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(12))
            {
                WriteEtwEvent(12, param0, param1, param2, param3, param4, param5, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CompleteFaultWorkItem", Culture), new object[] { param0, param1, param2, param3, param4, param5 });
                WriteTraceSource(12, description, payload);
            }
        }

        internal static bool CompleteFaultWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(12);
            }
            return true;
        }

        internal static void CompleteRuntimeWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(13))
            {
                WriteEtwEvent(13, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CompleteRuntimeWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(13, description, payload);
            }
        }

        internal static bool CompleteRuntimeWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(13);
            }
            return true;
        }

        internal static void CompleteTransactionContextWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(14))
            {
                WriteEtwEvent(14, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CompleteTransactionContextWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(14, description, payload);
            }
        }

        internal static bool CompleteTransactionContextWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(14);
            }
            return true;
        }

        internal static void CreateBookmark(string param0, string param1, string param2, string param3, string param4)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(15))
            {
                WriteEtwEvent(15, param0, param1, param2, param3, param4, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CreateBookmark", Culture), new object[] { param0, param1, param2, param3, param4 });
                WriteTraceSource(15, description, payload);
            }
        }

        internal static bool CreateBookmarkIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(15);
            }
            return true;
        }

        internal static void CreateBookmarkScope(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x10))
            {
                WriteEtwEvent(0x10, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("CreateBookmarkScope", Culture), new object[] { param0 });
                WriteTraceSource(0x10, description, payload);
            }
        }

        internal static bool CreateBookmarkScopeIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x10);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static void EnsureEventDescriptors()
        {
            if (object.ReferenceEquals(TD.eventDescriptors, null))
            {
                EventDescriptor[] eventDescriptors = new EventDescriptor[] { 
                    new EventDescriptor(0x9a20, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0x9a21, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x9a22, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0x9a23, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x4c7, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xa10, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xa11, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0xa12, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3ff, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3fb, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3f8, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3f5, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x407, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x40a, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x404, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3fc, 0, 0x13, 5, 0, 0, 0x1000000000000000L), 
                    new EventDescriptor(0x400, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x40e, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x40f, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x410, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x40d, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x40c, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x40b, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3fd, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3f9, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3f6, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3f3, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x405, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x408, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x402, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3fe, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3fa, 0, 0x13, 5, 0, 0, 0x1000000000000000L), 
                    new EventDescriptor(0x3f7, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3f4, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x406, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x409, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x403, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x401, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0x450, 0, 0x13, 4, 7, 0, 0x1000000000000000L), new EventDescriptor(0x44d, 0, 0x13, 4, 1, 0, 0x1000000000000000L), new EventDescriptor(0x44e, 0, 0x13, 4, 2, 0, 0x1000000000000000L), new EventDescriptor(0x44f, 0, 0x13, 4, 8, 0, 0x1000000000000000L), new EventDescriptor(0x3f2, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3f1, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3ec, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3eb, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3e9, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3ed, 0, 0x13, 4, 0, 0, 0x1000000000000000L), 
                    new EventDescriptor(0x411, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3ef, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3ea, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3ee, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0x3f0, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x464, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x465, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x466, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x46b, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x46c, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x474, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x475, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0x477, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x47a, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x47b, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x47c, 0, 0x13, 4, 0, 0, 0x1000000000000000L), 
                    new EventDescriptor(0x47e, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x7d1, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x7d2, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x7d3, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x7d4, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0x9a24, 0, 0x13, 3, 0, 0, 0x1000000000000000L)
                 };
                FxTrace.UpdateEventDefinitions(eventDescriptors);
                TD.eventDescriptors = eventDescriptors;
            }
        }

        internal static void EnterNoPersistBlock()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x11))
            {
                WriteEtwEvent(0x11, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("EnterNoPersistBlock", Culture), new object[0]);
                WriteTraceSource(0x11, description, payload);
            }
        }

        internal static bool EnterNoPersistBlockIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x11);
            }
            return true;
        }

        internal static void ExitNoPersistBlock()
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x12))
            {
                WriteEtwEvent(0x12, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ExitNoPersistBlock", Culture), new object[0]);
                WriteTraceSource(0x12, description, payload);
            }
        }

        internal static bool ExitNoPersistBlockIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x12);
            }
            return true;
        }

        internal static void FlowchartEmpty(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x3b))
            {
                WriteEtwEvent(0x3b, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("FlowchartEmpty", Culture), new object[] { param0 });
                WriteTraceSource(0x3b, description, payload);
            }
        }

        internal static bool FlowchartEmptyIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x3b);
            }
            return true;
        }

        internal static void FlowchartNextNull(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(60))
            {
                WriteEtwEvent(60, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("FlowchartNextNull", Culture), new object[] { param0 });
                WriteTraceSource(60, description, payload);
            }
        }

        internal static bool FlowchartNextNullIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(60);
            }
            return true;
        }

        internal static void FlowchartStart(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x3a))
            {
                WriteEtwEvent(0x3a, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("FlowchartStart", Culture), new object[] { param0 });
                WriteTraceSource(0x3a, description, payload);
            }
        }

        internal static bool FlowchartStartIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x3a);
            }
            return true;
        }

        internal static void FlowchartSwitchCase(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x3d))
            {
                WriteEtwEvent(0x3d, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("FlowchartSwitchCase", Culture), new object[] { param0, param1 });
                WriteTraceSource(0x3d, description, payload);
            }
        }

        internal static bool FlowchartSwitchCaseIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x3d);
            }
            return true;
        }

        internal static void FlowchartSwitchCaseNotFound(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x3f))
            {
                WriteEtwEvent(0x3f, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("FlowchartSwitchCaseNotFound", Culture), new object[] { param0 });
                WriteTraceSource(0x3f, description, payload);
            }
        }

        internal static bool FlowchartSwitchCaseNotFoundIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x3f);
            }
            return true;
        }

        internal static void FlowchartSwitchDefault(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x3e))
            {
                WriteEtwEvent(0x3e, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("FlowchartSwitchDefault", Culture), new object[] { param0 });
                WriteTraceSource(0x3e, description, payload);
            }
        }

        internal static bool FlowchartSwitchDefaultIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x3e);
            }
            return true;
        }

        internal static void InArgumentBound(string param0, string param1, string param2, string param3, string param4)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x13))
            {
                WriteEtwEvent(0x13, param0, param1, param2, param3, param4, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("InArgumentBound", Culture), new object[] { param0, param1, param2, param3, param4 });
                WriteTraceSource(0x13, description, payload);
            }
        }

        internal static bool InArgumentBoundIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x13);
            }
            return true;
        }

        internal static void InvokedMethodThrewException(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x37))
            {
                WriteEtwEvent(0x37, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("InvokedMethodThrewException", Culture), new object[] { param0, param1 });
                WriteTraceSource(0x37, description, payload);
            }
        }

        internal static bool InvokedMethodThrewExceptionIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x37);
            }
            return true;
        }

        internal static void InvokeMethodDoesNotUseAsyncPattern(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x39))
            {
                WriteEtwEvent(0x39, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("InvokeMethodDoesNotUseAsyncPattern", Culture), new object[] { param0 });
                WriteTraceSource(0x39, description, payload);
            }
        }

        internal static bool InvokeMethodDoesNotUseAsyncPatternIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x39);
            }
            return true;
        }

        internal static void InvokeMethodIsNotStatic(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x36))
            {
                WriteEtwEvent(0x36, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("InvokeMethodIsNotStatic", Culture), new object[] { param0 });
                WriteTraceSource(0x36, description, payload);
            }
        }

        internal static bool InvokeMethodIsNotStaticIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x36);
            }
            return true;
        }

        internal static void InvokeMethodIsStatic(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x35))
            {
                WriteEtwEvent(0x35, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("InvokeMethodIsStatic", Culture), new object[] { param0 });
                WriteTraceSource(0x35, description, payload);
            }
        }

        internal static bool InvokeMethodIsStaticIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x35);
            }
            return true;
        }

        internal static void InvokeMethodUseAsyncPattern(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x38))
            {
                WriteEtwEvent(0x38, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("InvokeMethodUseAsyncPattern", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x38, description, payload);
            }
        }

        internal static bool InvokeMethodUseAsyncPatternIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x38);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static bool IsEtwEventEnabled(int eventIndex)
        {
            EnsureEventDescriptors();
            return FxTrace.IsEventEnabled(eventIndex);
        }

        internal static void MemberAccessExpressionResult(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x43))
            {
                WriteEtwEvent(0x43, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("MemberAccessExpressionResult", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x43, description, payload);
            }
        }

        internal static bool MemberAccessExpressionResultIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x43);
            }
            return true;
        }

        internal static void RuntimeTransactionComplete(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(20))
            {
                WriteEtwEvent(20, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("RuntimeTransactionComplete", Culture), new object[] { param0 });
                WriteTraceSource(20, description, payload);
            }
        }

        internal static bool RuntimeTransactionCompleteIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(20);
            }
            return true;
        }

        internal static void RuntimeTransactionCompletionRequested(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x15))
            {
                WriteEtwEvent(0x15, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("RuntimeTransactionCompletionRequested", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x15, description, payload);
            }
        }

        internal static bool RuntimeTransactionCompletionRequestedIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x15);
            }
            return true;
        }

        internal static void RuntimeTransactionSet(string param0, string param1, string param2, string param3, string param4, string param5)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x16))
            {
                WriteEtwEvent(0x16, param0, param1, param2, param3, param4, param5, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("RuntimeTransactionSet", Culture), new object[] { param0, param1, param2, param3, param4, param5 });
                WriteTraceSource(0x16, description, payload);
            }
        }

        internal static bool RuntimeTransactionSetIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x16);
            }
            return true;
        }

        internal static void ScheduleBookmarkWorkItem(string param0, string param1, string param2, string param3, string param4)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x17))
            {
                WriteEtwEvent(0x17, param0, param1, param2, param3, param4, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ScheduleBookmarkWorkItem", Culture), new object[] { param0, param1, param2, param3, param4 });
                WriteTraceSource(0x17, description, payload);
            }
        }

        internal static bool ScheduleBookmarkWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x17);
            }
            return true;
        }

        internal static void ScheduleCancelActivityWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x18))
            {
                WriteEtwEvent(0x18, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ScheduleCancelActivityWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x18, description, payload);
            }
        }

        internal static bool ScheduleCancelActivityWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x18);
            }
            return true;
        }

        internal static void ScheduleCompletionWorkItem(string param0, string param1, string param2, string param3, string param4, string param5)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x19))
            {
                WriteEtwEvent(0x19, param0, param1, param2, param3, param4, param5, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ScheduleCompletionWorkItem", Culture), new object[] { param0, param1, param2, param3, param4, param5 });
                WriteTraceSource(0x19, description, payload);
            }
        }

        internal static bool ScheduleCompletionWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x19);
            }
            return true;
        }

        internal static void ScheduleExecuteActivityWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x1a))
            {
                WriteEtwEvent(0x1a, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ScheduleExecuteActivityWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x1a, description, payload);
            }
        }

        internal static bool ScheduleExecuteActivityWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x1a);
            }
            return true;
        }

        internal static void ScheduleFaultWorkItem(string param0, string param1, string param2, string param3, string param4, string param5, Exception exception)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(0x1b))
            {
                WriteEtwEvent(0x1b, param0, param1, param2, param3, param4, param5, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ScheduleFaultWorkItem", Culture), new object[] { param0, param1, param2, param3, param4, param5 });
                WriteTraceSource(0x1b, description, payload);
            }
        }

        internal static bool ScheduleFaultWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x1b);
            }
            return true;
        }

        internal static void ScheduleRuntimeWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x1c))
            {
                WriteEtwEvent(0x1c, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ScheduleRuntimeWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x1c, description, payload);
            }
        }

        internal static bool ScheduleRuntimeWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x1c);
            }
            return true;
        }

        internal static void ScheduleTransactionContextWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x1d))
            {
                WriteEtwEvent(0x1d, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("ScheduleTransactionContextWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x1d, description, payload);
            }
        }

        internal static bool ScheduleTransactionContextWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x1d);
            }
            return true;
        }

        internal static void StartBookmarkWorkItem(string param0, string param1, string param2, string param3, string param4)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(30))
            {
                WriteEtwEvent(30, param0, param1, param2, param3, param4, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StartBookmarkWorkItem", Culture), new object[] { param0, param1, param2, param3, param4 });
                WriteTraceSource(30, description, payload);
            }
        }

        internal static bool StartBookmarkWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(30);
            }
            return true;
        }

        internal static void StartCancelActivityWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x1f))
            {
                WriteEtwEvent(0x1f, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StartCancelActivityWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x1f, description, payload);
            }
        }

        internal static bool StartCancelActivityWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x1f);
            }
            return true;
        }

        internal static void StartCompletionWorkItem(string param0, string param1, string param2, string param3, string param4, string param5)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x20))
            {
                WriteEtwEvent(0x20, param0, param1, param2, param3, param4, param5, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StartCompletionWorkItem", Culture), new object[] { param0, param1, param2, param3, param4, param5 });
                WriteTraceSource(0x20, description, payload);
            }
        }

        internal static bool StartCompletionWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x20);
            }
            return true;
        }

        internal static void StartExecuteActivityWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x21))
            {
                WriteEtwEvent(0x21, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StartExecuteActivityWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x21, description, payload);
            }
        }

        internal static bool StartExecuteActivityWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x21);
            }
            return true;
        }

        internal static void StartFaultWorkItem(string param0, string param1, string param2, string param3, string param4, string param5, Exception exception)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(0x22))
            {
                WriteEtwEvent(0x22, param0, param1, param2, param3, param4, param5, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StartFaultWorkItem", Culture), new object[] { param0, param1, param2, param3, param4, param5 });
                WriteTraceSource(0x22, description, payload);
            }
        }

        internal static bool StartFaultWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x22);
            }
            return true;
        }

        internal static void StartRuntimeWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x23))
            {
                WriteEtwEvent(0x23, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StartRuntimeWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x23, description, payload);
            }
        }

        internal static bool StartRuntimeWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x23);
            }
            return true;
        }

        internal static void StartTransactionContextWorkItem(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x24))
            {
                WriteEtwEvent(0x24, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("StartTransactionContextWorkItem", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x24, description, payload);
            }
        }

        internal static bool StartTransactionContextWorkItemIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x24);
            }
            return true;
        }

        internal static void SwitchCaseNotFound(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(4))
            {
                WriteEtwEvent(4, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("SwitchCaseNotFound", Culture), new object[] { param0 });
                WriteTraceSource(4, description, payload);
            }
        }

        internal static bool SwitchCaseNotFoundIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(4);
            }
            return true;
        }

        internal static void TrackingDataExtracted(string Data, string Activity)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(3))
            {
                WriteEtwEvent(3, Data, Activity, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("TrackingDataExtracted", Culture), new object[] { Data, Activity });
                WriteTraceSource(3, description, payload);
            }
        }

        internal static bool TrackingDataExtractedIsEnabled()
        {
            if (!FxTrace.ShouldTraceVerbose)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceVerboseToTraceSource)
            {
                return FxTrace.IsEventEnabled(3);
            }
            return true;
        }

        internal static void TrackingRecordDropped(long RecordNumber, Guid ProviderId)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0))
            {
                WriteEtwEvent(0, RecordNumber, ProviderId, payload.AppDomainFriendlyName);
            }
        }

        internal static bool TrackingRecordDroppedIsEnabled()
        {
            return (FxTrace.ShouldTraceWarning && FxTrace.IsEventEnabled(0));
        }

        internal static void TrackingRecordRaised(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(1))
            {
                WriteEtwEvent(1, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("TrackingRecordRaised", Culture), new object[] { param0, param1 });
                WriteTraceSource(1, description, payload);
            }
        }

        internal static bool TrackingRecordRaisedIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(1);
            }
            return true;
        }

        internal static void TrackingRecordTruncated(long RecordNumber, Guid ProviderId)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(2))
            {
                WriteEtwEvent(2, RecordNumber, ProviderId, payload.AppDomainFriendlyName);
            }
        }

        internal static bool TrackingRecordTruncatedIsEnabled()
        {
            return (FxTrace.ShouldTraceWarning && FxTrace.IsEventEnabled(2));
        }

        internal static void TrackingValueNotSerializable(string name)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x45))
            {
                WriteEtwEvent(0x45, name, payload.AppDomainFriendlyName);
            }
        }

        internal static bool TrackingValueNotSerializableIsEnabled()
        {
            return (FxTrace.ShouldTraceWarning && FxTrace.IsEventEnabled(0x45));
        }

        internal static void TryCatchExceptionDuringCancelation(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(6))
            {
                WriteEtwEvent(6, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("TryCatchExceptionDuringCancelation", Culture), new object[] { param0 });
                WriteTraceSource(6, description, payload);
            }
        }

        internal static bool TryCatchExceptionDuringCancelationIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(6);
            }
            return true;
        }

        internal static void TryCatchExceptionFromCatchOrFinally(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(7))
            {
                WriteEtwEvent(7, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceWarningToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("TryCatchExceptionFromCatchOrFinally", Culture), new object[] { param0 });
                WriteTraceSource(7, description, payload);
            }
        }

        internal static bool TryCatchExceptionFromCatchOrFinallyIsEnabled()
        {
            if (!FxTrace.ShouldTraceWarning)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceWarningToTraceSource)
            {
                return FxTrace.IsEventEnabled(7);
            }
            return true;
        }

        internal static void TryCatchExceptionFromTry(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(5))
            {
                WriteEtwEvent(5, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("TryCatchExceptionFromTry", Culture), new object[] { param0, param1 });
                WriteTraceSource(5, description, payload);
            }
        }

        internal static bool TryCatchExceptionFromTryIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(5);
            }
            return true;
        }

        internal static void UnaryExpressionResult(string param0, string param1, string param2)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x42))
            {
                WriteEtwEvent(0x42, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("UnaryExpressionResult", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(0x42, description, payload);
            }
        }

        internal static bool UnaryExpressionResultIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x42);
            }
            return true;
        }

        internal static void WorkflowActivityResume(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x26))
            {
                WriteEtwEvent(0x26, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowActivityResume", Culture), new object[] { param0 });
                WriteTraceSource(0x26, description, payload);
            }
        }

        internal static bool WorkflowActivityResumeIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x26);
            }
            return true;
        }

        internal static void WorkflowActivityStart(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x27))
            {
                WriteEtwEvent(0x27, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowActivityStart", Culture), new object[] { param0 });
                WriteTraceSource(0x27, description, payload);
            }
        }

        internal static bool WorkflowActivityStartIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x27);
            }
            return true;
        }

        internal static void WorkflowActivityStop(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(40))
            {
                WriteEtwEvent(40, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowActivityStop", Culture), new object[] { param0 });
                WriteTraceSource(40, description, payload);
            }
        }

        internal static bool WorkflowActivityStopIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(40);
            }
            return true;
        }

        internal static void WorkflowActivitySuspend(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x29))
            {
                WriteEtwEvent(0x29, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowActivitySuspend", Culture), new object[] { param0 });
                WriteTraceSource(0x29, description, payload);
            }
        }

        internal static bool WorkflowActivitySuspendIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x29);
            }
            return true;
        }

        internal static void WorkflowApplicationCompleted(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x2e))
            {
                WriteEtwEvent(0x2e, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowApplicationCompleted", Culture), new object[] { param0 });
                WriteTraceSource(0x2e, description, payload);
            }
        }

        internal static bool WorkflowApplicationCompletedIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x2e);
            }
            return true;
        }

        internal static void WorkflowApplicationIdled(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x2f))
            {
                WriteEtwEvent(0x2f, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowApplicationIdled", Culture), new object[] { param0 });
                WriteTraceSource(0x2f, description, payload);
            }
        }

        internal static bool WorkflowApplicationIdledIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x2f);
            }
            return true;
        }

        internal static void WorkflowApplicationPersistableIdle(string param0, string param1)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x30))
            {
                WriteEtwEvent(0x30, param0, param1, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowApplicationPersistableIdle", Culture), new object[] { param0, param1 });
                WriteTraceSource(0x30, description, payload);
            }
        }

        internal static bool WorkflowApplicationPersistableIdleIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x30);
            }
            return true;
        }

        internal static void WorkflowApplicationPersisted(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x31))
            {
                WriteEtwEvent(0x31, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowApplicationPersisted", Culture), new object[] { param0 });
                WriteTraceSource(0x31, description, payload);
            }
        }

        internal static bool WorkflowApplicationPersistedIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x31);
            }
            return true;
        }

        internal static void WorkflowApplicationTerminated(string param0, Exception exception)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(50))
            {
                WriteEtwEvent(50, param0, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowApplicationTerminated", Culture), new object[] { param0 });
                WriteTraceSource(50, description, payload);
            }
        }

        internal static bool WorkflowApplicationTerminatedIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(50);
            }
            return true;
        }

        internal static void WorkflowApplicationUnhandledException(string param0, string param1, string param2, string param3, Exception exception)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(0x33))
            {
                WriteEtwEvent(0x33, param0, param1, param2, param3, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceErrorToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowApplicationUnhandledException", Culture), new object[] { param0, param1, param2, param3 });
                WriteTraceSource(0x33, description, payload);
            }
        }

        internal static bool WorkflowApplicationUnhandledExceptionIsEnabled()
        {
            if (!FxTrace.ShouldTraceError)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceErrorToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x33);
            }
            return true;
        }

        internal static void WorkflowApplicationUnloaded(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x34))
            {
                WriteEtwEvent(0x34, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowApplicationUnloaded", Culture), new object[] { param0 });
                WriteTraceSource(0x34, description, payload);
            }
        }

        internal static bool WorkflowApplicationUnloadedIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x34);
            }
            return true;
        }

        internal static void WorkflowInstanceAborted(string param0, Exception exception)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(0x2c))
            {
                WriteEtwEvent(0x2c, param0, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowInstanceAborted", Culture), new object[] { param0 });
                WriteTraceSource(0x2c, description, payload);
            }
        }

        internal static bool WorkflowInstanceAbortedIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x2c);
            }
            return true;
        }

        internal static void WorkflowInstanceCanceled(string param0)
        {
            TracePayload payload = FxTrace.Trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(0x2d))
            {
                WriteEtwEvent(0x2d, param0, payload.AppDomainFriendlyName);
            }
            if (FxTrace.ShouldTraceInformationToTraceSource)
            {
                string description = string.Format(Culture, ResourceManager.GetString("WorkflowInstanceCanceled", Culture), new object[] { param0 });
                WriteTraceSource(0x2d, description, payload);
            }
        }

        internal static bool WorkflowInstanceCanceledIsEnabled()
        {
            if (!FxTrace.ShouldTraceInformation)
            {
                return false;
            }
            if (!FxTrace.ShouldTraceInformationToTraceSource)
            {
                return FxTrace.IsEventEnabled(0x2d);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, long eventParam0, Guid eventParam1, string eventParam2)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], new object[] { eventParam0, eventParam1, eventParam2 });
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2, string eventParam3)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2, string eventParam3, string eventParam4)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2, string eventParam3, string eventParam4, string eventParam5)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2, string eventParam3, string eventParam4, string eventParam5, string eventParam6)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5, eventParam6);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(int eventIndex, string eventParam0, string eventParam1, string eventParam2, string eventParam3, string eventParam4, string eventParam5, string eventParam6, string eventParam7)
        {
            EnsureEventDescriptors();
            return FxTrace.Trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3, eventParam4, eventParam5, eventParam6, eventParam7);
        }

        [SecuritySafeCritical]
        private static void WriteTraceSource(int eventIndex, string description, TracePayload payload)
        {
            EnsureEventDescriptors();
            FxTrace.Trace.WriteTraceSource(ref eventDescriptors[eventIndex], description, payload);
        }

        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        private static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceManager, null))
                {
                    resourceManager = new System.Resources.ResourceManager("System.Activities.TD", typeof(TD).Assembly);
                }
                return resourceManager;
            }
        }
    }
}

