namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Dispatcher;

    public class CorrelationQuery
    {
        private Collection<MessageQuerySet> selectAdditional;

        internal CorrelationQuery Clone()
        {
            CorrelationQuery query = new CorrelationQuery {
                Select = this.Select,
                IsDefaultContextQuery = this.IsDefaultContextQuery,
                Where = this.Where
            };
            if (this.selectAdditional != null)
            {
                foreach (MessageQuerySet set in this.selectAdditional)
                {
                    query.SelectAdditional.Add(set);
                }
            }
            return query;
        }

        public override bool Equals(object other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            CorrelationQuery query = other as CorrelationQuery;
            if (query == null)
            {
                return false;
            }
            if (this.Where == null)
            {
                return (query.Where == null);
            }
            return this.Where.Equals(query.Where);
        }

        internal static CorrelationQuery FindCorrelationQueryForAction(IEnumerable<CorrelationQuery> queries, string action)
        {
            string str = (action != null) ? action : string.Empty;
            foreach (CorrelationQuery query in queries)
            {
                if (query.Where is CorrelationActionMessageFilter)
                {
                    if ((((CorrelationActionMessageFilter) query.Where).Action == str) || (str == "*"))
                    {
                        return query;
                    }
                }
                else if ((query.Where is ActionMessageFilter) && (((ActionMessageFilter) query.Where).Actions.Contains(str) || (str == "*")))
                {
                    return query;
                }
            }
            return null;
        }

        public override int GetHashCode()
        {
            if (this.Where == null)
            {
                return 0;
            }
            return this.Where.GetHashCode();
        }

        internal static bool IsQueryCollectionSearchable(IEnumerable<CorrelationQuery> queries)
        {
            foreach (CorrelationQuery query in queries)
            {
                if (!(query.Where is CorrelationActionMessageFilter) && !(query.Where is ActionMessageFilter))
                {
                    return false;
                }
            }
            return true;
        }

        internal bool IsDefaultContextQuery { get; set; }

        [DefaultValue((string) null)]
        public MessageQuerySet Select { get; set; }

        public Collection<MessageQuerySet> SelectAdditional
        {
            get
            {
                if (this.selectAdditional == null)
                {
                    this.selectAdditional = new QueryCollection();
                }
                return this.selectAdditional;
            }
        }

        [DefaultValue((string) null)]
        public MessageFilter Where { get; set; }

        private class QueryCollection : Collection<MessageQuerySet>
        {
            protected override void InsertItem(int index, MessageQuerySet item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }
                base.InsertItem(index, item);
            }

            protected override void SetItem(int index, MessageQuerySet item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }
                base.SetItem(index, item);
            }
        }
    }
}

