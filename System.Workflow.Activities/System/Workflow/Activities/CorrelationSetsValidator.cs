namespace System.Workflow.Activities
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime;

    internal static class CorrelationSetsValidator
    {
        private static Type FetchParameterType(MemberInfo memberInfo, string paramPath)
        {
            MethodInfo method = null;
            if (memberInfo is EventInfo)
            {
                method = Helpers.GetDelegateFromEvent((EventInfo) memberInfo).GetMethod("Invoke");
            }
            else
            {
                method = (MethodInfo) memberInfo;
            }
            return GetCorrelationParameterType(paramPath, method.GetParameters());
        }

        private static void FillCorrelationAliasAttrs(MemberInfo memberInfo, Hashtable correlationAliasAttrs, ValidationErrorCollection validationErrors)
        {
            foreach (object obj2 in memberInfo.GetCustomAttributes(typeof(CorrelationAliasAttribute), false))
            {
                CorrelationAliasAttribute attributeFromObject = Helpers.GetAttributeFromObject<CorrelationAliasAttribute>(obj2);
                if (string.IsNullOrEmpty(attributeFromObject.Name) || (attributeFromObject.Name.Trim().Length == 0))
                {
                    ValidationError item = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_CorrelationAttributeInvalid", new object[] { typeof(CorrelationAliasAttribute).Name, "Name", memberInfo.Name }), 0x150);
                    item.UserData.Add(typeof(CorrelationAliasAttribute), memberInfo.Name);
                    validationErrors.Add(item);
                }
                else if (string.IsNullOrEmpty(attributeFromObject.Path) || (attributeFromObject.Path.Trim().Length == 0))
                {
                    ValidationError error2 = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_CorrelationAttributeInvalid", new object[] { typeof(CorrelationAliasAttribute).Name, "Path", memberInfo.Name }), 0x150);
                    error2.UserData.Add(typeof(CorrelationAliasAttribute), memberInfo.Name);
                    validationErrors.Add(error2);
                }
                else if (correlationAliasAttrs.Contains(attributeFromObject.Name))
                {
                    ValidationError error3 = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_DuplicateCorrelationAttribute", new object[] { typeof(CorrelationAliasAttribute).Name, attributeFromObject.Name, memberInfo.Name }), 0x151);
                    error3.UserData.Add(typeof(CorrelationAliasAttribute), memberInfo.Name);
                    validationErrors.Add(error3);
                }
                else
                {
                    correlationAliasAttrs.Add(attributeFromObject.Name, attributeFromObject);
                }
            }
        }

        private static Type GetCorrelationParameterType(string parameterPropertyName, object parametersCollection)
        {
            string[] strArray = parameterPropertyName.Split(new char[] { '.' });
            Type type = null;
            int index = 0;
            if (strArray.Length == 1)
            {
                Type parameterType = null;
                if (parametersCollection is CodeParameterDeclarationExpressionCollection)
                {
                    foreach (CodeParameterDeclarationExpression expression in (CodeParameterDeclarationExpressionCollection) parametersCollection)
                    {
                        if (string.Compare("e", expression.Name, StringComparison.Ordinal) == 0)
                        {
                            parameterType = expression.UserData[typeof(Type)] as Type;
                        }
                    }
                }
                else if (parametersCollection is ParameterInfo[])
                {
                    foreach (ParameterInfo info in (ParameterInfo[]) parametersCollection)
                    {
                        if (string.Compare("e", info.Name, StringComparison.Ordinal) == 0)
                        {
                            parameterType = info.ParameterType;
                        }
                    }
                }
                if (parameterType != null)
                {
                    string str = strArray[0];
                    strArray = new string[] { "e", str };
                }
            }
            if (parametersCollection is CodeParameterDeclarationExpressionCollection)
            {
                foreach (CodeParameterDeclarationExpression expression2 in (CodeParameterDeclarationExpressionCollection) parametersCollection)
                {
                    if (string.Compare(strArray[0], expression2.Name, StringComparison.Ordinal) == 0)
                    {
                        type = expression2.UserData[typeof(Type)] as Type;
                    }
                }
            }
            else
            {
                if (!(parametersCollection is ParameterInfo[]))
                {
                    return null;
                }
                foreach (ParameterInfo info2 in (ParameterInfo[]) parametersCollection)
                {
                    if (string.Compare(strArray[0], info2.Name, StringComparison.Ordinal) == 0)
                    {
                        type = info2.ParameterType;
                    }
                }
            }
            if (strArray.Length == 1)
            {
                return type;
            }
            index = 1;
            while ((index < strArray.Length) && (type != null))
            {
                Type propertyType = null;
                foreach (PropertyInfo info3 in type.GetProperties())
                {
                    propertyType = null;
                    if (string.Compare(info3.Name, strArray[index], StringComparison.Ordinal) == 0)
                    {
                        propertyType = info3.PropertyType;
                        break;
                    }
                }
                if (propertyType != null)
                {
                    type = propertyType;
                }
                else
                {
                    foreach (FieldInfo info4 in type.GetFields())
                    {
                        propertyType = null;
                        if (string.Compare(info4.Name, strArray[index], StringComparison.Ordinal) == 0)
                        {
                            propertyType = info4.FieldType;
                            break;
                        }
                    }
                    if (propertyType != null)
                    {
                        type = propertyType;
                    }
                    else if (propertyType == null)
                    {
                        return null;
                    }
                }
                index++;
            }
            if (index == strArray.Length)
            {
                return type;
            }
            return null;
        }

        private static Activity GetTransactionalScopeParent(Activity activity)
        {
            Activity parent = activity;
            while (parent != null)
            {
                if ((parent is CompensatableTransactionScopeActivity) || (parent is TransactionScopeActivity))
                {
                    return parent;
                }
                parent = parent.Parent;
            }
            return parent;
        }

        private static bool IsFollowerInTxnlScope(Activity parent, Activity activity)
        {
            for (Activity activity2 = activity; activity2 != null; activity2 = activity2.Parent)
            {
                if (activity2 == parent)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsOwnerActivitySame(string ownerActivityName, string existingOwnerActivityName, Activity currentActivity, Activity existingActivity)
        {
            if (ownerActivityName.Equals(existingOwnerActivityName))
            {
                return true;
            }
            Activity activityByName = currentActivity.GetActivityByName(ownerActivityName);
            if (activityByName == null)
            {
                activityByName = Helpers.ParseActivityForBind(currentActivity, ownerActivityName);
            }
            Activity activity2 = currentActivity.GetActivityByName(existingOwnerActivityName);
            if (activity2 == null)
            {
                activity2 = Helpers.ParseActivityForBind(existingActivity, existingOwnerActivityName);
            }
            return (((activityByName != null) && (activity2 != null)) && activityByName.QualifiedName.Equals(activity2.QualifiedName));
        }

        internal static ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            object[] attributes;
            CorrelationToken correlator;
            string qualifiedCorrelationToken;
            Activity sourceActivity;
            ValidationErrorCollection errors = new ValidationErrorCollection();
            Activity activity = obj as Activity;
            if (!(activity is CallExternalMethodActivity) && !(activity is HandleExternalEventActivity))
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "obj");
            }
            Type interfaceType = (activity is CallExternalMethodActivity) ? ((CallExternalMethodActivity) activity).InterfaceType : ((HandleExternalEventActivity) activity).InterfaceType;
            if (interfaceType != null)
            {
                if (interfaceType.ContainsGenericParameters)
                {
                    ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_GenericMethodsNotSupported"), new object[] { interfaceType.FullName }), 0x155) {
                        PropertyName = "InterfaceType"
                    };
                    errors.Add(item);
                    return errors;
                }
                attributes = interfaceType.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), false);
                if (attributes.Length == 0)
                {
                    ValidationError error2 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_ExternalDataExchangeException"), new object[] { interfaceType.FullName }), 0x113) {
                        PropertyName = "InterfaceType"
                    };
                    errors.Add(error2);
                    return errors;
                }
                if (activity.Site == null)
                {
                    ValidationErrorCollection errors2 = ValidateHostInterface(manager, interfaceType, activity);
                    if (errors2.Count != 0)
                    {
                        errors.AddRange(errors2);
                        return errors;
                    }
                }
                MemberInfo info = null;
                if (activity is CallExternalMethodActivity)
                {
                    if ((((CallExternalMethodActivity) activity).MethodName == null) || (((CallExternalMethodActivity) activity).MethodName.Length == 0))
                    {
                        return errors;
                    }
                    MethodInfo info2 = interfaceType.GetMethod(((CallExternalMethodActivity) activity).MethodName, BindingFlags.Public | BindingFlags.Instance);
                    if ((info2 == null) || info2.IsSpecialName)
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_MissingMethodName", new object[] { activity.Name, ((CallExternalMethodActivity) activity).MethodName }), 0x528));
                        return errors;
                    }
                    if (info2.ContainsGenericParameters)
                    {
                        ValidationError error3 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_GenericMethodsNotSupported"), new object[] { info2.Name }), 0x155) {
                            PropertyName = "MethodName"
                        };
                        errors.Add(error3);
                        return errors;
                    }
                    info = info2;
                }
                else
                {
                    if ((((HandleExternalEventActivity) activity).EventName == null) || (((HandleExternalEventActivity) activity).EventName.Length == 0))
                    {
                        return errors;
                    }
                    EventInfo info3 = interfaceType.GetEvent(((HandleExternalEventActivity) activity).EventName, BindingFlags.Public | BindingFlags.Instance);
                    if (info3 == null)
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_MissingEventName", new object[] { activity.Name, ((HandleExternalEventActivity) activity).EventName }), 0x528));
                        return errors;
                    }
                    info = info3;
                }
                attributes = interfaceType.GetCustomAttributes(typeof(CorrelationProviderAttribute), false);
                if (attributes.Length == 0)
                {
                    correlator = activity.GetValue((activity is CallExternalMethodActivity) ? CallExternalMethodActivity.CorrelationTokenProperty : HandleExternalEventActivity.CorrelationTokenProperty) as CorrelationToken;
                    if (interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), false).Length == 0)
                    {
                        if (correlator != null)
                        {
                            errors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_CorrelationTokenSpecifiedForUncorrelatedInterface"), new object[] { activity.QualifiedName, interfaceType }), 0x119, false, "CorrelationToken"));
                        }
                        return errors;
                    }
                    if (activity.Parent == null)
                    {
                        return errors;
                    }
                    if ((correlator == null) || string.IsNullOrEmpty(correlator.Name))
                    {
                        errors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_MissingCorrelationTokenProperty"), new object[] { activity.QualifiedName }), 0x109, false, "CorrelationToken"));
                        return errors;
                    }
                    if (string.IsNullOrEmpty(correlator.OwnerActivityName))
                    {
                        errors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_MissingCorrelationTokenOwnerNameProperty"), new object[] { activity.QualifiedName }), 0x109, false, "CorrelationToken"));
                        return errors;
                    }
                    qualifiedCorrelationToken = null;
                    sourceActivity = activity.GetActivityByName(correlator.OwnerActivityName);
                    if (sourceActivity == null)
                    {
                        sourceActivity = Helpers.ParseActivityForBind(activity, correlator.OwnerActivityName);
                    }
                    if (sourceActivity != null)
                    {
                        qualifiedCorrelationToken = sourceActivity.QualifiedName;
                    }
                    Activity seedActivity = null;
                    CompositeActivity parent = activity.Parent;
                    Activity activity3 = parent;
                    bool flag = false;
                    while (parent != null)
                    {
                        if ((parent is ReplicatorActivity) && (seedActivity == null))
                        {
                            seedActivity = parent;
                        }
                        if (qualifiedCorrelationToken == parent.QualifiedName)
                        {
                            flag = true;
                        }
                        activity3 = parent;
                        parent = parent.Parent;
                    }
                    if (!flag)
                    {
                        ValidationError error4 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_OwnerActivityIsNotParent"), new object[] { activity.QualifiedName }), 0x109) {
                            PropertyName = "CorrelationToken"
                        };
                        errors.Add(error4);
                    }
                    bool flag2 = false;
                    attributes = info.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false);
                    if (attributes.Length > 0)
                    {
                        flag2 = true;
                    }
                    if ((flag2 && (seedActivity != null)) && (activity is HandleExternalEventActivity))
                    {
                        ValidationError error5 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_InitializerInReplicator"), new object[] { seedActivity.QualifiedName }), 0x62d, false) {
                            PropertyName = "CorrelationToken"
                        };
                        errors.Add(error5);
                    }
                    if (string.IsNullOrEmpty(qualifiedCorrelationToken))
                    {
                        return errors;
                    }
                    if (seedActivity != null)
                    {
                        bool isValid = false;
                        System.Workflow.Activities.Common.Walker walker = new System.Workflow.Activities.Common.Walker();
                        walker.FoundActivity += delegate (System.Workflow.Activities.Common.Walker w, System.Workflow.Activities.Common.WalkerEventArgs args) {
                            if (args.CurrentActivity.Enabled && (args.CurrentActivity.QualifiedName == qualifiedCorrelationToken))
                            {
                                isValid = true;
                                args.Action = System.Workflow.Activities.Common.WalkerAction.Abort;
                            }
                        };
                        walker.Walk(seedActivity);
                        if (!isValid)
                        {
                            ValidationError error6 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_CorrelationTokenInReplicator", new object[] { correlator.Name, seedActivity.QualifiedName }), new object[0]), 0x618, true) {
                                PropertyName = "CorrelationToken"
                            };
                            errors.Add(error6);
                        }
                    }
                    if (flag2)
                    {
                        return errors;
                    }
                    bool isValid = false;
                    bool ownerNameValidated = false;
                    bool initFollowerInTxnlScope = false;
                    System.Workflow.Activities.Common.Walker walker2 = new System.Workflow.Activities.Common.Walker();
                    walker2.FoundActivity += delegate (System.Workflow.Activities.Common.Walker w, System.Workflow.Activities.Common.WalkerEventArgs args) {
                        Activity currentActivity = args.CurrentActivity;
                        if (currentActivity.Enabled && ((currentActivity is CallExternalMethodActivity) || (currentActivity is HandleExternalEventActivity)))
                        {
                            CorrelationToken token = currentActivity.GetValue((currentActivity is CallExternalMethodActivity) ? CallExternalMethodActivity.CorrelationTokenProperty : HandleExternalEventActivity.CorrelationTokenProperty) as CorrelationToken;
                            if (((token != null) && (!(currentActivity is CallExternalMethodActivity) || interfaceType.Equals(((CallExternalMethodActivity) currentActivity).InterfaceType))) && (!(currentActivity is HandleExternalEventActivity) || interfaceType.Equals(((HandleExternalEventActivity) currentActivity).InterfaceType)))
                            {
                                MemberInfo info = null;
                                if (currentActivity is CallExternalMethodActivity)
                                {
                                    if ((((CallExternalMethodActivity) currentActivity).MethodName == null) || (((CallExternalMethodActivity) currentActivity).MethodName.Length == 0))
                                    {
                                        return;
                                    }
                                    MethodInfo method = interfaceType.GetMethod(((CallExternalMethodActivity) currentActivity).MethodName, BindingFlags.Public | BindingFlags.Instance);
                                    if ((method == null) || method.IsSpecialName)
                                    {
                                        return;
                                    }
                                    info = method;
                                }
                                else
                                {
                                    if ((((HandleExternalEventActivity) currentActivity).EventName == null) || (((HandleExternalEventActivity) currentActivity).EventName.Length == 0))
                                    {
                                        return;
                                    }
                                    EventInfo info3 = interfaceType.GetEvent(((HandleExternalEventActivity) currentActivity).EventName, BindingFlags.Public | BindingFlags.Instance);
                                    if (info3 == null)
                                    {
                                        return;
                                    }
                                    info = info3;
                                }
                                attributes = info.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false);
                                if (attributes.Length != 0)
                                {
                                    if (activity is HandleExternalEventActivity)
                                    {
                                        Activity transactionalScopeParent = GetTransactionalScopeParent(currentActivity);
                                        if ((transactionalScopeParent != null) && IsFollowerInTxnlScope(transactionalScopeParent, activity))
                                        {
                                            initFollowerInTxnlScope = true;
                                        }
                                    }
                                    sourceActivity = activity.GetActivityByName(token.OwnerActivityName);
                                    if (sourceActivity == null)
                                    {
                                        sourceActivity = Helpers.ParseActivityForBind(activity, token.OwnerActivityName);
                                    }
                                    if (sourceActivity != null)
                                    {
                                        string qualifiedName = sourceActivity.QualifiedName;
                                    }
                                    if ((correlator.Name == token.Name) && IsOwnerActivitySame(correlator.OwnerActivityName, token.OwnerActivityName, activity, currentActivity))
                                    {
                                        isValid = true;
                                        ownerNameValidated = true;
                                        args.Action = System.Workflow.Activities.Common.WalkerAction.Abort;
                                    }
                                }
                            }
                        }
                    };
                    walker2.Walk(activity3);
                    if (!isValid)
                    {
                        ValidationError error7 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_UninitializedCorrelation"), new object[0]), 0x538, true) {
                            PropertyName = "CorrelationToken"
                        };
                        errors.Add(error7);
                        if (ownerNameValidated)
                        {
                            error7 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_MisMatchCorrelationTokenOwnerNameProperty"), new object[] { correlator.Name }), 0x538, false) {
                                PropertyName = "CorrelationToken"
                            };
                            errors.Add(error7);
                        }
                    }
                    if (initFollowerInTxnlScope)
                    {
                        ValidationError error8 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_InitializerFollowerInTxnlScope"), new object[0]), 0x62e, false) {
                            PropertyName = "CorrelationToken"
                        };
                        errors.Add(error8);
                    }
                }
            }
            return errors;
        }

        private static ValidationErrorCollection ValidateHostInterface(IServiceProvider serviceProvider, Type interfaceType, Activity activity)
        {
            Dictionary<Type, ValidationErrorCollection> service = serviceProvider.GetService(typeof(Dictionary<Type, ValidationErrorCollection>)) as Dictionary<Type, ValidationErrorCollection>;
            if (service == null)
            {
                service = new Dictionary<Type, ValidationErrorCollection>();
                IServiceContainer container = serviceProvider.GetService(typeof(IServiceContainer)) as IServiceContainer;
                if (container != null)
                {
                    container.AddService(typeof(Dictionary<Type, ValidationErrorCollection>), service);
                }
            }
            if (service.ContainsKey(interfaceType))
            {
                return new ValidationErrorCollection();
            }
            service.Add(interfaceType, new ValidationErrorCollection());
            if (interfaceType.GetCustomAttributes(typeof(CorrelationProviderAttribute), false).Length == 0)
            {
                object[] customAttributes = interfaceType.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), false);
                object[] objArray3 = interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), false);
                if ((customAttributes.Length != 0) && (objArray3.Length != 0))
                {
                    service[interfaceType].AddRange(ValidateHostInterfaceMembers(interfaceType, activity));
                    service[interfaceType].AddRange(ValidateHostInterfaceAttributes(interfaceType));
                }
                else
                {
                    service[interfaceType].AddRange(ValidateInvalidHostInterfaceAttributes(interfaceType));
                }
            }
            return service[interfaceType];
        }

        private static ValidationErrorCollection ValidateHostInterfaceAttributes(Type interfaceType)
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            ArrayList list = new ArrayList();
            foreach (object obj2 in interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), false))
            {
                CorrelationParameterAttribute attributeFromObject = Helpers.GetAttributeFromObject<CorrelationParameterAttribute>(obj2);
                if (string.IsNullOrEmpty(attributeFromObject.Name) || (attributeFromObject.Name.Trim().Length == 0))
                {
                    ValidationError item = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_CorrelationAttributeInvalid", new object[] { typeof(CorrelationParameterAttribute).Name, "Name", interfaceType.Name }), 0x150);
                    item.UserData.Add(typeof(CorrelationParameterAttribute), interfaceType.Name);
                    validationErrors.Add(item);
                }
                else if (list.Contains(attributeFromObject.Name))
                {
                    ValidationError error2 = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_DuplicateCorrelationAttribute", new object[] { typeof(CorrelationParameterAttribute).Name, attributeFromObject.Name, interfaceType.Name }), 0x151);
                    error2.UserData.Add(typeof(CorrelationParameterAttribute), interfaceType.Name);
                    validationErrors.Add(error2);
                }
                else
                {
                    list.Add(attributeFromObject.Name);
                }
            }
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            Hashtable hashtable3 = new Hashtable();
            int num = 0;
            foreach (MemberInfo info in interfaceType.GetMembers())
            {
                if ((info is MethodInfo) && !((MethodInfo) info).IsSpecialName)
                {
                    Hashtable hashtable4 = new Hashtable();
                    hashtable2.Add(info, hashtable4);
                    FillCorrelationAliasAttrs(info, hashtable4, validationErrors);
                    int length = info.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false).Length;
                    num += length;
                    if (length > 0)
                    {
                        foreach (string str in list)
                        {
                            string paramPath = str;
                            if (hashtable4.Contains(str))
                            {
                                paramPath = ((CorrelationAliasAttribute) hashtable4[str]).Path;
                            }
                            Type type = FetchParameterType(info, paramPath);
                            if ((type != null) && !hashtable.ContainsKey(str))
                            {
                                hashtable[str] = type;
                            }
                        }
                    }
                }
                else if (info is EventInfo)
                {
                    int num3 = info.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false).Length;
                    num += num3;
                    Hashtable hashtable5 = new Hashtable();
                    hashtable2.Add(info, hashtable5);
                    FillCorrelationAliasAttrs(info, hashtable5, validationErrors);
                    Type delegateFromEvent = Helpers.GetDelegateFromEvent((EventInfo) info);
                    delegateFromEvent.GetMethod("Invoke");
                    FillCorrelationAliasAttrs(delegateFromEvent, hashtable5, validationErrors);
                    Hashtable correlationAliasAttrs = new Hashtable();
                    FillCorrelationAliasAttrs(delegateFromEvent, correlationAliasAttrs, validationErrors);
                    if (hashtable3[delegateFromEvent] == null)
                    {
                        hashtable3.Add(delegateFromEvent, correlationAliasAttrs);
                    }
                    if (num3 > 0)
                    {
                        foreach (string str3 in list)
                        {
                            string path = str3;
                            if (hashtable5.Contains(str3))
                            {
                                path = ((CorrelationAliasAttribute) hashtable5[str3]).Path;
                            }
                            Type type3 = FetchParameterType(info, path);
                            if ((type3 != null) && !hashtable.ContainsKey(str3))
                            {
                                hashtable[str3] = type3;
                            }
                        }
                    }
                }
            }
            foreach (DictionaryEntry entry in hashtable2)
            {
                MemberInfo key = entry.Key as MemberInfo;
                Hashtable hashtable7 = (Hashtable) entry.Value;
                foreach (string str5 in hashtable7.Keys)
                {
                    if (!list.Contains(str5) && (!(key is EventInfo) || (((Hashtable) hashtable3[Helpers.GetDelegateFromEvent((EventInfo) key)])[str5] == null)))
                    {
                        ValidationError error3 = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_CorrelationParameterNotFound", new object[] { typeof(CorrelationAliasAttribute).Name, str5, key.Name, typeof(CorrelationParameterAttribute).Name, interfaceType.Name }), 0x153);
                        error3.UserData.Add(typeof(CorrelationAliasAttribute), key.Name);
                        validationErrors.Add(error3);
                    }
                }
            }
            foreach (string str6 in list)
            {
                foreach (DictionaryEntry entry2 in hashtable2)
                {
                    string str7 = str6;
                    MemberInfo info3 = (MemberInfo) entry2.Key;
                    Hashtable hashtable8 = (Hashtable) entry2.Value;
                    if (hashtable8.Contains(str6))
                    {
                        str7 = ((CorrelationAliasAttribute) hashtable8[str6]).Path;
                    }
                    Type type4 = FetchParameterType((MemberInfo) entry2.Key, str7);
                    if (type4 == null)
                    {
                        if (!(info3 is EventInfo) || (((Hashtable) hashtable3[Helpers.GetDelegateFromEvent((EventInfo) info3)])[str6] == null))
                        {
                            ValidationError error4 = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_CorrelationInvalid", new object[] { (info3.DeclaringType == interfaceType) ? info3.Name : info3.DeclaringType.Name, str6 }), 0x158);
                            error4.UserData.Add(typeof(CorrelationParameterAttribute), (info3.DeclaringType == interfaceType) ? info3.Name : info3.DeclaringType.Name);
                            validationErrors.Add(error4);
                        }
                    }
                    else if (hashtable.ContainsKey(str6) && (((Type) hashtable[str6]) != type4))
                    {
                        ValidationError error5 = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_CorrelationTypeNotConsistent", new object[] { str7, typeof(CorrelationAliasAttribute).Name, (info3.DeclaringType == interfaceType) ? info3.Name : info3.DeclaringType.Name, type4.Name, ((Type) hashtable[str6]).Name, str6, interfaceType.Name }), 340);
                        error5.UserData.Add(typeof(CorrelationAliasAttribute), (info3.DeclaringType == interfaceType) ? info3.Name : info3.DeclaringType.Name);
                        validationErrors.Add(error5);
                    }
                }
            }
            if (num == 0)
            {
                ValidationError error6 = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_CorrelationInitializerNotDefinied", new object[] { interfaceType.Name }), 0x159);
                error6.UserData.Add(typeof(CorrelationInitializerAttribute), interfaceType.Name);
                validationErrors.Add(error6);
            }
            return validationErrors;
        }

        private static ValidationErrorCollection ValidateHostInterfaceMembers(Type interfaceType, Activity activity)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            foreach (MemberInfo info in interfaceType.GetMembers())
            {
                if (((info is MethodInfo) || (info is EventInfo)) && (!(info is MethodInfo) || !((MethodInfo) info).IsSpecialName))
                {
                    MethodInfo method = null;
                    Type eventHandlerType = null;
                    if (info is EventInfo)
                    {
                        EventInfo eventInfo = (EventInfo) info;
                        eventHandlerType = eventInfo.EventHandlerType;
                        if (eventHandlerType == null)
                        {
                            eventHandlerType = TypeProvider.GetEventHandlerType(eventInfo);
                        }
                        if (eventHandlerType == null)
                        {
                            throw new InvalidOperationException();
                        }
                        method = eventHandlerType.GetMethod("Invoke");
                    }
                    else
                    {
                        method = (MethodInfo) info;
                    }
                    if (method.IsGenericMethod)
                    {
                        ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_GenericMethodsNotSupported"), new object[] { (info is EventInfo) ? eventHandlerType.Name : method.Name }), 0x155);
                        if (info is EventInfo)
                        {
                            item.UserData.Add(typeof(EventInfo), ((EventInfo) info).Name);
                        }
                        else
                        {
                            item.UserData.Add(typeof(MethodInfo), method.Name);
                        }
                        errors.Add(item);
                    }
                    if ((method.ReturnType != typeof(void)) && (info is EventInfo))
                    {
                        ValidationError error2 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_ReturnTypeNotVoid"), new object[] { (info is EventInfo) ? eventHandlerType.Name : method.Name }), 0x156);
                        if (info is EventInfo)
                        {
                            error2.UserData.Add(typeof(EventInfo), ((EventInfo) info).Name);
                        }
                        else
                        {
                            error2.UserData.Add(typeof(MethodInfo), method.Name);
                        }
                        errors.Add(error2);
                    }
                    foreach (ParameterInfo info4 in method.GetParameters())
                    {
                        if (info4.IsOut || info4.IsRetval)
                        {
                            ValidationError error3 = new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_OutRefParameterNotSupported"), new object[] { (info is EventInfo) ? eventHandlerType.Name : method.Name, info4.Name }), 0x157);
                            if (info is EventInfo)
                            {
                                error3.UserData.Add(typeof(EventInfo), ((EventInfo) info).Name);
                            }
                            else
                            {
                                error3.UserData.Add(typeof(MethodInfo), method.Name);
                            }
                            error3.UserData.Add(typeof(ParameterInfo), info4.Name);
                            errors.Add(error3);
                        }
                    }
                }
            }
            return errors;
        }

        private static ValidationErrorCollection ValidateInvalidHostInterfaceAttributes(Type interfaceType)
        {
            if (interfaceType == null)
            {
                throw new ArgumentNullException("interfaceType");
            }
            ValidationErrorCollection errors = new ValidationErrorCollection();
            bool flag = false;
            foreach (MemberInfo info in interfaceType.GetMembers())
            {
                if ((info.GetCustomAttributes(typeof(CorrelationInitializerAttribute), false).Length != 0) || (info.GetCustomAttributes(typeof(CorrelationAliasAttribute), false).Length != 0))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                ValidationError item = new ValidationError(SR.GetString(CultureInfo.CurrentCulture, "Error_MissingCorrelationParameterAttribute", new object[] { interfaceType.Name }), 0x152);
                item.UserData.Add(typeof(CorrelationParameterAttribute), interfaceType.Name);
                errors.Add(item);
            }
            return errors;
        }
    }
}

