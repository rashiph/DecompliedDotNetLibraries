namespace System.Web.Services.Protocols
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Text;

    public abstract class UrlEncodedParameterWriter : MimeParameterWriter
    {
        private Encoding encoding;
        private int numberEncoded;
        private ParameterInfo[] paramInfos;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected UrlEncodedParameterWriter()
        {
        }

        protected void Encode(TextWriter writer, object[] values)
        {
            this.numberEncoded = 0;
            for (int i = 0; i < this.paramInfos.Length; i++)
            {
                ParameterInfo info = this.paramInfos[i];
                if (info.ParameterType.IsArray)
                {
                    Array array = (Array) values[i];
                    for (int j = 0; j < array.Length; j++)
                    {
                        this.Encode(writer, info.Name, array.GetValue(j));
                    }
                }
                else
                {
                    this.Encode(writer, info.Name, values[i]);
                }
            }
        }

        protected void Encode(TextWriter writer, string name, object value)
        {
            if (this.numberEncoded > 0)
            {
                writer.Write('&');
            }
            writer.Write(this.UrlEncode(name));
            writer.Write('=');
            writer.Write(this.UrlEncode(ScalarFormatter.ToString(value)));
            this.numberEncoded++;
        }

        public override object GetInitializer(LogicalMethodInfo methodInfo)
        {
            if (!ValueCollectionParameterReader.IsSupported(methodInfo))
            {
                return null;
            }
            return methodInfo.InParameters;
        }

        public override void Initialize(object initializer)
        {
            this.paramInfos = (ParameterInfo[]) initializer;
        }

        private string UrlEncode(string value)
        {
            if (this.encoding != null)
            {
                return UrlEncoder.UrlEscapeString(value, this.encoding);
            }
            return UrlEncoder.UrlEscapeStringUnicode(value);
        }

        public override Encoding RequestEncoding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.encoding;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.encoding = value;
            }
        }
    }
}

