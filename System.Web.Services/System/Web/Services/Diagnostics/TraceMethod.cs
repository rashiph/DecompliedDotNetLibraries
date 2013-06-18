namespace System.Web.Services.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Text;

    internal class TraceMethod
    {
        private object[] args;
        private string call;
        private string name;
        private object target;

        internal TraceMethod(object target, string name, params object[] args)
        {
            this.target = target;
            this.name = name;
            this.args = args;
        }

        internal static string CallString(object target, string method, params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            WriteObjectId(sb, target);
            sb.Append(':');
            sb.Append(':');
            sb.Append(method);
            sb.Append('(');
            for (int i = 0; i < args.Length; i++)
            {
                object o = args[i];
                WriteObjectId(sb, o);
                if (o != null)
                {
                    sb.Append('=');
                    WriteValue(sb, o);
                }
                if ((i + 1) < args.Length)
                {
                    sb.Append(',');
                    sb.Append(' ');
                }
            }
            sb.Append(')');
            return sb.ToString();
        }

        private static string HashString(object objectValue)
        {
            if (objectValue == null)
            {
                return "(null)";
            }
            return objectValue.GetHashCode().ToString(NumberFormatInfo.InvariantInfo);
        }

        internal static string MethodId(object target, string method)
        {
            StringBuilder sb = new StringBuilder();
            WriteObjectId(sb, target);
            sb.Append(':');
            sb.Append(':');
            sb.Append(method);
            return sb.ToString();
        }

        public override string ToString()
        {
            if (this.call == null)
            {
                this.call = CallString(this.target, this.name, this.args);
            }
            return this.call;
        }

        private static void WriteObjectId(StringBuilder sb, object o)
        {
            if (o == null)
            {
                sb.Append("(null)");
            }
            else if (o is Type)
            {
                Type type = (Type) o;
                sb.Append(type.FullName);
                if (!type.IsAbstract || !type.IsSealed)
                {
                    sb.Append('#');
                    sb.Append(HashString(o));
                }
            }
            else
            {
                sb.Append(o.GetType().FullName);
                sb.Append('#');
                sb.Append(HashString(o));
            }
        }

        private static void WriteValue(StringBuilder sb, object o)
        {
            if (o != null)
            {
                if (o is string)
                {
                    sb.Append('"');
                    sb.Append(o);
                    sb.Append('"');
                }
                else
                {
                    Type type = o.GetType();
                    if (type.IsArray)
                    {
                        sb.Append('[');
                        sb.Append(((Array) o).Length);
                        sb.Append(']');
                    }
                    else
                    {
                        string str = o.ToString();
                        if (type.FullName == str)
                        {
                            sb.Append('.');
                            sb.Append('.');
                        }
                        else
                        {
                            sb.Append(str);
                        }
                    }
                }
            }
        }
    }
}

