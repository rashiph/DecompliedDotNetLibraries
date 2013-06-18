namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class ActivityUtilities
    {
        private static Type activityDelegateType = typeof(ActivityDelegate);
        private static Type activityGenericType = typeof(Activity<>);
        private static Type activityType = typeof(Activity);
        private static Type argumentReferenceGenericType = typeof(ArgumentReference<>);
        private static Type argumentType = typeof(Argument);
        private static Type argumentValueGenericType = typeof(ArgumentValue<>);
        private static IList<Type> collectionInterfaces;
        private static Type constraintType = typeof(Constraint);
        private static Type delegateArgumentValueGenericType = typeof(DelegateArgumentValue<>);
        private static Type delegateInArgumentGenericType = typeof(DelegateInArgument<>);
        private static Type delegateInArgumentType = typeof(DelegateInArgument);
        private static Type delegateOutArgumentGenericType = typeof(DelegateOutArgument<>);
        private static Type delegateOutArgumentType = typeof(DelegateOutArgument);
        private static ReadOnlyDictionary<string, object> emptyParameters = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(0));
        private static Type handleType = typeof(Handle);
        private static Type iDictionaryGenericType = typeof(IDictionary<,>);
        private static Type inArgumentGenericType = typeof(InArgument<>);
        private static Type inArgumentOfObjectType = typeof(InArgument<object>);
        private static Type inArgumentType = typeof(InArgument);
        private static Type inOutArgumentGenericType = typeof(InOutArgument<>);
        private static Type inOutArgumentOfObjectType = typeof(InOutArgument<object>);
        private static Type inOutArgumentType = typeof(InOutArgument);
        private static Type locationGenericType = typeof(Location<>);
        private static Type locationReferenceValueType = typeof(LocationReferenceValue<>);
        private static Type outArgumentGenericType = typeof(OutArgument<>);
        private static Type outArgumentOfObjectType = typeof(OutArgument<object>);
        private static Type outArgumentType = typeof(OutArgument);
        private static Pop popActivity = new Pop();
        private static PropertyChangedEventArgs propertyChangedEventArgs;
        private static Type runtimeArgumentType = typeof(RuntimeArgument);
        private static Type variableGenericType = typeof(Variable<>);
        private static Type variableReferenceGenericType = typeof(VariableReference<>);
        private static Type variableType = typeof(Variable);
        private static Type variableValueGenericType = typeof(VariableValue<>);

        public static void Add<T>(ref IList<T> list, T data)
        {
            if (data != null)
            {
                if (list == null)
                {
                    list = new List<T>();
                }
                list.Add(data);
            }
        }

        public static void Add<T>(ref Collection<T> collection, T data)
        {
            if (data != null)
            {
                if (collection == null)
                {
                    collection = new Collection<T>();
                }
                collection.Add(data);
            }
        }

        public static void CacheRootMetadata(Activity activity, LocationReferenceEnvironment hostEnvironment, ProcessActivityTreeOptions options, ProcessActivityCallback callback, ref IList<ValidationError> validationErrors)
        {
            if (!ShouldShortcut(activity, options))
            {
                lock (activity.ThisLock)
                {
                    if (!ShouldShortcut(activity, options))
                    {
                        if (activity.HasBeenAssociatedWithAnInstance)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RootActivityAlreadyAssociatedWithInstance(activity.DisplayName)));
                        }
                        activity.InitializeAsRoot(hostEnvironment);
                        ProcessActivityTreeCore(new ChildActivity(activity, true), null, options, callback, ref validationErrors);
                        if (!ActivityValidationServices.HasErrors(validationErrors) && options.IsRuntimeReadyOptions)
                        {
                            activity.SetRuntimeReady();
                        }
                    }
                }
            }
        }

        public static Type CreateActivityWithResult(Type resultType)
        {
            return activityGenericType.MakeGenericType(new Type[] { resultType });
        }

        public static Argument CreateArgument(Type type, ArgumentDirection direction)
        {
            return (Argument) Activator.CreateInstance(ArgumentTypeDefinitionsCache.GetArgumentType(type, direction));
        }

        public static Argument CreateArgumentOfObject(ArgumentDirection direction)
        {
            if (direction == ArgumentDirection.In)
            {
                return (Argument) Activator.CreateInstance(inArgumentOfObjectType);
            }
            if (direction == ArgumentDirection.Out)
            {
                return (Argument) Activator.CreateInstance(outArgumentOfObjectType);
            }
            return (Argument) Activator.CreateInstance(inOutArgumentOfObjectType);
        }

        public static CompletionBookmark CreateCompletionBookmark(CompletionCallback onCompleted, System.Activities.ActivityInstance owningInstance)
        {
            if (onCompleted != null)
            {
                return new CompletionBookmark(new ActivityCompletionCallbackWrapper(onCompleted, owningInstance));
            }
            return null;
        }

        public static CompletionBookmark CreateCompletionBookmark<TResult>(CompletionCallback<TResult> onCompleted, System.Activities.ActivityInstance owningInstance)
        {
            if (onCompleted != null)
            {
                return new CompletionBookmark(new FuncCompletionCallbackWrapper<TResult>(onCompleted, owningInstance));
            }
            return null;
        }

        public static CompletionBookmark CreateCompletionBookmark(DelegateCompletionCallback onCompleted, System.Activities.ActivityInstance owningInstance)
        {
            if (onCompleted != null)
            {
                return new CompletionBookmark(new DelegateCompletionCallbackWrapper(onCompleted, owningInstance));
            }
            return null;
        }

        public static FaultBookmark CreateFaultBookmark(FaultCallback onFaulted, System.Activities.ActivityInstance owningInstance)
        {
            if (onFaulted != null)
            {
                return new FaultBookmark(new FaultCallbackWrapper(onFaulted, owningInstance));
            }
            return null;
        }

        public static Type CreateLocation(Type locationType)
        {
            return locationGenericType.MakeGenericType(new Type[] { locationType });
        }

        public static ActivityWithResult CreateLocationReferenceValue(LocationReference locationReference)
        {
            return (ActivityWithResult) Activator.CreateInstance(locationReferenceValueType.MakeGenericType(new Type[] { locationReference.Type }), new object[] { locationReference });
        }

        public static Argument CreateReferenceArgument(Type argumentType, ArgumentDirection direction, string referencedArgumentName)
        {
            Argument argument = Argument.Create(argumentType, direction);
            object obj2 = null;
            if (direction == ArgumentDirection.In)
            {
                obj2 = Activator.CreateInstance(argumentValueGenericType.MakeGenericType(new Type[] { argumentType }), new object[] { referencedArgumentName });
            }
            else
            {
                obj2 = Activator.CreateInstance(argumentReferenceGenericType.MakeGenericType(new Type[] { argumentType }), new object[] { referencedArgumentName });
            }
            argument.Expression = (ActivityWithResult) obj2;
            return argument;
        }

        public static Variable CreateVariable(string name, Type type, VariableModifiers modifiers)
        {
            Variable variable = (Variable) Activator.CreateInstance(variableGenericType.MakeGenericType(new Type[] { type }));
            variable.Name = name;
            variable.Modifiers = modifiers;
            return variable;
        }

        public static object CreateVariableReference(Variable variable)
        {
            Type type = variableReferenceGenericType.MakeGenericType(new Type[] { variable.Type });
            object obj2 = Activator.CreateInstance(type);
            type.GetProperty("Variable").SetValue(obj2, variable, null);
            return obj2;
        }

        public static RuntimeArgument FindArgument(string argumentName, Activity argumentConsumer)
        {
            if ((argumentConsumer.MemberOf != null) && (argumentConsumer.MemberOf.Owner != null))
            {
                Activity owner = argumentConsumer.MemberOf.Owner;
                for (int i = 0; i < owner.RuntimeArguments.Count; i++)
                {
                    RuntimeArgument argument = owner.RuntimeArguments[i];
                    if (argument.Name == argumentName)
                    {
                        return argument;
                    }
                }
            }
            return null;
        }

        public static void FinishCachingSubtree(ChildActivity subtreeRoot, ActivityCallStack parentChain, ProcessActivityTreeOptions options)
        {
            IList<ValidationError> validationErrors = null;
            ProcessActivityTreeCore(subtreeRoot, parentChain, ProcessActivityTreeOptions.GetFinishCachingSubtreeOptions(options), new ProcessActivityCallback(ActivityUtilities.NoOpCallback), ref validationErrors);
        }

        public static void FinishCachingSubtree(ChildActivity subtreeRoot, ActivityCallStack parentChain, ProcessActivityTreeOptions options, ProcessActivityCallback callback)
        {
            IList<ValidationError> validationErrors = null;
            ProcessActivityTreeCore(subtreeRoot, parentChain, ProcessActivityTreeOptions.GetFinishCachingSubtreeOptions(options), callback, ref validationErrors);
        }

        public static string GetDisplayName(object source)
        {
            return GetDisplayName(source.GetType());
        }

        private static string GetDisplayName(Type sourceType)
        {
            if (!sourceType.IsGenericType)
            {
                return sourceType.Name;
            }
            string name = sourceType.Name;
            int index = name.IndexOf('`');
            if (index > 0)
            {
                name = name.Substring(0, index);
            }
            Type[] genericArguments = sourceType.GetGenericArguments();
            StringBuilder builder = new StringBuilder(name);
            builder.Append("<");
            for (int i = 0; i < (genericArguments.Length - 1); i++)
            {
                builder.AppendFormat("{0},", GetDisplayName(genericArguments[i]));
            }
            builder.AppendFormat("{0}>", GetDisplayName(genericArguments[genericArguments.Length - 1]));
            return builder.ToString();
        }

        public static string GetTraceString(Bookmark bookmark)
        {
            if (bookmark.IsNamed)
            {
                return ("'" + bookmark.Name + "'");
            }
            return string.Format(CultureInfo.InvariantCulture, "<Unnamed Id={0}>", new object[] { bookmark.Id });
        }

        public static string GetTraceString(BookmarkScope bookmarkScope)
        {
            if (bookmarkScope == null)
            {
                return "<None>";
            }
            if (bookmarkScope.IsInitialized)
            {
                return ("'" + bookmarkScope.Id.ToString() + "'");
            }
            return string.Format(CultureInfo.InvariantCulture, "<Uninitialized TemporaryId={0}>", new object[] { bookmarkScope.TemporaryId });
        }

        public static bool IsActivityDelegateType(Type propertyType)
        {
            return TypeHelper.AreTypesCompatible(propertyType, activityDelegateType);
        }

        public static bool IsActivityType(Type propertyType)
        {
            return IsActivityType(propertyType, true);
        }

        public static bool IsActivityType(Type propertyType, bool includeConstraints)
        {
            if (!TypeHelper.AreTypesCompatible(propertyType, activityType))
            {
                return false;
            }
            if (!includeConstraints)
            {
                return !TypeHelper.AreTypesCompatible(propertyType, constraintType);
            }
            return true;
        }

        public static bool IsArgumentDictionaryType(Type type, out Type innerType)
        {
            if (type.IsGenericType)
            {
                bool flag = false;
                Type type2 = null;
                if (type.GetGenericTypeDefinition() == iDictionaryGenericType)
                {
                    flag = true;
                    type2 = type;
                }
                else
                {
                    foreach (Type type3 in type.GetInterfaces())
                    {
                        if (type3.IsGenericType && (type3.GetGenericTypeDefinition() == iDictionaryGenericType))
                        {
                            flag = true;
                            type2 = type3;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    Type[] genericArguments = type2.GetGenericArguments();
                    if ((genericArguments[0] == TypeHelper.StringType) && IsArgumentType(genericArguments[1]))
                    {
                        innerType = genericArguments[1];
                        return true;
                    }
                }
            }
            innerType = null;
            return false;
        }

        public static bool IsArgumentType(Type propertyType)
        {
            return TypeHelper.AreTypesCompatible(propertyType, argumentType);
        }

        public static bool IsCompletedState(ActivityInstanceState state)
        {
            return (state != ActivityInstanceState.Executing);
        }

        public static bool IsHandle(Type type)
        {
            return handleType.IsAssignableFrom(type);
        }

        public static bool IsInScope(System.Activities.ActivityInstance potentialChild, System.Activities.ActivityInstance scope)
        {
            if (scope == null)
            {
                return true;
            }
            System.Activities.ActivityInstance parent = potentialChild;
            while ((parent != null) && (parent != scope))
            {
                parent = parent.Parent;
            }
            return (parent != null);
        }

        public static bool IsKnownCollectionType(Type type, out Type innerType)
        {
            if (type.IsGenericType)
            {
                if (type.IsInterface)
                {
                    Type genericTypeDefinition = type.GetGenericTypeDefinition();
                    foreach (Type type3 in CollectionInterfaces)
                    {
                        if (genericTypeDefinition == type3)
                        {
                            Type[] genericArguments = type.GetGenericArguments();
                            if (genericArguments.Length == 1)
                            {
                                innerType = genericArguments[0];
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Type type4 in type.GetInterfaces())
                    {
                        if (type4.IsGenericType)
                        {
                            Type type5 = type4.GetGenericTypeDefinition();
                            foreach (Type type6 in CollectionInterfaces)
                            {
                                if (type5 == type6)
                                {
                                    Type[] typeArray3 = type4.GetGenericArguments();
                                    if (typeArray3.Length == 1)
                                    {
                                        innerType = typeArray3[0];
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            innerType = null;
            return false;
        }

        public static bool IsLocationGenericType(Type type, out Type genericArgumentType)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == locationGenericType))
            {
                genericArgumentType = type.GetGenericArguments()[0];
                return true;
            }
            genericArgumentType = null;
            return false;
        }

        public static bool IsRuntimeArgumentType(Type propertyType)
        {
            return TypeHelper.AreTypesCompatible(propertyType, runtimeArgumentType);
        }

        public static bool IsVariableType(Type propertyType)
        {
            return ((propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == variableGenericType)) || TypeHelper.AreTypesCompatible(propertyType, variableType));
        }

        public static bool IsVariableType(Type propertyType, out Type innerType)
        {
            if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == variableGenericType))
            {
                innerType = propertyType.GetGenericArguments()[0];
                return true;
            }
            innerType = null;
            return TypeHelper.AreTypesCompatible(propertyType, variableType);
        }

        private static void NoOpCallback(ChildActivity element, ActivityCallStack parentChain)
        {
        }

        private static void ProcessActivity(ChildActivity childActivity, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining, ActivityCallStack parentChain, ref IList<ValidationError> validationErrors, ProcessActivityTreeOptions options, ProcessActivityCallback callback)
        {
            Activity element = childActivity.Activity;
            IList<Constraint> runtimeConstraints = element.RuntimeConstraints;
            IList<ValidationError> list2 = null;
            if (!element.HasStartedCachingMetadata)
            {
                element.MemberOf.AddMember(element);
                element.InternalCacheMetadata(options.CreateEmptyBindings, ref list2);
                ActivityValidationServices.ValidateArguments(element, element.Parent == null, ref list2);
                ActivityLocationReferenceEnvironment environment = null;
                ActivityLocationReferenceEnvironment environment2 = new ActivityLocationReferenceEnvironment(element.HostEnvironment) {
                    InternalRoot = element
                };
                int nextEnvironmentId = 0;
                ProcessChildren(element, element.Children, ActivityCollectionType.Public, true, ref nextActivity, ref activitiesRemaining, ref list2);
                ProcessChildren(element, element.ImportedChildren, ActivityCollectionType.Imports, true, ref nextActivity, ref activitiesRemaining, ref list2);
                ProcessChildren(element, element.ImplementationChildren, ActivityCollectionType.Implementation, !options.SkipPrivateChildren, ref nextActivity, ref activitiesRemaining, ref list2);
                ProcessArguments(element, element.RuntimeArguments, true, ref environment2, ref nextEnvironmentId, ref nextActivity, ref activitiesRemaining, ref list2);
                ProcessVariables(element, element.RuntimeVariables, ActivityCollectionType.Public, true, ref environment, ref nextEnvironmentId, ref nextActivity, ref activitiesRemaining, ref list2);
                ProcessVariables(element, element.ImplementationVariables, ActivityCollectionType.Implementation, !options.SkipPrivateChildren, ref environment2, ref nextEnvironmentId, ref nextActivity, ref activitiesRemaining, ref list2);
                if (element.HandlerOf != null)
                {
                    for (int i = 0; i < element.HandlerOf.RuntimeDelegateArguments.Count; i++)
                    {
                        RuntimeDelegateArgument argument = element.HandlerOf.RuntimeDelegateArguments[i];
                        DelegateArgument boundArgument = argument.BoundArgument;
                        if ((boundArgument != null) && boundArgument.InitializeRelationship(element, ref list2))
                        {
                            boundArgument.Id = nextEnvironmentId;
                            nextEnvironmentId++;
                        }
                    }
                }
                if (environment == null)
                {
                    element.PublicEnvironment = new ActivityLocationReferenceEnvironment(element.GetParentEnvironment());
                }
                else
                {
                    if (environment.Parent == null)
                    {
                        environment.InternalRoot = element;
                    }
                    element.PublicEnvironment = environment;
                }
                element.ImplementationEnvironment = environment2;
                ProcessDelegates(element, element.Delegates, ActivityCollectionType.Public, true, ref nextActivity, ref activitiesRemaining, ref list2);
                ProcessDelegates(element, element.ImportedDelegates, ActivityCollectionType.Imports, true, ref nextActivity, ref activitiesRemaining, ref list2);
                ProcessDelegates(element, element.ImplementationDelegates, ActivityCollectionType.Implementation, !options.SkipPrivateChildren, ref nextActivity, ref activitiesRemaining, ref list2);
                if (callback != null)
                {
                    callback(childActivity, parentChain);
                }
                if (list2 != null)
                {
                    Activity activity2;
                    if (validationErrors == null)
                    {
                        validationErrors = new List<ValidationError>();
                    }
                    string str = ActivityValidationServices.GenerateValidationErrorPrefix(childActivity.Activity, parentChain, out activity2);
                    for (int j = 0; j < list2.Count; j++)
                    {
                        ValidationError item = list2[j];
                        item.Source = activity2;
                        item.Id = activity2.Id;
                        if (!string.IsNullOrEmpty(str))
                        {
                            item.Message = str + item.Message;
                        }
                        validationErrors.Add(item);
                    }
                    list2 = null;
                }
                if (options.StoreTempViolations && (validationErrors != null))
                {
                    childActivity.Activity.SetTempValidationErrorCollection(validationErrors);
                    validationErrors = null;
                }
            }
            else
            {
                SetupForProcessing(element.Children, true, ref nextActivity, ref activitiesRemaining);
                SetupForProcessing(element.ImportedChildren, false, ref nextActivity, ref activitiesRemaining);
                SetupForProcessing(element.RuntimeArguments, ref nextActivity, ref activitiesRemaining);
                SetupForProcessing(element.RuntimeVariables, ref nextActivity, ref activitiesRemaining);
                SetupForProcessing(element.Delegates, true, ref nextActivity, ref activitiesRemaining);
                SetupForProcessing(element.ImportedDelegates, false, ref nextActivity, ref activitiesRemaining);
                if (!options.SkipPrivateChildren)
                {
                    SetupForProcessing(element.ImplementationChildren, true, ref nextActivity, ref activitiesRemaining);
                    SetupForProcessing(element.ImplementationDelegates, true, ref nextActivity, ref activitiesRemaining);
                    SetupForProcessing(element.ImplementationVariables, ref nextActivity, ref activitiesRemaining);
                }
                if ((callback != null) && !options.OnlyCallCallbackForDeclarations)
                {
                    callback(childActivity, parentChain);
                }
                if (childActivity.Activity.HasTempViolations && !options.StoreTempViolations)
                {
                    childActivity.Activity.TransferTempValidationErrors(ref validationErrors);
                }
            }
            if ((!options.SkipConstraints && parentChain.WillExecute) && (childActivity.CanBeExecuted && (runtimeConstraints.Count > 0)))
            {
                ActivityValidationServices.RunConstraints(childActivity, parentChain, runtimeConstraints, options, false, ref validationErrors);
            }
        }

        public static void ProcessActivityInstanceTree(System.Activities.ActivityInstance rootInstance, ActivityExecutor executor, Func<System.Activities.ActivityInstance, ActivityExecutor, bool> callback)
        {
            Queue<IList<System.Activities.ActivityInstance>> instancesRemaining = null;
            TreeProcessingList otherList = new TreeProcessingList();
            otherList.Add(rootInstance);
            TreeProcessingList nextInstanceList = null;
            if (rootInstance.HasChildren)
            {
                nextInstanceList = new TreeProcessingList();
            }
            while (((instancesRemaining != null) && (instancesRemaining.Count > 0)) || (otherList.Count != 0))
            {
                if (otherList.Count == 0)
                {
                    otherList.Set(instancesRemaining.Dequeue());
                }
                for (int i = 0; i < otherList.Count; i++)
                {
                    System.Activities.ActivityInstance instance = otherList[i];
                    if (callback(instance, executor) && instance.HasChildren)
                    {
                        instance.AppendChildren(nextInstanceList, ref instancesRemaining);
                    }
                }
                if ((nextInstanceList != null) && (nextInstanceList.Count > 0))
                {
                    nextInstanceList.TransferTo(otherList);
                }
                else
                {
                    otherList.Reset();
                }
            }
        }

        private static void ProcessActivityTreeCore(ChildActivity currentActivity, ActivityCallStack parentChain, ProcessActivityTreeOptions options, ProcessActivityCallback callback, ref IList<ValidationError> validationErrors)
        {
            ChildActivity empty = ChildActivity.Empty;
            Stack<ChildActivity> activitiesRemaining = null;
            if (parentChain == null)
            {
                parentChain = new ActivityCallStack();
            }
            if (options.OnlyVisitSingleLevel)
            {
                ProcessActivity(currentActivity, ref empty, ref activitiesRemaining, parentChain, ref validationErrors, options, callback);
            }
            else
            {
                while (!currentActivity.Equals(ChildActivity.Empty))
                {
                    if (object.ReferenceEquals(currentActivity.Activity, popActivity))
                    {
                        parentChain.Pop().Activity.SetCached();
                    }
                    else
                    {
                        SetupForProcessing(popActivity, true, ref empty, ref activitiesRemaining);
                        ProcessActivity(currentActivity, ref empty, ref activitiesRemaining, parentChain, ref validationErrors, options, callback);
                        parentChain.Push(currentActivity);
                    }
                    currentActivity = empty;
                    if ((activitiesRemaining != null) && (activitiesRemaining.Count > 0))
                    {
                        empty = activitiesRemaining.Pop();
                    }
                    else
                    {
                        empty = ChildActivity.Empty;
                    }
                }
            }
        }

        private static void ProcessArguments(Activity parent, IList<RuntimeArgument> arguments, bool addChildren, ref ActivityLocationReferenceEnvironment environment, ref int nextEnvironmentId, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining, ref IList<ValidationError> validationErrors)
        {
            if (arguments.Count > 0)
            {
                if (environment == null)
                {
                    environment = new ActivityLocationReferenceEnvironment(parent.GetParentEnvironment());
                }
                for (int i = 0; i < arguments.Count; i++)
                {
                    RuntimeArgument locationReference = arguments[i];
                    if (locationReference.InitializeRelationship(parent, ref validationErrors))
                    {
                        locationReference.Id = nextEnvironmentId;
                        nextEnvironmentId++;
                        environment.Declare(locationReference, locationReference.Owner, ref validationErrors);
                        if (addChildren)
                        {
                            SetupForProcessing(locationReference, ref nextActivity, ref activitiesRemaining);
                        }
                    }
                }
            }
        }

        private static void ProcessChildren(Activity parent, IList<Activity> children, ActivityCollectionType collectionType, bool addChildren, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining, ref IList<ValidationError> validationErrors)
        {
            for (int i = 0; i < children.Count; i++)
            {
                Activity activity = children[i];
                if (activity.InitializeRelationship(parent, collectionType, ref validationErrors) && addChildren)
                {
                    SetupForProcessing(activity, collectionType != ActivityCollectionType.Imports, ref nextActivity, ref activitiesRemaining);
                }
            }
        }

        private static void ProcessDelegates(Activity parent, IList<ActivityDelegate> delegates, ActivityCollectionType collectionType, bool addChildren, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining, ref IList<ValidationError> validationErrors)
        {
            for (int i = 0; i < delegates.Count; i++)
            {
                ActivityDelegate activityDelegate = delegates[i];
                if (activityDelegate.InitializeRelationship(parent, collectionType, ref validationErrors) && addChildren)
                {
                    SetupForProcessing(activityDelegate, collectionType != ActivityCollectionType.Imports, ref nextActivity, ref activitiesRemaining);
                }
            }
        }

        private static void ProcessVariables(Activity parent, IList<Variable> variables, ActivityCollectionType collectionType, bool addChildren, ref ActivityLocationReferenceEnvironment environment, ref int nextEnvironmentId, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining, ref IList<ValidationError> validationErrors)
        {
            if (variables.Count > 0)
            {
                if (environment == null)
                {
                    environment = new ActivityLocationReferenceEnvironment(parent.GetParentEnvironment());
                }
                for (int i = 0; i < variables.Count; i++)
                {
                    Variable locationReference = variables[i];
                    if (locationReference.InitializeRelationship(parent, collectionType == ActivityCollectionType.Public, ref validationErrors))
                    {
                        locationReference.Id = nextEnvironmentId;
                        nextEnvironmentId++;
                        environment.Declare(locationReference, locationReference.Owner, ref validationErrors);
                        if (addChildren)
                        {
                            SetupForProcessing(locationReference, ref nextActivity, ref activitiesRemaining);
                        }
                    }
                }
            }
        }

        public static void RemoveNulls(IList list)
        {
            if (list != null)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] == null)
                    {
                        list.RemoveAt(i);
                    }
                }
            }
        }

        private static void SetupForProcessing(RuntimeArgument argument, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining)
        {
            if ((argument.BoundArgument != null) && !argument.BoundArgument.IsEmpty)
            {
                SetupForProcessing(argument.BoundArgument.Expression, true, ref nextActivity, ref activitiesRemaining);
            }
        }

        private static void SetupForProcessing(Variable variable, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining)
        {
            if (variable.Default != null)
            {
                SetupForProcessing(variable.Default, true, ref nextActivity, ref activitiesRemaining);
            }
        }

        private static void SetupForProcessing(IList<RuntimeArgument> arguments, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                SetupForProcessing(arguments[i], ref nextActivity, ref activitiesRemaining);
            }
        }

        private static void SetupForProcessing(IList<Variable> variables, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                SetupForProcessing(variables[i], ref nextActivity, ref activitiesRemaining);
            }
        }

        private static void SetupForProcessing(Activity activity, bool canBeExecuted, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining)
        {
            if (!nextActivity.Equals(ChildActivity.Empty))
            {
                if (activitiesRemaining == null)
                {
                    activitiesRemaining = new Stack<ChildActivity>();
                }
                activitiesRemaining.Push(nextActivity);
            }
            nextActivity = new ChildActivity(activity, canBeExecuted);
        }

        private static void SetupForProcessing(ActivityDelegate activityDelegate, bool canBeExecuted, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining)
        {
            if (activityDelegate.Handler != null)
            {
                SetupForProcessing(activityDelegate.Handler, canBeExecuted, ref nextActivity, ref activitiesRemaining);
            }
        }

        private static void SetupForProcessing(IList<Activity> children, bool canBeExecuted, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining)
        {
            for (int i = 0; i < children.Count; i++)
            {
                SetupForProcessing(children[i], canBeExecuted, ref nextActivity, ref activitiesRemaining);
            }
        }

        private static void SetupForProcessing(IList<ActivityDelegate> delegates, bool canBeExecuted, ref ChildActivity nextActivity, ref Stack<ChildActivity> activitiesRemaining)
        {
            for (int i = 0; i < delegates.Count; i++)
            {
                SetupForProcessing(delegates[i], canBeExecuted, ref nextActivity, ref activitiesRemaining);
            }
        }

        private static bool ShouldShortcut(Activity activity, ProcessActivityTreeOptions options)
        {
            return ((options.SkipIfCached && options.IsRuntimeReadyOptions) && activity.IsRuntimeReady);
        }

        public static bool TryGetArgumentDirectionAndType(Type propertyType, out ArgumentDirection direction, out Type argumentType)
        {
            direction = ArgumentDirection.In;
            argumentType = TypeHelper.ObjectType;
            if (propertyType.IsGenericType)
            {
                argumentType = propertyType.GetGenericArguments()[0];
                Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();
                if (genericTypeDefinition == inArgumentGenericType)
                {
                    return true;
                }
                if (genericTypeDefinition == outArgumentGenericType)
                {
                    direction = ArgumentDirection.Out;
                    return true;
                }
                if (genericTypeDefinition == inOutArgumentGenericType)
                {
                    direction = ArgumentDirection.InOut;
                    return true;
                }
            }
            else
            {
                if (propertyType == inArgumentType)
                {
                    return true;
                }
                if (propertyType == outArgumentType)
                {
                    direction = ArgumentDirection.Out;
                    return true;
                }
                if (propertyType == inOutArgumentType)
                {
                    direction = ArgumentDirection.InOut;
                    return true;
                }
            }
            return false;
        }

        public static bool TryGetDelegateArgumentDirectionAndType(Type propertyType, out ArgumentDirection direction, out Type argumentType)
        {
            direction = ArgumentDirection.In;
            argumentType = TypeHelper.ObjectType;
            if (propertyType.IsGenericType)
            {
                argumentType = propertyType.GetGenericArguments()[0];
                Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();
                if (genericTypeDefinition == delegateInArgumentGenericType)
                {
                    return true;
                }
                if (genericTypeDefinition == delegateOutArgumentGenericType)
                {
                    direction = ArgumentDirection.Out;
                    return true;
                }
            }
            else
            {
                if (propertyType == delegateInArgumentType)
                {
                    return true;
                }
                if (propertyType == delegateOutArgumentType)
                {
                    direction = ArgumentDirection.Out;
                    return true;
                }
            }
            return false;
        }

        private static IList<Type> CollectionInterfaces
        {
            get
            {
                if (collectionInterfaces == null)
                {
                    collectionInterfaces = new List<Type>(2) { typeof(IList<>), typeof(ICollection<>) };
                }
                return collectionInterfaces;
            }
        }

        public static ReadOnlyDictionary<string, object> EmptyParameters
        {
            get
            {
                return emptyParameters;
            }
        }

        internal static PropertyChangedEventArgs ValuePropertyChangedEventArgs
        {
            get
            {
                if (propertyChangedEventArgs == null)
                {
                    propertyChangedEventArgs = new PropertyChangedEventArgs("Value");
                }
                return propertyChangedEventArgs;
            }
        }

        public class ActivityCallStack
        {
            private Quack<ActivityUtilities.ChildActivity> callStack = new Quack<ActivityUtilities.ChildActivity>();
            private int nonExecutingParentCount;

            public ActivityUtilities.ChildActivity Pop()
            {
                ActivityUtilities.ChildActivity activity = this.callStack.Dequeue();
                if (!activity.CanBeExecuted)
                {
                    this.nonExecutingParentCount--;
                }
                return activity;
            }

            public void Push(ActivityUtilities.ChildActivity childActivity)
            {
                if (!childActivity.CanBeExecuted)
                {
                    this.nonExecutingParentCount++;
                }
                this.callStack.PushFront(childActivity);
            }

            public int Count
            {
                get
                {
                    return this.callStack.Count;
                }
            }

            public ActivityUtilities.ChildActivity this[int index]
            {
                get
                {
                    return this.callStack[index];
                }
            }

            public bool WillExecute
            {
                get
                {
                    return (this.nonExecutingParentCount == 0);
                }
            }
        }

        private static class ArgumentTypeDefinitionsCache
        {
            private static Hashtable inArgumentTypeDefinitions = new Hashtable();
            private static Hashtable inOutArgumentTypeDefinitions = new Hashtable();
            private static Hashtable outArgumentTypeDefinitions = new Hashtable();

            private static Type CreateArgumentType(Type type, ArgumentDirection direction)
            {
                if (direction == ArgumentDirection.In)
                {
                    return ActivityUtilities.inArgumentGenericType.MakeGenericType(new Type[] { type });
                }
                if (direction == ArgumentDirection.Out)
                {
                    return ActivityUtilities.outArgumentGenericType.MakeGenericType(new Type[] { type });
                }
                return ActivityUtilities.inOutArgumentGenericType.MakeGenericType(new Type[] { type });
            }

            public static Type GetArgumentType(Type type, ArgumentDirection direction)
            {
                Hashtable inArgumentTypeDefinitions = null;
                if (direction == ArgumentDirection.In)
                {
                    inArgumentTypeDefinitions = ActivityUtilities.ArgumentTypeDefinitionsCache.inArgumentTypeDefinitions;
                }
                else if (direction == ArgumentDirection.Out)
                {
                    inArgumentTypeDefinitions = outArgumentTypeDefinitions;
                }
                else
                {
                    inArgumentTypeDefinitions = inOutArgumentTypeDefinitions;
                }
                Type type2 = inArgumentTypeDefinitions[type] as Type;
                if (type2 == null)
                {
                    type2 = CreateArgumentType(type, direction);
                    lock (inArgumentTypeDefinitions)
                    {
                        inArgumentTypeDefinitions[type] = type2;
                    }
                }
                return type2;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ChildActivity : IEquatable<ActivityUtilities.ChildActivity>
        {
            public ChildActivity(System.Activities.Activity activity, bool canBeExecuted)
            {
                this = new ActivityUtilities.ChildActivity();
                this.Activity = activity;
                this.CanBeExecuted = canBeExecuted;
            }

            public static ActivityUtilities.ChildActivity Empty
            {
                get
                {
                    return new ActivityUtilities.ChildActivity();
                }
            }
            public System.Activities.Activity Activity { get; set; }
            public bool CanBeExecuted { get; set; }
            public bool Equals(ActivityUtilities.ChildActivity other)
            {
                return (object.ReferenceEquals(this.Activity, other.Activity) && (this.CanBeExecuted == other.CanBeExecuted));
            }
        }

        private class Pop : Activity
        {
            internal override void InternalExecute(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                throw Fx.AssertAndThrow("should never get here");
            }

            internal override void OnInternalCacheMetadata(bool createEmptyBindings)
            {
                throw Fx.AssertAndThrow("should never get here");
            }
        }

        public delegate void ProcessActivityCallback(ActivityUtilities.ChildActivity childActivity, ActivityUtilities.ActivityCallStack parentChain);

        public class TreeProcessingList
        {
            private bool addRequiresNewList;
            private IList<System.Activities.ActivityInstance> multipleItems;
            private System.Activities.ActivityInstance singleItem;

            public void Add(System.Activities.ActivityInstance item)
            {
                if (this.multipleItems != null)
                {
                    if (this.addRequiresNewList)
                    {
                        this.multipleItems = new List<System.Activities.ActivityInstance>(this.multipleItems);
                        this.addRequiresNewList = false;
                    }
                    this.multipleItems.Add(item);
                }
                else if (this.singleItem != null)
                {
                    this.multipleItems = new List<System.Activities.ActivityInstance>(2);
                    this.multipleItems.Add(this.singleItem);
                    this.multipleItems.Add(item);
                    this.singleItem = null;
                }
                else
                {
                    this.singleItem = item;
                }
            }

            public void Reset()
            {
                this.addRequiresNewList = false;
                this.multipleItems = null;
                this.singleItem = null;
            }

            public void Set(IList<System.Activities.ActivityInstance> listToSet)
            {
                this.multipleItems = listToSet;
                this.addRequiresNewList = true;
            }

            public void TransferTo(ActivityUtilities.TreeProcessingList otherList)
            {
                otherList.singleItem = this.singleItem;
                otherList.multipleItems = this.multipleItems;
                otherList.addRequiresNewList = this.addRequiresNewList;
                this.Reset();
            }

            public int Count
            {
                get
                {
                    if (this.singleItem != null)
                    {
                        return 1;
                    }
                    if (this.multipleItems != null)
                    {
                        return this.multipleItems.Count;
                    }
                    return 0;
                }
            }

            public System.Activities.ActivityInstance this[int index]
            {
                get
                {
                    if (this.singleItem != null)
                    {
                        return this.singleItem;
                    }
                    return this.multipleItems[index];
                }
            }
        }
    }
}

