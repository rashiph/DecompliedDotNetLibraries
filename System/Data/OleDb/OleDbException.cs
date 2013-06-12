namespace System.Data.OleDb
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;

    [Serializable]
    public sealed class OleDbException : DbException
    {
        private OleDbErrorCollection oledbErrors;

        internal OleDbException(OleDbException previous, Exception inner) : base(previous.Message, inner)
        {
            base.HResult = previous.ErrorCode;
            this.oledbErrors = previous.oledbErrors;
        }

        private OleDbException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
            this.oledbErrors = (OleDbErrorCollection) si.GetValue("oledbErrors", typeof(OleDbErrorCollection));
        }

        internal OleDbException(string message, OleDbHResult errorCode, Exception inner) : base(message, inner)
        {
            base.HResult = (int) errorCode;
            this.oledbErrors = new OleDbErrorCollection(null);
        }

        private OleDbException(string message, Exception inner, string source, OleDbHResult errorCode, OleDbErrorCollection errors) : base(message, inner)
        {
            this.Source = source;
            base.HResult = (int) errorCode;
            this.oledbErrors = errors;
        }

        internal static OleDbException CombineExceptions(List<OleDbException> exceptions)
        {
            if (1 >= exceptions.Count)
            {
                return exceptions[0];
            }
            OleDbErrorCollection errors = new OleDbErrorCollection(null);
            StringBuilder builder = new StringBuilder();
            foreach (OleDbException exception in exceptions)
            {
                errors.AddRange(exception.Errors);
                builder.Append(exception.Message);
                builder.Append(Environment.NewLine);
            }
            return new OleDbException(builder.ToString(), null, exceptions[0].Source, (OleDbHResult) exceptions[0].ErrorCode, errors);
        }

        internal static OleDbException CreateException(System.Data.Common.UnsafeNativeMethods.IErrorInfo errorInfo, OleDbHResult errorCode, Exception inner)
        {
            OleDbErrorCollection errors = new OleDbErrorCollection(errorInfo);
            string pBstrDescription = null;
            string pBstrSource = null;
            OleDbHResult description = OleDbHResult.S_OK;
            if (errorInfo != null)
            {
                description = errorInfo.GetDescription(out pBstrDescription);
                Bid.Trace("<oledb.IErrorInfo.GetDescription|API|OS|RET> %08X{HRESULT}, Description='%ls'\n", description, pBstrDescription);
                description = errorInfo.GetSource(out pBstrSource);
                Bid.Trace("<oledb.IErrorInfo.GetSource|API|OS|RET> %08X{HRESULT}, Source='%ls'\n", description, pBstrSource);
            }
            int count = errors.Count;
            if (0 < errors.Count)
            {
                StringBuilder builder = new StringBuilder();
                if ((pBstrDescription != null) && (pBstrDescription != errors[0].Message))
                {
                    builder.Append(pBstrDescription.TrimEnd(ODB.ErrorTrimCharacters));
                    if (1 < count)
                    {
                        builder.Append(Environment.NewLine);
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    if (0 < i)
                    {
                        builder.Append(Environment.NewLine);
                    }
                    builder.Append(errors[i].Message.TrimEnd(ODB.ErrorTrimCharacters));
                }
                pBstrDescription = builder.ToString();
            }
            if (ADP.IsEmpty(pBstrDescription))
            {
                pBstrDescription = ODB.NoErrorMessage(errorCode);
            }
            return new OleDbException(pBstrDescription, inner, pBstrSource, errorCode, errors);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo si, StreamingContext context)
        {
            if (si == null)
            {
                throw new ArgumentNullException("si");
            }
            si.AddValue("oledbErrors", this.oledbErrors, typeof(OleDbErrorCollection));
            base.GetObjectData(si, context);
        }

        internal bool ShouldSerializeErrors()
        {
            OleDbErrorCollection oledbErrors = this.oledbErrors;
            return ((oledbErrors != null) && (0 < oledbErrors.Count));
        }

        [TypeConverter(typeof(ErrorCodeConverter))]
        public override int ErrorCode
        {
            get
            {
                return base.ErrorCode;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public OleDbErrorCollection Errors
        {
            get
            {
                OleDbErrorCollection oledbErrors = this.oledbErrors;
                if (oledbErrors == null)
                {
                    return new OleDbErrorCollection(null);
                }
                return oledbErrors;
            }
        }

        internal sealed class ErrorCodeConverter : Int32Converter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                {
                    throw ADP.ArgumentNull("destinationType");
                }
                if (((destinationType == typeof(string)) && (value != null)) && (value is int))
                {
                    return ODB.ELookup((OleDbHResult) value);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}

