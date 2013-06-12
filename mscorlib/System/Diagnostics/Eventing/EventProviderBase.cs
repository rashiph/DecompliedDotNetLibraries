namespace System.Diagnostics.Eventing
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [FriendAccessAllowed]
    internal class EventProviderBase : IDisposable
    {
        private bool m_completelyInited;
        private bool m_ETWManifestSent;
        private EventWrittenEventArgs m_eventCallbackArgs;
        internal EventData[] m_eventData;
        private Dictionary<string, string> m_eventsByName;
        private System.Guid m_guid;
        internal int m_id;
        internal EventLevel m_level;
        internal EventKeywords m_matchAnyKeyword;
        private string m_name;
        internal EventProviderDataStream m_OutputStreams;
        private OverideEventProvider m_provider;
        private bool m_providerEnabled;
        private byte[] m_rawManifest;

        protected EventProviderBase(System.Guid providerGuid) : this(providerGuid, null)
        {
        }

        [SecuritySafeCritical]
        protected EventProviderBase(System.Guid providerGuid, string providerName)
        {
            if (providerName == null)
            {
                providerName = base.GetType().Name;
            }
            this.m_name = providerName;
            this.m_guid = providerGuid;
            this.m_provider = new OverideEventProvider(this);
            try
            {
                this.m_provider.Register(providerGuid);
            }
            catch (ArgumentException)
            {
                this.m_provider = null;
            }
            if (this.m_providerEnabled && !this.m_ETWManifestSent)
            {
                this.SendManifest(this.m_rawManifest, null);
                this.m_ETWManifestSent = true;
            }
            this.m_completelyInited = true;
            EventProviderDataStream.AddProvider(this);
        }

        [SecuritySafeCritical]
        private void AddEventDescriptor(EventAttribute eventAttribute)
        {
            if ((this.m_eventData == null) || (this.m_eventData.Length <= eventAttribute.EventId))
            {
                EventData[] destinationArray = new EventData[this.m_eventData.Length + 0x10];
                Array.Copy(this.m_eventData, destinationArray, this.m_eventData.Length);
                this.m_eventData = destinationArray;
            }
            this.m_eventData[eventAttribute.EventId].Descriptor = new EventDescriptorInternal(eventAttribute.EventId, eventAttribute.Version, (byte) eventAttribute.Channel, (byte) eventAttribute.Level, (byte) eventAttribute.Opcode, (int) eventAttribute.Task, (long) eventAttribute.Keywords);
            this.m_eventData[eventAttribute.EventId].CaptureStack = eventAttribute.CaptureStack;
            this.m_eventData[eventAttribute.EventId].Message = eventAttribute.Message;
        }

        private void CaptureStack()
        {
        }

        [SecuritySafeCritical]
        private byte[] CreateManifestAndDescriptors(string providerDllName)
        {
            Type type = base.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            EventAttribute attribute = new EventAttribute(0);
            int num = 1;
            this.m_eventData = new EventData[methods.Length];
            ManifestBuilder builder = new ManifestBuilder(this.Name, this.Guid, providerDllName);
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (fields.Length > 0)
            {
                foreach (FieldInfo info in fields)
                {
                    Type fieldType = info.FieldType;
                    if (fieldType == typeof(EventOpcode))
                    {
                        builder.AddOpcode(info.Name, (int) info.GetRawConstantValue());
                    }
                    else if (fieldType == typeof(EventTask))
                    {
                        builder.AddTask(info.Name, (int) info.GetRawConstantValue());
                    }
                    else if (fieldType == typeof(EventKeywords))
                    {
                        builder.AddKeyword(info.Name, (ulong) ((long) info.GetRawConstantValue()));
                    }
                    else if (fieldType == typeof(EventChannel))
                    {
                        builder.AddChannel(info.Name, (int) info.GetRawConstantValue());
                    }
                }
            }
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo element = methods[i];
                ParameterInfo[] parameters = element.GetParameters();
                EventAttribute eventAttribute = (EventAttribute) Attribute.GetCustomAttribute(element, typeof(EventAttribute), false);
                if (element.ReturnType != typeof(void))
                {
                    if ((eventAttribute != null) && this.DoDebugChecks())
                    {
                        throw new ArgumentException("Event attribute placed on method " + element.Name + " which does not return 'void'");
                    }
                }
                else
                {
                    if (!element.IsVirtual && !element.IsStatic)
                    {
                        if (eventAttribute == null)
                        {
                            if (Attribute.GetCustomAttribute(element, typeof(NonEventAttribute), false) != null)
                            {
                                goto Label_0297;
                            }
                            attribute.EventId = num;
                            attribute.Opcode = EventOpcode.Info;
                            attribute.Task = EventTask.None;
                            eventAttribute = attribute;
                        }
                        else if (eventAttribute.EventId <= 0)
                        {
                            throw new ArgumentException("Event IDs <= 0 are illegal.");
                        }
                        num++;
                        if ((eventAttribute.Opcode == EventOpcode.Info) && (eventAttribute.Task == EventTask.None))
                        {
                            eventAttribute.Opcode = (EventOpcode) (10 + eventAttribute.EventId);
                        }
                        builder.StartEvent(element.Name, eventAttribute);
                        for (int j = 0; j < parameters.Length; j++)
                        {
                            builder.AddEventParameter(parameters[j].ParameterType, parameters[j].Name);
                        }
                        builder.EndEvent();
                        if (this.DoDebugChecks())
                        {
                            this.DebugCheckEvent(element, eventAttribute);
                        }
                        this.AddEventDescriptor(eventAttribute);
                    }
                Label_0297:;
                }
            }
            this.TrimEventDescriptors();
            this.m_eventsByName = null;
            return builder.CreateManifest();
        }

        private void DebugCheckEvent(MethodInfo method, EventAttribute eventAttribute)
        {
            int helperCallFirstArg = GetHelperCallFirstArg(method);
            if ((helperCallFirstArg >= 0) && (eventAttribute.EventId != helperCallFirstArg))
            {
                throw new ArgumentException(string.Concat(new object[] { "Error: event ", method.Name, " is given event ID ", eventAttribute.EventId, " but ", helperCallFirstArg, " was passed to the helper." }));
            }
            if ((eventAttribute.EventId < this.m_eventData.Length) && (this.m_eventData[eventAttribute.EventId].Descriptor.EventId != 0))
            {
                throw new ArgumentException(string.Concat(new object[] { "Event ", method.Name, " has ID ", eventAttribute.EventId, " which is the same as a previously defined event." }));
            }
            if (this.m_eventsByName == null)
            {
                this.m_eventsByName = new Dictionary<string, string>();
            }
            if (this.m_eventsByName.ContainsKey(method.Name))
            {
                throw new ArgumentException("Event name " + method.Name + " used more than once.  If you wish to overload a method, the overloaded method should have a [Event(-1)] attribute to indicate the method should not have associated meta-data.");
            }
            this.m_eventsByName[method.Name] = method.Name;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.m_provider != null))
            {
                this.m_provider.Dispose();
                this.m_provider = null;
            }
        }

        protected virtual bool DoDebugChecks()
        {
            return true;
        }

        ~EventProviderBase()
        {
            this.Dispose(false);
        }

        private static int GetHelperCallFirstArg(MethodInfo method)
        {
            byte[] iLAsByteArray = method.GetMethodBody().GetILAsByteArray();
            int num = -1;
            for (int i = 0; i < iLAsByteArray.Length; i++)
            {
                int num3;
                switch (iLAsByteArray[i])
                {
                    case 140:
                    case 0x8d:
                    {
                        i += 4;
                        continue;
                    }
                    case 0xa2:
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 20:
                    case 0x25:
                    case 0x67:
                    case 0x68:
                    case 0x69:
                    case 0x6a:
                    case 0x6d:
                    case 110:
                    {
                        continue;
                    }
                    case 0xfe:
                        i++;
                        if ((i < iLAsByteArray.Length) && (iLAsByteArray[i] < 6))
                        {
                            continue;
                        }
                        goto Label_0206;

                    case 14:
                    case 0x10:
                    {
                        i++;
                        continue;
                    }
                    case 0x15:
                    case 0x16:
                    case 0x17:
                    case 0x18:
                    case 0x19:
                    case 0x1a:
                    case 0x1b:
                    case 0x1c:
                    case 0x1d:
                    case 30:
                    {
                        if ((i > 0) && (iLAsByteArray[i - 1] == 2))
                        {
                            num = iLAsByteArray[i] - 0x16;
                        }
                        continue;
                    }
                    case 0x1f:
                    {
                        if ((i > 0) && (iLAsByteArray[i - 1] == 2))
                        {
                            num = iLAsByteArray[i + 1];
                        }
                        i++;
                        continue;
                    }
                    case 0x20:
                    {
                        i += 4;
                        continue;
                    }
                    case 40:
                        i += 4;
                        if (num < 0)
                        {
                            goto Label_01DC;
                        }
                        num3 = i + 1;
                        goto Label_01D6;

                    case 0x2c:
                    case 0x2d:
                    {
                        num = -1;
                        i++;
                        continue;
                    }
                    case 0x39:
                    case 0x3a:
                    {
                        num = -1;
                        i += 4;
                        continue;
                    }
                    default:
                        goto Label_0206;
                }
            Label_01C4:
                if (iLAsByteArray[num3] == 0x2a)
                {
                    return num;
                }
                if (iLAsByteArray[num3] != 0)
                {
                    goto Label_01DC;
                }
                num3++;
            Label_01D6:
                if (num3 < iLAsByteArray.Length)
                {
                    goto Label_01C4;
                }
            Label_01DC:
                num = -1;
                continue;
            Label_0206:
                return -1;
            }
            return -1;
        }

        [SecuritySafeCritical]
        private void InsureInitialized()
        {
            if (this.m_rawManifest == null)
            {
                lock (this)
                {
                    if (this.m_rawManifest == null)
                    {
                        this.m_rawManifest = this.CreateManifestAndDescriptors("");
                    }
                }
            }
        }

        public bool IsEnabled()
        {
            return this.m_providerEnabled;
        }

        public bool IsEnabled(EventLevel level, EventKeywords keywords)
        {
            if (!this.m_providerEnabled)
            {
                return false;
            }
            if ((this.m_level != EventLevel.LogAlways) && (this.m_level < level))
            {
                return false;
            }
            if (this.m_matchAnyKeyword != EventKeywords.None)
            {
                return ((keywords & this.m_matchAnyKeyword) != EventKeywords.None);
            }
            return true;
        }

        protected bool IsEnabled(EventProviderDataStream outputStream, int eventId)
        {
            return outputStream.m_EventEnabled[eventId];
        }

        private bool IsEnabledDefault(int eventNum, EventLevel currentLevel, EventKeywords currentMatchAnyKeyword)
        {
            if (!this.m_providerEnabled)
            {
                return false;
            }
            EventLevel level = (EventLevel) this.m_eventData[eventNum].Descriptor.Level;
            EventKeywords keywords = (EventKeywords) this.m_eventData[eventNum].Descriptor.Keywords;
            return (((level <= currentLevel) || (currentLevel == EventLevel.LogAlways)) && ((keywords == EventKeywords.None) || ((keywords & currentMatchAnyKeyword) != EventKeywords.None)));
        }

        protected virtual void OnControllerCommand(EventProviderDataStream outputStream, ControllerCommand command, IDictionary<string, string> arguments)
        {
        }

        public string ProviderManifestXmlFragment(string providerDllName)
        {
            byte[] rawManifest = this.m_rawManifest;
            if (rawManifest == null)
            {
                rawManifest = this.CreateManifestAndDescriptors(providerDllName);
            }
            return Encoding.UTF8.GetString(rawManifest);
        }

        [SecuritySafeCritical]
        internal void SendCommand(EventProviderDataStream outputStream, bool enable, EventLevel level, EventKeywords matchAnyKeyword, ControllerCommand command, IDictionary<string, string> commandArguments)
        {
            this.InsureInitialized();
            if ((this.m_OutputStreams != null) && (this.m_eventCallbackArgs == null))
            {
                this.m_eventCallbackArgs = new EventWrittenEventArgs(this);
            }
            this.m_providerEnabled = enable;
            this.m_level = level;
            this.m_matchAnyKeyword = matchAnyKeyword;
            EventProviderDataStream outputStreams = this.m_OutputStreams;
            if (outputStreams == null)
            {
                goto Label_0062;
            }
        Label_0042:
            if (outputStreams == null)
            {
                throw new ArgumentException("outputStream not found");
            }
            if (outputStreams.m_MasterStream != outputStream)
            {
                outputStreams = outputStreams.m_Next;
                goto Label_0042;
            }
        Label_0062:
            if (enable)
            {
                if (outputStreams != null)
                {
                    if (!outputStreams.m_ManifestSent)
                    {
                        outputStreams.m_ManifestSent = true;
                        this.SendManifest(this.m_rawManifest, outputStreams);
                    }
                }
                else if (!this.m_ETWManifestSent && this.m_completelyInited)
                {
                    this.m_ETWManifestSent = true;
                    this.SendManifest(this.m_rawManifest, outputStreams);
                }
            }
            else if (outputStreams != null)
            {
                outputStreams.m_ManifestSent = false;
            }
            else
            {
                this.m_ETWManifestSent = false;
            }
            for (int i = 0; i < this.m_eventData.Length; i++)
            {
                this.SetEnabled(outputStreams, i, this.IsEnabledDefault(i, level, matchAnyKeyword));
            }
            if (commandArguments == null)
            {
                commandArguments = new Dictionary<string, string>();
            }
            this.OnControllerCommand(outputStreams, command, commandArguments);
        }

        [SecuritySafeCritical]
        private unsafe bool SendManifest(byte[] rawManifest, EventProviderDataStream outputStream)
        {
            fixed (byte* numRef = rawManifest)
            {
                EventDescriptorInternal eventDescriptor = new EventDescriptorInternal(0xfffe, 1, 0, 0, 0xfe, 0, -1L);
                ManifestEnvelope envelope = new ManifestEnvelope {
                    Format = ManifestEnvelope.ManifestFormats.SimpleXmlFormat,
                    MajorVersion = 1,
                    MinorVersion = 0,
                    Magic = 0x5b
                };
                int length = rawManifest.Length;
                envelope.TotalChunks = (ushort) ((length + 0xfeff) / 0xff00);
                envelope.ChunkNumber = 0;
                EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 2) * sizeof(EventProvider.EventData))];
                dataPtr->Ptr = (ulong) ((IntPtr) &envelope);
                dataPtr->Size = (uint) sizeof(ManifestEnvelope);
                dataPtr->Reserved = 0;
                dataPtr[1].Ptr = (ulong) numRef;
                dataPtr[1].Reserved = 0;
                bool flag = true;
                while (length > 0)
                {
                    dataPtr[1].Size = (uint) Math.Min(length, 0xff00);
                    if (((outputStream == null) && (this.m_provider != null)) && !this.m_provider.WriteEvent(ref eventDescriptor, 2, (IntPtr) dataPtr))
                    {
                        flag = false;
                    }
                    if (outputStream != null)
                    {
                        byte[] destination = null;
                        byte[] buffer2 = null;
                        if (destination == null)
                        {
                            destination = new byte[dataPtr->Size];
                            buffer2 = new byte[dataPtr[1].Size];
                        }
                        Marshal.Copy((IntPtr) dataPtr->Ptr, destination, 0, (int) dataPtr->Size);
                        Marshal.Copy((IntPtr) dataPtr[1].Ptr, buffer2, 0, (int) dataPtr[1].Size);
                        this.m_eventCallbackArgs.EventId = eventDescriptor.EventId;
                        this.m_eventCallbackArgs.Payload = new object[] { destination, buffer2 };
                        outputStream.m_Callback(this.m_eventCallbackArgs);
                    }
                    length -= 0xff00;
                    envelope.ChunkNumber = (ushort) (envelope.ChunkNumber + 1);
                }
                return flag;
            }
        }

        protected void SetEnabled(EventProviderDataStream outputStream, int eventId, bool value)
        {
            if (outputStream == null)
            {
                this.m_eventData[eventId].EnabledForETW = value;
            }
            else
            {
                outputStream.m_EventEnabled[eventId] = value;
                if (value)
                {
                    this.m_providerEnabled = true;
                    this.m_eventData[eventId].EnabledForAnyStream = true;
                }
                else
                {
                    this.m_eventData[eventId].EnabledForAnyStream = false;
                    for (EventProviderDataStream stream = this.m_OutputStreams; stream != null; stream = stream.m_Next)
                    {
                        if (stream.m_EventEnabled[eventId])
                        {
                            this.m_eventData[eventId].EnabledForAnyStream = true;
                            return;
                        }
                    }
                }
            }
        }

        [SecuritySafeCritical]
        private void TrimEventDescriptors()
        {
            int length = this.m_eventData.Length;
            while (0 < length)
            {
                length--;
                if (this.m_eventData[length].Descriptor.EventId != 0)
                {
                    break;
                }
            }
            if ((this.m_eventData.Length - length) > 2)
            {
                EventData[] destinationArray = new EventData[length + 1];
                Array.Copy(this.m_eventData, destinationArray, destinationArray.Length);
                this.m_eventData = destinationArray;
            }
        }

        protected void WriteEvent(int eventId)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 0, IntPtr.Zero);
                if (this.m_eventData[eventId].CaptureStack)
                {
                    this.CaptureStack();
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[0]);
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, int value)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 1) * sizeof(EventProvider.EventData))];
                dataPtr->Ptr = (ulong) ((IntPtr) &value);
                dataPtr->Size = 4;
                dataPtr->Reserved = 0;
                this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 1, (IntPtr) dataPtr);
                if (this.m_eventData[eventId].CaptureStack)
                {
                    this.CaptureStack();
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, long value)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 1) * sizeof(EventProvider.EventData))];
                dataPtr->Ptr = (ulong) ((IntPtr) &value);
                dataPtr->Size = 8;
                dataPtr->Reserved = 0;
                this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 1, (IntPtr) dataPtr);
                if (this.m_eventData[eventId].CaptureStack)
                {
                    this.CaptureStack();
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, string value)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                fixed (char* str = ((char*) value))
                {
                    char* chPtr = str;
                    EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 1) * sizeof(EventProvider.EventData))];
                    dataPtr->Ptr = (ulong) chPtr;
                    dataPtr->Size = (uint) ((value.Length + 1) * 2);
                    dataPtr->Reserved = 0;
                    this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 1, (IntPtr) dataPtr);
                    if (this.m_eventData[eventId].CaptureStack)
                    {
                        this.CaptureStack();
                    }
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value });
            }
        }

        [SecuritySafeCritical]
        protected void WriteEvent(int eventId, params object[] args)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, args);
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, args);
            }
        }

        protected void WriteEvent(ref EventDescriptorInternal descriptor, params object[] args)
        {
            if ((this.m_provider != null) && this.m_eventData[descriptor.EventId].EnabledForETW)
            {
                this.m_provider.WriteEvent(ref descriptor, args);
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[descriptor.EventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(descriptor.EventId, args);
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, int value1, int value2)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 2) * sizeof(EventProvider.EventData))];
                dataPtr->Ptr = (ulong) ((IntPtr) &value1);
                dataPtr->Size = 4;
                dataPtr->Reserved = 0;
                dataPtr[1].Ptr = (ulong) ((IntPtr) &value2);
                dataPtr[1].Size = 4;
                dataPtr[1].Reserved = 0;
                this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 2, (IntPtr) dataPtr);
                if (this.m_eventData[eventId].CaptureStack)
                {
                    this.CaptureStack();
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value1, value2 });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, long value1, long value2)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 2) * sizeof(EventProvider.EventData))];
                dataPtr->Ptr = (ulong) ((IntPtr) &value1);
                dataPtr->Size = 8;
                dataPtr->Reserved = 0;
                dataPtr[1].Ptr = (ulong) ((IntPtr) &value2);
                dataPtr[1].Size = 8;
                dataPtr[1].Reserved = 0;
                this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 2, (IntPtr) dataPtr);
                if (this.m_eventData[eventId].CaptureStack)
                {
                    this.CaptureStack();
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value1, value2 });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, string value1, int value2)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                fixed (char* str = ((char*) value1))
                {
                    char* chPtr = str;
                    EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 2) * sizeof(EventProvider.EventData))];
                    dataPtr->Ptr = (ulong) chPtr;
                    dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                    dataPtr->Reserved = 0;
                    dataPtr[1].Ptr = (ulong) ((IntPtr) &value2);
                    dataPtr[1].Size = 4;
                    dataPtr[1].Reserved = 0;
                    this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 2, (IntPtr) dataPtr);
                    if (this.m_eventData[eventId].CaptureStack)
                    {
                        this.CaptureStack();
                    }
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value1, value2 });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, string value1, long value2)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                fixed (char* str = ((char*) value1))
                {
                    char* chPtr = str;
                    EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 2) * sizeof(EventProvider.EventData))];
                    dataPtr->Ptr = (ulong) chPtr;
                    dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                    dataPtr->Reserved = 0;
                    dataPtr[1].Ptr = (ulong) ((IntPtr) &value2);
                    dataPtr[1].Size = 8;
                    dataPtr[1].Reserved = 0;
                    this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 2, (IntPtr) dataPtr);
                    if (this.m_eventData[eventId].CaptureStack)
                    {
                        this.CaptureStack();
                    }
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value1, value2 });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, string value1, string value2)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                fixed (char* str = ((char*) value1))
                {
                    char* chPtr = str;
                    fixed (char* str2 = ((char*) value2))
                    {
                        char* chPtr2 = str2;
                        EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 2) * sizeof(EventProvider.EventData))];
                        dataPtr->Ptr = (ulong) chPtr;
                        dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                        dataPtr->Reserved = 0;
                        dataPtr[1].Ptr = (ulong) chPtr2;
                        dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                        dataPtr[1].Reserved = 0;
                        this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 2, (IntPtr) dataPtr);
                        if (this.m_eventData[eventId].CaptureStack)
                        {
                            this.CaptureStack();
                        }
                    }
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value1, value2 });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, int value1, int value2, int value3)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 3) * sizeof(EventProvider.EventData))];
                dataPtr->Ptr = (ulong) ((IntPtr) &value1);
                dataPtr->Size = 4;
                dataPtr->Reserved = 0;
                dataPtr[1].Ptr = (ulong) ((IntPtr) &value2);
                dataPtr[1].Size = 4;
                dataPtr[1].Reserved = 0;
                dataPtr[2].Ptr = (ulong) ((IntPtr) &value3);
                dataPtr[2].Size = 4;
                dataPtr[2].Reserved = 0;
                this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 3, (IntPtr) dataPtr);
                if (this.m_eventData[eventId].CaptureStack)
                {
                    this.CaptureStack();
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value1, value2, value3 });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, long value1, long value2, long value3)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 3) * sizeof(EventProvider.EventData))];
                dataPtr->Ptr = (ulong) ((IntPtr) &value1);
                dataPtr->Size = 8;
                dataPtr->Reserved = 0;
                dataPtr[1].Ptr = (ulong) ((IntPtr) &value2);
                dataPtr[1].Size = 8;
                dataPtr[1].Reserved = 0;
                dataPtr[2].Ptr = (ulong) ((IntPtr) &value3);
                dataPtr[2].Size = 8;
                dataPtr[2].Reserved = 0;
                this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 3, (IntPtr) dataPtr);
                if (this.m_eventData[eventId].CaptureStack)
                {
                    this.CaptureStack();
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value1, value2, value3 });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, string value1, int value2, int value3)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                fixed (char* str = ((char*) value1))
                {
                    char* chPtr = str;
                    EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 3) * sizeof(EventProvider.EventData))];
                    dataPtr->Ptr = (ulong) chPtr;
                    dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                    dataPtr->Reserved = 0;
                    dataPtr[1].Ptr = (ulong) ((IntPtr) &value2);
                    dataPtr[1].Size = 4;
                    dataPtr[1].Reserved = 0;
                    dataPtr[2].Ptr = (ulong) ((IntPtr) &value3);
                    dataPtr[2].Size = 4;
                    dataPtr[2].Reserved = 0;
                    this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 3, (IntPtr) dataPtr);
                    if (this.m_eventData[eventId].CaptureStack)
                    {
                        this.CaptureStack();
                    }
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value1, value2, value3 });
            }
        }

        [SecuritySafeCritical]
        protected unsafe void WriteEvent(int eventId, string value1, string value2, string value3)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                fixed (char* str = ((char*) value1))
                {
                    char* chPtr = str;
                    fixed (char* str2 = ((char*) value2))
                    {
                        char* chPtr2 = str2;
                        fixed (char* str3 = ((char*) value3))
                        {
                            char* chPtr3 = str3;
                            EventProvider.EventData* dataPtr = (EventProvider.EventData*) stackalloc byte[(((IntPtr) 3) * sizeof(EventProvider.EventData))];
                            dataPtr->Ptr = (ulong) chPtr;
                            dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                            dataPtr->Reserved = 0;
                            dataPtr[1].Ptr = (ulong) chPtr2;
                            dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                            dataPtr[1].Reserved = 0;
                            dataPtr[2].Ptr = (ulong) chPtr3;
                            dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                            dataPtr[2].Reserved = 0;
                            this.m_provider.WriteEvent(ref this.m_eventData[eventId].Descriptor, 3, (IntPtr) dataPtr);
                            if (this.m_eventData[eventId].CaptureStack)
                            {
                                this.CaptureStack();
                            }
                        }
                    }
                }
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(eventId, new object[] { value1, value2, value3 });
            }
        }

        public void WriteMessage(string eventMessage)
        {
            this.WriteMessage(eventMessage, EventLevel.LogAlways, EventKeywords.None);
        }

        public void WriteMessage(string eventMessage, EventLevel level, EventKeywords keywords)
        {
            if (this.m_provider != null)
            {
                this.m_provider.WriteMessageEvent(eventMessage, (byte) level, (long) keywords);
            }
            this.WriteToAllStreams(0, new object[] { eventMessage });
        }

        private void WriteToAllStreams(int eventId, params object[] args)
        {
            this.m_eventCallbackArgs.EventId = eventId;
            this.m_eventCallbackArgs.Payload = args;
            for (EventProviderDataStream stream = this.m_OutputStreams; stream != null; stream = stream.m_Next)
            {
                if (stream.m_EventEnabled[eventId])
                {
                    stream.m_Callback(this.m_eventCallbackArgs);
                }
            }
            if (this.m_eventData[eventId].CaptureStack)
            {
                this.CaptureStack();
            }
        }

        [SecuritySafeCritical]
        protected void WriteTransferEventHelper(int eventId, System.Guid relatedActivityId, params object[] args)
        {
            if ((this.m_provider != null) && this.m_eventData[eventId].EnabledForETW)
            {
                this.m_provider.WriteTransferEvent(ref this.m_eventData[eventId].Descriptor, relatedActivityId, args);
            }
            if ((this.m_OutputStreams != null) && this.m_eventData[eventId].EnabledForAnyStream)
            {
                this.WriteToAllStreams(0, args);
            }
        }

        protected internal int EventIdLimit
        {
            get
            {
                this.InsureInitialized();
                return this.m_eventData.Length;
            }
        }

        public System.Guid Guid
        {
            get
            {
                return this.m_guid;
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct EventData
        {
            public EventDescriptorInternal Descriptor;
            public string Message;
            public bool EnabledForAnyStream;
            public bool EnabledForETW;
            public bool CaptureStack;
        }

        private class OverideEventProvider : EventProvider
        {
            private EventProviderBase m_eventProviderBase;

            public OverideEventProvider(EventProviderBase eventProvider)
            {
                this.m_eventProviderBase = eventProvider;
            }

            protected override void OnControllerCommand(ControllerCommand command, IDictionary<string, string> arguments)
            {
                EventProviderDataStream outputStream = null;
                this.m_eventProviderBase.SendCommand(outputStream, base.IsEnabled(), base.Level, base.MatchAnyKeyword, command, arguments);
            }
        }
    }
}

