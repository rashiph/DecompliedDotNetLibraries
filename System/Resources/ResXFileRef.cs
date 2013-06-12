namespace System.Resources
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, TypeConverter(typeof(ResXFileRef.Converter)), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class ResXFileRef
    {
        private string fileName;
        [OptionalField(VersionAdded=2)]
        private Encoding textFileEncoding;
        private string typeName;

        public ResXFileRef(string fileName, string typeName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            this.fileName = fileName;
            this.typeName = typeName;
        }

        public ResXFileRef(string fileName, string typeName, Encoding textFileEncoding) : this(fileName, typeName)
        {
            this.textFileEncoding = textFileEncoding;
        }

        internal ResXFileRef Clone()
        {
            return new ResXFileRef(this.fileName, this.typeName, this.textFileEncoding);
        }

        internal void MakeFilePathRelative(string basePath)
        {
            if ((basePath != null) && (basePath.Length != 0))
            {
                this.fileName = PathDifference(basePath, this.fileName, false);
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.textFileEncoding = null;
        }

        private static string PathDifference(string path1, string path2, bool compareCase)
        {
            int num2 = -1;
            int num = 0;
            while ((num < path1.Length) && (num < path2.Length))
            {
                if ((path1[num] != path2[num]) && (compareCase || (char.ToLower(path1[num], CultureInfo.InvariantCulture) != char.ToLower(path2[num], CultureInfo.InvariantCulture))))
                {
                    break;
                }
                if (path1[num] == Path.DirectorySeparatorChar)
                {
                    num2 = num;
                }
                num++;
            }
            if (num == 0)
            {
                return path2;
            }
            if ((num == path1.Length) && (num == path2.Length))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            while (num < path1.Length)
            {
                if (path1[num] == Path.DirectorySeparatorChar)
                {
                    builder.Append(".." + Path.DirectorySeparatorChar);
                }
                num++;
            }
            return (builder.ToString() + path2.Substring(num2 + 1));
        }

        public override string ToString()
        {
            string str = "";
            if ((this.fileName.IndexOf(";") != -1) || (this.fileName.IndexOf("\"") != -1))
            {
                str = str + "\"" + this.fileName + "\";";
            }
            else
            {
                str = str + this.fileName + ";";
            }
            str = str + this.typeName;
            if (this.textFileEncoding != null)
            {
                str = str + ";" + this.textFileEncoding.WebName;
            }
            return str;
        }

        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }

        public Encoding TextFileEncoding
        {
            get
            {
                return this.textFileEncoding;
            }
        }

        public string TypeName
        {
            get
            {
                return this.typeName;
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        public class Converter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return (sourceType == typeof(string));
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return (destinationType == typeof(string));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                object obj2 = null;
                string stringValue = value as string;
                if (stringValue == null)
                {
                    return obj2;
                }
                string[] strArray = ParseResxFileRefString(stringValue);
                string path = strArray[0];
                Type type = Type.GetType(strArray[1], true);
                if (type.Equals(typeof(string)))
                {
                    Encoding encoding = Encoding.Default;
                    if (strArray.Length > 2)
                    {
                        encoding = Encoding.GetEncoding(strArray[2]);
                    }
                    using (StreamReader reader = new StreamReader(path, encoding))
                    {
                        return reader.ReadToEnd();
                    }
                }
                byte[] buffer = null;
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int) stream.Length);
                }
                if (type.Equals(typeof(byte[])))
                {
                    return buffer;
                }
                MemoryStream stream2 = new MemoryStream(buffer);
                if (type.Equals(typeof(MemoryStream)))
                {
                    return stream2;
                }
                if (type.Equals(typeof(Bitmap)) && path.EndsWith(".ico"))
                {
                    Icon icon = new Icon(stream2);
                    return icon.ToBitmap();
                }
                return Activator.CreateInstance(type, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, new object[] { stream2 }, null);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                object obj2 = null;
                if (destinationType == typeof(string))
                {
                    obj2 = ((ResXFileRef) value).ToString();
                }
                return obj2;
            }

            internal static string[] ParseResxFileRefString(string stringValue)
            {
                string[] strArray = null;
                string str;
                string str2;
                if (stringValue == null)
                {
                    return strArray;
                }
                stringValue = stringValue.Trim();
                if (stringValue.StartsWith("\""))
                {
                    int num = stringValue.LastIndexOf("\"");
                    if ((num - 1) < 0)
                    {
                        throw new ArgumentException("value");
                    }
                    str = stringValue.Substring(1, num - 1);
                    if ((num + 2) > stringValue.Length)
                    {
                        throw new ArgumentException("value");
                    }
                    str2 = stringValue.Substring(num + 2);
                }
                else
                {
                    int index = stringValue.IndexOf(";");
                    if (index == -1)
                    {
                        throw new ArgumentException("value");
                    }
                    str = stringValue.Substring(0, index);
                    if ((index + 1) > stringValue.Length)
                    {
                        throw new ArgumentException("value");
                    }
                    str2 = stringValue.Substring(index + 1);
                }
                string[] strArray2 = str2.Split(new char[] { ';' });
                if (strArray2.Length > 1)
                {
                    return new string[] { str, strArray2[0], strArray2[1] };
                }
                if (strArray2.Length > 0)
                {
                    return new string[] { str, strArray2[0] };
                }
                return new string[] { str };
            }
        }
    }
}

