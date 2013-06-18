namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [DataContract]
    public class ActionMessageFilter : MessageFilter
    {
        private Dictionary<string, int> actions;
        private ReadOnlyCollection<string> actionSet;

        public ActionMessageFilter(params string[] actions)
        {
            if (actions == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("actions");
            }
            this.Init(actions);
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return new ActionMessageFilterTable<FilterData>();
        }

        private void Init(string[] actions)
        {
            if (actions.Length == 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("ActionFilterEmptyList"), "actions"));
            }
            this.actions = new Dictionary<string, int>();
            for (int i = 0; i < actions.Length; i++)
            {
                if (!this.actions.ContainsKey(actions[i]))
                {
                    this.actions.Add(actions[i], 0);
                }
            }
        }

        private bool InnerMatch(Message message)
        {
            string action = message.Headers.Action;
            if (action == null)
            {
                action = string.Empty;
            }
            return this.actions.ContainsKey(action);
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return this.InnerMatch(message);
        }

        public override bool Match(MessageBuffer messageBuffer)
        {
            bool flag;
            if (messageBuffer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }
            Message message = messageBuffer.CreateMessage();
            try
            {
                flag = this.InnerMatch(message);
            }
            finally
            {
                message.Close();
            }
            return flag;
        }

        public ReadOnlyCollection<string> Actions
        {
            get
            {
                if (this.actionSet == null)
                {
                    this.actionSet = new ReadOnlyCollection<string>(new List<string>(this.actions.Keys));
                }
                return this.actionSet;
            }
        }

        [DataMember(IsRequired=true)]
        internal string[] DCActions
        {
            get
            {
                string[] array = new string[this.actions.Count];
                this.actions.Keys.CopyTo(array, 0);
                return array;
            }
            set
            {
                this.Init(value);
            }
        }
    }
}

