namespace System.ComponentModel.Design.Serialization
{
    using Microsoft.CSharp;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class CodeDomSerializerBase
    {
        private static readonly Attribute[] runTimeProperties = new Attribute[] { DesignOnlyAttribute.No };
        private static readonly CodeThisReferenceExpression thisRef = new CodeThisReferenceExpression();
        private static Stack traceScope;
        private static TraceSwitch traceSerialization = new TraceSwitch("DesignerSerialization", "Trace design time serialization");

        internal CodeDomSerializerBase()
        {
        }

        private static void AddStatement(IDictionary table, string name, CodeStatement statement)
        {
            OrderedCodeStatementCollection statements;
            if (table.Contains(name))
            {
                statements = (OrderedCodeStatementCollection) table[name];
            }
            else
            {
                statements = new OrderedCodeStatementCollection {
                    Order = table.Count,
                    Name = name
                };
                table[name] = statements;
            }
            statements.Add(statement);
        }

        private void DeserializeAssignStatement(IDesignerSerializationManager manager, CodeAssignStatement statement)
        {
            using (TraceScope("CodeDomSerializerBase::DeserializeAssignStatement"))
            {
                CodeExpression left = statement.Left;
                CodePropertyReferenceExpression propertyReferenceEx = left as CodePropertyReferenceExpression;
                if (propertyReferenceEx != null)
                {
                    this.DeserializePropertyAssignStatement(manager, statement, propertyReferenceEx, true);
                }
                else
                {
                    CodeFieldReferenceExpression expression3 = left as CodeFieldReferenceExpression;
                    if (expression3 != null)
                    {
                        object instance = this.DeserializeExpression(manager, expression3.FieldName, expression3.TargetObject);
                        if (instance != null)
                        {
                            RootContext context = (RootContext) manager.Context[typeof(RootContext)];
                            if ((context != null) && (context.Value == instance))
                            {
                                if (this.DeserializeExpression(manager, expression3.FieldName, statement.Right) is CodeExpression)
                                {
                                }
                            }
                            else
                            {
                                FieldInfo field;
                                object obj4;
                                Type type = instance as Type;
                                if (type != null)
                                {
                                    obj4 = null;
                                    field = GetReflectionTypeFromTypeHelper(manager, type).GetField(expression3.FieldName, BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static);
                                }
                                else
                                {
                                    obj4 = instance;
                                    field = GetReflectionTypeHelper(manager, instance).GetField(expression3.FieldName, BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);
                                }
                                if (field != null)
                                {
                                    object obj5 = this.DeserializeExpression(manager, expression3.FieldName, statement.Right);
                                    if (!(obj5 is CodeExpression))
                                    {
                                        IConvertible convertible = obj5 as IConvertible;
                                        if (convertible != null)
                                        {
                                            Type fieldType = field.FieldType;
                                            TypeDescriptionProvider targetFrameworkProviderForType = GetTargetFrameworkProviderForType(manager, fieldType);
                                            if (targetFrameworkProviderForType != null)
                                            {
                                                fieldType = targetFrameworkProviderForType.GetRuntimeType(fieldType);
                                            }
                                            if (fieldType != obj5.GetType())
                                            {
                                                try
                                                {
                                                    obj5 = convertible.ToType(fieldType, null);
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                        field.SetValue(obj4, obj5);
                                    }
                                }
                                else
                                {
                                    CodePropertyReferenceExpression expression6 = new CodePropertyReferenceExpression {
                                        TargetObject = expression3.TargetObject,
                                        PropertyName = expression3.FieldName
                                    };
                                    if (!this.DeserializePropertyAssignStatement(manager, statement, expression6, false))
                                    {
                                        Error(manager, System.Design.SR.GetString("SerializerNoSuchField", new object[] { instance.GetType().FullName, expression3.FieldName }), "SerializerNoSuchField");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        CodeVariableReferenceExpression expression4 = left as CodeVariableReferenceExpression;
                        if (expression4 != null)
                        {
                            object obj6 = this.DeserializeExpression(manager, expression4.VariableName, statement.Right);
                            if (!(obj6 is CodeExpression))
                            {
                                manager.SetName(obj6, expression4.VariableName);
                            }
                        }
                        else
                        {
                            CodeArrayIndexerExpression expression5 = left as CodeArrayIndexerExpression;
                            if (expression5 != null)
                            {
                                int[] indices = new int[expression5.Indices.Count];
                                object obj7 = this.DeserializeExpression(manager, null, expression5.TargetObject);
                                bool flag = true;
                                for (int i = 0; i < indices.Length; i++)
                                {
                                    IConvertible convertible2 = this.DeserializeExpression(manager, null, expression5.Indices[i]) as IConvertible;
                                    if (convertible2 != null)
                                    {
                                        indices[i] = convertible2.ToInt32(null);
                                    }
                                    else
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                                Array array = obj7 as Array;
                                if ((array != null) && flag)
                                {
                                    object obj9 = this.DeserializeExpression(manager, null, statement.Right);
                                    if (!(obj9 is CodeExpression))
                                    {
                                        array.SetValue(obj9, indices);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DeserializeAttachEventStatement(IDesignerSerializationManager manager, CodeAttachEventStatement statement)
        {
            using (TraceScope("CodeDomSerializerBase::DeserializeAttachEventStatement"))
            {
                string methodName = null;
                object obj2 = null;
                object instance = this.DeserializeExpression(manager, null, statement.Event.TargetObject);
                string eventName = statement.Event.EventName;
                if ((eventName != null) && (instance != null))
                {
                    CodeObjectCreateExpression listener = statement.Listener as CodeObjectCreateExpression;
                    if (listener != null)
                    {
                        if (listener.Parameters.Count == 1)
                        {
                            CodeMethodReferenceExpression expression2 = listener.Parameters[0] as CodeMethodReferenceExpression;
                            if (expression2 != null)
                            {
                                methodName = expression2.MethodName;
                                obj2 = this.DeserializeExpression(manager, null, expression2.TargetObject);
                            }
                        }
                    }
                    else
                    {
                        CodeDelegateCreateExpression expression3 = this.DeserializeExpression(manager, null, statement.Listener) as CodeDelegateCreateExpression;
                        if (expression3 != null)
                        {
                            obj2 = this.DeserializeExpression(manager, null, expression3.TargetObject);
                            methodName = expression3.MethodName;
                        }
                    }
                    RootContext context = (RootContext) manager.Context[typeof(RootContext)];
                    bool flag = (context == null) || ((context != null) && (context.Value == obj2));
                    if (((methodName != null) && flag) && !(instance is CodeExpression))
                    {
                        EventDescriptor e = GetEventsHelper(manager, instance, null)[eventName];
                        if (e != null)
                        {
                            IEventBindingService service = (IEventBindingService) manager.GetService(typeof(IEventBindingService));
                            if (service != null)
                            {
                                service.GetEventProperty(e).SetValue(instance, methodName);
                            }
                        }
                        else
                        {
                            Error(manager, System.Design.SR.GetString("SerializerNoSuchEvent", new object[] { instance.GetType().FullName, eventName }), "SerializerNoSuchEvent");
                        }
                    }
                }
            }
        }

        private void DeserializeDetachEventStatement(IDesignerSerializationManager manager, CodeRemoveEventStatement statement)
        {
            using (TraceScope("CodeDomSerializerBase::DeserializeDetachEventStatement"))
            {
                CodeDelegateCreateExpression expression = this.DeserializeExpression(manager, null, statement.Listener) as CodeDelegateCreateExpression;
                if (expression != null)
                {
                    object obj3 = this.DeserializeExpression(manager, null, expression.TargetObject);
                    RootContext context = (RootContext) manager.Context[typeof(RootContext)];
                    if ((context == null) || ((context != null) && (context.Value == obj3)))
                    {
                        object instance = this.DeserializeExpression(manager, null, statement.Event.TargetObject);
                        if (!(instance is CodeExpression))
                        {
                            EventDescriptor e = GetEventsHelper(manager, instance, null)[statement.Event.EventName];
                            if (e != null)
                            {
                                IEventBindingService service = (IEventBindingService) manager.GetService(typeof(IEventBindingService));
                                if (service != null)
                                {
                                    service.GetEventProperty(e).SetValue(instance, null);
                                }
                            }
                            else
                            {
                                Error(manager, System.Design.SR.GetString("SerializerNoSuchEvent", new object[] { instance.GetType().FullName, statement.Event.EventName }), "SerializerNoSuchEvent");
                            }
                        }
                    }
                }
            }
        }

        protected object DeserializeExpression(IDesignerSerializationManager manager, string name, CodeExpression expression)
        {
            object instance = expression;
            using (TraceScope("CodeDomSerializerBase::DeserializeExpression"))
            {
                while (instance != null)
                {
                    CodePrimitiveExpression expression2 = instance as CodePrimitiveExpression;
                    if (expression2 != null)
                    {
                        return expression2.Value;
                    }
                    CodePropertyReferenceExpression propertyReferenceEx = instance as CodePropertyReferenceExpression;
                    if (propertyReferenceEx != null)
                    {
                        return this.DeserializePropertyReferenceExpression(manager, propertyReferenceEx, true);
                    }
                    if (instance is CodeThisReferenceExpression)
                    {
                        RootContext context = (RootContext) manager.Context[typeof(RootContext)];
                        if (context != null)
                        {
                            instance = context.Value;
                        }
                        else
                        {
                            IDesignerHost host = manager.GetService(typeof(IDesignerHost)) as IDesignerHost;
                            if (host != null)
                            {
                                instance = host.RootComponent;
                            }
                        }
                        if (instance == null)
                        {
                            Error(manager, System.Design.SR.GetString("SerializerNoRootExpression"), "SerializerNoRootExpression");
                        }
                        return instance;
                    }
                    CodeTypeReferenceExpression expression4 = instance as CodeTypeReferenceExpression;
                    if (expression4 != null)
                    {
                        return manager.GetType(GetTypeNameFromCodeTypeReference(manager, expression4.Type));
                    }
                    CodeObjectCreateExpression expression5 = instance as CodeObjectCreateExpression;
                    if (expression5 != null)
                    {
                        instance = null;
                        Type c = manager.GetType(GetTypeNameFromCodeTypeReference(manager, expression5.CreateType));
                        if (c != null)
                        {
                            object[] parameters = new object[expression5.Parameters.Count];
                            bool flag = true;
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                parameters[i] = this.DeserializeExpression(manager, null, expression5.Parameters[i]);
                                if (parameters[i] is CodeExpression)
                                {
                                    if ((typeof(Delegate).IsAssignableFrom(c) && (parameters.Length == 1)) && (parameters[i] is CodeMethodReferenceExpression))
                                    {
                                        CodeMethodReferenceExpression expression19 = (CodeMethodReferenceExpression) parameters[i];
                                        if (!(expression19.TargetObject is CodeThisReferenceExpression))
                                        {
                                            object obj3 = this.DeserializeExpression(manager, null, expression19.TargetObject);
                                            if (!(obj3 is CodeExpression))
                                            {
                                                MethodInfo method = c.GetMethod("Invoke");
                                                if (method != null)
                                                {
                                                    ParameterInfo[] infoArray = method.GetParameters();
                                                    Type[] types = new Type[infoArray.Length];
                                                    for (int j = 0; j < types.Length; j++)
                                                    {
                                                        types[j] = infoArray[i].ParameterType;
                                                    }
                                                    if (GetReflectionTypeHelper(manager, obj3).GetMethod(expression19.MethodName, types) != null)
                                                    {
                                                        MethodInfo info2 = obj3.GetType().GetMethod(expression19.MethodName, types);
                                                        instance = Activator.CreateInstance(c, new object[] { obj3, info2.MethodHandle.GetFunctionPointer() });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                instance = this.DeserializeInstance(manager, c, parameters, name, name != null);
                            }
                            return instance;
                        }
                        Error(manager, System.Design.SR.GetString("SerializerTypeNotFound", new object[] { expression5.CreateType.BaseType }), "SerializerTypeNotFound");
                        return instance;
                    }
                    CodeArgumentReferenceExpression expression6 = instance as CodeArgumentReferenceExpression;
                    if (expression6 != null)
                    {
                        instance = manager.GetInstance(expression6.ParameterName);
                        if (instance == null)
                        {
                            Error(manager, System.Design.SR.GetString("SerializerUndeclaredName", new object[] { expression6.ParameterName }), "SerializerUndeclaredName");
                        }
                        return instance;
                    }
                    CodeFieldReferenceExpression expression7 = instance as CodeFieldReferenceExpression;
                    if (expression7 != null)
                    {
                        object obj4 = this.DeserializeExpression(manager, null, expression7.TargetObject);
                        if ((obj4 != null) && !(obj4 is CodeExpression))
                        {
                            FieldInfo field;
                            object obj6;
                            RootContext context2 = (RootContext) manager.Context[typeof(RootContext)];
                            if ((context2 != null) && (context2.Value == obj4))
                            {
                                object obj5 = manager.GetInstance(expression7.FieldName);
                                if (obj5 != null)
                                {
                                    return obj5;
                                }
                                Error(manager, System.Design.SR.GetString("SerializerUndeclaredName", new object[] { expression7.FieldName }), "SerializerUndeclaredName");
                                return instance;
                            }
                            Type type = obj4 as Type;
                            if (type != null)
                            {
                                obj6 = null;
                                field = GetReflectionTypeFromTypeHelper(manager, type).GetField(expression7.FieldName, BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static);
                            }
                            else
                            {
                                obj6 = obj4;
                                field = GetReflectionTypeHelper(manager, obj4).GetField(expression7.FieldName, BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);
                            }
                            if (field != null)
                            {
                                return field.GetValue(obj6);
                            }
                            CodePropertyReferenceExpression expression20 = new CodePropertyReferenceExpression {
                                TargetObject = expression7.TargetObject,
                                PropertyName = expression7.FieldName
                            };
                            instance = this.DeserializePropertyReferenceExpression(manager, expression20, false);
                            if (instance == expression7)
                            {
                                Error(manager, System.Design.SR.GetString("SerializerUndeclaredName", new object[] { expression7.FieldName }), "SerializerUndeclaredName");
                            }
                            return instance;
                        }
                        Error(manager, System.Design.SR.GetString("SerializerFieldTargetEvalFailed", new object[] { expression7.FieldName }), "SerializerFieldTargetEvalFailed");
                        return instance;
                    }
                    CodeMethodInvokeExpression expression8 = instance as CodeMethodInvokeExpression;
                    if (expression8 != null)
                    {
                        object component = this.DeserializeExpression(manager, null, expression8.Method.TargetObject);
                        if (component != null)
                        {
                            object[] args = new object[expression8.Parameters.Count];
                            bool flag2 = true;
                            for (int k = 0; k < args.Length; k++)
                            {
                                args[k] = this.DeserializeExpression(manager, null, expression8.Parameters[k]);
                                if (args[k] is CodeExpression)
                                {
                                    flag2 = false;
                                    break;
                                }
                            }
                            if (flag2)
                            {
                                IComponentChangeService service = (IComponentChangeService) manager.GetService(typeof(IComponentChangeService));
                                Type type3 = component as Type;
                                if (type3 != null)
                                {
                                    return GetReflectionTypeFromTypeHelper(manager, type3).InvokeMember(expression8.Method.MethodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, args, null, null, null);
                                }
                                if (service != null)
                                {
                                    service.OnComponentChanging(component, null);
                                }
                                try
                                {
                                    instance = GetReflectionTypeHelper(manager, component).InvokeMember(expression8.Method.MethodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, component, args, null, null, null);
                                }
                                catch (MissingMethodException)
                                {
                                    CodeCastExpression targetObject = expression8.Method.TargetObject as CodeCastExpression;
                                    if (targetObject == null)
                                    {
                                        throw;
                                    }
                                    Type type4 = manager.GetType(GetTypeNameFromCodeTypeReference(manager, targetObject.TargetType));
                                    if ((type4 == null) || !type4.IsInterface)
                                    {
                                        throw;
                                    }
                                    instance = GetReflectionTypeFromTypeHelper(manager, type4).InvokeMember(expression8.Method.MethodName, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, component, args, null, null, null);
                                }
                                if (service != null)
                                {
                                    service.OnComponentChanged(component, null, null, null);
                                }
                                return instance;
                            }
                            if ((args.Length == 1) && (args[0] is CodeDelegateCreateExpression))
                            {
                                string methodName = expression8.Method.MethodName;
                                if (methodName.StartsWith("add_"))
                                {
                                    methodName = methodName.Substring(4);
                                    this.DeserializeAttachEventStatement(manager, new CodeAttachEventStatement(expression8.Method.TargetObject, methodName, (CodeExpression) args[0]));
                                    instance = null;
                                }
                            }
                        }
                        return instance;
                    }
                    CodeVariableReferenceExpression expression9 = instance as CodeVariableReferenceExpression;
                    if (expression9 != null)
                    {
                        instance = manager.GetInstance(expression9.VariableName);
                        if (instance == null)
                        {
                            Error(manager, System.Design.SR.GetString("SerializerUndeclaredName", new object[] { expression9.VariableName }), "SerializerUndeclaredName");
                        }
                        return instance;
                    }
                    CodeCastExpression expression10 = instance as CodeCastExpression;
                    if (expression10 != null)
                    {
                        instance = this.DeserializeExpression(manager, name, expression10.Expression);
                        IConvertible convertible = instance as IConvertible;
                        if (convertible != null)
                        {
                            Type conversionType = manager.GetType(GetTypeNameFromCodeTypeReference(manager, expression10.TargetType));
                            if (conversionType != null)
                            {
                                instance = convertible.ToType(conversionType, null);
                            }
                        }
                        return instance;
                    }
                    if (instance is CodeBaseReferenceExpression)
                    {
                        RootContext context3 = (RootContext) manager.Context[typeof(RootContext)];
                        if (context3 != null)
                        {
                            return context3.Value;
                        }
                        return null;
                    }
                    CodeArrayCreateExpression expression11 = instance as CodeArrayCreateExpression;
                    if (expression11 != null)
                    {
                        Type type6 = manager.GetType(GetTypeNameFromCodeTypeReference(manager, expression11.CreateType));
                        Array array = null;
                        if (type6 != null)
                        {
                            if (expression11.Initializers.Count > 0)
                            {
                                ArrayList list = new ArrayList(expression11.Initializers.Count);
                                foreach (CodeExpression expression22 in expression11.Initializers)
                                {
                                    try
                                    {
                                        object o = this.DeserializeExpression(manager, null, expression22);
                                        if (!(o is CodeExpression))
                                        {
                                            if (!type6.IsInstanceOfType(o))
                                            {
                                                o = Convert.ChangeType(o, type6, CultureInfo.InvariantCulture);
                                            }
                                            list.Add(o);
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        manager.ReportError(exception);
                                    }
                                }
                                array = Array.CreateInstance(type6, list.Count);
                                list.CopyTo(array, 0);
                            }
                            else if (expression11.SizeExpression != null)
                            {
                                IConvertible convertible2 = this.DeserializeExpression(manager, name, expression11.SizeExpression) as IConvertible;
                                if (convertible2 != null)
                                {
                                    int length = convertible2.ToInt32(null);
                                    array = Array.CreateInstance(type6, length);
                                }
                            }
                            else
                            {
                                array = Array.CreateInstance(type6, expression11.Size);
                            }
                        }
                        else
                        {
                            Error(manager, System.Design.SR.GetString("SerializerTypeNotFound", new object[] { expression11.CreateType.BaseType }), "SerializerTypeNotFound");
                        }
                        instance = array;
                        if ((instance != null) && (name != null))
                        {
                            manager.SetName(instance, name);
                        }
                        return instance;
                    }
                    CodeArrayIndexerExpression expression12 = instance as CodeArrayIndexerExpression;
                    if (expression12 != null)
                    {
                        instance = null;
                        Array array2 = this.DeserializeExpression(manager, name, expression12.TargetObject) as Array;
                        if (array2 != null)
                        {
                            int[] indices = new int[expression12.Indices.Count];
                            bool flag3 = true;
                            for (int m = 0; m < indices.Length; m++)
                            {
                                IConvertible convertible3 = this.DeserializeExpression(manager, name, expression12.Indices[m]) as IConvertible;
                                if (convertible3 != null)
                                {
                                    indices[m] = convertible3.ToInt32(null);
                                }
                                else
                                {
                                    flag3 = false;
                                    break;
                                }
                            }
                            if (flag3)
                            {
                                instance = array2.GetValue(indices);
                            }
                        }
                        return instance;
                    }
                    CodeBinaryOperatorExpression expression13 = instance as CodeBinaryOperatorExpression;
                    if (expression13 != null)
                    {
                        object obj10 = this.DeserializeExpression(manager, null, expression13.Left);
                        object obj11 = this.DeserializeExpression(manager, null, expression13.Right);
                        instance = obj10;
                        IConvertible left = obj10 as IConvertible;
                        IConvertible right = obj11 as IConvertible;
                        if ((left != null) && (right != null))
                        {
                            instance = this.ExecuteBinaryExpression(left, right, expression13.Operator);
                        }
                        return instance;
                    }
                    CodeDelegateInvokeExpression expression14 = instance as CodeDelegateInvokeExpression;
                    if (expression14 != null)
                    {
                        Delegate delegate2 = this.DeserializeExpression(manager, null, expression14.TargetObject) as Delegate;
                        if (delegate2 != null)
                        {
                            object[] objArray3 = new object[expression14.Parameters.Count];
                            bool flag4 = true;
                            for (int n = 0; n < objArray3.Length; n++)
                            {
                                objArray3[n] = this.DeserializeExpression(manager, null, expression14.Parameters[n]);
                                if (objArray3[n] is CodeExpression)
                                {
                                    flag4 = false;
                                    break;
                                }
                            }
                            if (flag4)
                            {
                                delegate2.DynamicInvoke(objArray3);
                            }
                        }
                        return instance;
                    }
                    CodeDirectionExpression expression15 = instance as CodeDirectionExpression;
                    if (expression15 != null)
                    {
                        return this.DeserializeExpression(manager, name, expression15.Expression);
                    }
                    CodeIndexerExpression expression16 = instance as CodeIndexerExpression;
                    if (expression16 != null)
                    {
                        instance = null;
                        object target = this.DeserializeExpression(manager, null, expression16.TargetObject);
                        if (target != null)
                        {
                            object[] objArray4 = new object[expression16.Indices.Count];
                            bool flag5 = true;
                            for (int num7 = 0; num7 < objArray4.Length; num7++)
                            {
                                objArray4[num7] = this.DeserializeExpression(manager, null, expression16.Indices[num7]);
                                if (objArray4[num7] is CodeExpression)
                                {
                                    flag5 = false;
                                    break;
                                }
                            }
                            if (flag5)
                            {
                                instance = GetReflectionTypeHelper(manager, target).InvokeMember("Item", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, target, objArray4, null, null, null);
                            }
                        }
                        return instance;
                    }
                    if (instance is CodeSnippetExpression)
                    {
                        return null;
                    }
                    CodeParameterDeclarationExpression expression17 = instance as CodeParameterDeclarationExpression;
                    if (expression17 != null)
                    {
                        return manager.GetType(GetTypeNameFromCodeTypeReference(manager, expression17.Type));
                    }
                    CodeTypeOfExpression expression18 = instance as CodeTypeOfExpression;
                    if (expression18 != null)
                    {
                        string typeNameFromCodeTypeReference = GetTypeNameFromCodeTypeReference(manager, expression18.Type);
                        for (int num8 = 0; num8 < expression18.Type.ArrayRank; num8++)
                        {
                            typeNameFromCodeTypeReference = typeNameFromCodeTypeReference + "[]";
                        }
                        instance = manager.GetType(typeNameFromCodeTypeReference);
                        if (instance == null)
                        {
                            Error(manager, System.Design.SR.GetString("SerializerTypeNotFound", new object[] { typeNameFromCodeTypeReference }), "SerializerTypeNotFound");
                        }
                        return instance;
                    }
                    if (((instance is CodeEventReferenceExpression) || (instance is CodeMethodReferenceExpression)) || !(instance is CodeDelegateCreateExpression))
                    {
                    }
                    return instance;
                }
            }
            return instance;
        }

        protected virtual object DeserializeInstance(IDesignerSerializationManager manager, Type type, object[] parameters, string name, bool addToContainer)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return manager.CreateInstance(type, parameters, name, addToContainer);
        }

        protected void DeserializePropertiesFromResources(IDesignerSerializationManager manager, object value, Attribute[] filter)
        {
            using (TraceScope("ComponentCodeDomSerializerBase::DeserializePropertiesFromResources"))
            {
                IDictionaryEnumerator metadataEnumerator = ResourceCodeDomSerializer.Default.GetMetadataEnumerator(manager);
                if (metadataEnumerator == null)
                {
                    metadataEnumerator = ResourceCodeDomSerializer.Default.GetEnumerator(manager, CultureInfo.InvariantCulture);
                }
                if (metadataEnumerator != null)
                {
                    string name;
                    RootContext context = manager.Context[typeof(RootContext)] as RootContext;
                    if ((context != null) && (context.Value == value))
                    {
                        name = "$this";
                    }
                    else
                    {
                        name = manager.GetName(value);
                    }
                    PropertyDescriptorCollection descriptors = GetPropertiesHelper(manager, value, null);
                    while (metadataEnumerator.MoveNext())
                    {
                        string key = metadataEnumerator.Key as string;
                        int index = key.IndexOf('.');
                        if ((index != -1) && key.Substring(0, index).Equals(name))
                        {
                            string str4 = key.Substring(index + 1);
                            PropertyDescriptor descriptor = descriptors[str4];
                            if (descriptor != null)
                            {
                                bool flag = true;
                                if (filter != null)
                                {
                                    AttributeCollection attributes = descriptor.Attributes;
                                    foreach (Attribute attribute in filter)
                                    {
                                        if (!attributes.Contains(attribute))
                                        {
                                            flag = false;
                                            break;
                                        }
                                    }
                                }
                                if (flag)
                                {
                                    object obj2 = metadataEnumerator.Value;
                                    try
                                    {
                                        descriptor.SetValue(value, obj2);
                                        continue;
                                    }
                                    catch (Exception exception)
                                    {
                                        manager.ReportError(exception);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool DeserializePropertyAssignStatement(IDesignerSerializationManager manager, CodeAssignStatement statement, CodePropertyReferenceExpression propertyReferenceEx, bool reportError)
        {
            object instance = this.DeserializeExpression(manager, null, propertyReferenceEx.TargetObject);
            if ((instance != null) && !(instance is CodeExpression))
            {
                PropertyDescriptor member = GetPropertiesHelper(manager, instance, runTimeProperties)[propertyReferenceEx.PropertyName];
                if (member != null)
                {
                    object underlyingSystemType = this.DeserializeExpression(manager, null, statement.Right);
                    if (underlyingSystemType is CodeExpression)
                    {
                        return false;
                    }
                    IConvertible convertible = underlyingSystemType as IConvertible;
                    if ((convertible != null) && (member.PropertyType != underlyingSystemType.GetType()))
                    {
                        try
                        {
                            underlyingSystemType = convertible.ToType(member.PropertyType, null);
                        }
                        catch
                        {
                        }
                    }
                    Type type = underlyingSystemType as Type;
                    if ((type != null) && (type.UnderlyingSystemType != null))
                    {
                        underlyingSystemType = type.UnderlyingSystemType;
                    }
                    MemberRelationship empty = MemberRelationship.Empty;
                    MemberRelationshipService service = null;
                    if (statement.Right is CodePropertyReferenceExpression)
                    {
                        service = manager.GetService(typeof(MemberRelationshipService)) as MemberRelationshipService;
                        if (service != null)
                        {
                            CodePropertyReferenceExpression right = (CodePropertyReferenceExpression) statement.Right;
                            object obj4 = this.DeserializeExpression(manager, null, right.TargetObject);
                            PropertyDescriptor descriptor2 = GetPropertiesHelper(manager, obj4, null)[right.PropertyName];
                            if (descriptor2 != null)
                            {
                                MemberRelationship source = new MemberRelationship(instance, member);
                                MemberRelationship relationship = new MemberRelationship(obj4, descriptor2);
                                empty = service[source];
                                if (service.SupportsRelationship(source, relationship))
                                {
                                    service[source] = relationship;
                                }
                            }
                        }
                    }
                    else
                    {
                        service = manager.GetService(typeof(MemberRelationshipService)) as MemberRelationshipService;
                        if (service != null)
                        {
                            empty = service[instance, member];
                            service[instance, member] = MemberRelationship.Empty;
                        }
                    }
                    try
                    {
                        member.SetValue(instance, underlyingSystemType);
                    }
                    catch
                    {
                        if (service != null)
                        {
                            service[instance, member] = empty;
                        }
                        throw;
                    }
                    return true;
                }
                if (reportError)
                {
                    Error(manager, System.Design.SR.GetString("SerializerNoSuchProperty", new object[] { instance.GetType().FullName, propertyReferenceEx.PropertyName }), "SerializerNoSuchProperty");
                }
            }
            return false;
        }

        private object DeserializePropertyReferenceExpression(IDesignerSerializationManager manager, CodePropertyReferenceExpression propertyReferenceEx, bool reportError)
        {
            object obj2 = propertyReferenceEx;
            object instance = this.DeserializeExpression(manager, null, propertyReferenceEx.TargetObject);
            if ((instance != null) && !(instance is CodeExpression))
            {
                if (instance is Type)
                {
                    PropertyInfo property = GetReflectionTypeFromTypeHelper(manager, (Type) instance).GetProperty(propertyReferenceEx.PropertyName, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static);
                    if (property != null)
                    {
                        obj2 = property.GetValue(null, null);
                    }
                }
                else
                {
                    PropertyDescriptor descriptor = GetPropertiesHelper(manager, instance, null)[propertyReferenceEx.PropertyName];
                    if (descriptor != null)
                    {
                        obj2 = descriptor.GetValue(instance);
                    }
                    else if (this.GetExpression(manager, instance) is CodeThisReferenceExpression)
                    {
                        PropertyInfo info = GetReflectionTypeHelper(manager, instance).GetProperty(propertyReferenceEx.PropertyName, BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        if (info != null)
                        {
                            obj2 = info.GetValue(instance, null);
                        }
                    }
                }
                if ((obj2 == propertyReferenceEx) && reportError)
                {
                    string str = (instance is Type) ? ((Type) instance).FullName : GetReflectionTypeHelper(manager, instance).FullName;
                    Error(manager, System.Design.SR.GetString("SerializerNoSuchProperty", new object[] { str, propertyReferenceEx.PropertyName }), "SerializerNoSuchProperty");
                }
            }
            return obj2;
        }

        protected void DeserializeStatement(IDesignerSerializationManager manager, CodeStatement statement)
        {
            using (TraceScope("CodeDomSerializerBase::DeserializeStatement"))
            {
                manager.Context.Push(statement);
                try
                {
                    CodeAssignStatement statement2 = statement as CodeAssignStatement;
                    if (statement2 != null)
                    {
                        this.DeserializeAssignStatement(manager, statement2);
                    }
                    else
                    {
                        CodeVariableDeclarationStatement statement3 = statement as CodeVariableDeclarationStatement;
                        if (statement3 != null)
                        {
                            this.DeserializeVariableDeclarationStatement(manager, statement3);
                        }
                        else if (!(statement is CodeCommentStatement))
                        {
                            CodeExpressionStatement statement4 = statement as CodeExpressionStatement;
                            if (statement4 != null)
                            {
                                this.DeserializeExpression(manager, null, statement4.Expression);
                            }
                            else if (statement is CodeMethodReturnStatement)
                            {
                                this.DeserializeExpression(manager, null, statement4.Expression);
                            }
                            else
                            {
                                CodeAttachEventStatement statement6 = statement as CodeAttachEventStatement;
                                if (statement6 != null)
                                {
                                    this.DeserializeAttachEventStatement(manager, statement6);
                                }
                                else
                                {
                                    CodeRemoveEventStatement statement7 = statement as CodeRemoveEventStatement;
                                    if (statement7 != null)
                                    {
                                        this.DeserializeDetachEventStatement(manager, statement7);
                                    }
                                    else
                                    {
                                        CodeLabeledStatement statement8 = statement as CodeLabeledStatement;
                                        if (statement8 != null)
                                        {
                                            this.DeserializeStatement(manager, statement8.Statement);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (CheckoutException)
                {
                    throw;
                }
                catch (Exception innerException)
                {
                    if (innerException is TargetInvocationException)
                    {
                        innerException = innerException.InnerException;
                    }
                    if (!(innerException is CodeDomSerializerException) && (statement.LinePragma != null))
                    {
                        innerException = new CodeDomSerializerException(innerException, statement.LinePragma);
                    }
                    manager.ReportError(innerException);
                }
                finally
                {
                    manager.Context.Pop();
                }
            }
        }

        private void DeserializeVariableDeclarationStatement(IDesignerSerializationManager manager, CodeVariableDeclarationStatement statement)
        {
            using (TraceScope("CodeDomSerializerBase::DeserializeVariableDeclarationStatement"))
            {
                if (statement.InitExpression != null)
                {
                    this.DeserializeExpression(manager, statement.Name, statement.InitExpression);
                }
            }
        }

        internal static void Error(IDesignerSerializationManager manager, string exceptionText, string helpLink)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (exceptionText == null)
            {
                throw new ArgumentNullException("exceptionText");
            }
            CodeStatement statement = (CodeStatement) manager.Context[typeof(CodeStatement)];
            CodeLinePragma linePragma = null;
            if (statement != null)
            {
                linePragma = statement.LinePragma;
            }
            Exception exception = new CodeDomSerializerException(exceptionText, linePragma) {
                HelpLink = helpLink
            };
            throw exception;
        }

        private object ExecuteBinaryExpression(IConvertible left, IConvertible right, CodeBinaryOperatorType op)
        {
            CodeBinaryOperatorType[] typeArray = new CodeBinaryOperatorType[] { CodeBinaryOperatorType.IdentityInequality, CodeBinaryOperatorType.IdentityEquality, CodeBinaryOperatorType.ValueEquality, CodeBinaryOperatorType.BooleanOr, CodeBinaryOperatorType.BooleanAnd, CodeBinaryOperatorType.LessThan, CodeBinaryOperatorType.LessThanOrEqual, CodeBinaryOperatorType.GreaterThan, CodeBinaryOperatorType.GreaterThanOrEqual };
            CodeBinaryOperatorType[] typeArray5 = new CodeBinaryOperatorType[5];
            typeArray5[1] = CodeBinaryOperatorType.Subtract;
            typeArray5[2] = CodeBinaryOperatorType.Multiply;
            typeArray5[3] = CodeBinaryOperatorType.Divide;
            typeArray5[4] = CodeBinaryOperatorType.Modulus;
            CodeBinaryOperatorType[] typeArray2 = typeArray5;
            CodeBinaryOperatorType[] typeArray3 = new CodeBinaryOperatorType[] { CodeBinaryOperatorType.BitwiseOr, CodeBinaryOperatorType.BitwiseAnd };
            for (int i = 0; i < typeArray3.Length; i++)
            {
                if (op == typeArray3[i])
                {
                    return this.ExecuteBinaryOperator(left, right, op);
                }
            }
            for (int j = 0; j < typeArray2.Length; j++)
            {
                if (op == typeArray2[j])
                {
                    return this.ExecuteMathOperator(left, right, op);
                }
            }
            for (int k = 0; k < typeArray.Length; k++)
            {
                if (op == typeArray[k])
                {
                    return this.ExecuteBooleanOperator(left, right, op);
                }
            }
            return left;
        }

        private object ExecuteBinaryOperator(IConvertible left, IConvertible right, CodeBinaryOperatorType op)
        {
            TypeCode typeCode = left.GetTypeCode();
            TypeCode code2 = right.GetTypeCode();
            TypeCode[] codeArray = new TypeCode[] { TypeCode.Byte, TypeCode.Char, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64 };
            int num = -1;
            int num2 = -1;
            for (int i = 0; i < codeArray.Length; i++)
            {
                if (typeCode == codeArray[i])
                {
                    num = i;
                }
                if (code2 == codeArray[i])
                {
                    num2 = i;
                }
                if ((num != -1) && (num2 != -1))
                {
                    break;
                }
            }
            if ((num == -1) || (num2 == -1))
            {
                return left;
            }
            int index = Math.Max(num, num2);
            object obj2 = left;
            switch (codeArray[index])
            {
                case TypeCode.Char:
                {
                    char ch = left.ToChar(null);
                    char ch2 = right.ToChar(null);
                    if (op != CodeBinaryOperatorType.BitwiseOr)
                    {
                        obj2 = ch & ch2;
                        break;
                    }
                    obj2 = ch | ch2;
                    break;
                }
                case TypeCode.Byte:
                {
                    byte num5 = left.ToByte(null);
                    byte num6 = right.ToByte(null);
                    if (op != CodeBinaryOperatorType.BitwiseOr)
                    {
                        obj2 = num5 & num6;
                        break;
                    }
                    obj2 = num5 | num6;
                    break;
                }
                case TypeCode.Int16:
                {
                    short num7 = left.ToInt16(null);
                    short num8 = right.ToInt16(null);
                    if (op != CodeBinaryOperatorType.BitwiseOr)
                    {
                        obj2 = num7 & num8;
                        break;
                    }
                    obj2 = (short) (((ushort) num7) | ((ushort) num8));
                    break;
                }
                case TypeCode.UInt16:
                {
                    ushort num9 = left.ToUInt16(null);
                    ushort num10 = right.ToUInt16(null);
                    if (op != CodeBinaryOperatorType.BitwiseOr)
                    {
                        obj2 = num9 & num10;
                        break;
                    }
                    obj2 = num9 | num10;
                    break;
                }
                case TypeCode.Int32:
                {
                    int num11 = left.ToInt32(null);
                    int num12 = right.ToInt32(null);
                    if (op != CodeBinaryOperatorType.BitwiseOr)
                    {
                        obj2 = num11 & num12;
                        break;
                    }
                    obj2 = num11 | num12;
                    break;
                }
                case TypeCode.UInt32:
                {
                    uint num13 = left.ToUInt32(null);
                    uint num14 = right.ToUInt32(null);
                    if (op != CodeBinaryOperatorType.BitwiseOr)
                    {
                        obj2 = num13 & num14;
                        break;
                    }
                    obj2 = num13 | num14;
                    break;
                }
                case TypeCode.Int64:
                {
                    long num15 = left.ToInt64(null);
                    long num16 = right.ToInt64(null);
                    if (op != CodeBinaryOperatorType.BitwiseOr)
                    {
                        obj2 = num15 & num16;
                        break;
                    }
                    obj2 = num15 | num16;
                    break;
                }
                case TypeCode.UInt64:
                {
                    ulong num17 = left.ToUInt64(null);
                    ulong num18 = right.ToUInt64(null);
                    if (op != CodeBinaryOperatorType.BitwiseOr)
                    {
                        obj2 = num17 & num18;
                        break;
                    }
                    obj2 = num17 | num18;
                    break;
                }
            }
            if ((obj2 != left) && (left is Enum))
            {
                obj2 = Enum.ToObject(left.GetType(), obj2);
            }
            return obj2;
        }

        private object ExecuteBooleanOperator(IConvertible left, IConvertible right, CodeBinaryOperatorType op)
        {
            bool flag = false;
            switch (op)
            {
                case CodeBinaryOperatorType.IdentityInequality:
                    return (left != right);

                case CodeBinaryOperatorType.IdentityEquality:
                    return (left == right);

                case CodeBinaryOperatorType.ValueEquality:
                    return left.Equals(right);

                case CodeBinaryOperatorType.BitwiseOr:
                case CodeBinaryOperatorType.BitwiseAnd:
                case CodeBinaryOperatorType.LessThan:
                case CodeBinaryOperatorType.LessThanOrEqual:
                case CodeBinaryOperatorType.GreaterThan:
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    return flag;

                case CodeBinaryOperatorType.BooleanOr:
                    return (left.ToBoolean(null) || right.ToBoolean(null));

                case CodeBinaryOperatorType.BooleanAnd:
                    return (left.ToBoolean(null) && right.ToBoolean(null));
            }
            return flag;
        }

        private object ExecuteMathOperator(IConvertible left, IConvertible right, CodeBinaryOperatorType op)
        {
            if (op == CodeBinaryOperatorType.Add)
            {
                string str = left as string;
                string str2 = right as string;
                if ((str == null) && (left is char))
                {
                    str = left.ToString();
                }
                if ((str2 == null) && (right is char))
                {
                    str2 = right.ToString();
                }
                if ((str != null) && (str2 != null))
                {
                    return (str + str2);
                }
            }
            return left;
        }

        internal static void FillStatementTable(IDesignerSerializationManager manager, IDictionary table, CodeStatementCollection statements)
        {
            FillStatementTable(manager, table, null, statements, null);
        }

        internal static void FillStatementTable(IDesignerSerializationManager manager, IDictionary table, Dictionary<string, string> names, CodeStatementCollection statements, string className)
        {
            using (TraceScope("CodeDomSerializerBase::FillStatementTable"))
            {
                CodeExpressionStatement statement4 = null;
                foreach (CodeStatement statement6 in statements)
                {
                    CodeCastExpression expression2;
                    CodeExpression left = null;
                    CodeAssignStatement statement = statement6 as CodeAssignStatement;
                    if (statement != null)
                    {
                        left = statement.Left;
                    }
                    else
                    {
                        CodeAttachEventStatement statement2 = statement6 as CodeAttachEventStatement;
                        if (statement2 != null)
                        {
                            left = statement2.Event;
                        }
                        else
                        {
                            CodeRemoveEventStatement statement3 = statement6 as CodeRemoveEventStatement;
                            if (statement3 != null)
                            {
                                left = statement3.Event;
                            }
                            else
                            {
                                statement4 = statement6 as CodeExpressionStatement;
                                if (statement4 != null)
                                {
                                    left = statement4.Expression;
                                }
                                else
                                {
                                    CodeVariableDeclarationStatement statement5 = statement6 as CodeVariableDeclarationStatement;
                                    if (statement5 != null)
                                    {
                                        AddStatement(table, statement5.Name, statement5);
                                        if (((names != null) && (statement5.Type != null)) && !string.IsNullOrEmpty(statement5.Type.BaseType))
                                        {
                                            names[statement5.Name] = GetTypeNameFromCodeTypeReference(manager, statement5.Type);
                                        }
                                        left = null;
                                    }
                                }
                            }
                        }
                    }
                    if (left == null)
                    {
                        continue;
                    }
                Label_00E4:
                    while ((expression2 = left as CodeCastExpression) != null)
                    {
                        left = expression2.Expression;
                    }
                    CodeDelegateCreateExpression expression3 = left as CodeDelegateCreateExpression;
                    if (expression3 != null)
                    {
                        left = expression3.TargetObject;
                        goto Label_00E4;
                    }
                    CodeDelegateInvokeExpression expression4 = left as CodeDelegateInvokeExpression;
                    if (expression4 != null)
                    {
                        left = expression4.TargetObject;
                        goto Label_00E4;
                    }
                    CodeDirectionExpression expression5 = left as CodeDirectionExpression;
                    if (expression5 != null)
                    {
                        left = expression5.Expression;
                        goto Label_00E4;
                    }
                    CodeEventReferenceExpression expression6 = left as CodeEventReferenceExpression;
                    if (expression6 != null)
                    {
                        left = expression6.TargetObject;
                        goto Label_00E4;
                    }
                    CodeMethodInvokeExpression expression7 = left as CodeMethodInvokeExpression;
                    if (expression7 != null)
                    {
                        left = expression7.Method;
                        goto Label_00E4;
                    }
                    CodeMethodReferenceExpression expression8 = left as CodeMethodReferenceExpression;
                    if (expression8 != null)
                    {
                        left = expression8.TargetObject;
                        goto Label_00E4;
                    }
                    CodeArrayIndexerExpression expression9 = left as CodeArrayIndexerExpression;
                    if (expression9 != null)
                    {
                        left = expression9.TargetObject;
                        goto Label_00E4;
                    }
                    CodeFieldReferenceExpression expression10 = left as CodeFieldReferenceExpression;
                    if (expression10 != null)
                    {
                        bool flag = false;
                        if (expression10.TargetObject is CodeThisReferenceExpression)
                        {
                            Type objectType = GetType(manager, expression10.FieldName, names);
                            if (objectType != null)
                            {
                                CodeDomSerializer serializer = manager.GetSerializer(objectType, typeof(CodeDomSerializer)) as CodeDomSerializer;
                                if (serializer != null)
                                {
                                    string str = serializer.GetTargetComponentName(statement6, left, objectType);
                                    if (!string.IsNullOrEmpty(str))
                                    {
                                        AddStatement(table, str, statement6);
                                        flag = true;
                                    }
                                }
                            }
                            if (!flag)
                            {
                                AddStatement(table, expression10.FieldName, statement6);
                            }
                            continue;
                        }
                        left = expression10.TargetObject;
                        goto Label_00E4;
                    }
                    CodePropertyReferenceExpression expression11 = left as CodePropertyReferenceExpression;
                    if (expression11 != null)
                    {
                        if ((expression11.TargetObject is CodeThisReferenceExpression) && ((names == null) || names.ContainsKey(expression11.PropertyName)))
                        {
                            AddStatement(table, expression11.PropertyName, statement6);
                            continue;
                        }
                        left = expression11.TargetObject;
                        goto Label_00E4;
                    }
                    CodeVariableReferenceExpression expression12 = left as CodeVariableReferenceExpression;
                    if (expression12 != null)
                    {
                        bool flag2 = false;
                        if (names != null)
                        {
                            Type type2 = GetType(manager, expression12.VariableName, names);
                            if (type2 != null)
                            {
                                CodeDomSerializer serializer2 = manager.GetSerializer(type2, typeof(CodeDomSerializer)) as CodeDomSerializer;
                                if (serializer2 != null)
                                {
                                    string str2 = serializer2.GetTargetComponentName(statement6, left, type2);
                                    if (!string.IsNullOrEmpty(str2))
                                    {
                                        AddStatement(table, str2, statement6);
                                        flag2 = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            AddStatement(table, expression12.VariableName, statement6);
                            flag2 = true;
                        }
                        if (!flag2)
                        {
                            manager.ReportError(new CodeDomSerializerException(System.Design.SR.GetString("SerializerUndeclaredName", new object[] { expression12.VariableName }), manager));
                        }
                    }
                    else if (((left is CodeThisReferenceExpression) || (left is CodeBaseReferenceExpression)) && (className != null))
                    {
                        AddStatement(table, className, statement6);
                    }
                }
            }
        }

        protected static AttributeCollection GetAttributesFromTypeHelper(IDesignerSerializationManager manager, Type type)
        {
            if ((type == null) || (manager == null))
            {
                return null;
            }
            if (type.IsValueType)
            {
                TypeDescriptionProvider targetFrameworkProviderForType = GetTargetFrameworkProviderForType(manager, type);
                if (targetFrameworkProviderForType != null)
                {
                    if (targetFrameworkProviderForType.IsSupportedType(type))
                    {
                        ICustomTypeDescriptor typeDescriptor = targetFrameworkProviderForType.GetTypeDescriptor(type);
                        if (typeDescriptor != null)
                        {
                            return typeDescriptor.GetAttributes();
                        }
                    }
                    else
                    {
                        Error(manager, System.Design.SR.GetString("TypeNotFoundInTargetFramework", new object[] { type.FullName }), "SerializerUndeclaredName");
                    }
                }
            }
            return TypeDescriptor.GetAttributes(type);
        }

        protected static AttributeCollection GetAttributesHelper(IDesignerSerializationManager manager, object instance)
        {
            if ((instance == null) || (manager == null))
            {
                return null;
            }
            if (instance.GetType().IsValueType)
            {
                TypeDescriptionProvider targetFrameworkProvider = GetTargetFrameworkProvider(manager, instance);
                if (targetFrameworkProvider != null)
                {
                    if (targetFrameworkProvider.IsSupportedType(instance.GetType()))
                    {
                        ICustomTypeDescriptor typeDescriptor = targetFrameworkProvider.GetTypeDescriptor(instance);
                        if (typeDescriptor != null)
                        {
                            return typeDescriptor.GetAttributes();
                        }
                    }
                    else
                    {
                        Error(manager, System.Design.SR.GetString("TypeNotFoundInTargetFramework", new object[] { instance.GetType().FullName }), "SerializerUndeclaredName");
                    }
                }
            }
            return TypeDescriptor.GetAttributes(instance);
        }

        protected static EventDescriptorCollection GetEventsHelper(IDesignerSerializationManager manager, object instance, Attribute[] attributes)
        {
            if ((instance == null) || (manager == null))
            {
                return null;
            }
            if (instance.GetType().IsValueType)
            {
                TypeDescriptionProvider targetFrameworkProvider = GetTargetFrameworkProvider(manager, instance);
                if (targetFrameworkProvider != null)
                {
                    if (targetFrameworkProvider.IsSupportedType(instance.GetType()))
                    {
                        ICustomTypeDescriptor typeDescriptor = targetFrameworkProvider.GetTypeDescriptor(instance);
                        if (typeDescriptor != null)
                        {
                            if (attributes == null)
                            {
                                return typeDescriptor.GetEvents();
                            }
                            return typeDescriptor.GetEvents(attributes);
                        }
                    }
                    else
                    {
                        Error(manager, System.Design.SR.GetString("TypeNotFoundInTargetFramework", new object[] { instance.GetType().FullName }), "SerializerUndeclaredName");
                    }
                }
            }
            if (attributes == null)
            {
                return TypeDescriptor.GetEvents(instance);
            }
            return TypeDescriptor.GetEvents(instance, attributes);
        }

        protected CodeExpression GetExpression(IDesignerSerializationManager manager, object value)
        {
            CodeExpression expression = null;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            ExpressionTable table = manager.Context[typeof(ExpressionTable)] as ExpressionTable;
            if (table != null)
            {
                expression = table.GetExpression(value);
            }
            if (expression == null)
            {
                RootContext context = manager.Context[typeof(RootContext)] as RootContext;
                if ((context != null) && object.ReferenceEquals(context.Value, value))
                {
                    expression = context.Expression;
                }
            }
            if (expression == null)
            {
                string name = manager.GetName(value);
                if ((name == null) || (name.IndexOf('.') != -1))
                {
                    IReferenceService service = manager.GetService(typeof(IReferenceService)) as IReferenceService;
                    if (service != null)
                    {
                        name = service.GetName(value);
                        if ((name != null) && (name.IndexOf('.') != -1))
                        {
                            string[] strArray = name.Split(new char[] { '.' });
                            object instance = manager.GetInstance(strArray[0]);
                            if (instance != null)
                            {
                                CodeExpression targetObject = this.SerializeToExpression(manager, instance);
                                if (targetObject != null)
                                {
                                    for (int i = 1; i < strArray.Length; i++)
                                    {
                                        targetObject = new CodePropertyReferenceExpression(targetObject, strArray[i]);
                                    }
                                    expression = targetObject;
                                }
                            }
                        }
                    }
                }
            }
            if (expression == null)
            {
                ExpressionContext context2 = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                if ((context2 != null) && object.ReferenceEquals(context2.PresetValue, value))
                {
                    expression = context2.Expression;
                }
            }
            if (expression != null)
            {
                ComponentCache.Entry entry = (ComponentCache.Entry) manager.Context[typeof(ComponentCache.Entry)];
                ComponentCache cache = (ComponentCache) manager.Context[typeof(ComponentCache)];
                if (((entry != null) && (entry.Component != value)) && (cache != null))
                {
                    ComponentCache.Entry entryAll = null;
                    entryAll = cache.GetEntryAll(value);
                    if ((entryAll != null) && (entry.Component != null))
                    {
                        entryAll.AddDependency(entry.Component);
                    }
                }
            }
            return expression;
        }

        private PropertyDescriptorCollection GetFilteredProperties(IDesignerSerializationManager manager, object value, Attribute[] filter)
        {
            IComponent component = value as IComponent;
            PropertyDescriptorCollection properties = GetPropertiesHelper(manager, value, filter);
            if (component != null)
            {
                if (((IDictionary) properties).IsReadOnly)
                {
                    PropertyDescriptor[] array = new PropertyDescriptor[properties.Count];
                    properties.CopyTo(array, 0);
                    properties = new PropertyDescriptorCollection(array);
                }
                PropertyDescriptor descriptor = manager.Properties["FilteredProperties"];
                if (descriptor != null)
                {
                    ITypeDescriptorFilterService service = descriptor.GetValue(manager) as ITypeDescriptorFilterService;
                    if (service != null)
                    {
                        service.FilterProperties(component, properties);
                    }
                }
            }
            return properties;
        }

        private CodeExpression GetLegacyExpression(IDesignerSerializationManager manager, object value)
        {
            LegacyExpressionTable table = manager.Context[typeof(LegacyExpressionTable)] as LegacyExpressionTable;
            CodeExpression expression = null;
            if (table != null)
            {
                object obj2 = table[value];
                if (obj2 != value)
                {
                    return (obj2 as CodeExpression);
                }
                string name = manager.GetName(value);
                bool flag = false;
                if (name == null)
                {
                    IReferenceService service = (IReferenceService) manager.GetService(typeof(IReferenceService));
                    if (service != null)
                    {
                        name = service.GetName(value);
                        flag = name != null;
                    }
                }
                if (name != null)
                {
                    RootContext context = (RootContext) manager.Context[typeof(RootContext)];
                    if (context != null)
                    {
                        if (context.Value == value)
                        {
                            expression = context.Expression;
                        }
                        else if (flag && (name.IndexOf('.') != -1))
                        {
                            int index = name.IndexOf('.');
                            expression = new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(context.Expression, name.Substring(0, index)), name.Substring(index + 1));
                        }
                        else
                        {
                            expression = new CodeFieldReferenceExpression(context.Expression, name);
                        }
                    }
                    else if (flag && (name.IndexOf('.') != -1))
                    {
                        int length = name.IndexOf('.');
                        expression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(name.Substring(0, length)), name.Substring(length + 1));
                    }
                    else
                    {
                        expression = new CodeVariableReferenceExpression(name);
                    }
                }
                table[value] = expression;
            }
            return expression;
        }

        protected static PropertyDescriptorCollection GetPropertiesHelper(IDesignerSerializationManager manager, object instance, Attribute[] attributes)
        {
            if ((instance == null) || (manager == null))
            {
                return null;
            }
            if (instance.GetType().IsValueType)
            {
                TypeDescriptionProvider targetFrameworkProvider = GetTargetFrameworkProvider(manager, instance);
                if (targetFrameworkProvider != null)
                {
                    if (targetFrameworkProvider.IsSupportedType(instance.GetType()))
                    {
                        ICustomTypeDescriptor typeDescriptor = targetFrameworkProvider.GetTypeDescriptor(instance);
                        if (typeDescriptor != null)
                        {
                            if (attributes == null)
                            {
                                return typeDescriptor.GetProperties();
                            }
                            return typeDescriptor.GetProperties(attributes);
                        }
                    }
                    else
                    {
                        Error(manager, System.Design.SR.GetString("TypeNotFoundInTargetFramework", new object[] { instance.GetType().FullName }), "SerializerUndeclaredName");
                    }
                }
            }
            if (attributes == null)
            {
                return TypeDescriptor.GetProperties(instance);
            }
            return TypeDescriptor.GetProperties(instance, attributes);
        }

        protected static Type GetReflectionTypeFromTypeHelper(IDesignerSerializationManager manager, Type type)
        {
            if ((type == null) || (manager == null))
            {
                return null;
            }
            TypeDescriptionProvider targetFrameworkProviderForType = GetTargetFrameworkProviderForType(manager, type);
            if (targetFrameworkProviderForType != null)
            {
                if (targetFrameworkProviderForType.IsSupportedType(type))
                {
                    return targetFrameworkProviderForType.GetReflectionType(type);
                }
                Error(manager, System.Design.SR.GetString("TypeNotFoundInTargetFramework", new object[] { type.FullName }), "SerializerUndeclaredName");
            }
            return TypeDescriptor.GetReflectionType(type);
        }

        protected static Type GetReflectionTypeHelper(IDesignerSerializationManager manager, object instance)
        {
            if ((instance == null) || (manager == null))
            {
                return null;
            }
            Type type = instance.GetType();
            if (type.IsValueType)
            {
                TypeDescriptionProvider targetFrameworkProvider = GetTargetFrameworkProvider(manager, instance);
                if (targetFrameworkProvider != null)
                {
                    if (targetFrameworkProvider.IsSupportedType(type))
                    {
                        return targetFrameworkProvider.GetReflectionType(instance);
                    }
                    Error(manager, System.Design.SR.GetString("TypeNotFoundInTargetFramework", new object[] { instance.GetType().FullName }), "SerializerUndeclaredName");
                }
            }
            return TypeDescriptor.GetReflectionType(instance);
        }

        protected CodeDomSerializer GetSerializer(IDesignerSerializationManager manager, object value)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value != null)
            {
                AttributeCollection attributesHelper = GetAttributesHelper(manager, value);
                AttributeCollection attributesFromTypeHelper = GetAttributesFromTypeHelper(manager, value.GetType());
                if (attributesHelper.Count != attributesFromTypeHelper.Count)
                {
                    string typeName = null;
                    Type type = typeof(CodeDomSerializer);
                    DesignerSerializationManager manager2 = manager as DesignerSerializationManager;
                    foreach (Attribute attribute in attributesHelper)
                    {
                        DesignerSerializerAttribute attribute2 = attribute as DesignerSerializerAttribute;
                        if (attribute2 != null)
                        {
                            Type runtimeType;
                            if (manager2 != null)
                            {
                                runtimeType = manager2.GetRuntimeType(attribute2.SerializerBaseTypeName);
                            }
                            else
                            {
                                runtimeType = manager.GetType(attribute2.SerializerBaseTypeName);
                            }
                            if (runtimeType == type)
                            {
                                typeName = attribute2.SerializerTypeName;
                                break;
                            }
                        }
                    }
                    if (typeName != null)
                    {
                        foreach (Attribute attribute3 in attributesFromTypeHelper)
                        {
                            DesignerSerializerAttribute attribute4 = attribute3 as DesignerSerializerAttribute;
                            if (attribute4 != null)
                            {
                                Type type3;
                                if (manager2 != null)
                                {
                                    type3 = manager2.GetRuntimeType(attribute4.SerializerBaseTypeName);
                                }
                                else
                                {
                                    type3 = manager.GetType(attribute4.SerializerBaseTypeName);
                                }
                                if (type3 == type)
                                {
                                    if (typeName.Equals(attribute4.SerializerTypeName))
                                    {
                                        typeName = null;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    if (typeName != null)
                    {
                        Type c = (manager2 != null) ? manager2.GetRuntimeType(typeName) : manager.GetType(typeName);
                        if ((c != null) && type.IsAssignableFrom(c))
                        {
                            return (CodeDomSerializer) Activator.CreateInstance(c);
                        }
                    }
                }
            }
            Type objectType = null;
            if (value != null)
            {
                objectType = value.GetType();
            }
            return (CodeDomSerializer) manager.GetSerializer(objectType, typeof(CodeDomSerializer));
        }

        protected CodeDomSerializer GetSerializer(IDesignerSerializationManager manager, Type valueType)
        {
            return (manager.GetSerializer(valueType, typeof(CodeDomSerializer)) as CodeDomSerializer);
        }

        protected static TypeDescriptionProvider GetTargetFrameworkProvider(IServiceProvider provider, object instance)
        {
            TypeDescriptionProviderService service = provider.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
            if (service != null)
            {
                return service.GetProvider(instance);
            }
            return null;
        }

        private static TypeDescriptionProvider GetTargetFrameworkProviderForType(IServiceProvider provider, Type type)
        {
            TypeDescriptionProviderService service = provider.GetService(typeof(TypeDescriptionProviderService)) as TypeDescriptionProviderService;
            if (service != null)
            {
                return service.GetProvider(type);
            }
            return null;
        }

        internal static Type GetType(IDesignerSerializationManager manager, string name, Dictionary<string, string> names)
        {
            Type type = null;
            if ((names != null) && names.ContainsKey(name))
            {
                string str = names[name];
                if ((manager != null) && !string.IsNullOrEmpty(str))
                {
                    type = manager.GetType(str);
                }
            }
            return type;
        }

        internal static string GetTypeNameFromCodeTypeReference(IDesignerSerializationManager manager, CodeTypeReference typeref)
        {
            if ((typeref.TypeArguments != null) && (typeref.TypeArguments.Count != 0))
            {
                return GetTypeNameFromCodeTypeReferenceHelper(manager, typeref);
            }
            return typeref.BaseType;
        }

        private static string GetTypeNameFromCodeTypeReferenceHelper(IDesignerSerializationManager manager, CodeTypeReference typeref)
        {
            if ((typeref.TypeArguments == null) || (typeref.TypeArguments.Count == 0))
            {
                Type type = manager.GetType(typeref.BaseType);
                if (type != null)
                {
                    return GetReflectionTypeFromTypeHelper(manager, type).AssemblyQualifiedName;
                }
                return typeref.BaseType;
            }
            StringBuilder builder = new StringBuilder(typeref.BaseType);
            if (!typeref.BaseType.Contains("`"))
            {
                builder.Append("`");
                builder.Append(typeref.TypeArguments.Count);
            }
            builder.Append("[");
            bool flag = true;
            foreach (CodeTypeReference reference in typeref.TypeArguments)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                builder.Append("[");
                builder.Append(GetTypeNameFromCodeTypeReferenceHelper(manager, reference));
                builder.Append("]");
                flag = false;
            }
            builder.Append("]");
            return builder.ToString();
        }

        protected string GetUniqueName(IDesignerSerializationManager manager, object value)
        {
            string str2;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            string name = manager.GetName(value);
            if (name != null)
            {
                return name;
            }
            Type reflectionTypeHelper = GetReflectionTypeHelper(manager, value);
            INameCreationService service = manager.GetService(typeof(INameCreationService)) as INameCreationService;
            if (service != null)
            {
                str2 = service.CreateName(null, reflectionTypeHelper);
            }
            else
            {
                str2 = reflectionTypeHelper.Name.ToLower(CultureInfo.InvariantCulture);
            }
            int num = 1;
            ComponentCache cache = manager.Context[typeof(ComponentCache)] as ComponentCache;
            while (true)
            {
                name = string.Format(CultureInfo.CurrentCulture, "{0}{1}", new object[] { str2, num });
                if ((manager.GetInstance(name) == null) && ((cache == null) || !cache.ContainsLocalName(name)))
                {
                    manager.SetName(value, name);
                    ComponentCache.Entry entry = manager.Context[typeof(ComponentCache.Entry)] as ComponentCache.Entry;
                    if (entry != null)
                    {
                        entry.AddLocalName(name);
                    }
                    return name;
                }
                num++;
            }
        }

        protected bool IsSerialized(IDesignerSerializationManager manager, object value)
        {
            return this.IsSerialized(manager, value, false);
        }

        protected bool IsSerialized(IDesignerSerializationManager manager, object value, bool honorPreset)
        {
            bool flag = false;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            ExpressionTable table = manager.Context[typeof(ExpressionTable)] as ExpressionTable;
            if (((table == null) || (table.GetExpression(value) == null)) || (honorPreset && table.ContainsPresetExpression(value)))
            {
                return flag;
            }
            return true;
        }

        protected CodeExpression SerializeCreationExpression(IDesignerSerializationManager manager, object value, out bool isComplete)
        {
            isComplete = false;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            TypeConverter converter = TypeDescriptor.GetConverter(value);
            ExpressionContext context = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
            if ((context != null) && object.ReferenceEquals(context.PresetValue, value))
            {
                CodeExpression expression = context.Expression;
                if (converter.CanConvertTo(typeof(InstanceDescriptor)))
                {
                    InstanceDescriptor descriptor = converter.ConvertTo(value, typeof(InstanceDescriptor)) as InstanceDescriptor;
                    if ((descriptor != null) && (descriptor.MemberInfo != null))
                    {
                        isComplete = descriptor.IsComplete;
                    }
                }
                return expression;
            }
            if (converter.CanConvertTo(typeof(InstanceDescriptor)))
            {
                InstanceDescriptor descriptor2 = converter.ConvertTo(value, typeof(InstanceDescriptor)) as InstanceDescriptor;
                if ((descriptor2 != null) && (descriptor2.MemberInfo != null))
                {
                    isComplete = descriptor2.IsComplete;
                    return this.SerializeInstanceDescriptor(manager, value, descriptor2);
                }
            }
            if (GetReflectionTypeHelper(manager, value).IsSerializable && (!(value is IComponent) || (((IComponent) value).Site == null)))
            {
                CodeExpression expression2 = this.SerializeToResourceExpression(manager, value);
                if (expression2 != null)
                {
                    isComplete = true;
                    return expression2;
                }
            }
            if (GetReflectionTypeHelper(manager, value).GetConstructor(new Type[0]) != null)
            {
                isComplete = false;
                return new CodeObjectCreateExpression(TypeDescriptor.GetClassName(value), new CodeExpression[0]);
            }
            return null;
        }

        protected void SerializeEvent(IDesignerSerializationManager manager, CodeStatementCollection statements, object value, EventDescriptor descriptor)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (statements == null)
            {
                throw new ArgumentNullException("statements");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            using (TraceScope("CodeDomSerializerBase::SerializeEvent"))
            {
                manager.Context.Push(statements);
                manager.Context.Push(descriptor);
                try
                {
                    MemberCodeDomSerializer serializer = (MemberCodeDomSerializer) manager.GetSerializer(descriptor.GetType(), typeof(MemberCodeDomSerializer));
                    if ((serializer != null) && serializer.ShouldSerialize(manager, value, descriptor))
                    {
                        serializer.Serialize(manager, value, descriptor, statements);
                    }
                }
                finally
                {
                    manager.Context.Pop();
                    manager.Context.Pop();
                }
            }
        }

        protected void SerializeEvents(IDesignerSerializationManager manager, CodeStatementCollection statements, object value, params Attribute[] filter)
        {
            foreach (EventDescriptor descriptor in GetEventsHelper(manager, value, filter).Sort())
            {
                this.SerializeEvent(manager, statements, value, descriptor);
            }
        }

        private CodeExpression SerializeInstanceDescriptor(IDesignerSerializationManager manager, object value, InstanceDescriptor descriptor)
        {
            CodeExpression expression = null;
            using (TraceScope("CodeDomSerializerBase::SerializeInstanceDescriptor"))
            {
                CodeExpression[] parameters = new CodeExpression[descriptor.Arguments.Count];
                object[] array = new object[parameters.Length];
                ParameterInfo[] infoArray = null;
                if (parameters.Length > 0)
                {
                    descriptor.Arguments.CopyTo(array, 0);
                    MethodBase memberInfo = descriptor.MemberInfo as MethodBase;
                    if (memberInfo != null)
                    {
                        infoArray = memberInfo.GetParameters();
                    }
                }
                bool flag = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    object obj2 = array[i];
                    CodeExpression expression2 = null;
                    ExpressionContext context = null;
                    ExpressionContext context2 = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                    if (context2 != null)
                    {
                        context = new ExpressionContext(context2.Expression, infoArray[i].ParameterType, context2.Owner);
                        manager.Context.Push(context);
                    }
                    try
                    {
                        expression2 = this.SerializeToExpression(manager, obj2);
                    }
                    finally
                    {
                        if (context != null)
                        {
                            manager.Context.Pop();
                        }
                    }
                    if (expression2 != null)
                    {
                        if ((obj2 != null) && !infoArray[i].ParameterType.IsAssignableFrom(obj2.GetType()))
                        {
                            expression2 = new CodeCastExpression(infoArray[i].ParameterType, expression2);
                        }
                        parameters[i] = expression2;
                    }
                    else
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag)
                {
                    return expression;
                }
                Type declaringType = descriptor.MemberInfo.DeclaringType;
                CodeTypeReference createType = new CodeTypeReference(declaringType);
                if (descriptor.MemberInfo is ConstructorInfo)
                {
                    expression = new CodeObjectCreateExpression(createType, parameters);
                }
                else if (descriptor.MemberInfo is MethodInfo)
                {
                    CodeTypeReferenceExpression targetObject = new CodeTypeReferenceExpression(createType);
                    CodeMethodReferenceExpression method = new CodeMethodReferenceExpression(targetObject, descriptor.MemberInfo.Name);
                    expression = new CodeMethodInvokeExpression(method, parameters);
                    declaringType = ((MethodInfo) descriptor.MemberInfo).ReturnType;
                }
                else if (descriptor.MemberInfo is PropertyInfo)
                {
                    CodeTypeReferenceExpression expression5 = new CodeTypeReferenceExpression(createType);
                    CodePropertyReferenceExpression expression6 = new CodePropertyReferenceExpression(expression5, descriptor.MemberInfo.Name);
                    expression = expression6;
                    declaringType = ((PropertyInfo) descriptor.MemberInfo).PropertyType;
                }
                else if (descriptor.MemberInfo is FieldInfo)
                {
                    CodeTypeReferenceExpression expression7 = new CodeTypeReferenceExpression(createType);
                    expression = new CodeFieldReferenceExpression(expression7, descriptor.MemberInfo.Name);
                    declaringType = ((FieldInfo) descriptor.MemberInfo).FieldType;
                }
                Type type = value.GetType();
                while (!type.IsPublic)
                {
                    type = type.BaseType;
                }
                if (!type.IsAssignableFrom(declaringType))
                {
                    expression = new CodeCastExpression(type, expression);
                }
            }
            return expression;
        }

        protected void SerializeProperties(IDesignerSerializationManager manager, CodeStatementCollection statements, object value, Attribute[] filter)
        {
            using (TraceScope("CodeDomSerializerBase::SerializeProperties"))
            {
                PropertyDescriptorCollection descriptors = this.GetFilteredProperties(manager, value, filter).Sort();
                InheritanceAttribute context = (InheritanceAttribute) GetAttributesHelper(manager, value)[typeof(InheritanceAttribute)];
                if (context == null)
                {
                    context = InheritanceAttribute.NotInherited;
                }
                manager.Context.Push(context);
                try
                {
                    foreach (PropertyDescriptor descriptor in descriptors)
                    {
                        if (!descriptor.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden))
                        {
                            this.SerializeProperty(manager, statements, value, descriptor);
                        }
                    }
                }
                finally
                {
                    manager.Context.Pop();
                }
            }
        }

        protected void SerializePropertiesToResources(IDesignerSerializationManager manager, CodeStatementCollection statements, object value, Attribute[] filter)
        {
            using (TraceScope("ComponentCodeDomSerializerBase::SerializePropertiesToResources"))
            {
                PropertyDescriptorCollection descriptors = GetPropertiesHelper(manager, value, filter);
                manager.Context.Push(statements);
                try
                {
                    CodeExpression targetObject = this.SerializeToExpression(manager, value);
                    if (targetObject != null)
                    {
                        CodePropertyReferenceExpression expression = new CodePropertyReferenceExpression(targetObject, string.Empty);
                        foreach (PropertyDescriptor descriptor in descriptors)
                        {
                            ExpressionContext context = new ExpressionContext(expression, descriptor.PropertyType, value);
                            manager.Context.Push(context);
                            try
                            {
                                if (descriptor.Attributes.Contains(DesignerSerializationVisibilityAttribute.Visible))
                                {
                                    string name;
                                    expression.PropertyName = descriptor.Name;
                                    if (targetObject is CodeThisReferenceExpression)
                                    {
                                        name = "$this";
                                    }
                                    else
                                    {
                                        name = manager.GetName(value);
                                    }
                                    name = string.Format(CultureInfo.CurrentCulture, "{0}.{1}", new object[] { name, descriptor.Name });
                                    ResourceCodeDomSerializer.Default.SerializeMetadata(manager, name, descriptor.GetValue(value), descriptor.ShouldSerializeValue(value));
                                }
                            }
                            finally
                            {
                                manager.Context.Pop();
                            }
                        }
                    }
                }
                finally
                {
                    manager.Context.Pop();
                }
            }
        }

        protected void SerializeProperty(IDesignerSerializationManager manager, CodeStatementCollection statements, object value, PropertyDescriptor propertyToSerialize)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (propertyToSerialize == null)
            {
                throw new ArgumentNullException("propertyToSerialize");
            }
            if (statements == null)
            {
                throw new ArgumentNullException("statements");
            }
            manager.Context.Push(statements);
            manager.Context.Push(propertyToSerialize);
            try
            {
                MemberCodeDomSerializer serializer = (MemberCodeDomSerializer) manager.GetSerializer(propertyToSerialize.GetType(), typeof(MemberCodeDomSerializer));
                if ((serializer != null) && serializer.ShouldSerialize(manager, value, propertyToSerialize))
                {
                    serializer.Serialize(manager, value, propertyToSerialize, statements);
                }
            }
            finally
            {
                manager.Context.Pop();
                manager.Context.Pop();
            }
        }

        protected void SerializeResource(IDesignerSerializationManager manager, string resourceName, object value)
        {
            ResourceCodeDomSerializer.Default.WriteResource(manager, resourceName, value);
        }

        protected void SerializeResourceInvariant(IDesignerSerializationManager manager, string resourceName, object value)
        {
            ResourceCodeDomSerializer.Default.WriteResourceInvariant(manager, resourceName, value);
        }

        protected CodeExpression SerializeToExpression(IDesignerSerializationManager manager, object value)
        {
            CodeExpression legacyExpression = null;
            using (TraceScope("SerializeToExpression"))
            {
                if (value != null)
                {
                    if (this.IsSerialized(manager, value))
                    {
                        legacyExpression = this.GetExpression(manager, value);
                    }
                    else
                    {
                        legacyExpression = this.GetLegacyExpression(manager, value);
                        if (legacyExpression != null)
                        {
                            this.SetExpression(manager, value, legacyExpression);
                        }
                    }
                }
                if (legacyExpression != null)
                {
                    return legacyExpression;
                }
                CodeDomSerializer serializer = this.GetSerializer(manager, value);
                if (serializer != null)
                {
                    CodeStatementCollection statements = null;
                    if (value != null)
                    {
                        this.SetLegacyExpression(manager, value);
                        StatementContext context = manager.Context[typeof(StatementContext)] as StatementContext;
                        if (context != null)
                        {
                            statements = context.StatementCollection[value];
                        }
                        if (statements != null)
                        {
                            manager.Context.Push(statements);
                        }
                    }
                    object obj2 = null;
                    try
                    {
                        obj2 = serializer.Serialize(manager, value);
                    }
                    finally
                    {
                        if (statements != null)
                        {
                            manager.Context.Pop();
                        }
                    }
                    legacyExpression = obj2 as CodeExpression;
                    if ((legacyExpression == null) && (value != null))
                    {
                        legacyExpression = this.GetExpression(manager, value);
                    }
                    CodeStatementCollection statements2 = obj2 as CodeStatementCollection;
                    if (statements2 == null)
                    {
                        CodeStatement statement = obj2 as CodeStatement;
                        if (statement != null)
                        {
                            statements2 = new CodeStatementCollection();
                            statements2.Add(statement);
                        }
                    }
                    if (statements2 != null)
                    {
                        if (statements == null)
                        {
                            statements = manager.Context[typeof(CodeStatementCollection)] as CodeStatementCollection;
                        }
                        if (statements != null)
                        {
                            statements.AddRange(statements2);
                            return legacyExpression;
                        }
                        string name = "(null)";
                        if (value != null)
                        {
                            name = manager.GetName(value);
                            if (name == null)
                            {
                                name = value.GetType().Name;
                            }
                        }
                        manager.ReportError(System.Design.SR.GetString("SerializerLostStatements", new object[] { name }));
                    }
                    return legacyExpression;
                }
                manager.ReportError(System.Design.SR.GetString("SerializerNoSerializerForComponent", new object[] { value.GetType().FullName }));
            }
            return legacyExpression;
        }

        protected CodeExpression SerializeToResourceExpression(IDesignerSerializationManager manager, object value)
        {
            return this.SerializeToResourceExpression(manager, value, true);
        }

        protected CodeExpression SerializeToResourceExpression(IDesignerSerializationManager manager, object value, bool ensureInvariant)
        {
            CodeExpression expression = null;
            if ((value == null) || value.GetType().IsSerializable)
            {
                CodeStatementCollection statements = null;
                if (value != null)
                {
                    StatementContext context = manager.Context[typeof(StatementContext)] as StatementContext;
                    if (context != null)
                    {
                        statements = context.StatementCollection[value];
                    }
                    if (statements != null)
                    {
                        manager.Context.Push(statements);
                    }
                }
                try
                {
                    expression = ResourceCodeDomSerializer.Default.Serialize(manager, value, false, ensureInvariant) as CodeExpression;
                }
                finally
                {
                    if (statements != null)
                    {
                        manager.Context.Pop();
                    }
                }
            }
            return expression;
        }

        protected void SetExpression(IDesignerSerializationManager manager, object value, CodeExpression expression)
        {
            this.SetExpression(manager, value, expression, false);
        }

        protected void SetExpression(IDesignerSerializationManager manager, object value, CodeExpression expression, bool isPreset)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            ExpressionTable context = (ExpressionTable) manager.Context[typeof(ExpressionTable)];
            if (context == null)
            {
                context = new ExpressionTable();
                manager.Context.Append(context);
            }
            context.SetExpression(value, expression, isPreset);
        }

        private void SetLegacyExpression(IDesignerSerializationManager manager, object value)
        {
            if (value is IComponent)
            {
                LegacyExpressionTable context = (LegacyExpressionTable) manager.Context[typeof(LegacyExpressionTable)];
                if (context == null)
                {
                    context = new LegacyExpressionTable();
                    manager.Context.Append(context);
                }
                context[value] = value;
            }
        }

        [Conditional("DEBUG")]
        internal static void Trace(CodeTypeDeclaration typeDecl)
        {
            if (traceSerialization.TraceInfo)
            {
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                new CSharpCodeProvider().GenerateCodeFromType(typeDecl, writer, new CodeGeneratorOptions());
            }
        }

        [Conditional("DEBUG")]
        internal static void Trace(string message, params object[] values)
        {
            if (traceSerialization.TraceVerbose)
            {
                int count = 0;
                int indentLevel = Debug.IndentLevel;
                if (traceScope != null)
                {
                    count = traceScope.Count;
                }
                try
                {
                    Debug.IndentLevel = count;
                }
                finally
                {
                    Debug.IndentLevel = indentLevel;
                }
            }
        }

        [Conditional("DEBUG")]
        internal static void TraceError(string message, params object[] values)
        {
            if (traceSerialization.TraceError)
            {
                string str = string.Empty;
                if (traceScope != null)
                {
                    foreach (string str2 in traceScope)
                    {
                        if (str.Length > 0)
                        {
                            str = "/" + str;
                        }
                        str = str2 + str;
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        internal static void TraceErrorIf(bool condition, string message, params object[] values)
        {
        }

        [Conditional("DEBUG")]
        internal static void TraceIf(bool condition, string message, params object[] values)
        {
        }

        internal static IDisposable TraceScope(string name)
        {
            return new TracingScope();
        }

        [Conditional("DEBUG")]
        internal static void TraceWarning(string message, params object[] values)
        {
            if (traceSerialization.TraceWarning)
            {
                string str = string.Empty;
                if (traceScope != null)
                {
                    foreach (string str2 in traceScope)
                    {
                        if (str.Length > 0)
                        {
                            str = "/" + str;
                        }
                        str = str2 + str;
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        internal static void TraceWarningIf(bool condition, string message, params object[] values)
        {
        }

        private class LegacyExpressionTable : Hashtable
        {
        }

        internal class OrderedCodeStatementCollection : CodeStatementCollection
        {
            public string Name;
            public int Order;
        }

        [StructLayout(LayoutKind.Sequential, Size=1)]
        private struct TracingScope : IDisposable
        {
            public void Dispose()
            {
                if (CodeDomSerializerBase.traceScope != null)
                {
                    CodeDomSerializerBase.traceScope.Pop();
                }
            }
        }
    }
}

