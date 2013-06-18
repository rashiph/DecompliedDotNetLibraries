namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;

    internal class EnumCodeDomSerializer : CodeDomSerializer
    {
        private static EnumCodeDomSerializer defaultSerializer;

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            CodeExpression left = null;
            using (CodeDomSerializerBase.TraceScope("EnumCodeDomSerializer::Serialize"))
            {
                Enum[] enumArray;
                if (!(value is Enum))
                {
                    return left;
                }
                bool flag = false;
                TypeConverter converter = TypeDescriptor.GetConverter(value);
                if ((converter != null) && converter.CanConvertTo(typeof(Enum[])))
                {
                    enumArray = (Enum[]) converter.ConvertTo(value, typeof(Enum[]));
                    flag = enumArray.Length > 1;
                }
                else
                {
                    enumArray = new Enum[] { (Enum) value };
                    flag = true;
                }
                CodeTypeReferenceExpression targetObject = new CodeTypeReferenceExpression(value.GetType());
                TypeConverter converter2 = new EnumConverter(value.GetType());
                foreach (Enum enum2 in enumArray)
                {
                    string str = (converter2 != null) ? converter2.ConvertToString(enum2) : null;
                    CodeExpression right = !string.IsNullOrEmpty(str) ? new CodeFieldReferenceExpression(targetObject, str) : null;
                    if (right != null)
                    {
                        if (left == null)
                        {
                            left = right;
                        }
                        else
                        {
                            left = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BitwiseOr, right);
                        }
                    }
                }
                if ((left != null) && flag)
                {
                    left = new CodeCastExpression(value.GetType(), left);
                }
            }
            return left;
        }

        internal static EnumCodeDomSerializer Default
        {
            get
            {
                if (defaultSerializer == null)
                {
                    defaultSerializer = new EnumCodeDomSerializer();
                }
                return defaultSerializer;
            }
        }
    }
}

