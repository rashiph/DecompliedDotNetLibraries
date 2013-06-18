namespace System.Management
{
    using System;

    internal class SecuredIEnumWbemClassObjectHandler
    {
        private IEnumWbemClassObject pEnumWbemClassObjectsecurityHelper;
        private ManagementScope scope;

        internal SecuredIEnumWbemClassObjectHandler(ManagementScope theScope, IEnumWbemClassObject pEnumWbemClassObject)
        {
            this.scope = theScope;
            this.pEnumWbemClassObjectsecurityHelper = pEnumWbemClassObject;
        }

        internal int Clone_(ref IEnumWbemClassObject ppEnum)
        {
            int num = -2147217407;
            if (this.scope != null)
            {
                num = WmiNetUtilsHelper.CloneEnumWbemClassObject_f(out ppEnum, (int) this.scope.Options.Authentication, (int) this.scope.Options.Impersonation, this.pEnumWbemClassObjectsecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
            }
            return num;
        }

        internal int Next_(int lTimeout, uint uCount, IWbemClassObject_DoNotMarshal[] ppOutParams, ref uint puReturned)
        {
            return this.pEnumWbemClassObjectsecurityHelper.Next_(lTimeout, uCount, ppOutParams, out puReturned);
        }

        internal int NextAsync_(uint uCount, IWbemObjectSink pSink)
        {
            return this.pEnumWbemClassObjectsecurityHelper.NextAsync_(uCount, pSink);
        }

        internal int Reset_()
        {
            return this.pEnumWbemClassObjectsecurityHelper.Reset_();
        }

        internal int Skip_(int lTimeout, uint nCount)
        {
            return this.pEnumWbemClassObjectsecurityHelper.Skip_(lTimeout, nCount);
        }
    }
}

