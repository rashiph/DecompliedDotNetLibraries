namespace System.Windows.Forms.Design
{
    using System;
    using System.CodeDom;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Windows.Forms;

    public class ImageListCodeDomSerializer : CodeDomSerializer
    {
        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            if ((manager == null) || (codeObject == null))
            {
                throw new ArgumentNullException((manager == null) ? "manager" : "codeObject");
            }
            CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer(typeof(Component), typeof(CodeDomSerializer));
            if (serializer == null)
            {
                return null;
            }
            return serializer.Deserialize(manager, codeObject);
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            object obj2 = ((CodeDomSerializer) manager.GetSerializer(typeof(ImageList).BaseType, typeof(CodeDomSerializer))).Serialize(manager, value);
            ImageList list = value as ImageList;
            if (list != null)
            {
                StringCollection keys = list.Images.Keys;
                if (!(obj2 is CodeStatementCollection))
                {
                    return obj2;
                }
                CodeExpression targetObject = base.GetExpression(manager, value);
                if (targetObject == null)
                {
                    return obj2;
                }
                CodeExpression expression2 = new CodePropertyReferenceExpression(targetObject, "Images");
                if (expression2 == null)
                {
                    return obj2;
                }
                for (int i = 0; i < keys.Count; i++)
                {
                    if ((keys[i] != null) || (keys[i].Length != 0))
                    {
                        CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression(expression2, "SetKeyName", new CodeExpression[] { new CodePrimitiveExpression(i), new CodePrimitiveExpression(keys[i]) });
                        ((CodeStatementCollection) obj2).Add(expression3);
                    }
                }
            }
            return obj2;
        }
    }
}

