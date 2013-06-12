namespace System.Windows.Forms
{
    using System;

    internal class Command : WeakReference
    {
        private static Command[] cmds;
        private static int icmdTry;
        internal int id;
        private const int idLim = 0x10000;
        private const int idMin = 0x100;
        private static object internalSyncObject = new object();

        public Command(ICommandExecutor target) : base(target, false)
        {
            AssignID(this);
        }

        protected static void AssignID(Command cmd)
        {
            lock (internalSyncObject)
            {
                int num;
                if (cmds == null)
                {
                    cmds = new Command[20];
                    num = 0;
                }
                else
                {
                    int length = cmds.Length;
                    if (icmdTry >= length)
                    {
                        icmdTry = 0;
                    }
                    for (num = icmdTry; num < length; num++)
                    {
                        if (cmds[num] == null)
                        {
                            goto Label_0100;
                        }
                    }
                    for (num = 0; num < icmdTry; num++)
                    {
                        if (cmds[num] == null)
                        {
                            goto Label_0100;
                        }
                    }
                    for (num = 0; num < length; num++)
                    {
                        if (cmds[num].Target == null)
                        {
                            goto Label_0100;
                        }
                    }
                    num = cmds.Length;
                    length = Math.Min(0xff00, 2 * num);
                    if (length <= num)
                    {
                        GC.Collect();
                        num = 0;
                        while (num < length)
                        {
                            if ((cmds[num] == null) || (cmds[num].Target == null))
                            {
                                goto Label_0100;
                            }
                            num++;
                        }
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("CommandIdNotAllocated"));
                    }
                    Command[] destinationArray = new Command[length];
                    Array.Copy(cmds, 0, destinationArray, 0, num);
                    cmds = destinationArray;
                }
            Label_0100:
                cmd.id = num + 0x100;
                cmds[num] = cmd;
                icmdTry = num + 1;
            }
        }

        public static bool DispatchID(int id)
        {
            Command commandFromID = GetCommandFromID(id);
            if (commandFromID == null)
            {
                return false;
            }
            return commandFromID.Invoke();
        }

        public virtual void Dispose()
        {
            if (this.id >= 0x100)
            {
                Dispose(this);
            }
        }

        protected static void Dispose(Command cmd)
        {
            lock (internalSyncObject)
            {
                if (cmd.id >= 0x100)
                {
                    cmd.Target = null;
                    if (cmds[cmd.id - 0x100] == cmd)
                    {
                        cmds[cmd.id - 0x100] = null;
                    }
                    cmd.id = 0;
                }
            }
        }

        public static Command GetCommandFromID(int id)
        {
            lock (internalSyncObject)
            {
                if (cmds == null)
                {
                    return null;
                }
                int index = id - 0x100;
                if ((index < 0) || (index >= cmds.Length))
                {
                    return null;
                }
                return cmds[index];
            }
        }

        public virtual bool Invoke()
        {
            object target = this.Target;
            if (!(target is ICommandExecutor))
            {
                return false;
            }
            ((ICommandExecutor) target).Execute();
            return true;
        }

        public virtual int ID
        {
            get
            {
                return this.id;
            }
        }
    }
}

