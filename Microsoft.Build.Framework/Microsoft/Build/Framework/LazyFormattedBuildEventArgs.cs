namespace Microsoft.Build.Framework
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime;

    [Serializable]
    public class LazyFormattedBuildEventArgs : BuildEventArgs
    {
        private object[] arguments;
        private CultureInfo originalCulture;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected LazyFormattedBuildEventArgs()
        {
        }

        public LazyFormattedBuildEventArgs(string message, string helpKeyword, string senderName) : this(message, helpKeyword, senderName, DateTime.Now, null)
        {
        }

        public LazyFormattedBuildEventArgs(string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs) : base(message, helpKeyword, senderName, eventTimestamp)
        {
            this.arguments = messageArgs;
            this.originalCulture = CultureInfo.CurrentCulture;
        }

        internal override void CreateFromStream(BinaryReader reader)
        {
            base.CreateFromStream(reader);
            string[] strArray = null;
            int num = reader.ReadInt32();
            if (num >= 0)
            {
                strArray = new string[num];
                for (int i = 0; i < num; i++)
                {
                    strArray[i] = reader.ReadString();
                }
            }
            this.arguments = strArray;
            int culture = reader.ReadInt32();
            if (culture != 0)
            {
                if (culture == CultureInfo.CurrentCulture.LCID)
                {
                    this.originalCulture = CultureInfo.CurrentCulture;
                }
                else
                {
                    this.originalCulture = new CultureInfo(culture);
                }
            }
        }

        private static string FormatString(CultureInfo culture, string unformatted, params object[] args)
        {
            string str = unformatted;
            if ((args != null) && (args.Length > 0))
            {
                str = string.Format(culture, unformatted, args);
            }
            return str;
        }

        internal override void WriteToStream(BinaryWriter writer)
        {
            bool flag = this.arguments != null;
            base.WriteToStream(writer);
            if (flag && (this.arguments == null))
            {
                throw new InvalidOperationException("BuildEventArgs has formatted message while serializing!");
            }
            if (this.arguments != null)
            {
                writer.Write(this.arguments.Length);
                foreach (object obj2 in this.arguments)
                {
                    writer.Write(Convert.ToString(obj2, CultureInfo.CurrentCulture));
                }
            }
            else
            {
                writer.Write(-1);
            }
            writer.Write((this.originalCulture != null) ? this.originalCulture.LCID : 0);
        }

        public override string Message
        {
            get
            {
                if ((this.arguments != null) && (this.arguments.Length > 0))
                {
                    base.Message = FormatString(this.originalCulture, base.Message, this.arguments);
                    this.arguments = null;
                }
                return base.Message;
            }
        }
    }
}

