namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.Reflection;

    public class CollectionCodeDomSerializer : CodeDomSerializer
    {
        private static CollectionCodeDomSerializer defaultSerializer;

        private static MethodInfo ChooseMethodByType(TypeDescriptionProvider provider, List<MethodInfo> methods, ICollection values)
        {
            MethodInfo info = null;
            Type c = null;
            foreach (object obj2 in values)
            {
                Type reflectionType = provider.GetReflectionType(obj2);
                MethodInfo info2 = null;
                Type type3 = null;
                if ((info == null) || ((c != null) && !c.IsAssignableFrom(reflectionType)))
                {
                    foreach (MethodInfo info3 in methods)
                    {
                        ParameterInfo info4 = info3.GetParameters()[0];
                        if (info4 != null)
                        {
                            Type type4 = info4.ParameterType.IsArray ? info4.ParameterType.GetElementType() : info4.ParameterType;
                            if ((type4 != null) && type4.IsAssignableFrom(reflectionType))
                            {
                                if (info != null)
                                {
                                    if (!type4.IsAssignableFrom(c))
                                    {
                                        continue;
                                    }
                                    info = info3;
                                    c = type4;
                                    break;
                                }
                                if (info2 == null)
                                {
                                    info2 = info3;
                                    type3 = type4;
                                }
                                else
                                {
                                    bool flag = type3.IsAssignableFrom(type4);
                                    info2 = flag ? info3 : info2;
                                    type3 = flag ? type4 : type3;
                                }
                            }
                        }
                    }
                }
                if (info == null)
                {
                    info = info2;
                    c = type3;
                }
            }
            return info;
        }

        private ICollection GetCollectionDelta(ICollection original, ICollection modified)
        {
            if (((original != null) && (modified != null)) && (original.Count != 0))
            {
                IEnumerator enumerator = modified.GetEnumerator();
                if (enumerator != null)
                {
                    IDictionary dictionary = new HybridDictionary();
                    foreach (object obj2 in original)
                    {
                        if (dictionary.Contains(obj2))
                        {
                            int num = (int) dictionary[obj2];
                            dictionary[obj2] = ++num;
                        }
                        else
                        {
                            dictionary.Add(obj2, 1);
                        }
                    }
                    ArrayList list = null;
                    for (int i = 0; (i < modified.Count) && enumerator.MoveNext(); i++)
                    {
                        object current = enumerator.Current;
                        if (dictionary.Contains(current))
                        {
                            if (list == null)
                            {
                                list = new ArrayList();
                                enumerator.Reset();
                                for (int j = 0; (j < i) && enumerator.MoveNext(); j++)
                                {
                                    list.Add(enumerator.Current);
                                }
                                enumerator.MoveNext();
                            }
                            int num4 = (int) dictionary[current];
                            if (--num4 == 0)
                            {
                                dictionary.Remove(current);
                            }
                            else
                            {
                                dictionary[current] = num4;
                            }
                        }
                        else if (list != null)
                        {
                            list.Add(current);
                        }
                    }
                    if (list != null)
                    {
                        return list;
                    }
                }
            }
            return modified;
        }

        protected bool MethodSupportsSerialization(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            object[] customAttributes = method.GetCustomAttributes(typeof(DesignerSerializationVisibilityAttribute), true);
            if (customAttributes.Length > 0)
            {
                DesignerSerializationVisibilityAttribute attribute = (DesignerSerializationVisibilityAttribute) customAttributes[0];
                if ((attribute != null) && (attribute.Visibility == DesignerSerializationVisibility.Hidden))
                {
                    return false;
                }
            }
            return true;
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            object obj2 = null;
            using (CodeDomSerializerBase.TraceScope("CollectionCodeDomSerializer::Serialize"))
            {
                CodeExpression expression;
                ExpressionContext context = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                PropertyDescriptor descriptor = manager.Context[typeof(PropertyDescriptor)] as PropertyDescriptor;
                if (((context != null) && (context.PresetValue == value)) && ((descriptor != null) && (descriptor.PropertyType == context.ExpressionType)))
                {
                    expression = context.Expression;
                }
                else
                {
                    expression = null;
                    context = null;
                    descriptor = null;
                }
                ICollection is2 = value as ICollection;
                if (is2 == null)
                {
                    return obj2;
                }
                ICollection valuesToSerialize = is2;
                InheritedPropertyDescriptor descriptor2 = descriptor as InheritedPropertyDescriptor;
                Type c = (context == null) ? is2.GetType() : context.ExpressionType;
                bool flag = typeof(Array).IsAssignableFrom(c);
                if ((expression == null) && !flag)
                {
                    bool flag2;
                    expression = base.SerializeCreationExpression(manager, is2, out flag2);
                    if (flag2)
                    {
                        return expression;
                    }
                }
                if ((expression == null) && !flag)
                {
                    return obj2;
                }
                if ((descriptor2 != null) && !flag)
                {
                    valuesToSerialize = this.GetCollectionDelta(descriptor2.OriginalValue as ICollection, is2);
                }
                obj2 = this.SerializeCollection(manager, expression, c, is2, valuesToSerialize);
                if ((expression == null) || !this.ShouldClearCollection(manager, is2))
                {
                    return obj2;
                }
                CodeStatementCollection statements = obj2 as CodeStatementCollection;
                if ((is2.Count > 0) && ((obj2 == null) || ((statements != null) && (statements.Count == 0))))
                {
                    return null;
                }
                if (statements == null)
                {
                    statements = new CodeStatementCollection();
                    CodeStatement statement = obj2 as CodeStatement;
                    if (statement != null)
                    {
                        statements.Add(statement);
                    }
                    obj2 = statements;
                }
                if (statements != null)
                {
                    CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(expression, "Clear", new CodeExpression[0]);
                    CodeExpressionStatement statement2 = new CodeExpressionStatement(expression2);
                    statements.Insert(0, statement2);
                }
            }
            return obj2;
        }

        private CodeArrayCreateExpression SerializeArray(IDesignerSerializationManager manager, Type targetType, ICollection array, ICollection valuesToSerialize)
        {
            CodeArrayCreateExpression expression = null;
            using (CodeDomSerializerBase.TraceScope("CollectionCodeDomSerializer::SerializeArray"))
            {
                if (((Array) array).Rank != 1)
                {
                    manager.ReportError(System.Design.SR.GetString("SerializerInvalidArrayRank", new object[] { ((Array) array).Rank.ToString(CultureInfo.InvariantCulture) }));
                    return expression;
                }
                Type elementType = targetType.GetElementType();
                CodeTypeReference reference = new CodeTypeReference(elementType);
                CodeArrayCreateExpression expression2 = new CodeArrayCreateExpression {
                    CreateType = reference
                };
                bool flag = true;
                foreach (object obj2 in valuesToSerialize)
                {
                    if ((obj2 is IComponent) && TypeDescriptor.GetAttributes(obj2).Contains(InheritanceAttribute.InheritedReadOnly))
                    {
                        flag = false;
                        break;
                    }
                    CodeExpression expression3 = null;
                    ExpressionContext context = null;
                    ExpressionContext context2 = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                    if (context2 != null)
                    {
                        context = new ExpressionContext(context2.Expression, elementType, context2.Owner);
                        manager.Context.Push(context);
                    }
                    try
                    {
                        expression3 = base.SerializeToExpression(manager, obj2);
                    }
                    finally
                    {
                        if (context != null)
                        {
                            manager.Context.Pop();
                        }
                    }
                    if (expression3 != null)
                    {
                        if ((obj2 != null) && (obj2.GetType() != elementType))
                        {
                            expression3 = new CodeCastExpression(elementType, expression3);
                        }
                        expression2.Initializers.Add(expression3);
                    }
                    else
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    expression = expression2;
                }
            }
            return expression;
        }

        protected virtual object SerializeCollection(IDesignerSerializationManager manager, CodeExpression targetExpression, Type targetType, ICollection originalCollection, ICollection valuesToSerialize)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            if (originalCollection == null)
            {
                throw new ArgumentNullException("originalCollection");
            }
            if (valuesToSerialize == null)
            {
                throw new ArgumentNullException("valuesToSerialize");
            }
            object obj2 = null;
            bool flag = false;
            if (typeof(Array).IsAssignableFrom(targetType))
            {
                CodeArrayCreateExpression right = this.SerializeArray(manager, targetType, originalCollection, valuesToSerialize);
                if (right != null)
                {
                    if (targetExpression != null)
                    {
                        obj2 = new CodeAssignStatement(targetExpression, right);
                    }
                    else
                    {
                        obj2 = right;
                    }
                    flag = true;
                }
                return obj2;
            }
            if (valuesToSerialize.Count > 0)
            {
                TypeDescriptionProvider targetFrameworkProvider = CodeDomSerializerBase.GetTargetFrameworkProvider(manager, originalCollection);
                if (targetFrameworkProvider == null)
                {
                    targetFrameworkProvider = TypeDescriptor.GetProvider(originalCollection);
                }
                MethodInfo[] methods = targetFrameworkProvider.GetReflectionType(originalCollection).GetMethods(BindingFlags.Public | BindingFlags.Instance);
                List<MethodInfo> list = new List<MethodInfo>();
                List<MethodInfo> list2 = new List<MethodInfo>();
                foreach (MethodInfo info in methods)
                {
                    if (info.Name.Equals("AddRange"))
                    {
                        ParameterInfo[] parameters = info.GetParameters();
                        if (((parameters.Length == 1) && parameters[0].ParameterType.IsArray) && this.MethodSupportsSerialization(info))
                        {
                            list.Add(info);
                        }
                    }
                    if ((info.Name.Equals("Add") && (info.GetParameters().Length == 1)) && this.MethodSupportsSerialization(info))
                    {
                        list2.Add(info);
                    }
                }
                MethodInfo info2 = ChooseMethodByType(targetFrameworkProvider, list, valuesToSerialize);
                if (info2 != null)
                {
                    Type runtimeType = targetFrameworkProvider.GetRuntimeType(info2.GetParameters()[0].ParameterType.GetElementType());
                    obj2 = this.SerializeViaAddRange(manager, targetExpression, targetType, runtimeType, valuesToSerialize);
                    flag = true;
                }
                else
                {
                    MethodInfo info3 = ChooseMethodByType(targetFrameworkProvider, list2, valuesToSerialize);
                    if (info3 != null)
                    {
                        Type elementType = targetFrameworkProvider.GetRuntimeType(info3.GetParameters()[0].ParameterType);
                        obj2 = this.SerializeViaAdd(manager, targetExpression, targetType, elementType, valuesToSerialize);
                        flag = true;
                    }
                }
                if (!flag && originalCollection.GetType().IsSerializable)
                {
                    obj2 = base.SerializeToResourceExpression(manager, originalCollection, false);
                }
            }
            return obj2;
        }

        private object SerializeViaAdd(IDesignerSerializationManager manager, CodeExpression targetExpression, Type targetType, Type elementType, ICollection valuesToSerialize)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            using (CodeDomSerializerBase.TraceScope("CollectionCodeDomSerializer::SerializeViaAdd"))
            {
                CodeMethodReferenceExpression expression = new CodeMethodReferenceExpression(targetExpression, "Add");
                if (valuesToSerialize.Count <= 0)
                {
                    return statements;
                }
                ExpressionContext context = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                foreach (object obj2 in valuesToSerialize)
                {
                    bool flag = !(obj2 is IComponent);
                    if (!flag)
                    {
                        InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(obj2)[typeof(InheritanceAttribute)];
                        if (attribute != null)
                        {
                            if (attribute.InheritanceLevel == InheritanceLevel.InheritedReadOnly)
                            {
                                flag = false;
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression {
                            Method = expression
                        };
                        CodeExpression expression3 = null;
                        ExpressionContext context2 = null;
                        if (context != null)
                        {
                            context2 = new ExpressionContext(context.Expression, elementType, context.Owner);
                            manager.Context.Push(context2);
                        }
                        try
                        {
                            expression3 = base.SerializeToExpression(manager, obj2);
                        }
                        finally
                        {
                            if (context2 != null)
                            {
                                manager.Context.Pop();
                            }
                        }
                        if (((obj2 != null) && !elementType.IsAssignableFrom(obj2.GetType())) && obj2.GetType().IsPrimitive)
                        {
                            expression3 = new CodeCastExpression(elementType, expression3);
                        }
                        if (expression3 != null)
                        {
                            expression2.Parameters.Add(expression3);
                            statements.Add(expression2);
                        }
                    }
                }
            }
            return statements;
        }

        private object SerializeViaAddRange(IDesignerSerializationManager manager, CodeExpression targetExpression, Type targetType, Type elementType, ICollection valuesToSerialize)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            using (CodeDomSerializerBase.TraceScope("CollectionCodeDomSerializer::SerializeViaAddRange"))
            {
                if (valuesToSerialize.Count <= 0)
                {
                    return statements;
                }
                ArrayList list = new ArrayList(valuesToSerialize.Count);
                ExpressionContext context = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                foreach (object obj2 in valuesToSerialize)
                {
                    bool flag = !(obj2 is IComponent);
                    if (!flag)
                    {
                        InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(obj2)[typeof(InheritanceAttribute)];
                        if (attribute != null)
                        {
                            if (attribute.InheritanceLevel == InheritanceLevel.InheritedReadOnly)
                            {
                                flag = false;
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        CodeExpression expression = null;
                        ExpressionContext context2 = null;
                        if (context != null)
                        {
                            context2 = new ExpressionContext(context.Expression, elementType, context.Owner);
                            manager.Context.Push(context2);
                        }
                        try
                        {
                            expression = base.SerializeToExpression(manager, obj2);
                        }
                        finally
                        {
                            if (context2 != null)
                            {
                                manager.Context.Pop();
                            }
                        }
                        if (expression != null)
                        {
                            if ((obj2 != null) && !elementType.IsAssignableFrom(obj2.GetType()))
                            {
                                expression = new CodeCastExpression(elementType, expression);
                            }
                            list.Add(expression);
                        }
                    }
                }
                if (list.Count <= 0)
                {
                    return statements;
                }
                CodeTypeReference reference = new CodeTypeReference(elementType);
                CodeArrayCreateExpression expression2 = new CodeArrayCreateExpression {
                    CreateType = reference
                };
                foreach (CodeExpression expression3 in list)
                {
                    expression2.Initializers.Add(expression3);
                }
                CodeMethodReferenceExpression expression4 = new CodeMethodReferenceExpression(targetExpression, "AddRange");
                CodeMethodInvokeExpression expression5 = new CodeMethodInvokeExpression {
                    Method = expression4
                };
                expression5.Parameters.Add(expression2);
                statements.Add(new CodeExpressionStatement(expression5));
            }
            return statements;
        }

        private bool ShouldClearCollection(IDesignerSerializationManager manager, ICollection collection)
        {
            bool flag = false;
            PropertyDescriptor descriptor = manager.Properties["ClearCollections"];
            if (((descriptor != null) && (descriptor.PropertyType == typeof(bool))) && ((bool) descriptor.GetValue(manager)))
            {
                flag = true;
            }
            if (!flag)
            {
                SerializeAbsoluteContext context = (SerializeAbsoluteContext) manager.Context[typeof(SerializeAbsoluteContext)];
                PropertyDescriptor member = manager.Context[typeof(PropertyDescriptor)] as PropertyDescriptor;
                if ((context != null) && context.ShouldSerialize(member))
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                return flag;
            }
            MethodInfo method = TypeDescriptor.GetReflectionType(collection).GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            return (((method != null) && this.MethodSupportsSerialization(method)) && flag);
        }

        internal static CollectionCodeDomSerializer Default
        {
            get
            {
                if (defaultSerializer == null)
                {
                    defaultSerializer = new CollectionCodeDomSerializer();
                }
                return defaultSerializer;
            }
        }
    }
}

