namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;

    [Serializable, CLSCompliant(false), ComVisible(true)]
    public abstract class Formatter : IFormatter
    {
        protected ObjectIDGenerator m_idGenerator = new ObjectIDGenerator();
        protected Queue m_objectQueue = new Queue();

        protected Formatter()
        {
        }

        public abstract object Deserialize(Stream serializationStream);
        protected virtual object GetNext(out long objID)
        {
            bool flag;
            if (this.m_objectQueue.Count == 0)
            {
                objID = 0L;
                return null;
            }
            object obj2 = this.m_objectQueue.Dequeue();
            objID = this.m_idGenerator.HasId(obj2, out flag);
            if (flag)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_NoID"));
            }
            return obj2;
        }

        protected virtual long Schedule(object obj)
        {
            bool flag;
            if (obj == null)
            {
                return 0L;
            }
            long id = this.m_idGenerator.GetId(obj, out flag);
            if (flag)
            {
                this.m_objectQueue.Enqueue(obj);
            }
            return id;
        }

        public abstract void Serialize(Stream serializationStream, object graph);
        protected abstract void WriteArray(object obj, string name, Type memberType);
        protected abstract void WriteBoolean(bool val, string name);
        protected abstract void WriteByte(byte val, string name);
        protected abstract void WriteChar(char val, string name);
        protected abstract void WriteDateTime(DateTime val, string name);
        protected abstract void WriteDecimal(decimal val, string name);
        protected abstract void WriteDouble(double val, string name);
        protected abstract void WriteInt16(short val, string name);
        protected abstract void WriteInt32(int val, string name);
        protected abstract void WriteInt64(long val, string name);
        protected virtual void WriteMember(string memberName, object data)
        {
            if (data == null)
            {
                this.WriteObjectRef(data, memberName, typeof(object));
            }
            else
            {
                Type memberType = data.GetType();
                if (memberType == typeof(bool))
                {
                    this.WriteBoolean(Convert.ToBoolean(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(char))
                {
                    this.WriteChar(Convert.ToChar(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(sbyte))
                {
                    this.WriteSByte(Convert.ToSByte(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(byte))
                {
                    this.WriteByte(Convert.ToByte(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(short))
                {
                    this.WriteInt16(Convert.ToInt16(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(int))
                {
                    this.WriteInt32(Convert.ToInt32(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(long))
                {
                    this.WriteInt64(Convert.ToInt64(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(float))
                {
                    this.WriteSingle(Convert.ToSingle(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(double))
                {
                    this.WriteDouble(Convert.ToDouble(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(DateTime))
                {
                    this.WriteDateTime(Convert.ToDateTime(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(decimal))
                {
                    this.WriteDecimal(Convert.ToDecimal(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(ushort))
                {
                    this.WriteUInt16(Convert.ToUInt16(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(uint))
                {
                    this.WriteUInt32(Convert.ToUInt32(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType == typeof(ulong))
                {
                    this.WriteUInt64(Convert.ToUInt64(data, CultureInfo.InvariantCulture), memberName);
                }
                else if (memberType.IsArray)
                {
                    this.WriteArray(data, memberName, memberType);
                }
                else if (memberType.IsValueType)
                {
                    this.WriteValueType(data, memberName, memberType);
                }
                else
                {
                    this.WriteObjectRef(data, memberName, memberType);
                }
            }
        }

        protected abstract void WriteObjectRef(object obj, string name, Type memberType);
        [CLSCompliant(false)]
        protected abstract void WriteSByte(sbyte val, string name);
        protected abstract void WriteSingle(float val, string name);
        protected abstract void WriteTimeSpan(TimeSpan val, string name);
        [CLSCompliant(false)]
        protected abstract void WriteUInt16(ushort val, string name);
        [CLSCompliant(false)]
        protected abstract void WriteUInt32(uint val, string name);
        [CLSCompliant(false)]
        protected abstract void WriteUInt64(ulong val, string name);
        protected abstract void WriteValueType(object obj, string name, Type memberType);

        public abstract SerializationBinder Binder { get; set; }

        public abstract StreamingContext Context { get; set; }

        public abstract ISurrogateSelector SurrogateSelector { get; set; }
    }
}

