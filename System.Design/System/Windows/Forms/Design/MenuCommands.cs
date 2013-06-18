namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;

    public sealed class MenuCommands : StandardCommands
    {
        private const int cmdidDesignerProperties = 0x1001;
        private const int cmdidEditLabel = 0x152;
        private const int cmdidReverseCancel = 0x4001;
        private const int cmdidSetStatusRectangle = 0x4004;
        private const int cmdidSetStatusText = 0x4003;
        private const int cmdidSpace = 0x4015;
        public static readonly CommandID ComponentTrayMenu = new CommandID(wfMenuGroup, 0x506);
        public static readonly CommandID ContainerMenu = new CommandID(wfMenuGroup, 0x501);
        public static readonly CommandID DesignerProperties = new CommandID(wfCommandSet, 0x1001);
        private const int ECMD_BACKTAB = 5;
        private const int ECMD_CANCEL = 0x67;
        private const int ECMD_CTLMOVEDOWN = 0x4c9;
        private const int ECMD_CTLMOVELEFT = 0x4c8;
        private const int ECMD_CTLMOVERIGHT = 0x4ca;
        private const int ECMD_CTLMOVEUP = 0x4cb;
        private const int ECMD_CTLSIZEDOWN = 0x4cc;
        private const int ECMD_CTLSIZELEFT = 0x4ce;
        private const int ECMD_CTLSIZERIGHT = 0x4cf;
        private const int ECMD_CTLSIZEUP = 0x4cd;
        private const int ECMD_DOWN = 13;
        private const int ECMD_DOWN_EXT = 14;
        private const int ECMD_END = 0x11;
        private const int ECMD_END_EXT = 0x12;
        private const int ECMD_HOME = 15;
        private const int ECMD_HOME_EXT = 0x10;
        private const int ECMD_INVOKESMARTTAG = 0x93;
        private const int ECMD_LEFT = 7;
        private const int ECMD_LEFT_EXT = 8;
        private const int ECMD_RETURN = 3;
        private const int ECMD_RIGHT = 9;
        private const int ECMD_RIGHT_EXT = 10;
        private const int ECMD_TAB = 4;
        private const int ECMD_UP = 11;
        private const int ECMD_UP_EXT = 12;
        public static readonly CommandID EditLabel = new CommandID(VSStandardCommandSet97, 0x152);
        private static readonly Guid guidVSStd2K = new Guid("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}");
        public static readonly CommandID KeyCancel = new CommandID(guidVSStd2K, 0x67);
        public static readonly CommandID KeyDefaultAction = new CommandID(guidVSStd2K, 3);
        public static readonly CommandID KeyEnd = new CommandID(guidVSStd2K, 0x11);
        public static readonly CommandID KeyHome = new CommandID(guidVSStd2K, 15);
        public static readonly CommandID KeyInvokeSmartTag = new CommandID(guidVSStd2K, 0x93);
        public static readonly CommandID KeyMoveDown = new CommandID(guidVSStd2K, 13);
        public static readonly CommandID KeyMoveLeft = new CommandID(guidVSStd2K, 7);
        public static readonly CommandID KeyMoveRight = new CommandID(guidVSStd2K, 9);
        public static readonly CommandID KeyMoveUp = new CommandID(guidVSStd2K, 11);
        public static readonly CommandID KeyNudgeDown = new CommandID(guidVSStd2K, 0x4c9);
        public static readonly CommandID KeyNudgeHeightDecrease = new CommandID(guidVSStd2K, 0x4cd);
        public static readonly CommandID KeyNudgeHeightIncrease = new CommandID(guidVSStd2K, 0x4cc);
        public static readonly CommandID KeyNudgeLeft = new CommandID(guidVSStd2K, 0x4c8);
        public static readonly CommandID KeyNudgeRight = new CommandID(guidVSStd2K, 0x4ca);
        public static readonly CommandID KeyNudgeUp = new CommandID(guidVSStd2K, 0x4cb);
        public static readonly CommandID KeyNudgeWidthDecrease = new CommandID(guidVSStd2K, 0x4ce);
        public static readonly CommandID KeyNudgeWidthIncrease = new CommandID(guidVSStd2K, 0x4cf);
        public static readonly CommandID KeyReverseCancel = new CommandID(wfCommandSet, 0x4001);
        public static readonly CommandID KeySelectNext = new CommandID(guidVSStd2K, 4);
        public static readonly CommandID KeySelectPrevious = new CommandID(guidVSStd2K, 5);
        public static readonly CommandID KeyShiftEnd = new CommandID(guidVSStd2K, 0x12);
        public static readonly CommandID KeyShiftHome = new CommandID(guidVSStd2K, 0x10);
        public static readonly CommandID KeySizeHeightDecrease = new CommandID(guidVSStd2K, 14);
        public static readonly CommandID KeySizeHeightIncrease = new CommandID(guidVSStd2K, 12);
        public static readonly CommandID KeySizeWidthDecrease = new CommandID(guidVSStd2K, 8);
        public static readonly CommandID KeySizeWidthIncrease = new CommandID(guidVSStd2K, 10);
        public static readonly CommandID KeyTabOrderSelect = new CommandID(wfCommandSet, 0x4015);
        private const int mnuidComponentTray = 0x506;
        private const int mnuidContainer = 0x501;
        private const int mnuidSelection = 0x500;
        private const int mnuidTraySelection = 0x503;
        public static readonly CommandID SelectionMenu = new CommandID(wfMenuGroup, 0x500);
        public static readonly CommandID SetStatusRectangle = new CommandID(wfCommandSet, 0x4004);
        public static readonly CommandID SetStatusText = new CommandID(wfCommandSet, 0x4003);
        public static readonly CommandID TraySelectionMenu = new CommandID(wfMenuGroup, 0x503);
        private static readonly Guid VSStandardCommandSet97 = new Guid("{5efc7975-14bc-11cf-9b2b-00aa00573819}");
        private static readonly Guid wfCommandSet = new Guid("{74D21313-2AEE-11d1-8BFB-00A0C90F26F7}");
        private static readonly Guid wfMenuGroup = new Guid("{74D21312-2AEE-11d1-8BFB-00A0C90F26F7}");
    }
}

