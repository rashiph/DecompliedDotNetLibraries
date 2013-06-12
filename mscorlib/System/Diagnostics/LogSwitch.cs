namespace System.Diagnostics
{
    using System;
    using System.Security;

    [Serializable]
    internal class LogSwitch
    {
        private LogSwitch[] ChildSwitch;
        private int iChildArraySize;
        internal LoggingLevels iLevel;
        private int iNumChildren;
        internal LoggingLevels iOldLevel;
        private LogSwitch ParentSwitch;
        internal string strDescription;
        internal string strName;

        private LogSwitch()
        {
        }

        [SecuritySafeCritical]
        internal LogSwitch(string name, string description)
        {
            this.strName = name;
            this.strDescription = description;
            this.iLevel = LoggingLevels.ErrorLevel;
            this.iOldLevel = this.iLevel;
            this.ParentSwitch = null;
            this.ChildSwitch = null;
            this.iNumChildren = 0;
            this.iChildArraySize = 0;
            Log.m_Hashtable.Add(this.strName, this);
            Log.AddLogSwitch(this);
            Log.iNumOfSwitches++;
        }

        [SecuritySafeCritical]
        public LogSwitch(string name, string description, LogSwitch parent)
        {
            if ((name != null) && (name.Length == 0))
            {
                throw new ArgumentOutOfRangeException("Name", Environment.GetResourceString("Argument_StringZeroLength"));
            }
            if ((name == null) || (parent == null))
            {
                throw new ArgumentNullException((name == null) ? "name" : "parent");
            }
            this.strName = name;
            this.strDescription = description;
            this.iLevel = LoggingLevels.ErrorLevel;
            this.iOldLevel = this.iLevel;
            parent.AddChildSwitch(this);
            this.ParentSwitch = parent;
            this.ChildSwitch = null;
            this.iNumChildren = 0;
            this.iChildArraySize = 0;
            Log.m_Hashtable.Add(this.strName, this);
            Log.AddLogSwitch(this);
            Log.iNumOfSwitches++;
        }

        private void AddChildSwitch(LogSwitch child)
        {
            if (this.iChildArraySize <= this.iNumChildren)
            {
                int num;
                if (this.iChildArraySize == 0)
                {
                    num = 10;
                }
                else
                {
                    num = (this.iChildArraySize * 3) / 2;
                }
                LogSwitch[] destinationArray = new LogSwitch[num];
                if (this.iNumChildren > 0)
                {
                    Array.Copy(this.ChildSwitch, destinationArray, this.iNumChildren);
                }
                this.iChildArraySize = num;
                this.ChildSwitch = destinationArray;
            }
            this.ChildSwitch[this.iNumChildren++] = child;
        }

        public virtual bool CheckLevel(LoggingLevels level)
        {
            if (this.iLevel <= level)
            {
                return true;
            }
            if (this.ParentSwitch == null)
            {
                return false;
            }
            return this.ParentSwitch.CheckLevel(level);
        }

        public static LogSwitch GetSwitch(string name)
        {
            return (LogSwitch) Log.m_Hashtable[name];
        }

        public virtual string Description
        {
            get
            {
                return this.strDescription;
            }
        }

        public virtual LoggingLevels MinimumLevel
        {
            get
            {
                return this.iLevel;
            }
            [SecuritySafeCritical]
            set
            {
                this.iLevel = value;
                this.iOldLevel = value;
                string strParentName = (this.ParentSwitch != null) ? this.ParentSwitch.Name : "";
                if (Debugger.IsAttached)
                {
                    Log.ModifyLogSwitch((int) this.iLevel, this.strName, strParentName);
                }
                Log.InvokeLogSwitchLevelHandlers(this, this.iLevel);
            }
        }

        public virtual string Name
        {
            get
            {
                return this.strName;
            }
        }

        public virtual LogSwitch Parent
        {
            get
            {
                return this.ParentSwitch;
            }
        }
    }
}

