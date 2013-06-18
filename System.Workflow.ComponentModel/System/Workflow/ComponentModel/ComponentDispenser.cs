namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Resources;
    using System.Workflow.ComponentModel.Compiler;

    internal static class ComponentDispenser
    {
        private static IDictionary<Type, List<IExtenderProvider>> componentExtenderMap = new Dictionary<Type, List<IExtenderProvider>>();

        private static void AddComponents(Dictionary<Type, object> components, object[] attribComponents)
        {
            foreach (object obj2 in attribComponents)
            {
                if (!components.ContainsKey(obj2.GetType()))
                {
                    components.Add(obj2.GetType(), obj2);
                }
            }
        }

        internal static ActivityExecutor[] CreateActivityExecutors(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            List<ActivityExecutor> list = new List<ActivityExecutor>();
            if (activity.SupportsSynchronization)
            {
                list.Add(new SynchronizationFilter());
            }
            if (activity.SupportsTransaction)
            {
                list.Add(new TransactedContextFilter());
            }
            if (activity is CompositeActivity)
            {
                if (activity is ICompensatableActivity)
                {
                    list.Add(new CompensationHandlingFilter());
                }
                list.Add(new FaultAndCancellationHandlingFilter());
                list.Add(new CompositeActivityExecutor<CompositeActivity>());
            }
            else
            {
                list.Add(new ActivityExecutor<Activity>());
            }
            return list.ToArray();
        }

        private static object CreateComponentInstance(string typeName, Type referenceType)
        {
            object obj2 = null;
            Type type = null;
            try
            {
                string name = typeName;
                int num = typeName.LastIndexOf(']');
                if (num != -1)
                {
                    name = typeName.Substring(0, num + 1);
                }
                else
                {
                    int index = typeName.IndexOf(',');
                    if (index != -1)
                    {
                        name = typeName.Substring(0, index);
                    }
                }
                type = referenceType.Assembly.GetType(name, false);
            }
            catch
            {
            }
            if (type == null)
            {
                try
                {
                    type = Type.GetType(typeName, false);
                }
                catch
                {
                }
            }
            string message = null;
            if (type != null)
            {
                try
                {
                    obj2 = Activator.CreateInstance(type);
                }
                catch (Exception exception)
                {
                    message = exception.Message;
                }
            }
            if (obj2 != null)
            {
                return obj2;
            }
            ResourceManager manager = new ResourceManager("System.Workflow.ComponentModel.StringResources", typeof(Activity).Assembly);
            if (manager != null)
            {
                message = string.Format(CultureInfo.CurrentCulture, manager.GetString("Error_CantCreateInstanceOfComponent"), new object[] { typeName, message });
            }
            throw new Exception(message);
        }

        internal static object[] CreateComponents(Type objectType, Type componentTypeAttribute)
        {
            Dictionary<Type, object> components = new Dictionary<Type, object>();
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            ArrayList list4 = new ArrayList();
            GetCustomAttributes(objectType, typeof(ActivityCodeGeneratorAttribute), true);
            GetCustomAttributes(objectType, typeof(ActivityValidatorAttribute), true);
            GetCustomAttributes(objectType, typeof(DesignerAttribute), true);
            GetCustomAttributes(objectType, typeof(DesignerSerializerAttribute), true);
            if ((objectType.GetCustomAttributes(typeof(SupportsTransactionAttribute), true).Length > 0) && (componentTypeAttribute == typeof(ActivityValidatorAttribute)))
            {
                list.Add(new TransactionContextValidator());
            }
            if ((objectType.GetCustomAttributes(typeof(SupportsSynchronizationAttribute), true).Length > 0) && (componentTypeAttribute == typeof(ActivityValidatorAttribute)))
            {
                list3.Add(new SynchronizationValidator());
            }
            AddComponents(components, list3.ToArray());
            AddComponents(components, list.ToArray());
            AddComponents(components, list4.ToArray());
            AddComponents(components, list2.ToArray());
            ArrayList list5 = new ArrayList();
            foreach (Type type in objectType.GetInterfaces())
            {
                list5.AddRange(GetCustomAttributes(type, componentTypeAttribute, true));
            }
            list5.AddRange(GetCustomAttributes(objectType, componentTypeAttribute, true));
            string codeGeneratorTypeName = null;
            foreach (Attribute attribute in list5)
            {
                Type type2 = null;
                if (componentTypeAttribute == typeof(ActivityCodeGeneratorAttribute))
                {
                    codeGeneratorTypeName = ((ActivityCodeGeneratorAttribute) attribute).CodeGeneratorTypeName;
                    type2 = typeof(ActivityCodeGenerator);
                }
                else if (componentTypeAttribute == typeof(ActivityValidatorAttribute))
                {
                    codeGeneratorTypeName = ((ActivityValidatorAttribute) attribute).ValidatorTypeName;
                    type2 = typeof(Validator);
                }
                object obj2 = null;
                try
                {
                    if (!string.IsNullOrEmpty(codeGeneratorTypeName))
                    {
                        obj2 = CreateComponentInstance(codeGeneratorTypeName, objectType);
                    }
                }
                catch
                {
                }
                if (((obj2 == null) || (type2 == null)) || !type2.IsAssignableFrom(obj2.GetType()))
                {
                    throw new InvalidOperationException(SR.GetString("Error_InvalidAttribute", new object[] { componentTypeAttribute.Name, objectType.FullName }));
                }
                if (!components.ContainsKey(obj2.GetType()))
                {
                    components.Add(obj2.GetType(), obj2);
                }
            }
            return new ArrayList(components.Values).ToArray();
        }

        private static object[] GetCustomAttributes(Type objectType, Type attributeType, bool inherit)
        {
            object[] customAttributes = null;
            try
            {
                if (attributeType == null)
                {
                    return objectType.GetCustomAttributes(inherit);
                }
                customAttributes = objectType.GetCustomAttributes(attributeType, inherit);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidAttributes", new object[] { objectType.FullName }), exception);
            }
            return customAttributes;
        }

        internal static void RegisterComponentExtenders(Type extendingType, IExtenderProvider[] extenders)
        {
            List<IExtenderProvider> list = null;
            if (!componentExtenderMap.ContainsKey(extendingType))
            {
                list = new List<IExtenderProvider>();
                componentExtenderMap.Add(extendingType, list);
            }
            else
            {
                list = componentExtenderMap[extendingType];
            }
            list.AddRange(extenders);
        }

        internal static IList<IExtenderProvider> Extenders
        {
            get
            {
                List<IExtenderProvider> list = new List<IExtenderProvider>();
                foreach (IList<IExtenderProvider> list2 in componentExtenderMap.Values)
                {
                    list.AddRange(list2);
                }
                return list.AsReadOnly();
            }
        }
    }
}

