namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;

    [StandardModule]
    public sealed class Globals
    {
        public static string ScriptEngine
        {
            get
            {
                return "VB";
            }
        }

        public static int ScriptEngineBuildVersion
        {
            get
            {
                return 0x766f;
            }
        }

        public static int ScriptEngineMajorVersion
        {
            get
            {
                return 10;
            }
        }

        public static int ScriptEngineMinorVersion
        {
            get
            {
                return 0;
            }
        }
    }
}

