namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Reflection;
    using System.Workflow.ComponentModel;

    public class DependencyObjectCodeDomSerializer : CodeDomSerializer
    {
        public override object Serialize(IDesignerSerializationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (manager.Context == null)
            {
                throw new ArgumentException("manager", SR.GetString("Error_MissingContextProperty"));
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            DependencyObject component = obj as DependencyObject;
            if (component == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(DependencyObject).FullName }), "obj");
            }
            Activity context = obj as Activity;
            if (context != null)
            {
                manager.Context.Push(context);
            }
            CodeStatementCollection statements = null;
            try
            {
                if (context != null)
                {
                    CodeDomSerializer serializer = manager.GetSerializer(typeof(Component), typeof(CodeDomSerializer)) as CodeDomSerializer;
                    if (serializer == null)
                    {
                        throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(CodeDomSerializer).FullName }));
                    }
                    statements = serializer.Serialize(manager, context) as CodeStatementCollection;
                }
                else
                {
                    statements = base.Serialize(manager, obj) as CodeStatementCollection;
                }
                if (statements == null)
                {
                    return statements;
                }
                CodeStatementCollection statements2 = new CodeStatementCollection(statements);
                CodeExpression targetObject = base.SerializeToExpression(manager, obj);
                if (targetObject == null)
                {
                    return statements;
                }
                ArrayList list = new ArrayList();
                List<DependencyProperty> list2 = new List<DependencyProperty>(component.MetaDependencyProperties);
                foreach (DependencyProperty property in component.DependencyPropertyValues.Keys)
                {
                    if (property.IsAttached && ((property.IsEvent && (property.OwnerType.GetField(property.Name + "Event", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly) != null)) || (!property.IsEvent && (property.OwnerType.GetField(property.Name + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly) != null))))
                    {
                        list2.Add(property);
                    }
                }
                foreach (DependencyProperty property2 in list2)
                {
                    object binding = null;
                    if (component.IsBindingSet(property2))
                    {
                        binding = component.GetBinding(property2);
                    }
                    else if (!property2.IsEvent)
                    {
                        binding = component.GetValue(property2);
                    }
                    else
                    {
                        binding = component.GetHandler(property2);
                    }
                    if ((binding != null) && (property2.IsAttached || (!property2.DefaultMetadata.IsMetaProperty && (binding is ActivityBind))))
                    {
                        object[] attributes = property2.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                        if (attributes.Length > 0)
                        {
                            DesignerSerializationVisibilityAttribute attribute = attributes[0] as DesignerSerializationVisibilityAttribute;
                            if (attribute.Visibility == DesignerSerializationVisibility.Hidden)
                            {
                                continue;
                            }
                        }
                        CodeExpression expression2 = null;
                        string name = property2.Name + (property2.IsEvent ? "Event" : "Property");
                        FieldInfo field = property2.OwnerType.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                        if ((field != null) && !field.IsPublic)
                        {
                            expression2 = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(DependencyProperty)), "FromName", new CodeExpression[] { new CodePrimitiveExpression(property2.Name), new CodeTypeOfExpression(property2.OwnerType) });
                        }
                        else
                        {
                            expression2 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(property2.OwnerType), name);
                        }
                        CodeExpression expression = base.SerializeToExpression(manager, binding);
                        if ((expression2 != null) && (expression != null))
                        {
                            CodeMethodInvokeExpression expression4 = null;
                            if (binding is ActivityBind)
                            {
                                expression4 = new CodeMethodInvokeExpression(targetObject, "SetBinding", new CodeExpression[] { expression2, new CodeCastExpression(new CodeTypeReference(typeof(ActivityBind)), expression) });
                            }
                            else
                            {
                                expression4 = new CodeMethodInvokeExpression(targetObject, property2.IsEvent ? "AddHandler" : "SetValue", new CodeExpression[] { expression2, expression });
                            }
                            statements.Add(expression4);
                            foreach (CodeStatement statement in statements2)
                            {
                                if ((statement is CodeAssignStatement) && (((CodeAssignStatement) statement).Left is CodePropertyReferenceExpression))
                                {
                                    CodePropertyReferenceExpression left = ((CodeAssignStatement) statement).Left as CodePropertyReferenceExpression;
                                    if ((left.PropertyName == property2.Name) && left.TargetObject.Equals(targetObject))
                                    {
                                        statements.Remove(statement);
                                    }
                                }
                            }
                        }
                        list.Add(property2);
                    }
                }
                if (!(manager.GetService(typeof(IEventBindingService)) is IEventBindingService))
                {
                    foreach (EventDescriptor descriptor in TypeDescriptor.GetEvents(component))
                    {
                        string eventHandlerName = WorkflowMarkupSerializationHelpers.GetEventHandlerName(component, descriptor.Name);
                        if (!string.IsNullOrEmpty(eventHandlerName))
                        {
                            CodeEventReferenceExpression eventRef = new CodeEventReferenceExpression(targetObject, descriptor.Name);
                            CodeDelegateCreateExpression listener = new CodeDelegateCreateExpression(new CodeTypeReference(descriptor.EventType), new CodeThisReferenceExpression(), eventHandlerName);
                            statements.Add(new CodeAttachEventStatement(eventRef, listener));
                        }
                    }
                }
                if (!component.UserData.Contains(UserDataKeys.DesignTimeTypeNames))
                {
                    return statements;
                }
                Hashtable hashtable = component.UserData[UserDataKeys.DesignTimeTypeNames] as Hashtable;
                foreach (object obj4 in hashtable.Keys)
                {
                    string str3 = null;
                    string fullName = null;
                    string str5 = hashtable[obj4] as string;
                    DependencyProperty item = obj4 as DependencyProperty;
                    if (item != null)
                    {
                        if (list.Contains(item))
                        {
                            continue;
                        }
                        object[] objArray2 = item.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                        if (objArray2.Length > 0)
                        {
                            DesignerSerializationVisibilityAttribute attribute2 = objArray2[0] as DesignerSerializationVisibilityAttribute;
                            if (attribute2.Visibility == DesignerSerializationVisibility.Hidden)
                            {
                                continue;
                            }
                        }
                        str3 = item.Name;
                        fullName = item.OwnerType.FullName;
                    }
                    else if (obj4 is string)
                    {
                        int length = ((string) obj4).LastIndexOf('.');
                        if (length != -1)
                        {
                            fullName = ((string) obj4).Substring(0, length);
                            str3 = ((string) obj4).Substring(length + 1);
                        }
                    }
                    if ((!string.IsNullOrEmpty(str5) && !string.IsNullOrEmpty(str3)) && !string.IsNullOrEmpty(fullName))
                    {
                        if (fullName == obj.GetType().FullName)
                        {
                            CodePropertyReferenceExpression expression8 = new CodePropertyReferenceExpression(targetObject, str3);
                            statements.Add(new CodeAssignStatement(expression8, new CodeTypeOfExpression(str5)));
                        }
                        else
                        {
                            CodeExpression expression9 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(fullName), str3 + "Property");
                            CodeExpression expression10 = new CodeTypeOfExpression(str5);
                            statements.Add(new CodeMethodInvokeExpression(targetObject, "SetValue", new CodeExpression[] { expression9, expression10 }));
                        }
                    }
                }
            }
            finally
            {
                if (context != null)
                {
                    manager.Context.Pop();
                }
            }
            return statements;
        }
    }
}

