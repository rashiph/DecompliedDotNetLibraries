namespace System.Management
{
    using System;

    internal class SecuredIWbemServicesHandler
    {
        private IWbemServices pWbemServiecsSecurityHelper;
        private ManagementScope scope;

        internal SecuredIWbemServicesHandler(ManagementScope theScope, IWbemServices pWbemServiecs)
        {
            this.scope = theScope;
            this.pWbemServiecsSecurityHelper = pWbemServiecs;
        }

        internal int CancelAsyncCall_(IWbemObjectSink pSink)
        {
            return this.pWbemServiecsSecurityHelper.CancelAsyncCall_(pSink);
        }

        internal int CreateClassEnum_(string strSuperClass, int lFlags, IWbemContext pCtx, ref IEnumWbemClassObject ppEnum)
        {
            int num = -2147217407;
            if (this.scope != null)
            {
                num = WmiNetUtilsHelper.CreateClassEnumWmi_f(strSuperClass, lFlags, pCtx, out ppEnum, (int) this.scope.Options.Authentication, (int) this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
            }
            return num;
        }

        internal int CreateClassEnumAsync_(string strSuperClass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.CreateClassEnumAsync_(strSuperClass, lFlags, pCtx, pResponseHandler);
        }

        internal int CreateInstanceEnum_(string strFilter, int lFlags, IWbemContext pCtx, ref IEnumWbemClassObject ppEnum)
        {
            int num = -2147217407;
            if (this.scope != null)
            {
                num = WmiNetUtilsHelper.CreateInstanceEnumWmi_f(strFilter, lFlags, pCtx, out ppEnum, (int) this.scope.Options.Authentication, (int) this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
            }
            return num;
        }

        internal int CreateInstanceEnumAsync_(string strFilter, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.CreateInstanceEnumAsync_(strFilter, lFlags, pCtx, pResponseHandler);
        }

        internal int DeleteClass_(string strClass, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
        {
            int num = -2147217407;
            if (!object.ReferenceEquals(ppCallResult, IntPtr.Zero))
            {
                num = this.pWbemServiecsSecurityHelper.DeleteClass_(strClass, lFlags, pCtx, ppCallResult);
            }
            return num;
        }

        internal int DeleteClassAsync_(string strClass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.DeleteClassAsync_(strClass, lFlags, pCtx, pResponseHandler);
        }

        internal int DeleteInstance_(string strObjectPath, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
        {
            int num = -2147217407;
            if (!object.ReferenceEquals(ppCallResult, IntPtr.Zero))
            {
                num = this.pWbemServiecsSecurityHelper.DeleteInstance_(strObjectPath, lFlags, pCtx, ppCallResult);
            }
            return num;
        }

        internal int DeleteInstanceAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.DeleteInstanceAsync_(strObjectPath, lFlags, pCtx, pResponseHandler);
        }

        internal int ExecMethod_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IWbemClassObjectFreeThreaded pInParams, ref IWbemClassObjectFreeThreaded ppOutParams, IntPtr ppCallResult)
        {
            int num = -2147217407;
            if (!object.ReferenceEquals(ppCallResult, IntPtr.Zero))
            {
                num = this.pWbemServiecsSecurityHelper.ExecMethod_(strObjectPath, strMethodName, lFlags, pCtx, (IntPtr) pInParams, out ppOutParams, ppCallResult);
            }
            return num;
        }

        internal int ExecMethodAsync_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IWbemClassObjectFreeThreaded pInParams, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.ExecMethodAsync_(strObjectPath, strMethodName, lFlags, pCtx, (IntPtr) pInParams, pResponseHandler);
        }

        internal int ExecNotificationQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, ref IEnumWbemClassObject ppEnum)
        {
            int num = -2147217407;
            if (this.scope != null)
            {
                num = WmiNetUtilsHelper.ExecNotificationQueryWmi_f(strQueryLanguage, strQuery, lFlags, pCtx, out ppEnum, (int) this.scope.Options.Authentication, (int) this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
            }
            return num;
        }

        internal int ExecNotificationQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.ExecNotificationQueryAsync_(strQueryLanguage, strQuery, lFlags, pCtx, pResponseHandler);
        }

        internal int ExecQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, ref IEnumWbemClassObject ppEnum)
        {
            int num = -2147217407;
            if (this.scope != null)
            {
                num = WmiNetUtilsHelper.ExecQueryWmi_f(strQueryLanguage, strQuery, lFlags, pCtx, out ppEnum, (int) this.scope.Options.Authentication, (int) this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
            }
            return num;
        }

        internal int ExecQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.ExecQueryAsync_(strQueryLanguage, strQuery, lFlags, pCtx, pResponseHandler);
        }

        internal int GetObject_(string strObjectPath, int lFlags, IWbemContext pCtx, ref IWbemClassObjectFreeThreaded ppObject, IntPtr ppCallResult)
        {
            int num = -2147217407;
            if (!object.ReferenceEquals(ppCallResult, IntPtr.Zero))
            {
                num = this.pWbemServiecsSecurityHelper.GetObject_(strObjectPath, lFlags, pCtx, out ppObject, ppCallResult);
            }
            return num;
        }

        internal int GetObjectAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.GetObjectAsync_(strObjectPath, lFlags, pCtx, pResponseHandler);
        }

        internal int OpenNamespace_(string strNamespace, int lFlags, ref IWbemServices ppWorkingNamespace, IntPtr ppCallResult)
        {
            return -2147217396;
        }

        internal int PutClass_(IWbemClassObjectFreeThreaded pObject, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
        {
            int num = -2147217407;
            if (this.scope != null)
            {
                num = WmiNetUtilsHelper.PutClassWmi_f((IntPtr) pObject, lFlags, pCtx, ppCallResult, (int) this.scope.Options.Authentication, (int) this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
            }
            return num;
        }

        internal int PutClassAsync_(IWbemClassObjectFreeThreaded pObject, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.PutClassAsync_((IntPtr) pObject, lFlags, pCtx, pResponseHandler);
        }

        internal int PutInstance_(IWbemClassObjectFreeThreaded pInst, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
        {
            int num = -2147217407;
            if (this.scope != null)
            {
                num = WmiNetUtilsHelper.PutInstanceWmi_f((IntPtr) pInst, lFlags, pCtx, ppCallResult, (int) this.scope.Options.Authentication, (int) this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
            }
            return num;
        }

        internal int PutInstanceAsync_(IWbemClassObjectFreeThreaded pInst, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.PutInstanceAsync_((IntPtr) pInst, lFlags, pCtx, pResponseHandler);
        }

        internal int QueryObjectSink_(int lFlags, ref IWbemObjectSink ppResponseHandler)
        {
            return this.pWbemServiecsSecurityHelper.QueryObjectSink_(lFlags, out ppResponseHandler);
        }
    }
}

