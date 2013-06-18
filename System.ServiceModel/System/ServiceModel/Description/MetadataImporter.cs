namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Threading;
    using System.Xml;

    public abstract class MetadataImporter
    {
        private readonly Collection<MetadataConversionError> errors;
        private readonly Dictionary<XmlQualifiedName, ContractDescription> knownContracts;
        private readonly KeyedByTypeCollection<IPolicyImportExtension> policyExtensions;
        private PolicyReader policyNormalizer;
        internal MetadataImporterQuotas Quotas;
        private readonly Dictionary<object, object> state;

        internal event PolicyWarningHandler PolicyWarningOccured;

        internal MetadataImporter() : this(null, MetadataImporterQuotas.Defaults)
        {
        }

        internal MetadataImporter(IEnumerable<IPolicyImportExtension> policyImportExtensions) : this(policyImportExtensions, MetadataImporterQuotas.Defaults)
        {
        }

        internal MetadataImporter(IEnumerable<IPolicyImportExtension> policyImportExtensions, MetadataImporterQuotas quotas)
        {
            this.knownContracts = new Dictionary<XmlQualifiedName, ContractDescription>();
            this.errors = new Collection<MetadataConversionError>();
            this.state = new Dictionary<object, object>();
            if (quotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("quotas");
            }
            if (policyImportExtensions == null)
            {
                policyImportExtensions = LoadPolicyExtensionsFromConfig();
            }
            this.Quotas = quotas;
            this.policyExtensions = new KeyedByTypeCollection<IPolicyImportExtension>(policyImportExtensions);
        }

        private Exception CreateExtensionException(IPolicyImportExtension importer, Exception e)
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("PolicyExtensionImportError", new object[] { importer.GetType(), e.Message }), e);
        }

        internal static IEnumerable<PolicyConversionContext> GetPolicyConversionContextEnumerator(ServiceEndpoint endpoint, PolicyAlternatives policyAlternatives)
        {
            return ImportedPolicyConversionContext.GetPolicyConversionContextEnumerator(endpoint, policyAlternatives, MetadataImporterQuotas.Defaults);
        }

        internal static IEnumerable<PolicyConversionContext> GetPolicyConversionContextEnumerator(ServiceEndpoint endpoint, PolicyAlternatives policyAlternatives, MetadataImporterQuotas quotas)
        {
            return ImportedPolicyConversionContext.GetPolicyConversionContextEnumerator(endpoint, policyAlternatives, quotas);
        }

        public abstract Collection<ContractDescription> ImportAllContracts();
        public abstract ServiceEndpointCollection ImportAllEndpoints();
        internal BindingElementCollection ImportPolicy(ServiceEndpoint endpoint, Collection<Collection<XmlElement>> policyAlternatives)
        {
            foreach (Collection<XmlElement> collection in policyAlternatives)
            {
                BindingOnlyPolicyConversionContext policyContext = new BindingOnlyPolicyConversionContext(endpoint, collection);
                if (this.TryImportPolicy(policyContext))
                {
                    return policyContext.BindingElements;
                }
            }
            return null;
        }

        [SecuritySafeCritical]
        private static Collection<IPolicyImportExtension> LoadPolicyExtensionsFromConfig()
        {
            return ClientSection.UnsafeGetSection().Metadata.LoadPolicyImportExtensions();
        }

        internal IEnumerable<IEnumerable<XmlElement>> NormalizePolicy(IEnumerable<XmlElement> policyAssertions)
        {
            if (this.policyNormalizer == null)
            {
                this.policyNormalizer = new PolicyReader(this);
            }
            return this.policyNormalizer.NormalizePolicy(policyAssertions);
        }

        internal virtual XmlElement ResolvePolicyReference(string policyReference, XmlElement contextAssertion)
        {
            return null;
        }

        internal bool TryImportPolicy(PolicyConversionContext policyContext)
        {
            foreach (IPolicyImportExtension extension in this.policyExtensions)
            {
                try
                {
                    extension.ImportPolicy(this, policyContext);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreateExtensionException(extension, exception));
                }
            }
            if (policyContext.GetBindingAssertions().Count != 0)
            {
                return false;
            }
            foreach (OperationDescription description in policyContext.Contract.Operations)
            {
                if (policyContext.GetOperationBindingAssertions(description).Count != 0)
                {
                    return false;
                }
                foreach (MessageDescription description2 in description.Messages)
                {
                    if (policyContext.GetMessageBindingAssertions(description2).Count != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Collection<MetadataConversionError> Errors
        {
            get
            {
                return this.errors;
            }
        }

        public Dictionary<XmlQualifiedName, ContractDescription> KnownContracts
        {
            get
            {
                return this.knownContracts;
            }
        }

        public KeyedByTypeCollection<IPolicyImportExtension> PolicyImportExtensions
        {
            get
            {
                return this.policyExtensions;
            }
        }

        public Dictionary<object, object> State
        {
            get
            {
                return this.state;
            }
        }

        internal class BindingOnlyPolicyConversionContext : PolicyConversionContext
        {
            private readonly BindingElementCollection bindingElements;
            private readonly PolicyAssertionCollection bindingPolicy;
            private static readonly PolicyAssertionCollection noPolicy = new PolicyAssertionCollection();

            internal BindingOnlyPolicyConversionContext(ServiceEndpoint endpoint, IEnumerable<XmlElement> bindingPolicy) : base(endpoint)
            {
                this.bindingElements = new BindingElementCollection();
                this.bindingPolicy = new PolicyAssertionCollection(bindingPolicy);
            }

            public override PolicyAssertionCollection GetBindingAssertions()
            {
                return this.bindingPolicy;
            }

            public override PolicyAssertionCollection GetFaultBindingAssertions(FaultDescription fault)
            {
                return noPolicy;
            }

            public override PolicyAssertionCollection GetMessageBindingAssertions(MessageDescription message)
            {
                return noPolicy;
            }

            public override PolicyAssertionCollection GetOperationBindingAssertions(OperationDescription operation)
            {
                return noPolicy;
            }

            public override BindingElementCollection BindingElements
            {
                get
                {
                    return this.bindingElements;
                }
            }
        }

        internal sealed class ImportedPolicyConversionContext : PolicyConversionContext
        {
            private BindingElementCollection bindingElements;
            private readonly PolicyAssertionCollection endpointAssertions;
            private readonly Dictionary<FaultDescription, PolicyAssertionCollection> faultBindingAssertions;
            private readonly Dictionary<MessageDescription, PolicyAssertionCollection> messageBindingAssertions;
            private readonly Dictionary<OperationDescription, PolicyAssertionCollection> operationBindingAssertions;

            private ImportedPolicyConversionContext(ServiceEndpoint endpoint, IEnumerable<XmlElement> endpointAssertions, Dictionary<OperationDescription, IEnumerable<XmlElement>> operationBindingAssertions, Dictionary<MessageDescription, IEnumerable<XmlElement>> messageBindingAssertions, Dictionary<FaultDescription, IEnumerable<XmlElement>> faultBindingAssertions, MetadataImporterQuotas quotas) : base(endpoint)
            {
                this.bindingElements = new BindingElementCollection();
                this.operationBindingAssertions = new Dictionary<OperationDescription, PolicyAssertionCollection>();
                this.messageBindingAssertions = new Dictionary<MessageDescription, PolicyAssertionCollection>();
                this.faultBindingAssertions = new Dictionary<FaultDescription, PolicyAssertionCollection>();
                int maxPolicyAssertions = quotas.MaxPolicyAssertions;
                this.endpointAssertions = new PolicyAssertionCollection(new MaxItemsEnumerable<XmlElement>(endpointAssertions, maxPolicyAssertions));
                maxPolicyAssertions -= this.endpointAssertions.Count;
                foreach (OperationDescription description in endpoint.Contract.Operations)
                {
                    this.operationBindingAssertions.Add(description, new PolicyAssertionCollection());
                    foreach (MessageDescription description2 in description.Messages)
                    {
                        this.messageBindingAssertions.Add(description2, new PolicyAssertionCollection());
                    }
                    foreach (FaultDescription description3 in description.Faults)
                    {
                        this.faultBindingAssertions.Add(description3, new PolicyAssertionCollection());
                    }
                }
                foreach (KeyValuePair<OperationDescription, IEnumerable<XmlElement>> pair in operationBindingAssertions)
                {
                    this.operationBindingAssertions[pair.Key].AddRange(new MaxItemsEnumerable<XmlElement>(pair.Value, maxPolicyAssertions));
                    maxPolicyAssertions -= this.operationBindingAssertions[pair.Key].Count;
                }
                foreach (KeyValuePair<MessageDescription, IEnumerable<XmlElement>> pair2 in messageBindingAssertions)
                {
                    this.messageBindingAssertions[pair2.Key].AddRange(new MaxItemsEnumerable<XmlElement>(pair2.Value, maxPolicyAssertions));
                    maxPolicyAssertions -= this.messageBindingAssertions[pair2.Key].Count;
                }
                foreach (KeyValuePair<FaultDescription, IEnumerable<XmlElement>> pair3 in faultBindingAssertions)
                {
                    this.faultBindingAssertions[pair3.Key].AddRange(new MaxItemsEnumerable<XmlElement>(pair3.Value, maxPolicyAssertions));
                    maxPolicyAssertions -= this.faultBindingAssertions[pair3.Key].Count;
                }
            }

            public override PolicyAssertionCollection GetBindingAssertions()
            {
                return this.endpointAssertions;
            }

            public override PolicyAssertionCollection GetFaultBindingAssertions(FaultDescription message)
            {
                return this.faultBindingAssertions[message];
            }

            public override PolicyAssertionCollection GetMessageBindingAssertions(MessageDescription message)
            {
                return this.messageBindingAssertions[message];
            }

            public override PolicyAssertionCollection GetOperationBindingAssertions(OperationDescription operation)
            {
                return this.operationBindingAssertions[operation];
            }

            public static IEnumerable<PolicyConversionContext> GetPolicyConversionContextEnumerator(ServiceEndpoint endpoint, MetadataImporter.PolicyAlternatives policyAlternatives, MetadataImporterQuotas quotas)
            {
                IEnumerable<Dictionary<FaultDescription, IEnumerable<XmlElement>>> cartesianProduct = PolicyIterationHelper.GetCartesianProduct<FaultDescription, IEnumerable<XmlElement>>(policyAlternatives.FaultBindingAlternatives);
                IEnumerable<Dictionary<MessageDescription, IEnumerable<XmlElement>>> iteratorVariable0 = PolicyIterationHelper.GetCartesianProduct<MessageDescription, IEnumerable<XmlElement>>(policyAlternatives.MessageBindingAlternatives);
                IEnumerable<Dictionary<OperationDescription, IEnumerable<XmlElement>>> iteratorVariable2 = PolicyIterationHelper.GetCartesianProduct<OperationDescription, IEnumerable<XmlElement>>(policyAlternatives.OperationBindingAlternatives);
                foreach (Dictionary<FaultDescription, IEnumerable<XmlElement>> iteratorVariable3 in cartesianProduct)
                {
                    foreach (Dictionary<MessageDescription, IEnumerable<XmlElement>> iteratorVariable4 in iteratorVariable0)
                    {
                        foreach (Dictionary<OperationDescription, IEnumerable<XmlElement>> iteratorVariable5 in iteratorVariable2)
                        {
                            foreach (IEnumerable<XmlElement> iteratorVariable6 in policyAlternatives.EndpointAlternatives)
                            {
                                MetadataImporter.ImportedPolicyConversionContext iteratorVariable7;
                                try
                                {
                                    iteratorVariable7 = new MetadataImporter.ImportedPolicyConversionContext(endpoint, iteratorVariable6, iteratorVariable5, iteratorVariable4, iteratorVariable3, quotas);
                                }
                                catch (MaxItemsEnumeratorExceededMaxItemsException)
                                {
                                    break;
                                }
                                yield return iteratorVariable7;
                            }
                        }
                    }
                }
            }

            public override BindingElementCollection BindingElements
            {
                get
                {
                    return this.bindingElements;
                }
            }


            internal class MaxItemsEnumerable<T> : IEnumerable<T>, IEnumerable
            {
                private IEnumerable<T> inner;
                private int maxItems;

                public MaxItemsEnumerable(IEnumerable<T> inner, int maxItems)
                {
                    this.inner = inner;
                    this.maxItems = maxItems;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    return new MetadataImporter.ImportedPolicyConversionContext.MaxItemsEnumerator<T>(this.inner.GetEnumerator(), this.maxItems);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }
            }

            internal class MaxItemsEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
            {
                private int currentItem;
                private IEnumerator<T> inner;
                private int maxItems;

                public MaxItemsEnumerator(IEnumerator<T> inner, int maxItems)
                {
                    this.maxItems = maxItems;
                    this.currentItem = 0;
                    this.inner = inner;
                }

                public void Dispose()
                {
                    this.inner.Dispose();
                }

                public bool MoveNext()
                {
                    bool flag = this.inner.MoveNext();
                    if (++this.currentItem > this.maxItems)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MetadataImporter.ImportedPolicyConversionContext.MaxItemsEnumeratorExceededMaxItemsException());
                    }
                    return flag;
                }

                public void Reset()
                {
                    this.currentItem = 0;
                    this.inner.Reset();
                }

                public T Current
                {
                    get
                    {
                        return this.inner.Current;
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return this.inner.Current;
                    }
                }
            }

            internal class MaxItemsEnumeratorExceededMaxItemsException : Exception
            {
            }

            private static class PolicyIterationHelper
            {
                internal static IEnumerable<Dictionary<K, V>> GetCartesianProduct<K, V>(Dictionary<K, IEnumerable<V>> sets)
                {
                    Dictionary<K, V> counterValue = new Dictionary<K, V>(sets.Count);
                    KeyValuePair<K, IEnumerator<V>>[] digits = InitializeCounter<K, V>(sets, counterValue);
                    do
                    {
                        yield return counterValue;
                    }
                    while (IncrementCounter<K, V>(digits, sets, counterValue));
                }

                private static bool IncrementCounter<K, V>(KeyValuePair<K, IEnumerator<V>>[] digits, Dictionary<K, IEnumerable<V>> sets, Dictionary<K, V> counterValue)
                {
                    int index = 0;
                    while ((index < digits.Length) && !digits[index].Value.MoveNext())
                    {
                        IEnumerator<V> enumerator = sets[digits[index].Key].GetEnumerator();
                        digits[index] = new KeyValuePair<K, IEnumerator<V>>(digits[index].Key, enumerator);
                        digits[index].Value.MoveNext();
                        index++;
                    }
                    if (index == digits.Length)
                    {
                        return false;
                    }
                    for (int i = index; i >= 0; i--)
                    {
                        counterValue[digits[i].Key] = digits[i].Value.Current;
                    }
                    return true;
                }

                private static KeyValuePair<K, IEnumerator<V>>[] InitializeCounter<K, V>(Dictionary<K, IEnumerable<V>> sets, Dictionary<K, V> counterValue)
                {
                    KeyValuePair<K, IEnumerator<V>>[] pairArray = new KeyValuePair<K, IEnumerator<V>>[sets.Count];
                    int index = 0;
                    foreach (KeyValuePair<K, IEnumerable<V>> pair in sets)
                    {
                        pairArray[index] = new KeyValuePair<K, IEnumerator<V>>(pair.Key, pair.Value.GetEnumerator());
                        if (!pairArray[index].Value.MoveNext())
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Each set must have at least one item in it", new object[0])));
                        }
                        counterValue[pairArray[index].Key] = pairArray[index].Value.Current;
                        index++;
                    }
                    return pairArray;
                }

                [CompilerGenerated]
                private sealed class <GetCartesianProduct>d__13<K, V> : IEnumerable<Dictionary<K, V>>, IEnumerable, IEnumerator<Dictionary<K, V>>, IEnumerator, IDisposable
                {
                    private int <>1__state;
                    private Dictionary<K, V> <>2__current;
                    public Dictionary<K, IEnumerable<V>> <>3__sets;
                    private int <>l__initialThreadId;
                    public Dictionary<K, V> <counterValue>5__14;
                    public KeyValuePair<K, IEnumerator<V>>[] <digits>5__15;
                    public Dictionary<K, IEnumerable<V>> sets;

                    [DebuggerHidden]
                    public <GetCartesianProduct>d__13(int <>1__state)
                    {
                        this.<>1__state = <>1__state;
                        this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
                    }

                    private bool MoveNext()
                    {
                        switch (this.<>1__state)
                        {
                            case 0:
                                this.<>1__state = -1;
                                this.<counterValue>5__14 = new Dictionary<K, V>(this.sets.Count);
                                this.<digits>5__15 = MetadataImporter.ImportedPolicyConversionContext.PolicyIterationHelper.InitializeCounter<K, V>(this.sets, this.<counterValue>5__14);
                                break;

                            case 1:
                                this.<>1__state = -1;
                                if (MetadataImporter.ImportedPolicyConversionContext.PolicyIterationHelper.IncrementCounter<K, V>(this.<digits>5__15, this.sets, this.<counterValue>5__14))
                                {
                                    break;
                                }
                                goto Label_0080;

                            default:
                                goto Label_0080;
                        }
                        this.<>2__current = this.<counterValue>5__14;
                        this.<>1__state = 1;
                        return true;
                    Label_0080:
                        return false;
                    }

                    [DebuggerHidden]
                    IEnumerator<Dictionary<K, V>> IEnumerable<Dictionary<K, V>>.GetEnumerator()
                    {
                        MetadataImporter.ImportedPolicyConversionContext.PolicyIterationHelper.<GetCartesianProduct>d__13<K, V> d__;
                        if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                        {
                            this.<>1__state = 0;
                            d__ = (MetadataImporter.ImportedPolicyConversionContext.PolicyIterationHelper.<GetCartesianProduct>d__13<K, V>) this;
                        }
                        else
                        {
                            d__ = new MetadataImporter.ImportedPolicyConversionContext.PolicyIterationHelper.<GetCartesianProduct>d__13<K, V>(0);
                        }
                        d__.sets = this.<>3__sets;
                        return d__;
                    }

                    [DebuggerHidden]
                    IEnumerator IEnumerable.GetEnumerator()
                    {
                        return this.System.Collections.Generic.IEnumerable<System.Collections.Generic.Dictionary<K,V>>.GetEnumerator();
                    }

                    [DebuggerHidden]
                    void IEnumerator.Reset()
                    {
                        throw new NotSupportedException();
                    }

                    void IDisposable.Dispose()
                    {
                    }

                    Dictionary<K, V> IEnumerator<Dictionary<K, V>>.Current
                    {
                        [DebuggerHidden]
                        get
                        {
                            return this.<>2__current;
                        }
                    }

                    object IEnumerator.Current
                    {
                        [DebuggerHidden]
                        get
                        {
                            return this.<>2__current;
                        }
                    }
                }
            }
        }

        internal class PolicyAlternatives
        {
            public IEnumerable<IEnumerable<XmlElement>> EndpointAlternatives;
            public Dictionary<FaultDescription, IEnumerable<IEnumerable<XmlElement>>> FaultBindingAlternatives;
            public Dictionary<MessageDescription, IEnumerable<IEnumerable<XmlElement>>> MessageBindingAlternatives;
            public Dictionary<OperationDescription, IEnumerable<IEnumerable<XmlElement>>> OperationBindingAlternatives;
        }

        internal static class PolicyHelper
        {
            private static IEnumerable<IEnumerable<T>> AtLeastOne<T>(IEnumerable<IEnumerable<T>> xs, MetadataImporter.YieldLimiter yieldLimiter)
            {
                bool iteratorVariable0 = false;
                foreach (IEnumerable<T> iteratorVariable1 in xs)
                {
                    iteratorVariable0 = true;
                    if (yieldLimiter.IncrementAndLogIfExceededLimit())
                    {
                        goto Label_00DA;
                    }
                    yield return iteratorVariable1;
                }
                if (!iteratorVariable0 && !yieldLimiter.IncrementAndLogIfExceededLimit())
                {
                    yield return new EmptyEnumerable<T>();
                }
            Label_00DA:
                yield break;
            }

            internal static IEnumerable<IEnumerable<T>> CrossProduct<T>(IEnumerable<IEnumerable<T>> xs, IEnumerable<IEnumerable<T>> ys, MetadataImporter.YieldLimiter yieldLimiter)
            {
                foreach (IEnumerable<T> iteratorVariable0 in AtLeastOne<T>(xs, yieldLimiter))
                {
                    foreach (IEnumerable<T> iteratorVariable1 in AtLeastOne<T>(ys, yieldLimiter))
                    {
                        if (yieldLimiter.IncrementAndLogIfExceededLimit())
                        {
                            break;
                        }
                        yield return Merge<T>(iteratorVariable0, iteratorVariable1, yieldLimiter);
                    }
                }
            }

            internal static string GetFragmentIdentifier(XmlElement element)
            {
                string attribute = element.GetAttribute("Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                if (attribute == null)
                {
                    attribute = element.GetAttribute("id", "http://www.w3.org/XML/1998/namespace");
                }
                if (string.IsNullOrEmpty(attribute))
                {
                    return string.Empty;
                }
                return string.Format(CultureInfo.InvariantCulture, "#{0}", new object[] { attribute });
            }

            internal static NodeType GetNodeType(System.Xml.XmlNode node)
            {
                XmlElement element = node as XmlElement;
                if (element == null)
                {
                    return NodeType.NonElement;
                }
                if ((element.NamespaceURI != "http://schemas.xmlsoap.org/ws/2004/09/policy") && (element.NamespaceURI != "http://www.w3.org/ns/ws-policy"))
                {
                    return NodeType.Assertion;
                }
                if (element.LocalName == "Policy")
                {
                    return NodeType.Policy;
                }
                if (element.LocalName == "All")
                {
                    return NodeType.All;
                }
                if (element.LocalName == "ExactlyOne")
                {
                    return NodeType.ExactlyOne;
                }
                if (element.LocalName == "PolicyReference")
                {
                    return NodeType.PolicyReference;
                }
                return NodeType.UnrecognizedWSPolicy;
            }

            internal static bool IsPolicyURIs(System.Xml.XmlAttribute attribute)
            {
                if (!(attribute.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/policy") && !(attribute.NamespaceURI == "http://www.w3.org/ns/ws-policy"))
                {
                    return false;
                }
                return (attribute.LocalName == "PolicyURIs");
            }

            private static IEnumerable<T> Merge<T>(IEnumerable<T> e1, IEnumerable<T> e2, MetadataImporter.YieldLimiter yieldLimiter)
            {
                foreach (T iteratorVariable0 in e1)
                {
                    if (yieldLimiter.IncrementAndLogIfExceededLimit())
                    {
                        goto Label_0115;
                    }
                    yield return iteratorVariable0;
                }
                foreach (T iteratorVariable1 in e2)
                {
                    if (yieldLimiter.IncrementAndLogIfExceededLimit())
                    {
                        break;
                    }
                    yield return iteratorVariable1;
                }
            Label_0115:
                yield break;
            }

            [CompilerGenerated]
            private sealed class <AtLeastOne>d__2e<T> : IEnumerable<IEnumerable<T>>, IEnumerable, IEnumerator<IEnumerable<T>>, IEnumerator, IDisposable
            {
                private int <>1__state;
                private IEnumerable<T> <>2__current;
                public IEnumerable<IEnumerable<T>> <>3__xs;
                public MetadataImporter.YieldLimiter <>3__yieldLimiter;
                public IEnumerator<IEnumerable<T>> <>7__wrap31;
                private int <>l__initialThreadId;
                public bool <gotOne>5__2f;
                public IEnumerable<T> <x>5__30;
                public IEnumerable<IEnumerable<T>> xs;
                public MetadataImporter.YieldLimiter yieldLimiter;

                [DebuggerHidden]
                public <AtLeastOne>d__2e(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                    this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
                }

                private void <>m__Finally32()
                {
                    this.<>1__state = -1;
                    if (this.<>7__wrap31 != null)
                    {
                        this.<>7__wrap31.Dispose();
                    }
                }

                private bool MoveNext()
                {
                    bool flag;
                    try
                    {
                        switch (this.<>1__state)
                        {
                            case 0:
                                this.<>1__state = -1;
                                this.<gotOne>5__2f = false;
                                this.<>7__wrap31 = this.xs.GetEnumerator();
                                this.<>1__state = 1;
                                goto Label_0095;

                            case 2:
                                this.<>1__state = 1;
                                goto Label_0095;

                            case 3:
                                this.<>1__state = -1;
                                goto Label_00DA;

                            default:
                                goto Label_00DA;
                        }
                    Label_004A:
                        this.<x>5__30 = this.<>7__wrap31.Current;
                        this.<gotOne>5__2f = true;
                        if (this.yieldLimiter.IncrementAndLogIfExceededLimit())
                        {
                            this.System.IDisposable.Dispose();
                            goto Label_00DA;
                        }
                        this.<>2__current = this.<x>5__30;
                        this.<>1__state = 2;
                        return true;
                    Label_0095:
                        if (this.<>7__wrap31.MoveNext())
                        {
                            goto Label_004A;
                        }
                        this.<>m__Finally32();
                        if (!this.<gotOne>5__2f && !this.yieldLimiter.IncrementAndLogIfExceededLimit())
                        {
                            this.<>2__current = new MetadataImporter.PolicyHelper.EmptyEnumerable<T>();
                            this.<>1__state = 3;
                            return true;
                        }
                    Label_00DA:
                        flag = false;
                    }
                    fault
                    {
                        this.System.IDisposable.Dispose();
                    }
                    return flag;
                }

                [DebuggerHidden]
                IEnumerator<IEnumerable<T>> IEnumerable<IEnumerable<T>>.GetEnumerator()
                {
                    MetadataImporter.PolicyHelper.<AtLeastOne>d__2e<T> d__e;
                    if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                    {
                        this.<>1__state = 0;
                        d__e = (MetadataImporter.PolicyHelper.<AtLeastOne>d__2e<T>) this;
                    }
                    else
                    {
                        d__e = new MetadataImporter.PolicyHelper.<AtLeastOne>d__2e<T>(0);
                    }
                    d__e.xs = this.<>3__xs;
                    d__e.yieldLimiter = this.<>3__yieldLimiter;
                    return d__e;
                }

                [DebuggerHidden]
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>>.GetEnumerator();
                }

                [DebuggerHidden]
                void IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }

                void IDisposable.Dispose()
                {
                    switch (this.<>1__state)
                    {
                        case 1:
                        case 2:
                            try
                            {
                            }
                            finally
                            {
                                this.<>m__Finally32();
                            }
                            return;
                    }
                }

                IEnumerable<T> IEnumerator<IEnumerable<T>>.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }

                object IEnumerator.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }
            }

            [CompilerGenerated]
            private sealed class <CrossProduct>d__25<T> : IEnumerable<IEnumerable<T>>, IEnumerable, IEnumerator<IEnumerable<T>>, IEnumerator, IDisposable
            {
                private int <>1__state;
                private IEnumerable<T> <>2__current;
                public IEnumerable<IEnumerable<T>> <>3__xs;
                public MetadataImporter.YieldLimiter <>3__yieldLimiter;
                public IEnumerable<IEnumerable<T>> <>3__ys;
                public IEnumerator<IEnumerable<T>> <>7__wrap28;
                public IEnumerator<IEnumerable<T>> <>7__wrap2a;
                private int <>l__initialThreadId;
                public IEnumerable<T> <x>5__26;
                public IEnumerable<T> <y>5__27;
                public IEnumerable<IEnumerable<T>> xs;
                public MetadataImporter.YieldLimiter yieldLimiter;
                public IEnumerable<IEnumerable<T>> ys;

                [DebuggerHidden]
                public <CrossProduct>d__25(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                    this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
                }

                private void <>m__Finally29()
                {
                    this.<>1__state = -1;
                    if (this.<>7__wrap28 != null)
                    {
                        this.<>7__wrap28.Dispose();
                    }
                }

                private void <>m__Finally2b()
                {
                    this.<>1__state = 1;
                    if (this.<>7__wrap2a != null)
                    {
                        this.<>7__wrap2a.Dispose();
                    }
                }

                private bool MoveNext()
                {
                    try
                    {
                        switch (this.<>1__state)
                        {
                            case 0:
                                this.<>1__state = -1;
                                this.<>7__wrap28 = MetadataImporter.PolicyHelper.AtLeastOne<T>(this.xs, this.yieldLimiter).GetEnumerator();
                                this.<>1__state = 1;
                                while (this.<>7__wrap28.MoveNext())
                                {
                                    this.<x>5__26 = this.<>7__wrap28.Current;
                                    this.<>7__wrap2a = MetadataImporter.PolicyHelper.AtLeastOne<T>(this.ys, this.yieldLimiter).GetEnumerator();
                                    this.<>1__state = 2;
                                    while (this.<>7__wrap2a.MoveNext())
                                    {
                                        this.<y>5__27 = this.<>7__wrap2a.Current;
                                        if (this.yieldLimiter.IncrementAndLogIfExceededLimit())
                                        {
                                            this.System.IDisposable.Dispose();
                                            break;
                                        }
                                        this.<>2__current = MetadataImporter.PolicyHelper.Merge<T>(this.<x>5__26, this.<y>5__27, this.yieldLimiter);
                                        this.<>1__state = 3;
                                        return true;
                                    Label_00CA:
                                        this.<>1__state = 2;
                                    }
                                    this.<>m__Finally2b();
                                }
                                this.<>m__Finally29();
                                break;

                            case 3:
                                goto Label_00CA;
                        }
                        return false;
                    }
                    fault
                    {
                        this.System.IDisposable.Dispose();
                    }
                }

                [DebuggerHidden]
                IEnumerator<IEnumerable<T>> IEnumerable<IEnumerable<T>>.GetEnumerator()
                {
                    MetadataImporter.PolicyHelper.<CrossProduct>d__25<T> d__;
                    if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                    {
                        this.<>1__state = 0;
                        d__ = (MetadataImporter.PolicyHelper.<CrossProduct>d__25<T>) this;
                    }
                    else
                    {
                        d__ = new MetadataImporter.PolicyHelper.<CrossProduct>d__25<T>(0);
                    }
                    d__.xs = this.<>3__xs;
                    d__.ys = this.<>3__ys;
                    d__.yieldLimiter = this.<>3__yieldLimiter;
                    return d__;
                }

                [DebuggerHidden]
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>>.GetEnumerator();
                }

                [DebuggerHidden]
                void IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }

                void IDisposable.Dispose()
                {
                    switch (this.<>1__state)
                    {
                        case 1:
                        case 2:
                        case 3:
                            try
                            {
                                switch (this.<>1__state)
                                {
                                    case 2:
                                    case 3:
                                        try
                                        {
                                        }
                                        finally
                                        {
                                            this.<>m__Finally2b();
                                        }
                                        return;
                                }
                            }
                            finally
                            {
                                this.<>m__Finally29();
                            }
                            break;

                        default:
                            return;
                    }
                }

                IEnumerable<T> IEnumerator<IEnumerable<T>>.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }

                object IEnumerator.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }
            }

            [CompilerGenerated]
            private sealed class <Merge>d__35<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
            {
                private int <>1__state;
                private T <>2__current;
                public IEnumerable<T> <>3__e1;
                public IEnumerable<T> <>3__e2;
                public MetadataImporter.YieldLimiter <>3__yieldLimiter;
                public IEnumerator<T> <>7__wrap38;
                public IEnumerator<T> <>7__wrap3a;
                private int <>l__initialThreadId;
                public T <t1>5__36;
                public T <t2>5__37;
                public IEnumerable<T> e1;
                public IEnumerable<T> e2;
                public MetadataImporter.YieldLimiter yieldLimiter;

                [DebuggerHidden]
                public <Merge>d__35(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                    this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
                }

                private void <>m__Finally39()
                {
                    this.<>1__state = -1;
                    if (this.<>7__wrap38 != null)
                    {
                        this.<>7__wrap38.Dispose();
                    }
                }

                private void <>m__Finally3b()
                {
                    this.<>1__state = -1;
                    if (this.<>7__wrap3a != null)
                    {
                        this.<>7__wrap3a.Dispose();
                    }
                }

                private bool MoveNext()
                {
                    bool flag;
                    try
                    {
                        switch (this.<>1__state)
                        {
                            case 0:
                                this.<>1__state = -1;
                                this.<>7__wrap38 = this.e1.GetEnumerator();
                                this.<>1__state = 1;
                                goto Label_0091;

                            case 2:
                                this.<>1__state = 1;
                                goto Label_0091;

                            case 4:
                                goto Label_00FB;

                            default:
                                goto Label_0115;
                        }
                    Label_0047:
                        this.<t1>5__36 = this.<>7__wrap38.Current;
                        if (this.yieldLimiter.IncrementAndLogIfExceededLimit())
                        {
                            this.System.IDisposable.Dispose();
                            goto Label_0115;
                        }
                        this.<>2__current = this.<t1>5__36;
                        this.<>1__state = 2;
                        return true;
                    Label_0091:
                        if (this.<>7__wrap38.MoveNext())
                        {
                            goto Label_0047;
                        }
                        this.<>m__Finally39();
                        this.<>7__wrap3a = this.e2.GetEnumerator();
                        this.<>1__state = 3;
                        while (this.<>7__wrap3a.MoveNext())
                        {
                            this.<t2>5__37 = this.<>7__wrap3a.Current;
                            if (this.yieldLimiter.IncrementAndLogIfExceededLimit())
                            {
                                this.System.IDisposable.Dispose();
                                goto Label_0115;
                            }
                            this.<>2__current = this.<t2>5__37;
                            this.<>1__state = 4;
                            return true;
                        Label_00FB:
                            this.<>1__state = 3;
                        }
                        this.<>m__Finally3b();
                    Label_0115:
                        flag = false;
                    }
                    fault
                    {
                        this.System.IDisposable.Dispose();
                    }
                    return flag;
                }

                [DebuggerHidden]
                IEnumerator<T> IEnumerable<T>.GetEnumerator()
                {
                    MetadataImporter.PolicyHelper.<Merge>d__35<T> d__;
                    if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                    {
                        this.<>1__state = 0;
                        d__ = (MetadataImporter.PolicyHelper.<Merge>d__35<T>) this;
                    }
                    else
                    {
                        d__ = new MetadataImporter.PolicyHelper.<Merge>d__35<T>(0);
                    }
                    d__.e1 = this.<>3__e1;
                    d__.e2 = this.<>3__e2;
                    d__.yieldLimiter = this.<>3__yieldLimiter;
                    return d__;
                }

                [DebuggerHidden]
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
                }

                [DebuggerHidden]
                void IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }

                void IDisposable.Dispose()
                {
                    switch (this.<>1__state)
                    {
                        case 1:
                        case 2:
                            try
                            {
                            }
                            finally
                            {
                                this.<>m__Finally39();
                            }
                            break;

                        case 3:
                        case 4:
                            try
                            {
                            }
                            finally
                            {
                                this.<>m__Finally3b();
                            }
                            return;
                    }
                }

                T IEnumerator<T>.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }

                object IEnumerator.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }
            }

            internal class EmptyEnumerable<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IDisposable, IEnumerator
            {
                public void Dispose()
                {
                }

                public IEnumerator<T> GetEnumerator()
                {
                    return this;
                }

                public bool MoveNext()
                {
                    return false;
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                void IEnumerator.Reset()
                {
                }

                public T Current
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoValue0")));
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return this.Current;
                    }
                }
            }

            internal enum NodeType
            {
                NonElement,
                Policy,
                All,
                ExactlyOne,
                Assertion,
                PolicyReference,
                UnrecognizedWSPolicy
            }

            internal class SingleEnumerable<T> : IEnumerable<T>, IEnumerable
            {
                private T value;

                internal SingleEnumerable(T value)
                {
                    this.value = value;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    yield return this.value;
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                [CompilerGenerated]
                private sealed class <GetEnumerator>d__3e : IEnumerator<T>, IEnumerator, IDisposable
                {
                    private int <>1__state;
                    private T <>2__current;
                    public MetadataImporter.PolicyHelper.SingleEnumerable<T> <>4__this;

                    [DebuggerHidden]
                    public <GetEnumerator>d__3e(int <>1__state)
                    {
                        this.<>1__state = <>1__state;
                    }

                    private bool MoveNext()
                    {
                        switch (this.<>1__state)
                        {
                            case 0:
                                this.<>1__state = -1;
                                this.<>2__current = this.<>4__this.value;
                                this.<>1__state = 1;
                                return true;

                            case 1:
                                this.<>1__state = -1;
                                break;
                        }
                        return false;
                    }

                    [DebuggerHidden]
                    void IEnumerator.Reset()
                    {
                        throw new NotSupportedException();
                    }

                    void IDisposable.Dispose()
                    {
                    }

                    T IEnumerator<T>.Current
                    {
                        [DebuggerHidden]
                        get
                        {
                            return this.<>2__current;
                        }
                    }

                    object IEnumerator.Current
                    {
                        [DebuggerHidden]
                        get
                        {
                            return this.<>2__current;
                        }
                    }
                }
            }
        }

        private sealed class PolicyReader
        {
            private static IEnumerable<XmlElement> Empty = new MetadataImporter.PolicyHelper.EmptyEnumerable<XmlElement>();
            private static IEnumerable<IEnumerable<XmlElement>> EmptyEmpty = new MetadataImporter.PolicyHelper.SingleEnumerable<IEnumerable<XmlElement>>(new MetadataImporter.PolicyHelper.EmptyEnumerable<XmlElement>());
            private readonly MetadataImporter metadataImporter;
            private int nodesRead;

            internal PolicyReader(MetadataImporter metadataImporter)
            {
                this.metadataImporter = metadataImporter;
            }

            internal IEnumerable<IEnumerable<XmlElement>> NormalizePolicy(IEnumerable<XmlElement> policyAssertions)
            {
                IEnumerable<IEnumerable<XmlElement>> emptyEmpty = EmptyEmpty;
                MetadataImporter.YieldLimiter yieldLimiter = new MetadataImporter.YieldLimiter(this.metadataImporter.Quotas.MaxYields, this.metadataImporter);
                foreach (XmlElement element in policyAssertions)
                {
                    IEnumerable<IEnumerable<XmlElement>> ys = this.ReadNode(element, element, yieldLimiter);
                    emptyEmpty = MetadataImporter.PolicyHelper.CrossProduct<XmlElement>(emptyEmpty, ys, yieldLimiter);
                }
                return emptyEmpty;
            }

            private IEnumerable<IEnumerable<XmlElement>> ReadNode(System.Xml.XmlNode node, XmlElement contextAssertion, MetadataImporter.YieldLimiter yieldLimiter)
            {
                if (this.nodesRead >= this.metadataImporter.Quotas.MaxPolicyNodes)
                {
                    if (this.nodesRead == this.metadataImporter.Quotas.MaxPolicyNodes)
                    {
                        string warningMessage = System.ServiceModel.SR.GetString("ExceededMaxPolicyComplexity", new object[] { node.Name, MetadataImporter.PolicyHelper.GetFragmentIdentifier((XmlElement) node) });
                        this.metadataImporter.PolicyWarningOccured(contextAssertion, warningMessage);
                        this.nodesRead++;
                    }
                    return EmptyEmpty;
                }
                this.nodesRead++;
                IEnumerable<IEnumerable<XmlElement>> emptyEmpty = EmptyEmpty;
                switch (MetadataImporter.PolicyHelper.GetNodeType(node))
                {
                    case MetadataImporter.PolicyHelper.NodeType.Policy:
                    case MetadataImporter.PolicyHelper.NodeType.All:
                        return this.ReadNode_PolicyOrAll((XmlElement) node, contextAssertion, yieldLimiter);

                    case MetadataImporter.PolicyHelper.NodeType.ExactlyOne:
                        return this.ReadNode_ExactlyOne((XmlElement) node, contextAssertion, yieldLimiter);

                    case MetadataImporter.PolicyHelper.NodeType.Assertion:
                        return this.ReadNode_Assertion((XmlElement) node, yieldLimiter);

                    case MetadataImporter.PolicyHelper.NodeType.PolicyReference:
                        return this.ReadNode_PolicyReference((XmlElement) node, contextAssertion, yieldLimiter);

                    case MetadataImporter.PolicyHelper.NodeType.UnrecognizedWSPolicy:
                    {
                        string str2 = System.ServiceModel.SR.GetString("UnrecognizedPolicyElementInNamespace", new object[] { node.Name, node.NamespaceURI });
                        this.metadataImporter.PolicyWarningOccured(contextAssertion, str2);
                        return emptyEmpty;
                    }
                }
                return emptyEmpty;
            }

            private IEnumerable<IEnumerable<XmlElement>> ReadNode_Assertion(XmlElement element, MetadataImporter.YieldLimiter yieldLimiter)
            {
                if (!yieldLimiter.IncrementAndLogIfExceededLimit())
                {
                    yield return new MetadataImporter.PolicyHelper.SingleEnumerable<XmlElement>(element);
                }
                else
                {
                    yield return Empty;
                }
            }

            private IEnumerable<IEnumerable<XmlElement>> ReadNode_ExactlyOne(XmlElement element, XmlElement contextAssertion, MetadataImporter.YieldLimiter yieldLimiter)
            {
                IEnumerator enumerator = element.ChildNodes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    System.Xml.XmlNode current = (System.Xml.XmlNode) enumerator.Current;
                    if (current.NodeType == XmlNodeType.Element)
                    {
                        foreach (IEnumerable<XmlElement> iteratorVariable1 in this.ReadNode(current, contextAssertion, yieldLimiter))
                        {
                            if (yieldLimiter.IncrementAndLogIfExceededLimit())
                            {
                                break;
                            }
                            yield return iteratorVariable1;
                        }
                    }
                }
            }

            private IEnumerable<IEnumerable<XmlElement>> ReadNode_PolicyOrAll(XmlElement element, XmlElement contextAssertion, MetadataImporter.YieldLimiter yieldLimiter)
            {
                IEnumerable<IEnumerable<XmlElement>> emptyEmpty = EmptyEmpty;
                foreach (System.Xml.XmlNode node in element.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        IEnumerable<IEnumerable<XmlElement>> ys = this.ReadNode(node, contextAssertion, yieldLimiter);
                        emptyEmpty = MetadataImporter.PolicyHelper.CrossProduct<XmlElement>(emptyEmpty, ys, yieldLimiter);
                    }
                }
                return emptyEmpty;
            }

            private IEnumerable<IEnumerable<XmlElement>> ReadNode_PolicyReference(XmlElement element, XmlElement contextAssertion, MetadataImporter.YieldLimiter yieldLimiter)
            {
                string attribute = element.GetAttribute("URI");
                switch (attribute)
                {
                    case null:
                    {
                        string warningMessage = System.ServiceModel.SR.GetString("PolicyReferenceMissingURI", new object[] { "URI" });
                        this.metadataImporter.PolicyWarningOccured(contextAssertion, warningMessage);
                        return EmptyEmpty;
                    }
                    case string.Empty:
                    {
                        string str3 = System.ServiceModel.SR.GetString("PolicyReferenceInvalidId");
                        this.metadataImporter.PolicyWarningOccured(contextAssertion, str3);
                        return EmptyEmpty;
                    }
                }
                XmlElement element2 = this.metadataImporter.ResolvePolicyReference(attribute, contextAssertion);
                if (element2 == null)
                {
                    string str4 = System.ServiceModel.SR.GetString("UnableToFindPolicyWithId", new object[] { attribute });
                    this.metadataImporter.PolicyWarningOccured(contextAssertion, str4);
                    return EmptyEmpty;
                }
                return this.ReadNode_PolicyOrAll(element2, element2, yieldLimiter);
            }


        }

        internal delegate void PolicyWarningHandler(XmlElement contextAssertion, string warningMessage);

        internal class YieldLimiter
        {
            private int maxYields;
            private readonly MetadataImporter metadataImporter;
            private int yieldsHit;

            internal YieldLimiter(int maxYields, MetadataImporter metadataImporter)
            {
                this.metadataImporter = metadataImporter;
                this.yieldsHit = 0;
                this.maxYields = maxYields;
            }

            internal bool IncrementAndLogIfExceededLimit()
            {
                if (++this.yieldsHit > this.maxYields)
                {
                    string warningMessage = System.ServiceModel.SR.GetString("ExceededMaxPolicySize");
                    this.metadataImporter.PolicyWarningOccured(null, warningMessage);
                    return true;
                }
                return false;
            }
        }
    }
}

