namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Security;

    internal sealed class GenericParameterDataContract : DataContract
    {
        [SecurityCritical]
        private GenericParameterDataContractCriticalHelper helper;

        [SecuritySafeCritical]
        internal GenericParameterDataContract(Type type) : base(new GenericParameterDataContractCriticalHelper(type))
        {
            this.helper = base.Helper as GenericParameterDataContractCriticalHelper;
        }

        internal override DataContract BindGenericParameters(DataContract[] paramContracts, Dictionary<DataContract, DataContract> boundContracts)
        {
            return paramContracts[this.ParameterPosition];
        }

        internal override bool IsBuiltInDataContract
        {
            get
            {
                return true;
            }
        }

        internal int ParameterPosition
        {
            [SecuritySafeCritical]
            get
            {
                return this.helper.ParameterPosition;
            }
        }

        [SecurityCritical(SecurityCriticalScope.Everything)]
        private class GenericParameterDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            private int parameterPosition;

            internal GenericParameterDataContractCriticalHelper(Type type) : base(type)
            {
                base.SetDataContractName(DataContract.GetStableName(type));
                this.parameterPosition = type.GenericParameterPosition;
            }

            internal int ParameterPosition
            {
                get
                {
                    return this.parameterPosition;
                }
            }
        }
    }
}

