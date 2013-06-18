namespace System.Runtime.Serialization
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential), DataContract(Name="DateTimeOffset", Namespace="http://schemas.datacontract.org/2004/07/System")]
    internal struct DateTimeOffsetAdapter
    {
        private DateTime utcDateTime;
        private short offsetMinutes;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DateTimeOffsetAdapter(DateTime dateTime, short offsetMinutes)
        {
            this.utcDateTime = dateTime;
            this.offsetMinutes = offsetMinutes;
        }

        [DataMember(Name="DateTime", IsRequired=true)]
        public DateTime UtcDateTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.utcDateTime;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.utcDateTime = value;
            }
        }
        [DataMember(Name="OffsetMinutes", IsRequired=true)]
        public short OffsetMinutes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.offsetMinutes;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.offsetMinutes = value;
            }
        }
        public static DateTimeOffset GetDateTimeOffset(DateTimeOffsetAdapter value)
        {
            DateTimeOffset offset2;
            try
            {
                if (value.UtcDateTime.Kind == DateTimeKind.Unspecified)
                {
                    return new DateTimeOffset(value.UtcDateTime, new TimeSpan(0, value.OffsetMinutes, 0));
                }
                offset2 = new DateTimeOffset(value.UtcDateTime).ToOffset(new TimeSpan(0, value.OffsetMinutes, 0));
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value.ToString(CultureInfo.InvariantCulture), "DateTimeOffset", exception));
            }
            return offset2;
        }

        public static DateTimeOffsetAdapter GetDateTimeOffsetAdapter(DateTimeOffset value)
        {
            return new DateTimeOffsetAdapter(value.UtcDateTime, (short) value.Offset.TotalMinutes);
        }

        public string ToString(IFormatProvider provider)
        {
            return string.Concat(new object[] { "DateTime: ", this.UtcDateTime, ", Offset: ", this.OffsetMinutes });
        }
    }
}

