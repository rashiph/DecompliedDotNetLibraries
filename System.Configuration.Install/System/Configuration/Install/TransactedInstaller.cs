namespace System.Configuration.Install
{
    using System;
    using System.Collections;

    public class TransactedInstaller : Installer
    {
        public override void Install(IDictionary savedState)
        {
            if (base.Context == null)
            {
                base.Context = new InstallContext();
            }
            base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoTransacted"));
            try
            {
                bool flag = true;
                try
                {
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginInstall"));
                    base.Install(savedState);
                }
                catch (Exception exception)
                {
                    flag = false;
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoException"));
                    Installer.LogException(exception, base.Context);
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginRollback"));
                    try
                    {
                        this.Rollback(savedState);
                    }
                    catch (Exception)
                    {
                    }
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoRollbackDone"));
                    throw new InvalidOperationException(Res.GetString("InstallRollback"), exception);
                }
                if (flag)
                {
                    base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoBeginCommit"));
                    try
                    {
                        this.Commit(savedState);
                    }
                    finally
                    {
                        base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoCommitDone"));
                    }
                }
            }
            finally
            {
                base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoTransactedDone"));
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            if (base.Context == null)
            {
                base.Context = new InstallContext();
            }
            base.Context.LogMessage(Environment.NewLine + Environment.NewLine + Res.GetString("InstallInfoBeginUninstall"));
            try
            {
                base.Uninstall(savedState);
            }
            finally
            {
                base.Context.LogMessage(Environment.NewLine + Res.GetString("InstallInfoUninstallDone"));
            }
        }
    }
}

