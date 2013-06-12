namespace System.Resources
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization.Formatters.Soap;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;

    [Serializable, PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class ResXDataNode : ISerializable
    {
        private IFormatter binaryFormatter;
        private string comment;
        private ResXFileRef fileRef;
        private string fileRefFullPath;
        private string fileRefTextEncoding;
        private string fileRefType;
        private static ITypeResolutionService internalTypeResolver = new AssemblyNamesTypeResolutionService(new AssemblyName[] { new AssemblyName("System.Windows.Forms") });
        private string name;
        private DataNodeInfo nodeInfo;
        private static readonly char[] SpecialChars = new char[] { ' ', '\r', '\n' };
        private string typeName;
        private Func<System.Type, string> typeNameConverter;
        private object value;

        private ResXDataNode()
        {
        }

        internal ResXDataNode(DataNodeInfo nodeInfo, string basePath)
        {
            this.nodeInfo = nodeInfo;
            this.InitializeDataNode(basePath);
        }

        private ResXDataNode(SerializationInfo info, StreamingContext context)
        {
            DataNodeInfo info2 = new DataNodeInfo {
                Name = (string) info.GetValue("Name", typeof(string)),
                Comment = (string) info.GetValue("Comment", typeof(string)),
                TypeName = (string) info.GetValue("TypeName", typeof(string)),
                MimeType = (string) info.GetValue("MimeType", typeof(string)),
                ValueData = (string) info.GetValue("ValueData", typeof(string))
            };
            this.nodeInfo = info2;
            this.InitializeDataNode(null);
        }

        public ResXDataNode(string name, object value) : this(name, value, null)
        {
        }

        public ResXDataNode(string name, ResXFileRef fileRef) : this(name, fileRef, null)
        {
        }

        public ResXDataNode(string name, object value, Func<System.Type, string> typeNameConverter)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("name");
            }
            this.typeNameConverter = typeNameConverter;
            System.Type type = (value == null) ? typeof(object) : value.GetType();
            if ((value != null) && !type.IsSerializable)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("NotSerializableType", new object[] { name, type.FullName }));
            }
            if (value != null)
            {
                this.typeName = MultitargetUtil.GetAssemblyQualifiedName(type, this.typeNameConverter);
            }
            this.name = name;
            this.value = value;
        }

        public ResXDataNode(string name, ResXFileRef fileRef, Func<System.Type, string> typeNameConverter)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (fileRef == null)
            {
                throw new ArgumentNullException("fileRef");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("name");
            }
            this.name = name;
            this.fileRef = fileRef;
            this.typeNameConverter = typeNameConverter;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IFormatter CreateSoapFormatter()
        {
            return new SoapFormatter();
        }

        internal ResXDataNode DeepClone()
        {
            return new ResXDataNode { nodeInfo = (this.nodeInfo != null) ? this.nodeInfo.Clone() : null, name = this.name, comment = this.comment, typeName = this.typeName, fileRefFullPath = this.fileRefFullPath, fileRefType = this.fileRefType, fileRefTextEncoding = this.fileRefTextEncoding, value = this.value, fileRef = (this.fileRef != null) ? this.fileRef.Clone() : null, typeNameConverter = this.typeNameConverter };
        }

        private void FillDataNodeInfoFromObject(DataNodeInfo nodeInfo, object value)
        {
            CultureInfo info = value as CultureInfo;
            if (info != null)
            {
                nodeInfo.ValueData = info.Name;
                nodeInfo.TypeName = MultitargetUtil.GetAssemblyQualifiedName(typeof(CultureInfo), this.typeNameConverter);
            }
            else if (value is string)
            {
                nodeInfo.ValueData = (string) value;
                if (value == null)
                {
                    nodeInfo.TypeName = MultitargetUtil.GetAssemblyQualifiedName(typeof(ResXNullRef), this.typeNameConverter);
                }
            }
            else if (value is byte[])
            {
                nodeInfo.ValueData = ToBase64WrappedString((byte[]) value);
                nodeInfo.TypeName = MultitargetUtil.GetAssemblyQualifiedName(typeof(byte[]), this.typeNameConverter);
            }
            else
            {
                System.Type type = (value == null) ? typeof(object) : value.GetType();
                if ((value != null) && !type.IsSerializable)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("NotSerializableType", new object[] { this.name, type.FullName }));
                }
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                bool flag = converter.CanConvertTo(typeof(string));
                bool flag2 = converter.CanConvertFrom(typeof(string));
                try
                {
                    if (flag && flag2)
                    {
                        nodeInfo.ValueData = converter.ConvertToInvariantString(value);
                        nodeInfo.TypeName = MultitargetUtil.GetAssemblyQualifiedName(type, this.typeNameConverter);
                        return;
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                    {
                        throw;
                    }
                }
                bool flag3 = converter.CanConvertTo(typeof(byte[]));
                bool flag4 = converter.CanConvertFrom(typeof(byte[]));
                if (!flag3 || !flag4)
                {
                    if (value == null)
                    {
                        nodeInfo.ValueData = string.Empty;
                        nodeInfo.TypeName = MultitargetUtil.GetAssemblyQualifiedName(typeof(ResXNullRef), this.typeNameConverter);
                    }
                    else
                    {
                        if (this.binaryFormatter == null)
                        {
                            this.binaryFormatter = new BinaryFormatter();
                        }
                        MemoryStream serializationStream = new MemoryStream();
                        if (this.binaryFormatter == null)
                        {
                            this.binaryFormatter = new BinaryFormatter();
                        }
                        this.binaryFormatter.Serialize(serializationStream, value);
                        string str2 = ToBase64WrappedString(serializationStream.ToArray());
                        nodeInfo.ValueData = str2;
                        nodeInfo.MimeType = ResXResourceWriter.DefaultSerializedObjectMimeType;
                    }
                }
                else
                {
                    byte[] data = (byte[]) converter.ConvertTo(value, typeof(byte[]));
                    string str = ToBase64WrappedString(data);
                    nodeInfo.ValueData = str;
                    nodeInfo.MimeType = ResXResourceWriter.ByteArraySerializedObjectMimeType;
                    nodeInfo.TypeName = MultitargetUtil.GetAssemblyQualifiedName(type, this.typeNameConverter);
                }
            }
        }

        private static byte[] FromBase64WrappedString(string text)
        {
            if (text.IndexOfAny(SpecialChars) == -1)
            {
                return Convert.FromBase64String(text);
            }
            StringBuilder builder = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (((ch != '\n') && (ch != '\r')) && (ch != ' '))
                {
                    builder.Append(text[i]);
                }
            }
            return Convert.FromBase64String(builder.ToString());
        }

        private object GenerateObjectFromDataNodeInfo(DataNodeInfo dataNodeInfo, ITypeResolutionService typeResolver)
        {
            object obj2 = null;
            string mimeType = dataNodeInfo.MimeType;
            string typeName = ((dataNodeInfo.TypeName == null) || (dataNodeInfo.TypeName.Length == 0)) ? MultitargetUtil.GetAssemblyQualifiedName(typeof(string), this.typeNameConverter) : dataNodeInfo.TypeName;
            if ((mimeType != null) && (mimeType.Length > 0))
            {
                if ((string.Equals(mimeType, ResXResourceWriter.BinSerializedObjectMimeType) || string.Equals(mimeType, ResXResourceWriter.Beta2CompatSerializedObjectMimeType)) || string.Equals(mimeType, ResXResourceWriter.CompatBinSerializedObjectMimeType))
                {
                    byte[] buffer = FromBase64WrappedString(dataNodeInfo.ValueData);
                    if (this.binaryFormatter == null)
                    {
                        this.binaryFormatter = new BinaryFormatter();
                        this.binaryFormatter.Binder = new ResXSerializationBinder(typeResolver);
                    }
                    IFormatter binaryFormatter = this.binaryFormatter;
                    if ((buffer != null) && (buffer.Length > 0))
                    {
                        obj2 = binaryFormatter.Deserialize(new MemoryStream(buffer));
                        if (obj2 is ResXNullRef)
                        {
                            obj2 = null;
                        }
                    }
                    return obj2;
                }
                if (string.Equals(mimeType, ResXResourceWriter.SoapSerializedObjectMimeType) || string.Equals(mimeType, ResXResourceWriter.CompatSoapSerializedObjectMimeType))
                {
                    byte[] buffer2 = FromBase64WrappedString(dataNodeInfo.ValueData);
                    if ((buffer2 != null) && (buffer2.Length > 0))
                    {
                        obj2 = this.CreateSoapFormatter().Deserialize(new MemoryStream(buffer2));
                        if (obj2 is ResXNullRef)
                        {
                            obj2 = null;
                        }
                    }
                    return obj2;
                }
                if ((!string.Equals(mimeType, ResXResourceWriter.ByteArraySerializedObjectMimeType) || (typeName == null)) || (typeName.Length <= 0))
                {
                    return obj2;
                }
                System.Type type = this.ResolveType(typeName, typeResolver);
                if (type != null)
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    if (converter.CanConvertFrom(typeof(byte[])))
                    {
                        byte[] buffer3 = FromBase64WrappedString(dataNodeInfo.ValueData);
                        if (buffer3 != null)
                        {
                            obj2 = converter.ConvertFrom(buffer3);
                        }
                    }
                    return obj2;
                }
                string str6 = System.Windows.Forms.SR.GetString("TypeLoadException", new object[] { typeName, dataNodeInfo.ReaderPosition.Y, dataNodeInfo.ReaderPosition.X });
                XmlException exception = new XmlException(str6, null, dataNodeInfo.ReaderPosition.Y, dataNodeInfo.ReaderPosition.X);
                TypeLoadException exception2 = new TypeLoadException(str6, exception);
                throw exception2;
            }
            if ((typeName == null) || (typeName.Length <= 0))
            {
                return obj2;
            }
            System.Type type2 = this.ResolveType(typeName, typeResolver);
            if (type2 != null)
            {
                if (type2 == typeof(ResXNullRef))
                {
                    return null;
                }
                if ((typeName.IndexOf("System.Byte[]") != -1) && (typeName.IndexOf("mscorlib") != -1))
                {
                    return FromBase64WrappedString(dataNodeInfo.ValueData);
                }
                TypeConverter converter2 = TypeDescriptor.GetConverter(type2);
                if (!converter2.CanConvertFrom(typeof(string)))
                {
                    return obj2;
                }
                string valueData = dataNodeInfo.ValueData;
                try
                {
                    return converter2.ConvertFromInvariantString(valueData);
                }
                catch (NotSupportedException exception3)
                {
                    string str8 = System.Windows.Forms.SR.GetString("NotSupported", new object[] { typeName, dataNodeInfo.ReaderPosition.Y, dataNodeInfo.ReaderPosition.X, exception3.Message });
                    XmlException innerException = new XmlException(str8, exception3, dataNodeInfo.ReaderPosition.Y, dataNodeInfo.ReaderPosition.X);
                    NotSupportedException exception5 = new NotSupportedException(str8, innerException);
                    throw exception5;
                }
            }
            string message = System.Windows.Forms.SR.GetString("TypeLoadException", new object[] { typeName, dataNodeInfo.ReaderPosition.Y, dataNodeInfo.ReaderPosition.X });
            XmlException inner = new XmlException(message, null, dataNodeInfo.ReaderPosition.Y, dataNodeInfo.ReaderPosition.X);
            TypeLoadException exception7 = new TypeLoadException(message, inner);
            throw exception7;
        }

        internal DataNodeInfo GetDataNodeInfo()
        {
            bool flag = true;
            if (this.nodeInfo != null)
            {
                flag = false;
            }
            else
            {
                this.nodeInfo = new DataNodeInfo();
            }
            this.nodeInfo.Name = this.Name;
            this.nodeInfo.Comment = this.Comment;
            if (flag || (this.FileRefFullPath != null))
            {
                if (this.FileRefFullPath != null)
                {
                    this.nodeInfo.ValueData = this.FileRef.ToString();
                    this.nodeInfo.MimeType = null;
                    this.nodeInfo.TypeName = MultitargetUtil.GetAssemblyQualifiedName(typeof(ResXFileRef), this.typeNameConverter);
                }
                else
                {
                    this.FillDataNodeInfoFromObject(this.nodeInfo, this.value);
                }
            }
            return this.nodeInfo;
        }

        public Point GetNodePosition()
        {
            if (this.nodeInfo == null)
            {
                return new Point();
            }
            return this.nodeInfo.ReaderPosition;
        }

        public object GetValue(ITypeResolutionService typeResolver)
        {
            if (this.value != null)
            {
                return this.value;
            }
            object obj2 = null;
            if (this.FileRefFullPath != null)
            {
                if (this.ResolveType(this.FileRefType, typeResolver) != null)
                {
                    if (this.FileRefTextEncoding != null)
                    {
                        this.fileRef = new ResXFileRef(this.FileRefFullPath, this.FileRefType, Encoding.GetEncoding(this.FileRefTextEncoding));
                    }
                    else
                    {
                        this.fileRef = new ResXFileRef(this.FileRefFullPath, this.FileRefType);
                    }
                    return TypeDescriptor.GetConverter(typeof(ResXFileRef)).ConvertFrom(this.fileRef.ToString());
                }
                TypeLoadException exception = new TypeLoadException(System.Windows.Forms.SR.GetString("TypeLoadExceptionShort", new object[] { this.FileRefType }));
                throw exception;
            }
            if ((obj2 == null) && (this.nodeInfo.ValueData != null))
            {
                return this.GenerateObjectFromDataNodeInfo(this.nodeInfo, typeResolver);
            }
            return null;
        }

        public object GetValue(AssemblyName[] names)
        {
            return this.GetValue(new AssemblyNamesTypeResolutionService(names));
        }

        public string GetValueTypeName(ITypeResolutionService typeResolver)
        {
            if ((this.typeName != null) && (this.typeName.Length > 0))
            {
                if (this.typeName.Equals(MultitargetUtil.GetAssemblyQualifiedName(typeof(ResXNullRef), this.typeNameConverter)))
                {
                    return MultitargetUtil.GetAssemblyQualifiedName(typeof(object), this.typeNameConverter);
                }
                return this.typeName;
            }
            string fileRefType = this.FileRefType;
            System.Type type = null;
            if (fileRefType != null)
            {
                type = this.ResolveType(this.FileRefType, typeResolver);
            }
            else if (this.nodeInfo != null)
            {
                fileRefType = this.nodeInfo.TypeName;
                if ((fileRefType == null) || (fileRefType.Length == 0))
                {
                    if ((this.nodeInfo.MimeType != null) && (this.nodeInfo.MimeType.Length > 0))
                    {
                        object obj2 = null;
                        try
                        {
                            obj2 = this.GenerateObjectFromDataNodeInfo(this.nodeInfo, typeResolver);
                        }
                        catch (Exception exception)
                        {
                            if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                            {
                                throw;
                            }
                            fileRefType = MultitargetUtil.GetAssemblyQualifiedName(typeof(object), this.typeNameConverter);
                        }
                        if (obj2 != null)
                        {
                            fileRefType = MultitargetUtil.GetAssemblyQualifiedName(obj2.GetType(), this.typeNameConverter);
                        }
                    }
                    else
                    {
                        fileRefType = MultitargetUtil.GetAssemblyQualifiedName(typeof(string), this.typeNameConverter);
                    }
                }
                else
                {
                    type = this.ResolveType(this.nodeInfo.TypeName, typeResolver);
                }
            }
            if (type == null)
            {
                return fileRefType;
            }
            if (type == typeof(ResXNullRef))
            {
                return MultitargetUtil.GetAssemblyQualifiedName(typeof(object), this.typeNameConverter);
            }
            return MultitargetUtil.GetAssemblyQualifiedName(type, this.typeNameConverter);
        }

        public string GetValueTypeName(AssemblyName[] names)
        {
            return this.GetValueTypeName(new AssemblyNamesTypeResolutionService(names));
        }

        private void InitializeDataNode(string basePath)
        {
            System.Type type = null;
            if (!string.IsNullOrEmpty(this.nodeInfo.TypeName))
            {
                type = internalTypeResolver.GetType(this.nodeInfo.TypeName, false, true);
            }
            if ((type != null) && type.Equals(typeof(ResXFileRef)))
            {
                string[] strArray = ResXFileRef.Converter.ParseResxFileRefString(this.nodeInfo.ValueData);
                if ((strArray != null) && (strArray.Length > 1))
                {
                    if (!Path.IsPathRooted(strArray[0]) && (basePath != null))
                    {
                        this.fileRefFullPath = Path.Combine(basePath, strArray[0]);
                    }
                    else
                    {
                        this.fileRefFullPath = strArray[0];
                    }
                    this.fileRefType = strArray[1];
                    if (strArray.Length > 2)
                    {
                        this.fileRefTextEncoding = strArray[2];
                    }
                }
            }
        }

        private System.Type ResolveType(string typeName, ITypeResolutionService typeResolver)
        {
            System.Type type = null;
            if (typeResolver != null)
            {
                type = typeResolver.GetType(typeName, false);
                if (type == null)
                {
                    string[] strArray = typeName.Split(new char[] { ',' });
                    if ((strArray != null) && (strArray.Length >= 2))
                    {
                        string name = strArray[0].Trim();
                        string str2 = strArray[1].Trim();
                        name = name + ", " + str2;
                        type = typeResolver.GetType(name, false);
                    }
                }
            }
            if (type == null)
            {
                type = System.Type.GetType(typeName, false);
            }
            return type;
        }

        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            DataNodeInfo dataNodeInfo = this.GetDataNodeInfo();
            si.AddValue("Name", dataNodeInfo.Name, typeof(string));
            si.AddValue("Comment", dataNodeInfo.Comment, typeof(string));
            si.AddValue("TypeName", dataNodeInfo.TypeName, typeof(string));
            si.AddValue("MimeType", dataNodeInfo.MimeType, typeof(string));
            si.AddValue("ValueData", dataNodeInfo.ValueData, typeof(string));
        }

        private static string ToBase64WrappedString(byte[] data)
        {
            string str = Convert.ToBase64String(data);
            if (str.Length <= 80)
            {
                return str;
            }
            StringBuilder builder = new StringBuilder(str.Length + ((str.Length / 80) * 3));
            int startIndex = 0;
            while (startIndex < (str.Length - 80))
            {
                builder.Append("\r\n");
                builder.Append("        ");
                builder.Append(str, startIndex, 80);
                startIndex += 80;
            }
            builder.Append("\r\n");
            builder.Append("        ");
            builder.Append(str, startIndex, str.Length - startIndex);
            builder.Append("\r\n");
            return builder.ToString();
        }

        public string Comment
        {
            get
            {
                string comment = this.comment;
                if ((comment == null) && (this.nodeInfo != null))
                {
                    comment = this.nodeInfo.Comment;
                }
                if (comment != null)
                {
                    return comment;
                }
                return "";
            }
            set
            {
                this.comment = value;
            }
        }

        public ResXFileRef FileRef
        {
            get
            {
                if (this.FileRefFullPath == null)
                {
                    return null;
                }
                if (this.fileRef == null)
                {
                    if (string.IsNullOrEmpty(this.fileRefTextEncoding))
                    {
                        this.fileRef = new ResXFileRef(this.FileRefFullPath, this.FileRefType);
                    }
                    else
                    {
                        this.fileRef = new ResXFileRef(this.FileRefFullPath, this.FileRefType, Encoding.GetEncoding(this.FileRefTextEncoding));
                    }
                }
                return this.fileRef;
            }
        }

        private string FileRefFullPath
        {
            get
            {
                string fileRefFullPath = (this.fileRef == null) ? null : this.fileRef.FileName;
                if (fileRefFullPath == null)
                {
                    fileRefFullPath = this.fileRefFullPath;
                }
                return fileRefFullPath;
            }
        }

        private string FileRefTextEncoding
        {
            get
            {
                string fileRefTextEncoding = (this.fileRef == null) ? null : ((this.fileRef.TextFileEncoding == null) ? null : this.fileRef.TextFileEncoding.BodyName);
                if (fileRefTextEncoding == null)
                {
                    fileRefTextEncoding = this.fileRefTextEncoding;
                }
                return fileRefTextEncoding;
            }
        }

        private string FileRefType
        {
            get
            {
                string fileRefType = (this.fileRef == null) ? null : this.fileRef.TypeName;
                if (fileRefType == null)
                {
                    fileRefType = this.fileRefType;
                }
                return fileRefType;
            }
        }

        public string Name
        {
            get
            {
                string name = this.name;
                if ((name == null) && (this.nodeInfo != null))
                {
                    name = this.nodeInfo.Name;
                }
                return name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Name");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException("Name");
                }
                this.name = value;
            }
        }
    }
}

