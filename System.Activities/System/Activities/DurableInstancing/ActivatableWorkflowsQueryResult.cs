namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;

    public sealed class ActivatableWorkflowsQueryResult : InstanceStoreQueryResult
    {
        private static readonly ReadOnlyDictionary<XName, object> emptyDictionary = new ReadOnlyDictionary<XName, object>(new Dictionary<XName, object>(0));

        public ActivatableWorkflowsQueryResult()
        {
            this.ActivationParameters = new List<IDictionary<XName, object>>(0);
        }

        public ActivatableWorkflowsQueryResult(IDictionary<XName, object> parameters)
        {
            this.ActivationParameters = new List<IDictionary<XName, object>> { (parameters == null) ? emptyDictionary : new ReadOnlyDictionary<XName, object>(parameters, 1) };
        }

        public ActivatableWorkflowsQueryResult(IEnumerable<IDictionary<XName, object>> parameters)
        {
            if (parameters == null)
            {
                this.ActivationParameters = new List<IDictionary<XName, object>>(0);
            }
            else
            {
                this.ActivationParameters = new List<IDictionary<XName, object>>((IEnumerable<IDictionary<XName, object>>) parameters.Select<IDictionary<XName, object>, ReadOnlyDictionary<XName, object>>(delegate (IDictionary<XName, object> dictionary) {
                    if (dictionary != null)
                    {
                        return new ReadOnlyDictionary<XName, object>(dictionary, true);
                    }
                    return emptyDictionary;
                }));
            }
        }

        public List<IDictionary<XName, object>> ActivationParameters { get; private set; }
    }
}

