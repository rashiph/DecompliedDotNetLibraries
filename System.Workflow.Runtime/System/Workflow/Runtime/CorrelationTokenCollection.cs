namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [Serializable]
    public sealed class CorrelationTokenCollection : KeyedCollection<string, CorrelationToken>
    {
        public static readonly DependencyProperty CorrelationTokenCollectionProperty = DependencyProperty.RegisterAttached("CorrelationTokenCollection", typeof(CorrelationTokenCollection), typeof(CorrelationTokenCollection));

        protected override void ClearItems()
        {
            base.ClearItems();
        }

        public static CorrelationToken GetCorrelationToken(Activity activity, string correlationTokenName, string ownerActivityName)
        {
            if (correlationTokenName == null)
            {
                throw new ArgumentNullException("correlationTokenName");
            }
            if (ownerActivityName == null)
            {
                throw new ArgumentNullException("ownerActivityName");
            }
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            Activity activity2 = ContextActivityUtils.ContextActivity(activity);
            Activity activityByName = null;
            if (!string.IsNullOrEmpty(ownerActivityName))
            {
                while (activity2 != null)
                {
                    activityByName = activity2.GetActivityByName(ownerActivityName, true);
                    if (activityByName != null)
                    {
                        break;
                    }
                    activity2 = ContextActivityUtils.ParentContextActivity(activity2);
                }
                if (activityByName == null)
                {
                    activityByName = Helpers.ParseActivityForBind(activity, ownerActivityName);
                }
            }
            if (activityByName == null)
            {
                throw new InvalidOperationException(ExecutionStringManager.OwnerActivityMissing);
            }
            CorrelationTokenCollection tokens = activityByName.GetValue(CorrelationTokenCollectionProperty) as CorrelationTokenCollection;
            if (tokens == null)
            {
                tokens = new CorrelationTokenCollection();
                activityByName.SetValue(CorrelationTokenCollectionProperty, tokens);
            }
            if (!tokens.Contains(correlationTokenName))
            {
                tokens.Add(new CorrelationToken(correlationTokenName));
            }
            return tokens[correlationTokenName];
        }

        public CorrelationToken GetItem(string key)
        {
            return base[key];
        }

        protected override string GetKeyForItem(CorrelationToken item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, CorrelationToken item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, CorrelationToken item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }

        internal static void UninitializeCorrelationTokens(Activity activity)
        {
            CorrelationTokenCollection tokens = activity.GetValue(CorrelationTokenCollectionProperty) as CorrelationTokenCollection;
            if (tokens != null)
            {
                foreach (CorrelationToken token in tokens)
                {
                    token.Uninitialize(activity);
                }
            }
        }
    }
}

