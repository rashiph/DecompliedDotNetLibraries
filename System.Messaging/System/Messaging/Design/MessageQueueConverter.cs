namespace System.Messaging.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Messaging;

    internal class MessageQueueConverter : TypeConverter
    {
        private static Hashtable componentsCreated = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        internal static void AddToCache(MessageQueue queue)
        {
            componentsCreated[queue.Path] = queue;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if ((value != null) && (value is string))
            {
                string path = ((string) value).Trim();
                if (path == string.Empty)
                {
                    return null;
                }
                if (path.CompareTo(Res.GetString("NotSet")) != 0)
                {
                    MessageQueue fromCache = GetFromCache(path);
                    if (fromCache == null)
                    {
                        fromCache = new MessageQueue(path);
                        AddToCache(fromCache);
                        if (context != null)
                        {
                            context.Container.Add(fromCache);
                        }
                    }
                    return fromCache;
                }
            }
            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == null) || !(destinationType == typeof(string)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (value != null)
            {
                return ((MessageQueue) value).Path;
            }
            return Res.GetString("NotSet");
        }

        internal static MessageQueue GetFromCache(string path)
        {
            if (componentsCreated.ContainsKey(path))
            {
                MessageQueue queue = (MessageQueue) componentsCreated[path];
                if (queue.Site == null)
                {
                    componentsCreated.Remove(path);
                }
                else
                {
                    if (queue.Path == path)
                    {
                        return queue;
                    }
                    componentsCreated.Remove(path);
                }
            }
            return null;
        }
    }
}

