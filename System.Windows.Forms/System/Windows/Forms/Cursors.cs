namespace System.Windows.Forms
{
    using System;

    public sealed class Cursors
    {
        private static Cursor appStarting;
        private static Cursor arrow;
        private static Cursor cross;
        private static Cursor defaultCursor;
        private static Cursor hand;
        private static Cursor help;
        private static Cursor hSplit;
        private static Cursor iBeam;
        private static Cursor no;
        private static Cursor noMove2D;
        private static Cursor noMoveHoriz;
        private static Cursor noMoveVert;
        private static Cursor panEast;
        private static Cursor panNE;
        private static Cursor panNorth;
        private static Cursor panNW;
        private static Cursor panSE;
        private static Cursor panSouth;
        private static Cursor panSW;
        private static Cursor panWest;
        private static Cursor sizeAll;
        private static Cursor sizeNESW;
        private static Cursor sizeNS;
        private static Cursor sizeNWSE;
        private static Cursor sizeWE;
        private static Cursor upArrow;
        private static Cursor vSplit;
        private static Cursor wait;

        private Cursors()
        {
        }

        internal static Cursor KnownCursorFromHCursor(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return null;
            }
            return new Cursor(handle);
        }

        public static Cursor AppStarting
        {
            get
            {
                if (appStarting == null)
                {
                    appStarting = new Cursor(0x7f8a, 0);
                }
                return appStarting;
            }
        }

        public static Cursor Arrow
        {
            get
            {
                if (arrow == null)
                {
                    arrow = new Cursor(0x7f00, 0);
                }
                return arrow;
            }
        }

        public static Cursor Cross
        {
            get
            {
                if (cross == null)
                {
                    cross = new Cursor(0x7f03, 0);
                }
                return cross;
            }
        }

        public static Cursor Default
        {
            get
            {
                if (defaultCursor == null)
                {
                    defaultCursor = new Cursor(0x7f00, 0);
                }
                return defaultCursor;
            }
        }

        public static Cursor Hand
        {
            get
            {
                if (hand == null)
                {
                    hand = new Cursor("hand.cur", 0);
                }
                return hand;
            }
        }

        public static Cursor Help
        {
            get
            {
                if (help == null)
                {
                    help = new Cursor(0x7f8b, 0);
                }
                return help;
            }
        }

        public static Cursor HSplit
        {
            get
            {
                if (hSplit == null)
                {
                    hSplit = new Cursor("hsplit.cur", 0);
                }
                return hSplit;
            }
        }

        public static Cursor IBeam
        {
            get
            {
                if (iBeam == null)
                {
                    iBeam = new Cursor(0x7f01, 0);
                }
                return iBeam;
            }
        }

        public static Cursor No
        {
            get
            {
                if (no == null)
                {
                    no = new Cursor(0x7f88, 0);
                }
                return no;
            }
        }

        public static Cursor NoMove2D
        {
            get
            {
                if (noMove2D == null)
                {
                    noMove2D = new Cursor("nomove2d.cur", 0);
                }
                return noMove2D;
            }
        }

        public static Cursor NoMoveHoriz
        {
            get
            {
                if (noMoveHoriz == null)
                {
                    noMoveHoriz = new Cursor("nomoveh.cur", 0);
                }
                return noMoveHoriz;
            }
        }

        public static Cursor NoMoveVert
        {
            get
            {
                if (noMoveVert == null)
                {
                    noMoveVert = new Cursor("nomovev.cur", 0);
                }
                return noMoveVert;
            }
        }

        public static Cursor PanEast
        {
            get
            {
                if (panEast == null)
                {
                    panEast = new Cursor("east.cur", 0);
                }
                return panEast;
            }
        }

        public static Cursor PanNE
        {
            get
            {
                if (panNE == null)
                {
                    panNE = new Cursor("ne.cur", 0);
                }
                return panNE;
            }
        }

        public static Cursor PanNorth
        {
            get
            {
                if (panNorth == null)
                {
                    panNorth = new Cursor("north.cur", 0);
                }
                return panNorth;
            }
        }

        public static Cursor PanNW
        {
            get
            {
                if (panNW == null)
                {
                    panNW = new Cursor("nw.cur", 0);
                }
                return panNW;
            }
        }

        public static Cursor PanSE
        {
            get
            {
                if (panSE == null)
                {
                    panSE = new Cursor("se.cur", 0);
                }
                return panSE;
            }
        }

        public static Cursor PanSouth
        {
            get
            {
                if (panSouth == null)
                {
                    panSouth = new Cursor("south.cur", 0);
                }
                return panSouth;
            }
        }

        public static Cursor PanSW
        {
            get
            {
                if (panSW == null)
                {
                    panSW = new Cursor("sw.cur", 0);
                }
                return panSW;
            }
        }

        public static Cursor PanWest
        {
            get
            {
                if (panWest == null)
                {
                    panWest = new Cursor("west.cur", 0);
                }
                return panWest;
            }
        }

        public static Cursor SizeAll
        {
            get
            {
                if (sizeAll == null)
                {
                    sizeAll = new Cursor(0x7f86, 0);
                }
                return sizeAll;
            }
        }

        public static Cursor SizeNESW
        {
            get
            {
                if (sizeNESW == null)
                {
                    sizeNESW = new Cursor(0x7f83, 0);
                }
                return sizeNESW;
            }
        }

        public static Cursor SizeNS
        {
            get
            {
                if (sizeNS == null)
                {
                    sizeNS = new Cursor(0x7f85, 0);
                }
                return sizeNS;
            }
        }

        public static Cursor SizeNWSE
        {
            get
            {
                if (sizeNWSE == null)
                {
                    sizeNWSE = new Cursor(0x7f82, 0);
                }
                return sizeNWSE;
            }
        }

        public static Cursor SizeWE
        {
            get
            {
                if (sizeWE == null)
                {
                    sizeWE = new Cursor(0x7f84, 0);
                }
                return sizeWE;
            }
        }

        public static Cursor UpArrow
        {
            get
            {
                if (upArrow == null)
                {
                    upArrow = new Cursor(0x7f04, 0);
                }
                return upArrow;
            }
        }

        public static Cursor VSplit
        {
            get
            {
                if (vSplit == null)
                {
                    vSplit = new Cursor("vsplit.cur", 0);
                }
                return vSplit;
            }
        }

        public static Cursor WaitCursor
        {
            get
            {
                if (wait == null)
                {
                    wait = new Cursor(0x7f02, 0);
                }
                return wait;
            }
        }
    }
}

