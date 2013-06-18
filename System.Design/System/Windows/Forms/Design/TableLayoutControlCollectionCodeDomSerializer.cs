namespace System.Windows.Forms.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Windows.Forms;

    internal class TableLayoutControlCollectionCodeDomSerializer : CollectionCodeDomSerializer
    {
        protected override object SerializeCollection(IDesignerSerializationManager manager, CodeExpression targetExpression, System.Type targetType, ICollection originalCollection, ICollection valuesToSerialize)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            CodeMethodReferenceExpression expression = new CodeMethodReferenceExpression(targetExpression, "Add");
            TableLayoutControlCollection controls = (TableLayoutControlCollection) originalCollection;
            if (valuesToSerialize.Count > 0)
            {
                bool flag = false;
                ExpressionContext context = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                if ((context != null) && (context.Expression == targetExpression))
                {
                    IComponent owner = context.Owner as IComponent;
                    if (owner != null)
                    {
                        InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(owner)[typeof(InheritanceAttribute)];
                        flag = (attribute != null) && (attribute.InheritanceLevel == InheritanceLevel.Inherited);
                    }
                }
                foreach (object obj2 in valuesToSerialize)
                {
                    bool flag2 = !(obj2 is IComponent);
                    if (!flag2)
                    {
                        InheritanceAttribute attribute2 = (InheritanceAttribute) TypeDescriptor.GetAttributes(obj2)[typeof(InheritanceAttribute)];
                        if (attribute2 != null)
                        {
                            if (attribute2.InheritanceLevel == InheritanceLevel.InheritedReadOnly)
                            {
                                flag2 = false;
                            }
                            else if ((attribute2.InheritanceLevel == InheritanceLevel.Inherited) && flag)
                            {
                                flag2 = false;
                            }
                            else
                            {
                                flag2 = true;
                            }
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (flag2)
                    {
                        CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression {
                            Method = expression
                        };
                        CodeExpression expression3 = base.SerializeToExpression(manager, obj2);
                        if ((expression3 != null) && !typeof(Control).IsAssignableFrom(obj2.GetType()))
                        {
                            expression3 = new CodeCastExpression(typeof(Control), expression3);
                        }
                        if (expression3 != null)
                        {
                            int column = controls.Container.GetColumn((Control) obj2);
                            int row = controls.Container.GetRow((Control) obj2);
                            expression2.Parameters.Add(expression3);
                            if ((column != -1) || (row != -1))
                            {
                                expression2.Parameters.Add(new CodePrimitiveExpression(column));
                                expression2.Parameters.Add(new CodePrimitiveExpression(row));
                            }
                            statements.Add(expression2);
                        }
                    }
                }
            }
            return statements;
        }
    }
}

