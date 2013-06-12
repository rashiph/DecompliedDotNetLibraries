namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.UI.WebControls;

    public sealed class ObjectStateFormatter : IStateFormatter, IFormatter
    {
        private byte[] _macKeyBytes;
        private Page _page;
        private static readonly Stack _streams = new Stack();
        private string[] _stringList;
        private IDictionary _stringTable;
        private int _stringTableCount;
        private bool _throwOnErrorDeserializing;
        private IList _typeList;
        private IDictionary _typeTable;
        private static readonly Type[] KnownTypes = new Type[] { typeof(object), typeof(int), typeof(string), typeof(bool) };
        private const byte Marker_Format = 0xff;
        private const byte Marker_Version_1 = 1;
        private const int StringTableSize = 0xff;
        private const byte Token_Array = 20;
        private const byte Token_ArrayList = 0x16;
        private const byte Token_BinarySerialized = 50;
        private const byte Token_Byte = 3;
        private const byte Token_Char = 4;
        private const byte Token_Color = 9;
        private const byte Token_DateTime = 6;
        private const byte Token_Double = 7;
        private const byte Token_EmptyColor = 12;
        private const byte Token_EmptyString = 0x65;
        private const byte Token_EmptyUnit = 0x1c;
        private const byte Token_False = 0x68;
        private const byte Token_Hashtable = 0x17;
        private const byte Token_HybridDictionary = 0x18;
        private const byte Token_IndexedString = 0x1f;
        private const byte Token_IndexedStringAdd = 30;
        private const byte Token_Int16 = 1;
        private const byte Token_Int32 = 2;
        private const byte Token_IntEnum = 11;
        private const byte Token_KnownColor = 10;
        private const byte Token_Null = 100;
        private const byte Token_Pair = 15;
        private const byte Token_Single = 8;
        private const byte Token_SparseArray = 60;
        private const byte Token_String = 5;
        private const byte Token_StringArray = 0x15;
        private const byte Token_StringFormatted = 40;
        private const byte Token_Triplet = 0x10;
        private const byte Token_True = 0x67;
        private const byte Token_Type = 0x19;
        private const byte Token_TypeRef = 0x2b;
        private const byte Token_TypeRefAdd = 0x29;
        private const byte Token_TypeRefAddLocal = 0x2a;
        private const byte Token_Unit = 0x1b;
        private const byte Token_ZeroInt32 = 0x66;

        public ObjectStateFormatter() : this(null)
        {
        }

        internal ObjectStateFormatter(byte[] macEncodingKey) : this(null, true)
        {
            this._macKeyBytes = macEncodingKey;
        }

        internal ObjectStateFormatter(Page page, bool throwOnErrorDeserializing)
        {
            this._page = page;
            this._throwOnErrorDeserializing = throwOnErrorDeserializing;
        }

        private void AddDeserializationStringReference(string s)
        {
            if (this._stringTableCount == 0xff)
            {
                this._stringTableCount = 0;
            }
            this._stringList[this._stringTableCount] = s;
            this._stringTableCount++;
        }

        private void AddDeserializationTypeReference(Type type)
        {
            this._typeList.Add(type);
        }

        private void AddSerializationStringReference(string s)
        {
            if (this._stringTableCount == 0xff)
            {
                this._stringTableCount = 0;
            }
            string key = this._stringList[this._stringTableCount];
            if (key != null)
            {
                this._stringTable.Remove(key);
            }
            this._stringTable[s] = this._stringTableCount;
            this._stringList[this._stringTableCount] = s;
            this._stringTableCount++;
        }

        private void AddSerializationTypeReference(Type type)
        {
            int count = this._typeTable.Count;
            this._typeTable[type] = count;
        }

        public object Deserialize(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            Exception innerException = null;
            this.InitializeDeserializer();
            SerializerBinaryReader reader = new SerializerBinaryReader(inputStream);
            try
            {
                if ((reader.ReadByte() == 0xff) && (reader.ReadByte() == 1))
                {
                    return this.DeserializeValue(reader);
                }
            }
            catch (Exception exception2)
            {
                innerException = exception2;
            }
            throw new ArgumentException(System.Web.SR.GetString("InvalidSerializedData"), innerException);
        }

        public object Deserialize(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentNullException("inputString");
            }
            byte[] buf = Convert.FromBase64String(inputString);
            int length = buf.Length;
            try
            {
                if ((this._page != null) && this._page.ContainsEncryptedViewState)
                {
                    buf = MachineKeySection.EncryptOrDecryptData(false, buf, this.GetMacKeyModifier(), 0, length);
                    length = buf.Length;
                }
                else if (((this._page != null) && this._page.EnableViewStateMac) || (this._macKeyBytes != null))
                {
                    buf = MachineKeySection.GetDecodedData(buf, this.GetMacKeyModifier(), 0, length, ref length);
                }
            }
            catch
            {
                PerfCounters.IncrementCounter(AppPerfCounter.VIEWSTATE_MAC_FAIL);
                ViewStateException.ThrowMacValidationError(null, inputString);
            }
            object obj2 = null;
            MemoryStream memoryStream = GetMemoryStream();
            try
            {
                memoryStream.Write(buf, 0, length);
                memoryStream.Position = 0L;
                obj2 = this.Deserialize(memoryStream);
            }
            finally
            {
                ReleaseMemoryStream(memoryStream);
            }
            return obj2;
        }

        private IndexedString DeserializeIndexedString(SerializerBinaryReader reader, byte token)
        {
            if (token == 0x1f)
            {
                int index = reader.ReadByte();
                return new IndexedString(this._stringList[index]);
            }
            string s = reader.ReadString();
            this.AddDeserializationStringReference(s);
            return new IndexedString(s);
        }

        private Type DeserializeType(SerializerBinaryReader reader)
        {
            byte num = reader.ReadByte();
            if (num == 0x2b)
            {
                int num2 = reader.ReadEncodedInt32();
                return (Type) this._typeList[num2];
            }
            string name = reader.ReadString();
            Type type = null;
            try
            {
                if (num == 0x2a)
                {
                    type = HttpContext.SystemWebAssembly.GetType(name, true);
                }
                else
                {
                    type = Type.GetType(name, true);
                }
            }
            catch (Exception exception)
            {
                if (this._throwOnErrorDeserializing)
                {
                    throw;
                }
                WebBaseEvent.RaiseSystemEvent(System.Web.SR.GetString("Webevent_msg_OSF_Deserialization_Type", new object[] { name }), this, 0xbc3, 0, exception);
            }
            this.AddDeserializationTypeReference(type);
            return type;
        }

        private object DeserializeValue(SerializerBinaryReader reader)
        {
            int num4;
            IDictionary dictionary;
            byte token = reader.ReadByte();
            switch (token)
            {
                case 1:
                    return reader.ReadInt16();

                case 2:
                    return reader.ReadEncodedInt32();

                case 3:
                    return reader.ReadByte();

                case 4:
                    return reader.ReadChar();

                case 5:
                    return reader.ReadString();

                case 6:
                    return DateTime.FromBinary(reader.ReadInt64());

                case 7:
                    return reader.ReadDouble();

                case 8:
                    return reader.ReadSingle();

                case 9:
                    return Color.FromArgb(reader.ReadInt32());

                case 10:
                    return Color.FromKnownColor((KnownColor) reader.ReadEncodedInt32());

                case 11:
                {
                    Type enumType = this.DeserializeType(reader);
                    int num10 = reader.ReadEncodedInt32();
                    return Enum.ToObject(enumType, num10);
                }
                case 12:
                    return Color.Empty;

                case 15:
                    return new Pair(this.DeserializeValue(reader), this.DeserializeValue(reader));

                case 0x10:
                    return new Triplet(this.DeserializeValue(reader), this.DeserializeValue(reader), this.DeserializeValue(reader));

                case 20:
                {
                    Type elementType = this.DeserializeType(reader);
                    int length = reader.ReadEncodedInt32();
                    Array array = Array.CreateInstance(elementType, length);
                    for (int j = 0; j < length; j++)
                    {
                        array.SetValue(this.DeserializeValue(reader), j);
                    }
                    return array;
                }
                case 0x15:
                {
                    int num6 = reader.ReadEncodedInt32();
                    string[] strArray = new string[num6];
                    for (int k = 0; k < num6; k++)
                    {
                        strArray[k] = reader.ReadString();
                    }
                    return strArray;
                }
                case 0x16:
                {
                    int capacity = reader.ReadEncodedInt32();
                    ArrayList list = new ArrayList(capacity);
                    for (int m = 0; m < capacity; m++)
                    {
                        list.Add(this.DeserializeValue(reader));
                    }
                    return list;
                }
                case 0x17:
                case 0x18:
                    num4 = reader.ReadEncodedInt32();
                    if (token != 0x17)
                    {
                        dictionary = new HybridDictionary(num4);
                        break;
                    }
                    dictionary = new Hashtable(num4);
                    break;

                case 0x19:
                    return this.DeserializeType(reader);

                case 0x1b:
                    return new Unit(reader.ReadDouble(), (UnitType) reader.ReadInt32());

                case 0x1c:
                    return Unit.Empty;

                case 30:
                case 0x1f:
                    return this.DeserializeIndexedString(reader, token);

                case 40:
                {
                    object obj2 = null;
                    Type type = this.DeserializeType(reader);
                    string text = reader.ReadString();
                    if (type != null)
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(type);
                        try
                        {
                            obj2 = converter.ConvertFromInvariantString(text);
                        }
                        catch (Exception exception)
                        {
                            if (this._throwOnErrorDeserializing)
                            {
                                throw;
                            }
                            WebBaseEvent.RaiseSystemEvent(System.Web.SR.GetString("Webevent_msg_OSF_Deserialization_String", new object[] { type.AssemblyQualifiedName }), this, 0xbc3, 0, exception);
                        }
                    }
                    return obj2;
                }
                case 50:
                {
                    int count = reader.ReadEncodedInt32();
                    byte[] buffer = new byte[count];
                    if (count != 0)
                    {
                        reader.Read(buffer, 0, count);
                    }
                    object obj3 = null;
                    MemoryStream memoryStream = GetMemoryStream();
                    try
                    {
                        memoryStream.Write(buffer, 0, count);
                        memoryStream.Position = 0L;
                        IFormatter formatter = new BinaryFormatter();
                        obj3 = formatter.Deserialize(memoryStream);
                    }
                    catch (Exception exception2)
                    {
                        if (this._throwOnErrorDeserializing)
                        {
                            throw;
                        }
                        WebBaseEvent.RaiseSystemEvent(System.Web.SR.GetString("Webevent_msg_OSF_Deserialization_Binary"), this, 0xbc3, 0, exception2);
                    }
                    finally
                    {
                        ReleaseMemoryStream(memoryStream);
                    }
                    return obj3;
                }
                case 60:
                {
                    Type type3 = this.DeserializeType(reader);
                    int num11 = reader.ReadEncodedInt32();
                    int num12 = reader.ReadEncodedInt32();
                    if (num12 > num11)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("InvalidSerializedData"));
                    }
                    Array array2 = Array.CreateInstance(type3, num11);
                    for (int n = 0; n < num12; n++)
                    {
                        int index = reader.ReadEncodedInt32();
                        if ((index >= num11) || (index < 0))
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("InvalidSerializedData"));
                        }
                        array2.SetValue(this.DeserializeValue(reader), index);
                    }
                    return array2;
                }
                case 100:
                    return null;

                case 0x65:
                    return string.Empty;

                case 0x66:
                    return 0;

                case 0x67:
                    return true;

                case 0x68:
                    return false;

                default:
                    throw new InvalidOperationException(System.Web.SR.GetString("InvalidSerializedData"));
            }
            for (int i = 0; i < num4; i++)
            {
                dictionary.Add(this.DeserializeValue(reader), this.DeserializeValue(reader));
            }
            return dictionary;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        internal object DeserializeWithAssert(Stream inputStream)
        {
            return this.Deserialize(inputStream);
        }

        private byte[] GetMacKeyModifier()
        {
            if (this._macKeyBytes == null)
            {
                if (this._page == null)
                {
                    return null;
                }
                int num = StringComparer.InvariantCultureIgnoreCase.GetHashCode(this._page.TemplateSourceDirectory) + StringComparer.InvariantCultureIgnoreCase.GetHashCode(this._page.GetType().Name);
                string viewStateUserKey = this._page.ViewStateUserKey;
                if (viewStateUserKey != null)
                {
                    int byteCount = Encoding.Unicode.GetByteCount(viewStateUserKey);
                    this._macKeyBytes = new byte[byteCount + 4];
                    Encoding.Unicode.GetBytes(viewStateUserKey, 0, viewStateUserKey.Length, this._macKeyBytes, 4);
                }
                else
                {
                    this._macKeyBytes = new byte[4];
                }
                this._macKeyBytes[0] = (byte) num;
                this._macKeyBytes[1] = (byte) (num >> 8);
                this._macKeyBytes[2] = (byte) (num >> 0x10);
                this._macKeyBytes[3] = (byte) (num >> 0x18);
            }
            return this._macKeyBytes;
        }

        private static MemoryStream GetMemoryStream()
        {
            MemoryStream stream = null;
            if (_streams.Count > 0)
            {
                lock (_streams)
                {
                    if (_streams.Count > 0)
                    {
                        stream = (MemoryStream) _streams.Pop();
                    }
                }
            }
            if (stream == null)
            {
                stream = new MemoryStream(0x800);
            }
            return stream;
        }

        private void InitializeDeserializer()
        {
            this._typeList = new ArrayList();
            for (int i = 0; i < KnownTypes.Length; i++)
            {
                this.AddDeserializationTypeReference(KnownTypes[i]);
            }
            this._stringList = new string[0xff];
            this._stringTableCount = 0;
        }

        private void InitializeSerializer()
        {
            this._typeTable = new HybridDictionary();
            for (int i = 0; i < KnownTypes.Length; i++)
            {
                this.AddSerializationTypeReference(KnownTypes[i]);
            }
            this._stringList = new string[0xff];
            this._stringTable = new Hashtable();
            this._stringTableCount = 0;
        }

        private static void ReleaseMemoryStream(MemoryStream stream)
        {
            stream.Position = 0L;
            stream.SetLength(0L);
            lock (_streams)
            {
                _streams.Push(stream);
            }
        }

        public string Serialize(object stateGraph)
        {
            string str = null;
            MemoryStream memoryStream = GetMemoryStream();
            try
            {
                this.Serialize(memoryStream, stateGraph);
                memoryStream.SetLength(memoryStream.Position);
                byte[] buf = memoryStream.GetBuffer();
                int length = (int) memoryStream.Length;
                if ((this._page != null) && this._page.RequiresViewStateEncryptionInternal)
                {
                    buf = MachineKeySection.EncryptOrDecryptData(true, buf, this.GetMacKeyModifier(), 0, length);
                    length = buf.Length;
                }
                else if (((this._page != null) && this._page.EnableViewStateMac) || (this._macKeyBytes != null))
                {
                    buf = MachineKeySection.GetEncodedData(buf, this.GetMacKeyModifier(), 0, ref length);
                }
                str = Convert.ToBase64String(buf, 0, length);
            }
            finally
            {
                ReleaseMemoryStream(memoryStream);
            }
            return str;
        }

        public void Serialize(Stream outputStream, object stateGraph)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }
            this.InitializeSerializer();
            SerializerBinaryWriter writer = new SerializerBinaryWriter(outputStream);
            writer.Write((byte) 0xff);
            writer.Write((byte) 1);
            this.SerializeValue(writer, stateGraph);
        }

        private void SerializeIndexedString(SerializerBinaryWriter writer, string s)
        {
            object obj2 = this._stringTable[s];
            if (obj2 != null)
            {
                writer.Write((byte) 0x1f);
                writer.Write((byte) ((int) obj2));
            }
            else
            {
                this.AddSerializationStringReference(s);
                writer.Write((byte) 30);
                writer.Write(s);
            }
        }

        private void SerializeType(SerializerBinaryWriter writer, Type type)
        {
            object obj2 = this._typeTable[type];
            if (obj2 != null)
            {
                writer.Write((byte) 0x2b);
                writer.WriteEncoded((int) obj2);
            }
            else
            {
                this.AddSerializationTypeReference(type);
                if (type.Assembly == HttpContext.SystemWebAssembly)
                {
                    writer.Write((byte) 0x2a);
                    writer.Write(type.FullName);
                }
                else
                {
                    writer.Write((byte) 0x29);
                    writer.Write(type.AssemblyQualifiedName);
                }
            }
        }

        private void SerializeValue(SerializerBinaryWriter writer, object value)
        {
            try
            {
                Stack stack = new Stack();
                stack.Push(value);
            Label_000D:
                value = stack.Pop();
                if (value == null)
                {
                    writer.Write((byte) 100);
                }
                else if (value is string)
                {
                    string str = (string) value;
                    if (str.Length == 0)
                    {
                        writer.Write((byte) 0x65);
                    }
                    else
                    {
                        writer.Write((byte) 5);
                        writer.Write(str);
                    }
                }
                else if (value is int)
                {
                    int num = (int) value;
                    if (num == 0)
                    {
                        writer.Write((byte) 0x66);
                    }
                    else
                    {
                        writer.Write((byte) 2);
                        writer.WriteEncoded(num);
                    }
                }
                else if (value is Pair)
                {
                    writer.Write((byte) 15);
                    Pair pair = (Pair) value;
                    stack.Push(pair.Second);
                    stack.Push(pair.First);
                }
                else if (value is Triplet)
                {
                    writer.Write((byte) 0x10);
                    Triplet triplet = (Triplet) value;
                    stack.Push(triplet.Third);
                    stack.Push(triplet.Second);
                    stack.Push(triplet.First);
                }
                else if (value is IndexedString)
                {
                    this.SerializeIndexedString(writer, ((IndexedString) value).Value);
                }
                else if (value.GetType() == typeof(ArrayList))
                {
                    writer.Write((byte) 0x16);
                    ArrayList list = (ArrayList) value;
                    writer.WriteEncoded(list.Count);
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        stack.Push(list[i]);
                    }
                }
                else if (value is bool)
                {
                    if ((bool) value)
                    {
                        writer.Write((byte) 0x67);
                    }
                    else
                    {
                        writer.Write((byte) 0x68);
                    }
                }
                else if (value is byte)
                {
                    writer.Write((byte) 3);
                    writer.Write((byte) value);
                }
                else if (value is char)
                {
                    writer.Write((byte) 4);
                    writer.Write((char) value);
                }
                else if (value is DateTime)
                {
                    writer.Write((byte) 6);
                    writer.Write(((DateTime) value).ToBinary());
                }
                else if (value is double)
                {
                    writer.Write((byte) 7);
                    writer.Write((double) value);
                }
                else if (value is short)
                {
                    writer.Write((byte) 1);
                    writer.Write((short) value);
                }
                else if (value is float)
                {
                    writer.Write((byte) 8);
                    writer.Write((float) value);
                }
                else
                {
                    if (value is IDictionary)
                    {
                        bool flag = false;
                        if (value.GetType() == typeof(Hashtable))
                        {
                            writer.Write((byte) 0x17);
                            flag = true;
                        }
                        else if (value.GetType() == typeof(HybridDictionary))
                        {
                            writer.Write((byte) 0x18);
                            flag = true;
                        }
                        if (flag)
                        {
                            IDictionary dictionary = (IDictionary) value;
                            writer.WriteEncoded(dictionary.Count);
                            if (dictionary.Count != 0)
                            {
                                foreach (DictionaryEntry entry in dictionary)
                                {
                                    stack.Push(entry.Value);
                                    stack.Push(entry.Key);
                                }
                            }
                            goto Label_06C3;
                        }
                    }
                    if (value is Type)
                    {
                        writer.Write((byte) 0x19);
                        this.SerializeType(writer, (Type) value);
                    }
                    else
                    {
                        Type enumType = value.GetType();
                        if (value is Array)
                        {
                            if (((Array) value).Rank <= 1)
                            {
                                Type elementType = enumType.GetElementType();
                                if (elementType == typeof(string))
                                {
                                    string[] strArray = (string[]) value;
                                    bool flag2 = false;
                                    for (int k = 0; k < strArray.Length; k++)
                                    {
                                        if (strArray[k] == null)
                                        {
                                            flag2 = true;
                                            break;
                                        }
                                    }
                                    if (!flag2)
                                    {
                                        writer.Write((byte) 0x15);
                                        writer.WriteEncoded(strArray.Length);
                                        for (int m = 0; m < strArray.Length; m++)
                                        {
                                            writer.Write(strArray[m]);
                                        }
                                        goto Label_06C3;
                                    }
                                }
                                Array array = (Array) value;
                                if (array.Length > 3)
                                {
                                    int capacity = (array.Length / 4) + 1;
                                    int num6 = 0;
                                    List<int> list2 = new List<int>(capacity);
                                    for (int n = 0; n < array.Length; n++)
                                    {
                                        if (array.GetValue(n) != null)
                                        {
                                            num6++;
                                            if (num6 >= capacity)
                                            {
                                                break;
                                            }
                                            list2.Add(n);
                                        }
                                    }
                                    if (num6 < capacity)
                                    {
                                        writer.Write((byte) 60);
                                        this.SerializeType(writer, elementType);
                                        writer.WriteEncoded(array.Length);
                                        writer.WriteEncoded(num6);
                                        foreach (int num8 in list2)
                                        {
                                            writer.WriteEncoded(num8);
                                            this.SerializeValue(writer, array.GetValue(num8));
                                        }
                                        goto Label_06C3;
                                    }
                                }
                                writer.Write((byte) 20);
                                this.SerializeType(writer, elementType);
                                writer.WriteEncoded(array.Length);
                                for (int j = array.Length - 1; j >= 0; j--)
                                {
                                    stack.Push(array.GetValue(j));
                                }
                            }
                        }
                        else if (enumType.IsEnum && (Enum.GetUnderlyingType(enumType) == typeof(int)))
                        {
                            writer.Write((byte) 11);
                            this.SerializeType(writer, enumType);
                            writer.WriteEncoded((int) value);
                        }
                        else if (enumType == typeof(Color))
                        {
                            Color color = (Color) value;
                            if (color.IsEmpty)
                            {
                                writer.Write((byte) 12);
                            }
                            else if (!color.IsNamedColor)
                            {
                                writer.Write((byte) 9);
                                writer.Write(color.ToArgb());
                            }
                            else
                            {
                                writer.Write((byte) 10);
                                writer.WriteEncoded((int) color.ToKnownColor());
                            }
                        }
                        else if (value is Unit)
                        {
                            Unit unit = (Unit) value;
                            if (unit.IsEmpty)
                            {
                                writer.Write((byte) 0x1c);
                            }
                            else
                            {
                                writer.Write((byte) 0x1b);
                                writer.Write(unit.Value);
                                writer.Write((int) unit.Type);
                            }
                        }
                        else
                        {
                            TypeConverter converter = TypeDescriptor.GetConverter(enumType);
                            if (Util.CanConvertToFrom(converter, typeof(string)))
                            {
                                writer.Write((byte) 40);
                                this.SerializeType(writer, enumType);
                                writer.Write(converter.ConvertToInvariantString(null, value));
                            }
                            else
                            {
                                IFormatter formatter = new BinaryFormatter();
                                MemoryStream serializationStream = new MemoryStream(0x100);
                                formatter.Serialize(serializationStream, value);
                                byte[] buffer = serializationStream.GetBuffer();
                                int length = (int) serializationStream.Length;
                                writer.Write((byte) 50);
                                writer.WriteEncoded(length);
                                if (buffer.Length != 0)
                                {
                                    writer.Write(buffer, 0, length);
                                }
                            }
                        }
                    }
                }
            Label_06C3:
                if (stack.Count > 0)
                {
                    goto Label_000D;
                }
            }
            catch (Exception exception)
            {
                throw new ArgumentException(System.Web.SR.GetString("ErrorSerializingValue", new object[] { value.ToString(), value.GetType().FullName }), exception);
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        internal void SerializeWithAssert(Stream outputStream, object stateGraph)
        {
            this.Serialize(outputStream, stateGraph);
        }

        object IFormatter.Deserialize(Stream serializationStream)
        {
            return this.Deserialize(serializationStream);
        }

        void IFormatter.Serialize(Stream serializationStream, object stateGraph)
        {
            this.Serialize(serializationStream, stateGraph);
        }

        object IStateFormatter.Deserialize(string serializedState)
        {
            return this.Deserialize(serializedState);
        }

        string IStateFormatter.Serialize(object state)
        {
            return this.Serialize(state);
        }

        SerializationBinder IFormatter.Binder
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        StreamingContext IFormatter.Context
        {
            get
            {
                return new StreamingContext(StreamingContextStates.All);
            }
            set
            {
            }
        }

        ISurrogateSelector IFormatter.SurrogateSelector
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        private sealed class SerializerBinaryReader : BinaryReader
        {
            public SerializerBinaryReader(Stream stream) : base(stream)
            {
            }

            public int ReadEncodedInt32()
            {
                return base.Read7BitEncodedInt();
            }
        }

        private sealed class SerializerBinaryWriter : BinaryWriter
        {
            public SerializerBinaryWriter(Stream stream) : base(stream)
            {
            }

            public void WriteEncoded(int value)
            {
                uint num = (uint) value;
                while (num >= 0x80)
                {
                    this.Write((byte) (num | 0x80));
                    num = num >> 7;
                }
                this.Write((byte) num);
            }
        }
    }
}

