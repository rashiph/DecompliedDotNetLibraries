namespace System.Diagnostics.Eventing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    internal class ManifestBuilder
    {
        private Dictionary<int, string> channelTab;
        private StringBuilder events;
        private Dictionary<ulong, string> keywordTab;
        private int numParams;
        private Dictionary<int, string> opcodeTab;
        private Guid providerGuid;
        private StringBuilder sb;
        private Dictionary<int, string> taskTab;
        private string templateName;
        private StringBuilder templates;

        public ManifestBuilder(string providerName, Guid providerGuid, string dllName)
        {
            this.providerGuid = providerGuid;
            this.sb = new StringBuilder();
            this.events = new StringBuilder();
            this.templates = new StringBuilder();
            this.opcodeTab = new Dictionary<int, string>();
            this.sb.Append("<provider name=\"").Append(providerName).Append("\" guid=\"{").Append(providerGuid.ToString()).Append("}");
            if (dllName != null)
            {
                this.sb.Append("\" resourceFileName=\"").Append(dllName).Append("\" messageFileName=\"").Append(dllName);
            }
            this.sb.Append("\" symbol=\"").Append(providerName).Append("\" >").AppendLine();
        }

        public void AddChannel(string name, int value)
        {
            if (this.channelTab == null)
            {
                this.channelTab = new Dictionary<int, string>();
            }
            this.channelTab[value] = name;
        }

        public void AddEventParameter(Type type, string name)
        {
            if (this.numParams == 0)
            {
                this.templates.Append("  <template tid=\"").Append(this.templateName).Append("\">").AppendLine();
            }
            this.numParams++;
            this.templates.Append("   <data name=\"").Append(name).Append("\" inType=\"").Append(GetTypeName(type)).Append("\"/>").AppendLine();
        }

        public void AddKeyword(string name, ulong value)
        {
            if ((value & (value - ((ulong) 1L))) != 0L)
            {
                throw new ArgumentException("Value " + value.ToString("x", CultureInfo.CurrentCulture) + " for keyword " + name + " needs to be a power of 2.");
            }
            if (this.keywordTab == null)
            {
                this.keywordTab = new Dictionary<ulong, string>();
            }
            this.keywordTab[value] = name;
        }

        public void AddOpcode(string name, int value)
        {
            this.opcodeTab[value] = name;
        }

        public void AddTask(string name, int value)
        {
            if (this.taskTab == null)
            {
                this.taskTab = new Dictionary<int, string>();
            }
            this.taskTab[value] = name;
        }

        public byte[] CreateManifest()
        {
            if (this.channelTab != null)
            {
                this.sb.Append(" <channels>").AppendLine();
                foreach (int num in this.channelTab.Keys)
                {
                    this.sb.Append("  <channel name=\"").Append(this.channelTab[num]).Append("\" value=\"").Append(num).Append("\"/>").AppendLine();
                }
                this.sb.Append(" </channels>").AppendLine();
            }
            if (this.taskTab != null)
            {
                this.sb.Append(" <tasks>").AppendLine();
                foreach (int num2 in this.taskTab.Keys)
                {
                    Guid guid = EventProvider.GenTaskGuidFromProviderGuid(this.providerGuid, (ushort) num2);
                    this.sb.Append("  <task name=\"").Append(this.taskTab[num2]).Append("\" eventGUID=\"{").Append(guid.ToString()).Append("}").Append("\" value=\"").Append(num2).Append("\"/>").AppendLine();
                }
                this.sb.Append(" </tasks>").AppendLine();
            }
            this.sb.Append(" <opcodes>").AppendLine();
            foreach (int num3 in this.opcodeTab.Keys)
            {
                this.sb.Append("  <opcode name=\"").Append(this.opcodeTab[num3]).Append("\" value=\"").Append(num3).Append("\"/>").AppendLine();
            }
            this.sb.Append(" </opcodes>").AppendLine();
            if (this.keywordTab != null)
            {
                this.sb.Append(" <keywords>").AppendLine();
                foreach (ulong num4 in this.keywordTab.Keys)
                {
                    StringBuilder introduced9 = this.sb.Append("  <keyword name=\"").Append(this.keywordTab[num4]).Append("\" mask=\"");
                    introduced9.Append(num4.ToString("x", CultureInfo.InvariantCulture)).Append("\"/>").AppendLine();
                }
                this.sb.Append(" </keywords>").AppendLine();
            }
            this.sb.Append(" <events>").AppendLine();
            this.sb.Append(this.events);
            this.sb.Append(" </events>").AppendLine();
            if (this.templates.Length > 0)
            {
                this.sb.Append(" <templates>").AppendLine();
                this.sb.Append(this.templates);
                this.sb.Append(" </templates>").AppendLine();
            }
            this.sb.Append("</provider>").AppendLine();
            return Encoding.UTF8.GetBytes(this.sb.ToString());
        }

        public void EndEvent()
        {
            if (this.numParams > 0)
            {
                this.templates.Append("  </template>").AppendLine();
                this.events.Append(" template=\"").Append(this.templateName).Append("\"");
            }
            this.events.Append("/>").AppendLine();
            this.templateName = null;
            this.numParams = 0;
        }

        private string GetChannelName(EventChannel channel, string eventName)
        {
            string str = null;
            if ((this.channelTab == null) || !this.channelTab.TryGetValue((int) channel, out str))
            {
                throw new ArgumentException(string.Concat(new object[] { "Use of undefined channel value ", channel, " for event ", eventName }));
            }
            return str;
        }

        private string GetKeywords(ulong keywords, string eventName)
        {
            string str = "";
            for (ulong i = 1L; i != 0L; i = i << 1)
            {
                if ((keywords & i) != 0L)
                {
                    string str2;
                    if ((this.keywordTab == null) || !this.keywordTab.TryGetValue(i, out str2))
                    {
                        throw new ArgumentException("Use of undefined keyword value " + i.ToString("x", CultureInfo.CurrentCulture) + " for event " + eventName);
                    }
                    if (str.Length != 0)
                    {
                        str = str + " ";
                    }
                    str = str + str2;
                }
            }
            return str;
        }

        private static string GetLevelName(EventLevel level)
        {
            return ("win:" + level.ToString());
        }

        private string GetOpcodeName(EventOpcode opcode, string eventName)
        {
            switch (opcode)
            {
                case EventOpcode.Info:
                    return "win:Info";

                case EventOpcode.Start:
                    return "win:Start";

                case EventOpcode.Stop:
                    return "win:Stop";

                case EventOpcode.DataCollectionStart:
                    return "win:DC_Start";

                case EventOpcode.DataCollectionStop:
                    return "win:DC_Stop";

                case EventOpcode.Extension:
                    return "win:Extension";

                case EventOpcode.Reply:
                    return "win:Reply";

                case EventOpcode.Resume:
                    return "win:Resume";

                case EventOpcode.Suspend:
                    return "win:Suspend";

                case EventOpcode.Send:
                    return "win:Send";

                case EventOpcode.Receive:
                    return "win:Receive";
            }
            string str = null;
            if (this.opcodeTab == null)
            {
                this.opcodeTab = new Dictionary<int, string>();
            }
            if (!this.opcodeTab.TryGetValue((int) opcode, out str))
            {
                this.opcodeTab[(int) opcode] = eventName;
            }
            return eventName;
        }

        private string GetTaskName(EventTask task, string eventName)
        {
            string str = null;
            if ((this.taskTab == null) || !this.taskTab.TryGetValue((int) task, out str))
            {
                throw new ArgumentException(string.Concat(new object[] { "Use of undefined task value ", task, " for event ", eventName }));
            }
            return str;
        }

        private static string GetTypeName(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "win:Boolean";

                case TypeCode.SByte:
                    return "win:Int8";

                case TypeCode.Byte:
                    return "win:Uint8";

                case TypeCode.Int16:
                    return "win:Int16";

                case TypeCode.UInt16:
                    return "win:UInt16";

                case TypeCode.Int32:
                    return "win:Int32";

                case TypeCode.UInt32:
                    return "win:UInt32";

                case TypeCode.Int64:
                    return "win:Int64";

                case TypeCode.UInt64:
                    return "win:UInt64";

                case TypeCode.Single:
                    return "win:Float";

                case TypeCode.Double:
                    return "win:Double";

                case TypeCode.String:
                    return "win:UnicodeString";
            }
            if (type != typeof(Guid))
            {
                throw new ArgumentException("Unsupported type " + type.Name);
            }
            return "win:GUID";
        }

        public void StartEvent(string eventName, EventAttribute eventAttribute)
        {
            this.templateName = eventName + "Args";
            this.numParams = 0;
            this.events.Append("  <event name=\"").Append(eventName).Append("\"").Append(" value=\"").Append(eventAttribute.EventId).Append("\"").Append(" version=\"").Append(eventAttribute.Version).Append("\"").Append(" level=\"").Append(GetLevelName(eventAttribute.Level)).Append("\"");
            if (eventAttribute.Keywords != EventKeywords.None)
            {
                this.events.Append(" keywords=\"").Append(this.GetKeywords((ulong) eventAttribute.Keywords, eventName)).Append("\"");
            }
            if (eventAttribute.Opcode != EventOpcode.Info)
            {
                this.events.Append(" opcode=\"").Append(this.GetOpcodeName(eventAttribute.Opcode, eventName)).Append("\"");
            }
            if (eventAttribute.Task != EventTask.None)
            {
                this.events.Append(" task=\"").Append(this.GetTaskName(eventAttribute.Task, eventName)).Append("\"");
            }
            if (eventAttribute.Channel != EventChannel.Default)
            {
                this.events.Append(" channel=\"").Append(this.GetChannelName(eventAttribute.Channel, eventName)).Append("\"");
            }
        }
    }
}

