namespace System.Messaging
{
    using System;
    using System.ComponentModel;
    using System.Messaging.Design;

    [TypeConverter(typeof(MessageFormatterConverter))]
    public interface IMessageFormatter : ICloneable
    {
        bool CanRead(Message message);
        object Read(Message message);
        void Write(Message message, object obj);
    }
}

