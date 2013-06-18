namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xml;

    internal class JsonObjectDataContract : JsonDataContract
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public JsonObjectDataContract(DataContract traditionalDataContract) : base(traditionalDataContract)
        {
        }

        private static object ParseJsonNumber(string value)
        {
            TypeCode code;
            return ParseJsonNumber(value, out code);
        }

        internal static object ParseJsonNumber(string value, out TypeCode objectTypeCode)
        {
            decimal num3;
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidConversion", new object[] { value, Globals.TypeOfInt })));
            }
            if (value.IndexOfAny(JsonGlobals.floatingPointCharacters) == -1)
            {
                int num;
                long num2;
                if (int.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out num))
                {
                    objectTypeCode = TypeCode.Int32;
                    return num;
                }
                if (long.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out num2))
                {
                    objectTypeCode = TypeCode.Int64;
                    return num2;
                }
            }
            if (decimal.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out num3))
            {
                objectTypeCode = TypeCode.Decimal;
                if (num3 == 0M)
                {
                    double num4 = XmlConverter.ToDouble(value);
                    if (num4 != 0.0)
                    {
                        objectTypeCode = TypeCode.Double;
                        return num4;
                    }
                }
                return num3;
            }
            objectTypeCode = TypeCode.Double;
            return XmlConverter.ToDouble(value);
        }

        public override object ReadJsonValueCore(XmlReaderDelegator jsonReader, XmlObjectSerializerReadContextComplexJson context)
        {
            object obj2;
            string attribute = jsonReader.GetAttribute("type");
            string key = attribute;
            if (key != null)
            {
                int num;
                if (<PrivateImplementationDetails>{D290E7C2-4296-4D66-A436-0C17851A078B}.$$method0x60012be-1.TryGetValue(key, out num))
                {
                    switch (num)
                    {
                        case 0:
                            jsonReader.Skip();
                            obj2 = null;
                            goto Label_011B;

                        case 1:
                            obj2 = jsonReader.ReadElementContentAsBoolean();
                            goto Label_011B;

                        case 2:
                            goto Label_00BB;

                        case 3:
                            obj2 = ParseJsonNumber(jsonReader.ReadElementContentAsString());
                            goto Label_011B;

                        case 4:
                            jsonReader.Skip();
                            obj2 = new object();
                            goto Label_011B;

                        case 5:
                            return DataContractJsonSerializer.ReadJsonValue(DataContract.GetDataContract(Globals.TypeOfObjectArray), jsonReader, context);
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("JsonUnexpectedAttributeValue", new object[] { attribute })));
            }
        Label_00BB:
            obj2 = jsonReader.ReadElementContentAsString();
        Label_011B:
            if (context != null)
            {
                context.AddNewObject(obj2);
            }
            return obj2;
        }

        public override void WriteJsonValueCore(XmlWriterDelegator jsonWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, RuntimeTypeHandle declaredTypeHandle)
        {
            jsonWriter.WriteAttributeString(null, "type", null, "object");
        }
    }
}

