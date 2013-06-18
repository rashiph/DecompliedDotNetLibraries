namespace System.Configuration.Install
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime;
    using System.Text;

    [DefaultEvent("AfterInstall")]
    public class Installer : Component
    {
        private InstallContext context;
        private InstallerCollection installers;
        internal Installer parent;
        private const string wrappedExceptionSource = "WrappedExceptionSource";

        public event InstallEventHandler AfterInstall;

        public event InstallEventHandler AfterRollback;

        public event InstallEventHandler AfterUninstall;

        public event InstallEventHandler BeforeInstall;

        public event InstallEventHandler BeforeRollback;

        public event InstallEventHandler BeforeUninstall;

        public event InstallEventHandler Committed;

        public event InstallEventHandler Committing;

        public virtual void Commit(IDictionary savedState)
        {
            if (savedState == null)
            {
                throw new ArgumentException(Res.GetString("InstallNullParameter", new object[] { "savedState" }));
            }
            if ((savedState["_reserved_lastInstallerAttempted"] == null) || (savedState["_reserved_nestedSavedStates"] == null))
            {
                throw new ArgumentException(Res.GetString("InstallDictionaryMissingValues", new object[] { "savedState" }));
            }
            Exception e = null;
            try
            {
                this.OnCommitting(savedState);
            }
            catch (Exception exception2)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnCommitting", exception2);
                this.Context.LogMessage(Res.GetString("InstallCommitException"));
                e = exception2;
            }
            int num = (int) savedState["_reserved_lastInstallerAttempted"];
            IDictionary[] dictionaryArray = (IDictionary[]) savedState["_reserved_nestedSavedStates"];
            if (((num + 1) != dictionaryArray.Length) || (num >= this.Installers.Count))
            {
                throw new ArgumentException(Res.GetString("InstallDictionaryCorrupted", new object[] { "savedState" }));
            }
            for (int i = 0; i < this.Installers.Count; i++)
            {
                this.Installers[i].Context = this.Context;
            }
            for (int j = 0; j <= num; j++)
            {
                try
                {
                    this.Installers[j].Commit(dictionaryArray[j]);
                }
                catch (Exception exception3)
                {
                    if (!this.IsWrappedException(exception3))
                    {
                        this.Context.LogMessage(Res.GetString("InstallLogCommitException", new object[] { this.Installers[j].ToString() }));
                        LogException(exception3, this.Context);
                        this.Context.LogMessage(Res.GetString("InstallCommitException"));
                    }
                    e = exception3;
                }
            }
            savedState["_reserved_nestedSavedStates"] = dictionaryArray;
            savedState.Remove("_reserved_lastInstallerAttempted");
            try
            {
                this.OnCommitted(savedState);
            }
            catch (Exception exception4)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnCommitted", exception4);
                this.Context.LogMessage(Res.GetString("InstallCommitException"));
                e = exception4;
            }
            if (e != null)
            {
                Exception exception5 = e;
                if (!this.IsWrappedException(e))
                {
                    exception5 = new InstallException(Res.GetString("InstallCommitException"), e) {
                        Source = "WrappedExceptionSource"
                    };
                }
                throw exception5;
            }
        }

        public virtual void Install(IDictionary stateSaver)
        {
            if (stateSaver == null)
            {
                throw new ArgumentException(Res.GetString("InstallNullParameter", new object[] { "stateSaver" }));
            }
            try
            {
                this.OnBeforeInstall(stateSaver);
            }
            catch (Exception exception)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityError"), "OnBeforeInstall", exception);
                throw new InvalidOperationException(Res.GetString("InstallEventException", new object[] { "OnBeforeInstall", base.GetType().FullName }), exception);
            }
            int num = -1;
            ArrayList list = new ArrayList();
            try
            {
                for (int i = 0; i < this.Installers.Count; i++)
                {
                    this.Installers[i].Context = this.Context;
                }
                for (int j = 0; j < this.Installers.Count; j++)
                {
                    Installer installer = this.Installers[j];
                    IDictionary dictionary = new Hashtable();
                    try
                    {
                        num = j;
                        installer.Install(dictionary);
                    }
                    finally
                    {
                        list.Add(dictionary);
                    }
                }
            }
            finally
            {
                stateSaver.Add("_reserved_lastInstallerAttempted", num);
                stateSaver.Add("_reserved_nestedSavedStates", list.ToArray(typeof(IDictionary)));
            }
            try
            {
                this.OnAfterInstall(stateSaver);
            }
            catch (Exception exception2)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityError"), "OnAfterInstall", exception2);
                throw new InvalidOperationException(Res.GetString("InstallEventException", new object[] { "OnAfterInstall", base.GetType().FullName }), exception2);
            }
        }

        internal bool InstallerTreeContains(Installer target)
        {
            if (this.Installers.Contains(target))
            {
                return true;
            }
            foreach (Installer installer in this.Installers)
            {
                if (installer.InstallerTreeContains(target))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsWrappedException(Exception e)
        {
            return (((e is InstallException) && (e.Source == "WrappedExceptionSource")) && (e.TargetSite.ReflectedType == typeof(Installer)));
        }

        internal static void LogException(Exception e, InstallContext context)
        {
            bool flag = true;
            while (e != null)
            {
                if (flag)
                {
                    context.LogMessage(e.GetType().FullName + ": " + e.Message);
                    flag = false;
                }
                else
                {
                    context.LogMessage(Res.GetString("InstallLogInner", new object[] { e.GetType().FullName, e.Message }));
                }
                if (context.IsParameterTrue("showcallstack"))
                {
                    context.LogMessage(e.StackTrace);
                }
                e = e.InnerException;
            }
        }

        protected virtual void OnAfterInstall(IDictionary savedState)
        {
            if (this.afterInstallHandler != null)
            {
                this.afterInstallHandler(this, new InstallEventArgs(savedState));
            }
        }

        protected virtual void OnAfterRollback(IDictionary savedState)
        {
            if (this.afterRollbackHandler != null)
            {
                this.afterRollbackHandler(this, new InstallEventArgs(savedState));
            }
        }

        protected virtual void OnAfterUninstall(IDictionary savedState)
        {
            if (this.afterUninstallHandler != null)
            {
                this.afterUninstallHandler(this, new InstallEventArgs(savedState));
            }
        }

        protected virtual void OnBeforeInstall(IDictionary savedState)
        {
            if (this.beforeInstallHandler != null)
            {
                this.beforeInstallHandler(this, new InstallEventArgs(savedState));
            }
        }

        protected virtual void OnBeforeRollback(IDictionary savedState)
        {
            if (this.beforeRollbackHandler != null)
            {
                this.beforeRollbackHandler(this, new InstallEventArgs(savedState));
            }
        }

        protected virtual void OnBeforeUninstall(IDictionary savedState)
        {
            if (this.beforeUninstallHandler != null)
            {
                this.beforeUninstallHandler(this, new InstallEventArgs(savedState));
            }
        }

        protected virtual void OnCommitted(IDictionary savedState)
        {
            if (this.afterCommitHandler != null)
            {
                this.afterCommitHandler(this, new InstallEventArgs(savedState));
            }
        }

        protected virtual void OnCommitting(IDictionary savedState)
        {
            if (this.beforeCommitHandler != null)
            {
                this.beforeCommitHandler(this, new InstallEventArgs(savedState));
            }
        }

        public virtual void Rollback(IDictionary savedState)
        {
            if (savedState == null)
            {
                throw new ArgumentException(Res.GetString("InstallNullParameter", new object[] { "savedState" }));
            }
            if ((savedState["_reserved_lastInstallerAttempted"] == null) || (savedState["_reserved_nestedSavedStates"] == null))
            {
                throw new ArgumentException(Res.GetString("InstallDictionaryMissingValues", new object[] { "savedState" }));
            }
            Exception e = null;
            try
            {
                this.OnBeforeRollback(savedState);
            }
            catch (Exception exception2)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnBeforeRollback", exception2);
                this.Context.LogMessage(Res.GetString("InstallRollbackException"));
                e = exception2;
            }
            int num = (int) savedState["_reserved_lastInstallerAttempted"];
            IDictionary[] dictionaryArray = (IDictionary[]) savedState["_reserved_nestedSavedStates"];
            if (((num + 1) != dictionaryArray.Length) || (num >= this.Installers.Count))
            {
                throw new ArgumentException(Res.GetString("InstallDictionaryCorrupted", new object[] { "savedState" }));
            }
            for (int i = this.Installers.Count - 1; i >= 0; i--)
            {
                this.Installers[i].Context = this.Context;
            }
            for (int j = num; j >= 0; j--)
            {
                try
                {
                    this.Installers[j].Rollback(dictionaryArray[j]);
                }
                catch (Exception exception3)
                {
                    if (!this.IsWrappedException(exception3))
                    {
                        this.Context.LogMessage(Res.GetString("InstallLogRollbackException", new object[] { this.Installers[j].ToString() }));
                        LogException(exception3, this.Context);
                        this.Context.LogMessage(Res.GetString("InstallRollbackException"));
                    }
                    e = exception3;
                }
            }
            try
            {
                this.OnAfterRollback(savedState);
            }
            catch (Exception exception4)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnAfterRollback", exception4);
                this.Context.LogMessage(Res.GetString("InstallRollbackException"));
                e = exception4;
            }
            if (e != null)
            {
                Exception exception5 = e;
                if (!this.IsWrappedException(e))
                {
                    exception5 = new InstallException(Res.GetString("InstallRollbackException"), e) {
                        Source = "WrappedExceptionSource"
                    };
                }
                throw exception5;
            }
        }

        public virtual void Uninstall(IDictionary savedState)
        {
            Exception e = null;
            IDictionary[] dictionaryArray;
            try
            {
                this.OnBeforeUninstall(savedState);
            }
            catch (Exception exception2)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnBeforeUninstall", exception2);
                this.Context.LogMessage(Res.GetString("InstallUninstallException"));
                e = exception2;
            }
            if (savedState != null)
            {
                dictionaryArray = (IDictionary[]) savedState["_reserved_nestedSavedStates"];
                if ((dictionaryArray == null) || (dictionaryArray.Length != this.Installers.Count))
                {
                    throw new ArgumentException(Res.GetString("InstallDictionaryCorrupted", new object[] { "savedState" }));
                }
            }
            else
            {
                dictionaryArray = new IDictionary[this.Installers.Count];
            }
            for (int i = this.Installers.Count - 1; i >= 0; i--)
            {
                this.Installers[i].Context = this.Context;
            }
            for (int j = this.Installers.Count - 1; j >= 0; j--)
            {
                try
                {
                    this.Installers[j].Uninstall(dictionaryArray[j]);
                }
                catch (Exception exception3)
                {
                    if (!this.IsWrappedException(exception3))
                    {
                        this.Context.LogMessage(Res.GetString("InstallLogUninstallException", new object[] { this.Installers[j].ToString() }));
                        LogException(exception3, this.Context);
                        this.Context.LogMessage(Res.GetString("InstallUninstallException"));
                    }
                    e = exception3;
                }
            }
            try
            {
                this.OnAfterUninstall(savedState);
            }
            catch (Exception exception4)
            {
                this.WriteEventHandlerError(Res.GetString("InstallSeverityWarning"), "OnAfterUninstall", exception4);
                this.Context.LogMessage(Res.GetString("InstallUninstallException"));
                e = exception4;
            }
            if (e != null)
            {
                Exception exception5 = e;
                if (!this.IsWrappedException(e))
                {
                    exception5 = new InstallException(Res.GetString("InstallUninstallException"), e) {
                        Source = "WrappedExceptionSource"
                    };
                }
                throw exception5;
            }
        }

        private void WriteEventHandlerError(string severity, string eventName, Exception e)
        {
            this.Context.LogMessage(Res.GetString("InstallLogError", new object[] { severity, eventName, base.GetType().FullName }));
            LogException(e, this.Context);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public InstallContext Context
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.context;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.context = value;
            }
        }

        [ResDescription("Desc_Installer_HelpText")]
        public virtual string HelpText
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < this.Installers.Count; i++)
                {
                    string helpText = this.Installers[i].HelpText;
                    if (helpText.Length > 0)
                    {
                        builder.Append("\r\n");
                        builder.Append(helpText);
                    }
                }
                return builder.ToString();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Browsable(false)]
        public InstallerCollection Installers
        {
            get
            {
                if (this.installers == null)
                {
                    this.installers = new InstallerCollection(this);
                }
                return this.installers;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(true), TypeConverter(typeof(InstallerParentConverter)), ResDescription("Desc_Installer_Parent")]
        public Installer Parent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
            set
            {
                if (value == this)
                {
                    throw new InvalidOperationException(Res.GetString("InstallBadParent"));
                }
                if (value != this.parent)
                {
                    if ((value != null) && this.InstallerTreeContains(value))
                    {
                        throw new InvalidOperationException(Res.GetString("InstallRecursiveParent"));
                    }
                    if (this.parent != null)
                    {
                        int index = this.parent.Installers.IndexOf(this);
                        if (index != -1)
                        {
                            this.parent.Installers.RemoveAt(index);
                        }
                    }
                    this.parent = value;
                    if ((this.parent != null) && !this.parent.Installers.Contains(this))
                    {
                        this.parent.Installers.Add(this);
                    }
                }
            }
        }
    }
}

