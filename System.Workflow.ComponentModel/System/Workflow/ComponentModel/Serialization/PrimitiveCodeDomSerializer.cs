namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.ComponentModel.Design.Serialization;

    internal class PrimitiveCodeDomSerializer : CodeDomSerializer
    {
        private static System.Workflow.ComponentModel.Serialization.PrimitiveCodeDomSerializer defaultSerializer;
        private static readonly string JSharpFileExtension = ".jsl";

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            CodeExpression expression = new CodePrimitiveExpression(value);
            if ((((value == null) || (value is bool)) || ((value is char) || (value is int))) || ((value is float) || (value is double)))
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
                return expression;
            }
            return new CodeCastExpression(new CodeTypeReference(value.GetType()), expression);
        }

        internal static System.Workflow.ComponentModel.Serialization.PrimitiveCodeDomSerializer Default
        {
            get
            {
                if (defaultSerializer == null)
                {
                    defaultSerializer = new System.Workflow.ComponentModel.Serialization.PrimitiveCodeDomSerializer();
                }
                return defaultSerializer;
            }
        }
    }
}

