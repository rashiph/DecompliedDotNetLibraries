namespace Microsoft.Transactions.Bridge
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class StateMachineHistory : IXmlSerializable
    {
        private List<StringCount> history = new List<StringCount>(30);

        public void AddEvent(string newEvent)
        {
            int num = this.history.Count;
            if (num > 0)
            {
                StringCount count = this.history[num - 1];
                if (count.Name == newEvent)
                {
                    count.Count++;
                    this.history[num - 1] = count;
                    return;
                }
            }
            this.history.Add(new StringCount(newEvent));
        }

        public void AddState(string newState)
        {
            this.history.Add(new StringCount(null));
            this.history.Add(new StringCount(newState));
        }

        private StringCount ReadEvent(ref int cursor)
        {
            if (cursor >= this.history.Count)
            {
                return StringCount.Null;
            }
            if (this.history[cursor].Name == null)
            {
                return StringCount.Null;
            }
            return this.history[cursor++];
        }

        private string ReadState(ref int cursor)
        {
            int count = this.history.Count;
            if (cursor >= count)
            {
                return null;
            }
            if (this.history[cursor].Name != null)
            {
                return null;
            }
            cursor++;
            int num1 = cursor;
            return this.history[cursor++].Name;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("StateMachineHistory");
            int cursor = 0;
            int num2 = this.history.Count;
            while (cursor < num2)
            {
                string str = this.ReadState(ref cursor);
                writer.WriteStartElement("state");
                writer.WriteAttributeString("name", str);
                while (true)
                {
                    StringCount count = this.ReadEvent(ref cursor);
                    if (count.Name == null)
                    {
                        break;
                    }
                    for (int i = 0; i < count.Count; i++)
                    {
                        writer.WriteStartElement("event");
                        writer.WriteAttributeString("name", count.Name);
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public override string ToString()
        {
            int num = this.history.Count;
            if (num == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            int cursor = 0;
            while (cursor < num)
            {
                string str = this.ReadState(ref cursor);
                builder.Append("[");
                builder.Append(str);
                builder.AppendLine("]");
                while (true)
                {
                    StringCount count = this.ReadEvent(ref cursor);
                    if (count.Name != null)
                    {
                        for (int i = 0; i < count.Count; i++)
                        {
                            builder.Append("\t{");
                            builder.Append(count.Name);
                            builder.AppendLine("}");
                        }
                    }
                }
            }
            return builder.ToString();
        }
    }
}

