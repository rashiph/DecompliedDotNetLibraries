namespace System.Configuration.Install
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    [ComVisible(true), Guid("42EB0342-0393-448f-84AA-D4BEB0283595")]
    public class ManagedInstallerClass : IManagedInstaller
    {
        private static string GetHelp(Installer installerWithHelp)
        {
            return (Res.GetString("InstallHelpMessageStart") + Environment.NewLine + installerWithHelp.HelpText + Environment.NewLine + Res.GetString("InstallHelpMessageEnd") + Environment.NewLine);
        }

        public static void InstallHelper(string[] args)
        {
            bool flag = false;
            bool flag2 = false;
            TransactedInstaller installerWithHelp = new TransactedInstaller();
            bool flag3 = false;
            try
            {
                ArrayList list = new ArrayList();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("/", StringComparison.Ordinal) || args[i].StartsWith("-", StringComparison.Ordinal))
                    {
                        string strA = args[i].Substring(1);
                        if ((string.Compare(strA, "u", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "uninstall", StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            flag = true;
                        }
                        else if ((string.Compare(strA, "?", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "help", StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            flag3 = true;
                        }
                        else if (string.Compare(strA, "AssemblyName", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            list.Add(args[i]);
                        }
                    }
                    else
                    {
                        Assembly assembly = null;
                        try
                        {
                            if (flag2)
                            {
                                assembly = Assembly.Load(args[i]);
                            }
                            else
                            {
                                assembly = Assembly.LoadFrom(args[i]);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (args[i].IndexOf('=') != -1)
                            {
                                throw new ArgumentException(Res.GetString("InstallFileDoesntExistCommandLine", new object[] { args[i] }), exception);
                            }
                            throw;
                        }
                        AssemblyInstaller installer2 = new AssemblyInstaller(assembly, (string[]) list.ToArray(typeof(string)));
                        installerWithHelp.Installers.Add(installer2);
                    }
                }
                if (flag3 || (installerWithHelp.Installers.Count == 0))
                {
                    flag3 = true;
                    installerWithHelp.Installers.Add(new AssemblyInstaller());
                    throw new InvalidOperationException(GetHelp(installerWithHelp));
                }
                installerWithHelp.Context = new InstallContext("InstallUtil.InstallLog", (string[]) list.ToArray(typeof(string)));
            }
            catch (Exception exception2)
            {
                if (flag3)
                {
                    throw exception2;
                }
                throw new InvalidOperationException(Res.GetString("InstallInitializeException", new object[] { exception2.GetType().FullName, exception2.Message }));
            }
            try
            {
                string str2 = installerWithHelp.Context.Parameters["installtype"];
                if ((str2 != null) && (string.Compare(str2, "notransaction", StringComparison.OrdinalIgnoreCase) == 0))
                {
                    string str3 = installerWithHelp.Context.Parameters["action"];
                    if ((str3 != null) && (string.Compare(str3, "rollback", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        installerWithHelp.Context.LogMessage(Res.GetString("InstallRollbackNtRun"));
                        for (int j = 0; j < installerWithHelp.Installers.Count; j++)
                        {
                            installerWithHelp.Installers[j].Rollback(null);
                        }
                    }
                    else if ((str3 != null) && (string.Compare(str3, "commit", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        installerWithHelp.Context.LogMessage(Res.GetString("InstallCommitNtRun"));
                        for (int k = 0; k < installerWithHelp.Installers.Count; k++)
                        {
                            installerWithHelp.Installers[k].Commit(null);
                        }
                    }
                    else if ((str3 != null) && (string.Compare(str3, "uninstall", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        installerWithHelp.Context.LogMessage(Res.GetString("InstallUninstallNtRun"));
                        for (int m = 0; m < installerWithHelp.Installers.Count; m++)
                        {
                            installerWithHelp.Installers[m].Uninstall(null);
                        }
                    }
                    else
                    {
                        installerWithHelp.Context.LogMessage(Res.GetString("InstallInstallNtRun"));
                        for (int n = 0; n < installerWithHelp.Installers.Count; n++)
                        {
                            installerWithHelp.Installers[n].Install(null);
                        }
                    }
                }
                else if (!flag)
                {
                    IDictionary stateSaver = new Hashtable();
                    installerWithHelp.Install(stateSaver);
                }
                else
                {
                    installerWithHelp.Uninstall(null);
                }
            }
            catch (Exception exception3)
            {
                throw exception3;
            }
        }

        private static string[] StringToArgs(string cmdLine)
        {
            ArrayList list = new ArrayList();
            StringBuilder builder = null;
            bool flag = false;
            bool flag2 = false;
            for (int i = 0; i < cmdLine.Length; i++)
            {
                char c = cmdLine[i];
                if (builder == null)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        continue;
                    }
                    builder = new StringBuilder();
                }
                if (flag)
                {
                    if (flag2)
                    {
                        if ((c != '\\') && (c != '"'))
                        {
                            builder.Append('\\');
                        }
                        flag2 = false;
                        builder.Append(c);
                    }
                    else
                    {
                        switch (c)
                        {
                            case '"':
                            {
                                flag = false;
                                continue;
                            }
                            case '\\':
                            {
                                flag2 = true;
                                continue;
                            }
                        }
                        builder.Append(c);
                    }
                }
                else if (char.IsWhiteSpace(c))
                {
                    list.Add(builder.ToString());
                    builder = null;
                    flag2 = false;
                }
                else if (flag2)
                {
                    builder.Append(c);
                    flag2 = false;
                }
                else if (c == '^')
                {
                    flag2 = true;
                }
                else if (c == '"')
                {
                    flag = true;
                }
                else
                {
                    builder.Append(c);
                }
            }
            if (builder != null)
            {
                list.Add(builder.ToString());
            }
            string[] array = new string[list.Count];
            list.CopyTo(array);
            return array;
        }

        int IManagedInstaller.ManagedInstall(string argString, int hInstall)
        {
            try
            {
                InstallHelper(StringToArgs(argString));
            }
            catch (Exception innerException)
            {
                StringBuilder builder = new StringBuilder();
                while (innerException != null)
                {
                    builder.Append(innerException.Message);
                    innerException = innerException.InnerException;
                    if (innerException != null)
                    {
                        builder.Append(" --> ");
                    }
                }
                int hRecord = System.Configuration.Install.NativeMethods.MsiCreateRecord(2);
                if (((hRecord != 0) && (System.Configuration.Install.NativeMethods.MsiRecordSetInteger(hRecord, 1, 0x3e9) == 0)) && (System.Configuration.Install.NativeMethods.MsiRecordSetStringW(hRecord, 2, builder.ToString()) == 0))
                {
                    System.Configuration.Install.NativeMethods.MsiProcessMessage(hInstall, 0x1000000, hRecord);
                }
                return -1;
            }
            return 0;
        }
    }
}

