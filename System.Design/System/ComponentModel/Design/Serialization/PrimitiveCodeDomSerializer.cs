namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;

    internal class PrimitiveCodeDomSerializer : CodeDomSerializer
    {
        private static PrimitiveCodeDomSerializer defaultSerializer;
        private static readonly string JSharpFileExtension = ".jsl";

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            using (CodeDomSerializerBase.TraceScope("PrimitiveCodeDomSerializer::Serialize"))
            {
            }
            CodeExpression expression = new CodePrimitiveExpression(value);
            if (value == null)
            {
                return expression;
            }
            if (((value is bool) || (value is char)) || (((value is int) || (value is float)) || (value is double)))
            {
                CodeDomProvider service = manager.GetService(typeof(CodeDomProvider)) as CodeDomProvider;
                if ((service != null) && string.Equals(service.FileExtension, JSharpFileExtension))
                {
                    ExpressionContext context = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                    if ((context != null) && (context.ExpressionType == typeof(object)))
                    {
                        expression = new CodeCastExpression(value.GetType(), expression);
                        expression.UserData.Add("CastIsBoxing", true);
                    }
                }
                return expression;
            }
            if (value is string)
            {
                string str = value as string;
                if ((str != null) && (str.Length > 200))
                {
                    expression = base.SerializeToResourceExpression(manager, str);
                }
                return expression;
            }
            return new CodeCastExpression(new CodeTypeReference(value.GetType()), expression);
        }

        internal static PrimitiveCodeDomSerializer Default
        {
            get
            {
                if (defaultSerializer == null)
                {
                    defaultSerializer = new PrimitiveCodeDomSerializer();
                }
                return defaultSerializer;
            }
        }
    }
}

