namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class SessionActionFilter : HeaderFilter
    {
        private string[] actions;
        private SecurityStandardsManager standardsManager;

        public SessionActionFilter(SecurityStandardsManager standardsManager, params string[] actions)
        {
            this.actions = actions;
            this.standardsManager = standardsManager;
        }

        public override bool Match(Message message)
        {
            for (int i = 0; i < this.actions.Length; i++)
            {
                if (message.Headers.Action == this.actions[i])
                {
                    return this.standardsManager.DoesMessageContainSecurityHeader(message);
                }
            }
            return false;
        }
    }
}

