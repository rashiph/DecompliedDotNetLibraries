namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Persistence;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Transactions;
    using System.Workflow.Activities;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Hosting;

    public sealed class Interop : NativeActivity, ICustomTypeDescriptor
    {
        private Type activityType;
        private IList<InteropProperty> exposedBodyProperties;
        private bool exposedBodyPropertiesCacheIsValid;
        private HashSet<string> extraDynamicArguments;
        private static Func<TimerExtension> getDefaultTimerExtension = new Func<TimerExtension>(Interop.GetDefaultTimerExtension);
        private static Func<InteropPersistenceParticipant> getInteropPersistenceParticipant = new Func<InteropPersistenceParticipant>(Interop.GetInteropPersistenceParticipant);
        private bool hasNameCollision;
        private bool hasValidBody;
        internal const string InArgumentSuffix = "In";
        private Variable<InteropExecutor> interopActivityExecutor = new Variable<InteropExecutor>();
        private Variable<InteropEnlistment> interopEnlistment = new Variable<InteropEnlistment>();
        private Dictionary<string, object> metaProperties;
        private CompletionCallback onPersistComplete;
        private BookmarkCallback onResumeBookmark;
        private BookmarkCallback onTransactionComplete;
        internal const string OutArgumentSuffix = "Out";
        private IList<PropertyInfo> outputPropertyDefinitions;
        private Variable<Exception> outstandingException = new Variable<Exception>();
        private System.Activities.Statements.Persist persistActivity;
        private Variable<bool> persistOnClose = new Variable<bool>();
        private Dictionary<string, Argument> properties;
        private Variable<RuntimeTransactionHandle> runtimeTransactionHandle = new Variable<RuntimeTransactionHandle>();
        private object thisLock;
        private System.Workflow.ComponentModel.Activity v1Activity;

        public Interop()
        {
            this.onResumeBookmark = new BookmarkCallback(this.OnResumeBookmark);
            this.persistActivity = new System.Activities.Statements.Persist();
            this.thisLock = new object();
            base.Constraints.Add(this.ProcessAdvancedConstraints());
        }

        internal void AddResourceManager(NativeActivityContext context, VolatileResourceManager resourceManager)
        {
            if ((Transaction.Current != null) && (Transaction.Current.TransactionInformation.Status == TransactionStatus.Active))
            {
                InteropEnlistment enlistmentNotification = this.interopEnlistment.Get(context);
                if ((enlistmentNotification == null) || !enlistmentNotification.IsValid)
                {
                    enlistmentNotification = new InteropEnlistment(Transaction.Current, resourceManager);
                    Transaction.Current.EnlistVolatile(enlistmentNotification, EnlistmentOptions.EnlistDuringPrepareRequired);
                    this.interopEnlistment.Set(context, enlistmentNotification);
                }
            }
            else
            {
                context.GetExtension<InteropPersistenceParticipant>().Add(base.Id, resourceManager);
                this.persistOnClose.Set(context, true);
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (this.extraDynamicArguments != null)
            {
                this.extraDynamicArguments.Clear();
            }
            this.v1Activity = null;
            if (this.hasValidBody)
            {
                this.outputPropertyDefinitions = new List<PropertyInfo>();
                if (this.properties != null)
                {
                    if (this.extraDynamicArguments == null)
                    {
                        this.extraDynamicArguments = new HashSet<string>();
                    }
                    foreach (string str in this.properties.Keys)
                    {
                        this.extraDynamicArguments.Add(str);
                    }
                }
                PropertyInfo[] properties = this.ActivityType.GetProperties();
                this.hasNameCollision = InteropEnvironment.ParameterHelper.HasPropertyNameCollision(properties);
                foreach (PropertyInfo info in properties)
                {
                    if (InteropEnvironment.ParameterHelper.IsBindable(info))
                    {
                        string str2;
                        if (this.hasNameCollision)
                        {
                            str2 = info.Name + "In";
                        }
                        else
                        {
                            str2 = info.Name;
                        }
                        string name = info.Name + "Out";
                        RuntimeArgument argument = new RuntimeArgument(str2, info.PropertyType, ArgumentDirection.In);
                        RuntimeArgument argument2 = new RuntimeArgument(name, info.PropertyType, ArgumentDirection.Out);
                        if (this.properties != null)
                        {
                            Argument argument3 = null;
                            if (this.properties.TryGetValue(str2, out argument3))
                            {
                                if (argument3.Direction != ArgumentDirection.In)
                                {
                                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropArgumentDirectionMismatch, new object[] { str2, name }));
                                }
                                this.extraDynamicArguments.Remove(str2);
                                metadata.Bind(argument3, argument);
                            }
                            Argument argument4 = null;
                            if (this.properties.TryGetValue(name, out argument4))
                            {
                                this.extraDynamicArguments.Remove(name);
                                metadata.Bind(argument4, argument2);
                            }
                        }
                        metadata.AddArgument(argument);
                        metadata.AddArgument(argument2);
                        this.outputPropertyDefinitions.Add(info);
                    }
                }
            }
            Collection<Variable> implementationVariables = new Collection<Variable> {
                this.interopActivityExecutor,
                this.runtimeTransactionHandle,
                this.persistOnClose,
                this.interopEnlistment,
                this.outstandingException
            };
            metadata.SetImplementationVariablesCollection(implementationVariables);
            metadata.AddImplementationChild(this.persistActivity);
            if (!this.hasValidBody)
            {
                if (this.ActivityType == null)
                {
                    metadata.AddValidationError(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropBodyNotSet, new object[] { base.DisplayName }));
                }
                else
                {
                    if (!typeof(System.Workflow.ComponentModel.Activity).IsAssignableFrom(this.ActivityType))
                    {
                        metadata.AddValidationError(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropWrongBody, new object[] { base.DisplayName }));
                    }
                    if (this.ActivityType.GetConstructor(Type.EmptyTypes) == null)
                    {
                        metadata.AddValidationError(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropBodyMustHavePublicDefaultConstructor, new object[] { base.DisplayName }));
                    }
                }
            }
            else if ((this.extraDynamicArguments != null) && (this.extraDynamicArguments.Count > 0))
            {
                metadata.AddValidationError(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.AttemptToBindUnknownProperties, new object[] { base.DisplayName, this.extraDynamicArguments.First<string>() }));
            }
            else
            {
                try
                {
                    this.InitializeMetaProperties(this.ComponentModelActivity);
                }
                catch (InvalidOperationException exception)
                {
                    metadata.AddValidationError(exception.Message);
                }
            }
            metadata.AddDefaultExtensionProvider<TimerExtension>(getDefaultTimerExtension);
            metadata.AddDefaultExtensionProvider<InteropPersistenceParticipant>(getInteropPersistenceParticipant);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            InteropExecutor interopExecutor = this.interopActivityExecutor.Get(context);
            if (!interopExecutor.HasCheckedForTrackingParticipant)
            {
                interopExecutor.TrackingEnabled = context.GetExtension<TrackingParticipant>() != null;
                interopExecutor.HasCheckedForTrackingParticipant = true;
            }
            interopExecutor.EnsureReload(this);
            try
            {
                using (InteropEnvironment environment = new InteropEnvironment(interopExecutor, context, this.onResumeBookmark, this, this.runtimeTransactionHandle.Get(context).GetCurrentTransaction(context)))
                {
                    environment.Cancel();
                }
            }
            catch (Exception exception)
            {
                if (WorkflowExecutor.IsIrrecoverableException(exception) || !this.persistOnClose.Get(context))
                {
                    throw;
                }
            }
        }

        internal void CommitTransaction(NativeActivityContext context)
        {
            if (this.onTransactionComplete == null)
            {
                this.onTransactionComplete = new BookmarkCallback(this.OnTransactionComplete);
            }
            this.runtimeTransactionHandle.Get(context).CompleteTransaction(context, this.onTransactionComplete);
        }

        private System.Workflow.ComponentModel.Activity CreateActivity()
        {
            return (Activator.CreateInstance(this.ActivityType) as System.Workflow.ComponentModel.Activity);
        }

        internal void CreateTransaction(NativeActivityContext context, TransactionOptions txOptions)
        {
            this.runtimeTransactionHandle.Get(context).RequestTransactionContext(context, new Action<NativeActivityTransactionContext, object>(this.OnTransactionContextAcquired), txOptions);
        }

        protected override void Execute(NativeActivityContext context)
        {
            WorkflowRuntimeService extension = context.GetExtension<WorkflowRuntimeService>();
            if ((extension != null) && !(extension is ExternalDataExchangeService))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropWorkflowRuntimeServiceNotSupported, new object[0]));
            }
            lock (this.thisLock)
            {
                ((IDependencyObjectAccessor) this.ComponentModelActivity).InitializeDefinitionForRuntime(null);
            }
            if (this.ComponentModelActivity.Enabled)
            {
                System.Workflow.ComponentModel.Activity activity = this.CreateActivity();
                this.InitializeMetaProperties(activity);
                activity.SetValue(WorkflowExecutor.WorkflowInstanceIdProperty, context.WorkflowInstanceId);
                InteropExecutor executor = new InteropExecutor(context.WorkflowInstanceId, activity, this.OutputPropertyDefinitions, this.ComponentModelActivity);
                if (!executor.HasCheckedForTrackingParticipant)
                {
                    executor.TrackingEnabled = context.GetExtension<TrackingParticipant>() != null;
                    executor.HasCheckedForTrackingParticipant = true;
                }
                this.interopActivityExecutor.Set(context, executor);
                RuntimeTransactionHandle property = this.runtimeTransactionHandle.Get(context);
                context.Properties.Add(property.ExecutionPropertyName, property);
                try
                {
                    using (new ServiceEnvironment(activity))
                    {
                        using (InteropEnvironment environment = new InteropEnvironment(executor, context, this.onResumeBookmark, this, property.GetCurrentTransaction(context)))
                        {
                            environment.Execute(this.ComponentModelActivity, context);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (WorkflowExecutor.IsIrrecoverableException(exception) || !this.persistOnClose.Get(context))
                    {
                        throw;
                    }
                }
            }
        }

        private static TimerExtension GetDefaultTimerExtension()
        {
            return new DurableTimerExtension();
        }

        internal IDictionary<string, object> GetInputArgumentValues(NativeActivityContext context)
        {
            Dictionary<string, object> dictionary = null;
            if (this.properties != null)
            {
                foreach (KeyValuePair<string, Argument> pair in this.properties)
                {
                    Argument argument = pair.Value;
                    if (argument.Direction == ArgumentDirection.In)
                    {
                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<string, object>();
                        }
                        dictionary.Add(pair.Key, argument.Get<object>(context));
                    }
                }
            }
            return dictionary;
        }

        private static InteropPersistenceParticipant GetInteropPersistenceParticipant()
        {
            return new InteropPersistenceParticipant();
        }

        private void InitializeMetaProperties(System.Workflow.ComponentModel.Activity activity)
        {
            if ((this.metaProperties != null) && (this.metaProperties.Count > 0))
            {
                foreach (string str in this.metaProperties.Keys)
                {
                    PropertyInfo property = this.ActivityType.GetProperty(str);
                    if (property == null)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MetaPropertyDoesNotExist, new object[] { str, this.ActivityType.FullName }));
                    }
                    property.SetValue(activity, this.metaProperties[str], null);
                }
            }
        }

        internal void OnClose(NativeActivityContext context, Exception exception)
        {
            if (this.persistOnClose.Get(context))
            {
                if (exception == null)
                {
                    context.ScheduleActivity(this.persistActivity);
                }
                else
                {
                    this.outstandingException.Set(context, exception);
                    if (this.onPersistComplete == null)
                    {
                        this.onPersistComplete = new CompletionCallback(this.OnPersistComplete);
                    }
                    context.ScheduleActivity(this.persistActivity, this.onPersistComplete);
                }
            }
            this.interopEnlistment.Set(context, null);
        }

        internal void OnPersistComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            this.persistOnClose.Set(context, false);
            Exception exception = this.outstandingException.Get(context);
            if (exception != null)
            {
                this.outstandingException.Set(context, null);
                throw exception;
            }
            this.Resume(context, null);
        }

        private void OnResumeBookmark(NativeActivityContext context, Bookmark bookmark, object state)
        {
            InteropExecutor interopExecutor = this.interopActivityExecutor.Get(context);
            if (!interopExecutor.HasCheckedForTrackingParticipant)
            {
                interopExecutor.TrackingEnabled = context.GetExtension<TrackingParticipant>() != null;
                interopExecutor.HasCheckedForTrackingParticipant = true;
            }
            interopExecutor.EnsureReload(this);
            try
            {
                using (InteropEnvironment environment = new InteropEnvironment(interopExecutor, context, this.onResumeBookmark, this, this.runtimeTransactionHandle.Get(context).GetCurrentTransaction(context)))
                {
                    IComparable queueName = interopExecutor.BookmarkQueueMap[bookmark];
                    environment.EnqueueEvent(queueName, state);
                }
            }
            catch (Exception exception)
            {
                if (WorkflowExecutor.IsIrrecoverableException(exception) || !this.persistOnClose.Get(context))
                {
                    throw;
                }
            }
        }

        private void OnTransactionComplete(NativeActivityContext context, Bookmark bookmark, object state)
        {
            this.Resume(context, null);
        }

        private void OnTransactionContextAcquired(NativeActivityTransactionContext context, object state)
        {
            TransactionOptions options = (TransactionOptions) state;
            CommittableTransaction transaction = new CommittableTransaction(options);
            context.SetRuntimeTransaction(transaction);
            this.Resume(context, transaction);
        }

        internal void Persist(NativeActivityContext context)
        {
            if (this.onPersistComplete == null)
            {
                this.onPersistComplete = new CompletionCallback(this.OnPersistComplete);
            }
            context.ScheduleActivity(this.persistActivity, this.onPersistComplete);
        }

        private Constraint ProcessAdvancedConstraints()
        {
            DelegateInArgument<Interop> element = new DelegateInArgument<Interop> {
                Name = "element"
            };
            DelegateInArgument<ValidationContext> argument = new DelegateInArgument<ValidationContext> {
                Name = "validationContext"
            };
            DelegateInArgument<System.Activities.Activity> parent = new DelegateInArgument<System.Activities.Activity> {
                Name = "parent"
            };
            Variable<HashSet<InteropValidationEnum>> variable = new Variable<HashSet<InteropValidationEnum>>(context => new HashSet<InteropValidationEnum>());
            Variable<HashSet<InteropValidationEnum>> variable2 = new Variable<HashSet<InteropValidationEnum>>(context => new HashSet<InteropValidationEnum>());
            Constraint<Interop> constraint = new Constraint<Interop>();
            ActivityAction<Interop, ValidationContext> action = new ActivityAction<Interop, ValidationContext> {
                Argument1 = element,
                Argument2 = argument
            };
            If @if = new If {
                Condition = new InArgument<bool>(env => element.Get(env).hasValidBody)
            };
            Sequence sequence = new Sequence {
                Variables = { variable, variable2 }
            };
            WalkInteropBodyAndGatherData item = new WalkInteropBodyAndGatherData {
                RootLevelValidationData = new InArgument<HashSet<InteropValidationEnum>>(variable),
                NestedChildrenValidationData = new InArgument<HashSet<InteropValidationEnum>>(variable2),
                InteropActivity = element
            };
            sequence.Activities.Add(item);
            ValidateAtRootAndNestedLevels levels = new ValidateAtRootAndNestedLevels {
                RootLevelValidationData = variable,
                NestedChildrenValidationData = variable2,
                Interop = element
            };
            sequence.Activities.Add(levels);
            ForEach<System.Activities.Activity> each = new ForEach<System.Activities.Activity>();
            GetParentChain chain = new GetParentChain {
                ValidationContext = argument
            };
            each.Values = chain;
            ActivityAction<System.Activities.Activity> action2 = new ActivityAction<System.Activities.Activity> {
                Argument = parent
            };
            Sequence sequence2 = new Sequence();
            If if2 = new If();
            Or<bool, bool, bool> or = new Or<bool, bool, bool>();
            Equal<Type, Type, bool> equal = new Equal<Type, Type, bool>();
            ObtainType type = new ObtainType {
                Input = parent
            };
            equal.Left = type;
            equal.Right = new InArgument<Type>(context => typeof(System.Activities.Statements.TransactionScope));
            or.Left = equal;
            Equal<string, string, bool> equal2 = new Equal<string, string, bool> {
                Left = new InArgument<string>(env => parent.Get(env).GetType().FullName),
                Right = "System.ServiceModel.Activities.TransactedReceiveScope"
            };
            or.Right = equal2;
            if2.Condition = or;
            Sequence sequence3 = new Sequence();
            AssertValidation validation = new AssertValidation();
            CheckForTransactionScope scope = new CheckForTransactionScope {
                ValidationResults = variable2
            };
            validation.Assertion = scope;
            validation.Message = new InArgument<string>(ExecutionStringManager.InteropBodyNestedTransactionScope);
            sequence3.Activities.Add(validation);
            AssertValidation validation2 = new AssertValidation();
            CheckForPersistOnClose close = new CheckForPersistOnClose {
                NestedChildrenValidationData = variable2,
                RootLevelValidationData = variable
            };
            validation2.Assertion = close;
            validation2.Message = new InArgument<string>(ExecutionStringManager.InteropBodyNestedPersistOnCloseWithinTransactionScope);
            sequence3.Activities.Add(validation2);
            if2.Then = sequence3;
            sequence2.Activities.Add(if2);
            action2.Handler = sequence2;
            each.Body = action2;
            sequence.Activities.Add(each);
            ActivityTreeValidation validation3 = new ActivityTreeValidation {
                Interop = element
            };
            sequence.Activities.Add(validation3);
            @if.Then = sequence;
            action.Handler = @if;
            constraint.Body = action;
            return constraint;
        }

        private void Resume(NativeActivityContext context, Transaction transaction)
        {
            InteropExecutor interopExecutor = this.interopActivityExecutor.Get(context);
            if (!interopExecutor.HasCheckedForTrackingParticipant)
            {
                interopExecutor.TrackingEnabled = context.GetExtension<TrackingParticipant>() != null;
                interopExecutor.HasCheckedForTrackingParticipant = true;
            }
            interopExecutor.EnsureReload(this);
            try
            {
                using (InteropEnvironment environment = new InteropEnvironment(interopExecutor, context, this.onResumeBookmark, this, transaction))
                {
                    environment.Resume();
                }
            }
            catch (Exception exception)
            {
                if (WorkflowExecutor.IsIrrecoverableException(exception) || !this.persistOnClose.Get(context))
                {
                    throw;
                }
            }
        }

        internal void SetOutputArgumentValues(IDictionary<string, object> outputs, NativeActivityContext context)
        {
            if ((this.properties != null) && (outputs != null))
            {
                foreach (KeyValuePair<string, object> pair in outputs)
                {
                    Argument argument;
                    if ((this.properties.TryGetValue(pair.Key, out argument) && (argument != null)) && (argument.Direction == ArgumentDirection.Out))
                    {
                        argument.Set(context, pair.Value);
                    }
                }
            }
        }

        private static bool ShouldFilterProperty(PropertyDescriptor property, Attribute[] attributes)
        {
            if ((attributes != null) && (attributes.Length != 0))
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    Attribute attribute = attributes[i];
                    Attribute attribute2 = property.Attributes[attribute.GetType()];
                    if (attribute2 == null)
                    {
                        if (!attribute.IsDefaultAttribute())
                        {
                            return true;
                        }
                    }
                    else if (!attribute.Match(attribute2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor) this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection properties;
            List<PropertyDescriptor> list = new List<PropertyDescriptor>();
            if (attributes != null)
            {
                properties = TypeDescriptor.GetProperties(this, attributes, true);
            }
            else
            {
                properties = TypeDescriptor.GetProperties(this, true);
            }
            for (int i = 0; i < properties.Count; i++)
            {
                list.Add(properties[i]);
            }
            if (this.hasValidBody)
            {
                if (!this.exposedBodyPropertiesCacheIsValid)
                {
                    PropertyInfo[] infoArray = this.ActivityType.GetProperties();
                    this.hasNameCollision = InteropEnvironment.ParameterHelper.HasPropertyNameCollision(infoArray);
                    for (int j = 0; j < infoArray.Length; j++)
                    {
                        bool flag;
                        PropertyInfo propertyInfo = infoArray[j];
                        if (InteropEnvironment.ParameterHelper.IsBindableOrMetaProperty(propertyInfo, out flag))
                        {
                            Attribute[] customAttributes = Attribute.GetCustomAttributes(propertyInfo, true);
                            Attribute[] array = new Attribute[customAttributes.Length + 1];
                            customAttributes.CopyTo(array, 0);
                            array[customAttributes.Length] = new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden);
                            if (this.exposedBodyProperties == null)
                            {
                                this.exposedBodyProperties = new List<InteropProperty>(infoArray.Length);
                            }
                            if (flag)
                            {
                                InteropProperty item = new LiteralProperty(this, propertyInfo.Name, propertyInfo.PropertyType, array);
                                this.exposedBodyProperties.Add(item);
                            }
                            else
                            {
                                InteropProperty property2;
                                if (this.hasNameCollision)
                                {
                                    property2 = new ArgumentProperty(this, propertyInfo.Name + "In", Argument.Create(propertyInfo.PropertyType, ArgumentDirection.In), array);
                                }
                                else
                                {
                                    property2 = new ArgumentProperty(this, propertyInfo.Name, Argument.Create(propertyInfo.PropertyType, ArgumentDirection.In), array);
                                }
                                this.exposedBodyProperties.Add(property2);
                                InteropProperty property3 = new ArgumentProperty(this, propertyInfo.Name + "Out", Argument.Create(propertyInfo.PropertyType, ArgumentDirection.Out), array);
                                this.exposedBodyProperties.Add(property3);
                            }
                        }
                    }
                    this.exposedBodyPropertiesCacheIsValid = true;
                }
                if (this.exposedBodyProperties != null)
                {
                    for (int k = 0; k < this.exposedBodyProperties.Count; k++)
                    {
                        PropertyDescriptor property = this.exposedBodyProperties[k];
                        if ((attributes == null) || !ShouldFilterProperty(property, attributes))
                        {
                            list.Add(property);
                        }
                    }
                }
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            InteropProperty property = pd as InteropProperty;
            if (property != null)
            {
                return property.Owner;
            }
            return this;
        }

        [Browsable(false)]
        public IDictionary<string, object> ActivityMetaProperties
        {
            get
            {
                if (this.metaProperties == null)
                {
                    this.metaProperties = new Dictionary<string, object>();
                }
                return this.metaProperties;
            }
        }

        [Browsable(false)]
        public IDictionary<string, Argument> ActivityProperties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new Dictionary<string, Argument>();
                }
                return this.properties;
            }
        }

        [DefaultValue((string) null)]
        public Type ActivityType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityType;
            }
            set
            {
                if (value != this.activityType)
                {
                    this.hasValidBody = false;
                    if (((value != null) && typeof(System.Workflow.ComponentModel.Activity).IsAssignableFrom(value)) && (value.GetConstructor(Type.EmptyTypes) != null))
                    {
                        this.hasValidBody = true;
                    }
                    this.activityType = value;
                    if (this.metaProperties != null)
                    {
                        this.metaProperties.Clear();
                    }
                    if (this.outputPropertyDefinitions != null)
                    {
                        this.outputPropertyDefinitions.Clear();
                    }
                    if (this.properties != null)
                    {
                        this.properties.Clear();
                    }
                    if (this.exposedBodyProperties != null)
                    {
                        for (int i = 0; i < this.exposedBodyProperties.Count; i++)
                        {
                            this.exposedBodyProperties[i].Invalidate();
                        }
                        this.exposedBodyProperties.Clear();
                    }
                    this.exposedBodyPropertiesCacheIsValid = false;
                    this.v1Activity = null;
                }
            }
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        internal System.Workflow.ComponentModel.Activity ComponentModelActivity
        {
            get
            {
                if ((this.v1Activity == null) && (this.ActivityType != null))
                {
                    this.v1Activity = this.CreateActivity();
                }
                return this.v1Activity;
            }
        }

        internal bool HasNameCollision
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.hasNameCollision;
            }
        }

        internal IList<PropertyInfo> OutputPropertyDefinitions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.outputPropertyDefinitions;
            }
        }

        private class ActivityTreeValidation : NativeActivity
        {
            private static TypeProvider CreateTypeProvider(Type rootType)
            {
                TypeProvider provider = new TypeProvider(null);
                provider.SetLocalAssembly(rootType.Assembly);
                provider.AddAssembly(rootType.Assembly);
                foreach (AssemblyName name in rootType.Assembly.GetReferencedAssemblies())
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.Load(name);
                        if (assembly != null)
                        {
                            provider.AddAssembly(assembly);
                        }
                    }
                    catch
                    {
                    }
                    if ((assembly == null) && (name.CodeBase != null))
                    {
                        provider.AddAssemblyReference(name.CodeBase);
                    }
                }
                return provider;
            }

            protected override void Execute(NativeActivityContext context)
            {
                System.Activities.Statements.Interop interop = this.Interop.Get(context);
                if ((interop != null) && typeof(System.Workflow.ComponentModel.Activity).IsAssignableFrom(interop.ActivityType))
                {
                    ServiceContainer serviceProvider = new ServiceContainer();
                    serviceProvider.AddService(typeof(ITypeProvider), CreateTypeProvider(interop.ActivityType));
                    ValidationManager manager = new ValidationManager(serviceProvider);
                    System.Workflow.ComponentModel.Activity componentModelActivity = interop.ComponentModelActivity;
                    using (WorkflowCompilationContext.CreateScope(manager))
                    {
                        foreach (Validator validator in manager.GetValidators(interop.ActivityType))
                        {
                            foreach (System.Workflow.ComponentModel.Compiler.ValidationError error in validator.Validate(manager, componentModelActivity))
                            {
                                Constraint.AddValidationError(context, new System.Activities.Validation.ValidationError(error.ErrorText, error.IsWarning, error.PropertyName));
                            }
                        }
                    }
                }
            }

            public InArgument<System.Activities.Statements.Interop> Interop { get; set; }
        }

        private class ArgumentProperty : Interop.InteropProperty
        {
            private Argument argument;
            private string argumentName;

            public ArgumentProperty(Interop owner, string argumentName, Argument argument, Attribute[] attributes) : base(owner, argumentName, attributes)
            {
                this.argumentName = argumentName;
                this.argument = argument;
            }

            private Argument GetArgument()
            {
                Argument argument;
                if (!base.Owner.ActivityProperties.TryGetValue(this.argumentName, out argument))
                {
                    argument = this.argument;
                }
                return argument;
            }

            public override object GetValue(object component)
            {
                base.ThrowIfInvalid();
                return this.GetArgument();
            }

            public override void SetValue(object component, object value)
            {
                base.ThrowIfInvalid();
                if (value != null)
                {
                    base.Owner.ActivityProperties[this.argumentName] = (Argument) value;
                }
                else
                {
                    base.Owner.ActivityProperties.Remove(this.argumentName);
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    base.ThrowIfInvalid();
                    return false;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    base.ThrowIfInvalid();
                    return this.GetArgument().GetType();
                }
            }
        }

        private class CheckForPersistOnClose : CodeActivity<bool>
        {
            protected override bool Execute(CodeActivityContext context)
            {
                HashSet<Interop.InteropValidationEnum> set = this.NestedChildrenValidationData.Get(context);
                HashSet<Interop.InteropValidationEnum> set2 = this.RootLevelValidationData.Get(context);
                return (!set.Contains(Interop.InteropValidationEnum.PersistOnClose) && !set2.Contains(Interop.InteropValidationEnum.PersistOnClose));
            }

            public InArgument<HashSet<Interop.InteropValidationEnum>> NestedChildrenValidationData { get; set; }

            public InArgument<HashSet<Interop.InteropValidationEnum>> RootLevelValidationData { get; set; }
        }

        private class CheckForTransactionScope : CodeActivity<bool>
        {
            protected override bool Execute(CodeActivityContext context)
            {
                if (this.ValidationResults.Get(context).Contains(Interop.InteropValidationEnum.TransactionScope))
                {
                    return false;
                }
                return true;
            }

            public InArgument<HashSet<Interop.InteropValidationEnum>> ValidationResults { get; set; }
        }

        private class CompletedAsyncResult : IAsyncResult
        {
            private AsyncCallback callback;
            private bool endCalled;
            private ManualResetEvent manualResetEvent;
            private object state;
            private object thisLock;

            public CompletedAsyncResult(AsyncCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
                this.thisLock = new object();
                if (callback != null)
                {
                    try
                    {
                        callback(this);
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidProgramException(ExecutionStringManager.AsyncCallbackThrewException, exception);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                if (result == null)
                {
                    throw new ArgumentNullException("result");
                }
                Interop.CompletedAsyncResult result2 = result as Interop.CompletedAsyncResult;
                if (result2 == null)
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidAsyncResult, "result");
                }
                if (result2.endCalled)
                {
                    throw new InvalidOperationException(ExecutionStringManager.EndCalledTwice);
                }
                result2.endCalled = true;
                if (result2.manualResetEvent != null)
                {
                    result2.manualResetEvent.Close();
                }
            }

            public object AsyncState
            {
                get
                {
                    return this.state;
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    if (this.manualResetEvent == null)
                    {
                        lock (this.ThisLock)
                        {
                            if (this.manualResetEvent == null)
                            {
                                this.manualResetEvent = new ManualResetEvent(true);
                            }
                        }
                    }
                    return this.manualResetEvent;
                }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return true;
                }
            }

            private object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }
        }

        [DataContract]
        private class InteropEnlistment : IEnlistmentNotification
        {
            private VolatileResourceManager resourceManager;
            private Transaction transaction;

            public InteropEnlistment()
            {
            }

            public InteropEnlistment(Transaction transaction, VolatileResourceManager resourceManager)
            {
                this.resourceManager = resourceManager;
                this.transaction = transaction;
                this.IsValid = true;
            }

            public void Commit(Enlistment enlistment)
            {
                this.resourceManager.Complete();
                enlistment.Done();
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public void InDoubt(Enlistment enlistment)
            {
                this.Rollback(enlistment);
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope(this.transaction))
                {
                    this.resourceManager.Commit();
                    scope.Complete();
                }
                preparingEnlistment.Prepared();
            }

            public void Rollback(Enlistment enlistment)
            {
                this.resourceManager.ClearAllBatchedWork();
                enlistment.Done();
            }

            public bool IsValid
            {
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.<IsValid>k__BackingField;
                }
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.<IsValid>k__BackingField = value;
                }
            }
        }

        private class InteropPersistenceParticipant : PersistenceIOParticipant
        {
            public InteropPersistenceParticipant() : base(true, false)
            {
                this.ResourceManagers = new Dictionary<string, VolatileResourceManager>();
                this.CommittedResourceManagers = new Dictionary<Transaction, Dictionary<string, VolatileResourceManager>>();
            }

            protected override void Abort()
            {
                foreach (VolatileResourceManager manager in this.ResourceManagers.Values)
                {
                    manager.ClearAllBatchedWork();
                }
                this.ResourceManagers = new Dictionary<string, VolatileResourceManager>();
            }

            internal void Add(string activityId, VolatileResourceManager rm)
            {
                this.ResourceManagers.Add(activityId, rm);
            }

            protected override IAsyncResult BeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
            {
                try
                {
                    foreach (VolatileResourceManager manager in this.ResourceManagers.Values)
                    {
                        manager.Commit();
                    }
                }
                finally
                {
                    this.CommittedResourceManagers.Add(Transaction.Current, this.ResourceManagers);
                    this.ResourceManagers = new Dictionary<string, VolatileResourceManager>();
                    Transaction.Current.TransactionCompleted += new TransactionCompletedEventHandler(this.Current_TransactionCompleted);
                }
                return new Interop.CompletedAsyncResult(callback, state);
            }

            private void Current_TransactionCompleted(object sender, TransactionEventArgs e)
            {
                if (e.Transaction.TransactionInformation.Status == TransactionStatus.Committed)
                {
                    foreach (VolatileResourceManager manager in this.CommittedResourceManagers[e.Transaction].Values)
                    {
                        manager.Complete();
                    }
                }
                else
                {
                    foreach (VolatileResourceManager manager2 in this.CommittedResourceManagers[e.Transaction].Values)
                    {
                        manager2.ClearAllBatchedWork();
                    }
                }
                this.CommittedResourceManagers.Remove(e.Transaction);
            }

            protected override void EndOnSave(IAsyncResult result)
            {
                Interop.CompletedAsyncResult.End(result);
            }

            private Dictionary<Transaction, Dictionary<string, VolatileResourceManager>> CommittedResourceManagers { get; set; }

            private Dictionary<string, VolatileResourceManager> ResourceManagers { get; set; }
        }

        private abstract class InteropProperty : PropertyDescriptor
        {
            private bool isValid;
            private Interop owner;

            public InteropProperty(Interop owner, string name, Attribute[] propertyInfoAttributes) : base(name, propertyInfoAttributes)
            {
                this.owner = owner;
                this.isValid = true;
            }

            public override bool CanResetValue(object component)
            {
                this.ThrowIfInvalid();
                return false;
            }

            internal void Invalidate()
            {
                this.isValid = false;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public override void ResetValue(object component)
            {
                this.ThrowIfInvalid();
            }

            public override bool ShouldSerializeValue(object component)
            {
                this.ThrowIfInvalid();
                return false;
            }

            protected void ThrowIfInvalid()
            {
                if (!this.isValid)
                {
                    throw new InvalidOperationException(ExecutionStringManager.InteropInvalidPropertyDescriptor);
                }
            }

            public override Type ComponentType
            {
                get
                {
                    this.ThrowIfInvalid();
                    return this.owner.GetType();
                }
            }

            protected internal Interop Owner
            {
                get
                {
                    return this.owner;
                }
            }
        }

        private enum InteropValidationEnum
        {
            Code,
            Delay,
            InvokeWebService,
            InvokeWorkflow,
            Policy,
            Send,
            SetState,
            WebServiceFault,
            WebServiceInput,
            WebServiceOutput,
            Compensate,
            Suspend,
            ConditionedActivityGroup,
            EventHandlers,
            EventHandlingScope,
            IfElse,
            Listen,
            Parallel,
            Replicator,
            Sequence,
            CompensatableSequence,
            EventDriven,
            IfElseBranch,
            Receive,
            SequentialWorkflow,
            StateFinalization,
            StateInitialization,
            State,
            StateMachineWorkflow,
            While,
            CancellationHandler,
            CompensatableTransactionScope,
            CompensationHandler,
            FaultHandler,
            FaultHandlers,
            SynchronizationScope,
            TransactionScope,
            ICompensatable,
            PersistOnClose,
            Terminate,
            Throw
        }

        private class LiteralProperty : Interop.InteropProperty
        {
            private string literalName;
            private Type literalType;

            public LiteralProperty(Interop owner, string literalName, Type literalType, Attribute[] attributes) : base(owner, literalName, attributes)
            {
                this.literalName = literalName;
                this.literalType = literalType;
            }

            private object GetLiteral()
            {
                object obj2;
                if (base.Owner.ActivityMetaProperties.TryGetValue(this.literalName, out obj2))
                {
                    return obj2;
                }
                return null;
            }

            public override object GetValue(object component)
            {
                base.ThrowIfInvalid();
                return this.GetLiteral();
            }

            public override void SetValue(object component, object value)
            {
                base.ThrowIfInvalid();
                base.Owner.ActivityMetaProperties[this.literalName] = value;
            }

            public override bool IsReadOnly
            {
                get
                {
                    base.ThrowIfInvalid();
                    return false;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    base.ThrowIfInvalid();
                    return this.literalType;
                }
            }
        }

        private class ObtainType : CodeActivity<Type>
        {
            protected override Type Execute(CodeActivityContext context)
            {
                return this.Input.Get(context).GetType();
            }

            public InArgument<System.Activities.Activity> Input { get; set; }
        }

        private class ValidateAtRootAndNestedLevels : NativeActivity
        {
            protected override void Execute(NativeActivityContext context)
            {
                System.Activities.Statements.Interop interop = this.Interop.Get(context);
                foreach (System.Activities.Statements.Interop.InteropValidationEnum enum2 in this.RootLevelValidationData.Get(context))
                {
                    if (enum2 != System.Activities.Statements.Interop.InteropValidationEnum.PersistOnClose)
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropBodyRootLevelViolation, new object[] { interop.DisplayName, enum2.ToString() + "Activity" });
                        Constraint.AddValidationError(context, new System.Activities.Validation.ValidationError(message));
                    }
                }
                foreach (System.Activities.Statements.Interop.InteropValidationEnum enum3 in this.NestedChildrenValidationData.Get(context))
                {
                    if ((enum3 != System.Activities.Statements.Interop.InteropValidationEnum.PersistOnClose) && (enum3 != System.Activities.Statements.Interop.InteropValidationEnum.TransactionScope))
                    {
                        string str2 = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropBodyNestedViolation, new object[] { interop.DisplayName, enum3.ToString() + "Activity" });
                        Constraint.AddValidationError(context, new System.Activities.Validation.ValidationError(str2));
                    }
                }
            }

            public InArgument<System.Activities.Statements.Interop> Interop { get; set; }

            public InArgument<HashSet<System.Activities.Statements.Interop.InteropValidationEnum>> NestedChildrenValidationData { get; set; }

            public InArgument<HashSet<System.Activities.Statements.Interop.InteropValidationEnum>> RootLevelValidationData { get; set; }
        }

        private class WalkInteropBodyAndGatherData : System.Activities.CodeActivity
        {
            protected override void Execute(CodeActivityContext context)
            {
                System.Workflow.ComponentModel.Activity componentModelActivity = this.InteropActivity.Get(context).ComponentModelActivity;
                HashSet<Interop.InteropValidationEnum> validationResults = this.RootLevelValidationData.Get(context);
                this.ProcessAtRootLevel(componentModelActivity, validationResults);
                validationResults = null;
                validationResults = this.NestedChildrenValidationData.Get(context);
                if (componentModelActivity is CompositeActivity)
                {
                    this.ProcessNestedChildren(componentModelActivity, validationResults);
                }
            }

            private void ProcessAtRootLevel(System.Workflow.ComponentModel.Activity interopBody, HashSet<Interop.InteropValidationEnum> validationResults)
            {
                if (interopBody.PersistOnClose)
                {
                    validationResults.Add(Interop.InteropValidationEnum.PersistOnClose);
                }
                Type type = interopBody.GetType();
                if (type == typeof(TransactionScopeActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.TransactionScope);
                }
                else if (type == typeof(System.Workflow.Activities.CodeActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Code);
                }
                else if (type == typeof(DelayActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Delay);
                }
                else if (type == typeof(InvokeWebServiceActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.InvokeWebService);
                }
                else if (type == typeof(InvokeWorkflowActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.InvokeWorkflow);
                }
                else if (type == typeof(PolicyActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Policy);
                }
                else if (type.FullName == "System.Workflow.Activities.SendActivity")
                {
                    validationResults.Add(Interop.InteropValidationEnum.Send);
                }
                else if (type == typeof(SetStateActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.SetState);
                }
                else if (type == typeof(WebServiceFaultActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.WebServiceFault);
                }
                else if (type == typeof(WebServiceInputActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.WebServiceInput);
                }
                else if (type == typeof(WebServiceOutputActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.WebServiceOutput);
                }
                else if (type == typeof(CompensateActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Compensate);
                }
                else if (type == typeof(SuspendActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Suspend);
                }
                else if (type == typeof(TerminateActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Terminate);
                }
                else if (type == typeof(ThrowActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Throw);
                }
                else if (type == typeof(ConditionedActivityGroup))
                {
                    validationResults.Add(Interop.InteropValidationEnum.ConditionedActivityGroup);
                }
                else if (type == typeof(EventHandlersActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.EventHandlers);
                }
                else if (type == typeof(EventHandlingScopeActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.EventHandlingScope);
                }
                else if (type == typeof(IfElseActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.IfElse);
                }
                else if (type == typeof(ListenActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Listen);
                }
                else if (type == typeof(ParallelActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Parallel);
                }
                else if (type == typeof(ReplicatorActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Replicator);
                }
                else if (type == typeof(SequenceActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.Sequence);
                }
                else if (type == typeof(CompensatableSequenceActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.CompensatableSequence);
                }
                else if (type == typeof(EventDrivenActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.EventDriven);
                }
                else if (type == typeof(IfElseBranchActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.IfElseBranch);
                }
                else if (type.FullName == "System.Workflow.Activities.ReceiveActivity")
                {
                    validationResults.Add(Interop.InteropValidationEnum.Receive);
                }
                else if (type == typeof(SequentialWorkflowActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.SequentialWorkflow);
                }
                else if (type == typeof(StateFinalizationActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.StateFinalization);
                }
                else if (type == typeof(StateInitializationActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.StateInitialization);
                }
                else if (type == typeof(StateActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.State);
                }
                else if (type == typeof(StateMachineWorkflowActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.StateMachineWorkflow);
                }
                else if (type == typeof(WhileActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.While);
                }
                else if (type == typeof(CancellationHandlerActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.CancellationHandler);
                }
                else if (type == typeof(CompensatableTransactionScopeActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.CompensatableTransactionScope);
                }
                else if (type == typeof(CompensationHandlerActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.CompensationHandler);
                }
                else if (type == typeof(FaultHandlerActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.FaultHandler);
                }
                else if (type == typeof(FaultHandlersActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.FaultHandlers);
                }
                else if (type == typeof(SynchronizationScopeActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.SynchronizationScope);
                }
                else if (type == typeof(ICompensatableActivity))
                {
                    validationResults.Add(Interop.InteropValidationEnum.ICompensatable);
                }
            }

            private void ProcessNestedChildren(System.Workflow.ComponentModel.Activity interopBody, HashSet<Interop.InteropValidationEnum> validationResults)
            {
                bool flag = false;
                foreach (System.Workflow.ComponentModel.Activity activity in interopBody.CollectNestedActivities())
                {
                    if (activity.PersistOnClose)
                    {
                        flag = true;
                    }
                    if (activity is TransactionScopeActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.TransactionScope);
                    }
                    else if (activity is InvokeWorkflowActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.InvokeWorkflow);
                    }
                    else if (activity.GetType().FullName == "System.Workflow.Activities.SendActivity")
                    {
                        validationResults.Add(Interop.InteropValidationEnum.Send);
                    }
                    else if (activity is WebServiceFaultActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.WebServiceFault);
                    }
                    else if (activity is WebServiceInputActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.WebServiceInput);
                    }
                    else if (activity is WebServiceOutputActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.WebServiceOutput);
                    }
                    else if (activity is CompensateActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.Compensate);
                    }
                    else if (activity is SuspendActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.Suspend);
                    }
                    else if (activity is CompensatableSequenceActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.CompensatableSequence);
                    }
                    else if (activity.GetType().FullName == "System.Workflow.Activities.ReceiveActivity")
                    {
                        validationResults.Add(Interop.InteropValidationEnum.Receive);
                    }
                    else if (activity is CompensatableTransactionScopeActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.CompensatableTransactionScope);
                    }
                    else if (activity is CompensationHandlerActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.CompensationHandler);
                    }
                    else if (activity is ICompensatableActivity)
                    {
                        validationResults.Add(Interop.InteropValidationEnum.ICompensatable);
                    }
                }
                if (flag)
                {
                    validationResults.Add(Interop.InteropValidationEnum.PersistOnClose);
                }
            }

            public InArgument<Interop> InteropActivity { get; set; }

            public InArgument<HashSet<Interop.InteropValidationEnum>> NestedChildrenValidationData { get; set; }

            public InArgument<HashSet<Interop.InteropValidationEnum>> RootLevelValidationData { get; set; }
        }
    }
}

