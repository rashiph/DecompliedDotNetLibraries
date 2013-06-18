namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TransactionFlowAttribute : Attribute, IOperationBehavior
    {
        private TransactionFlowOption transactions;

        public TransactionFlowAttribute(TransactionFlowOption transactions)
        {
            TransactionFlowBindingElement.ValidateOption(transactions);
            this.transactions = transactions;
        }

        private void ApplyBehavior(OperationDescription description, BindingParameterCollection parameters)
        {
            EnsureDictionary(parameters)[new DirectionalAction(description.Messages[0].Direction, description.Messages[0].Action)] = this.transactions;
        }

        private static Dictionary<DirectionalAction, TransactionFlowOption> EnsureDictionary(BindingParameterCollection parameters)
        {
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = parameters.Find<Dictionary<DirectionalAction, TransactionFlowOption>>();
            if (dictionary == null)
            {
                dictionary = new Dictionary<DirectionalAction, TransactionFlowOption> {
                    dictionary
                };
            }
            return dictionary;
        }

        internal static void OverrideFlow(BindingParameterCollection parameters, string action, MessageDirection direction, TransactionFlowOption option)
        {
            Dictionary<DirectionalAction, TransactionFlowOption> dictionary = EnsureDictionary(parameters);
            DirectionalAction key = new DirectionalAction(direction, action);
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = option;
            }
            else
            {
                dictionary.Add(key, option);
            }
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            this.ApplyBehavior(description, parameters);
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
        }

        void IOperationBehavior.Validate(OperationDescription description)
        {
        }

        public TransactionFlowOption Transactions
        {
            get
            {
                return this.transactions;
            }
        }
    }
}

