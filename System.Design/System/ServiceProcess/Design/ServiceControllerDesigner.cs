namespace System.ServiceProcess.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ServiceControllerDesigner : ComponentDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            bool[] browsableSettings = new bool[9];
            browsableSettings[8] = true;
            RuntimeComponentFilter.FilterProperties(properties, new string[] { "ServiceName", "DisplayName" }, new string[] { "CanPauseAndContinue", "CanShutdown", "CanStop", "DisplayName", "DependentServices", "ServicesDependedOn", "Status", "ServiceType", "MachineName" }, browsableSettings);
        }
    }
}

