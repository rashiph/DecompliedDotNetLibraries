namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class FormatStringRegexRunner19 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            return true;
        }

        public override void Go()
        {
            int num5;
            string runtext = base.runtext;
            int runtextstart = base.runtextstart;
            int runtextbeg = base.runtextbeg;
            int runtextend = base.runtextend;
            int runtextpos = base.runtextpos;
            int[] runtrack = base.runtrack;
            int runtrackpos = base.runtrackpos;
            int[] runstack = base.runstack;
            int runstackpos = base.runstackpos;
            runtrack[--runtrackpos] = runtextpos;
            runtrack[--runtrackpos] = 0;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if ((runtextpos > runtextbeg) && (runtext[runtextpos - 1] != '\n'))
            {
                goto Label_039B;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            runstack[--runstackpos] = -1;
            runtrack[--runtrackpos] = 1;
            goto Label_02AF;
        Label_00C6:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            int start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0111;
                }
                runtextpos++;
            }
            while (runtext[runtextpos] != '"');
            runtextpos--;
        Label_0111:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_0144:
            runstack[--runstackpos] = -1;
            runstack[--runstackpos] = 0;
            runtrack[--runtrackpos] = 3;
            goto Label_01EB;
        Label_016D:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '"')) || (runtext[runtextpos + 1] != '"'))
            {
                goto Label_039B;
            }
            runtextpos += 2;
            start = runstack[runstackpos++];
            this.Capture(3, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 4;
        Label_01EB:
            start = runstack[runstackpos++];
            runtrack[--runtrackpos] = num5;
            if ((((num5 = runstack[runstackpos++]) != runtextpos) || (start < 0)) && (start < 1))
            {
                runstack[--runstackpos] = runtextpos;
                runstack[--runstackpos] = start + 1;
                runtrack[--runtrackpos] = 5;
                if ((runtrackpos > 60) && (runstackpos > 0x2d))
                {
                    goto Label_016D;
                }
                runtrack[--runtrackpos] = 6;
                goto Label_039B;
            }
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 7;
        Label_027F:
            start = runstack[runstackpos++];
            this.Capture(2, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 4;
        Label_02AF:
            runtrack[--runtrackpos] = start;
            if ((start = runstack[runstackpos++]) != runtextpos)
            {
                runtrack[--runtrackpos] = runtextpos;
                runstack[--runstackpos] = runtextpos;
                runtrack[--runtrackpos] = 8;
                if ((runtrackpos > 60) && (runstackpos > 0x2d))
                {
                    goto Label_00C6;
                }
                runtrack[--runtrackpos] = 9;
                goto Label_039B;
            }
            runtrack[--runtrackpos] = 10;
        Label_031C:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 4;
            if ((runtextpos < runtextend) && (runtext[runtextpos] != '\n'))
            {
                goto Label_039B;
            }
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 4;
        Label_0392:
            base.runtextpos = runtextpos;
            return;
        Label_039B:
            base.runtrackpos = runtrackpos;
            base.runstackpos = runstackpos;
            this.EnsureStorage();
            runtrackpos = base.runtrackpos;
            runstackpos = base.runstackpos;
            runtrack = base.runtrack;
            runstack = base.runstack;
            switch (runtrack[runtrackpos++])
            {
                case 1:
                    runstackpos++;
                    goto Label_039B;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_0144;

                case 3:
                    runstackpos += 2;
                    goto Label_039B;

                case 4:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_039B;

                case 5:
                    start = runstack[runstackpos++] - 1;
                    if (start < 0)
                    {
                        runstack[runstackpos] = runtrack[runtrackpos++];
                        runstack[--runstackpos] = start;
                        goto Label_039B;
                    }
                    runtextpos = runstack[runstackpos++];
                    runtrack[--runtrackpos] = start;
                    runtrack[--runtrackpos] = 7;
                    goto Label_027F;

                case 6:
                    goto Label_016D;

                case 7:
                    start = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    runstack[--runstackpos] = start;
                    goto Label_039B;

                case 8:
                {
                    runtextpos = runtrack[runtrackpos++];
                    int num1 = runstack[runstackpos++];
                    runtrack[--runtrackpos] = 10;
                    goto Label_031C;
                }
                case 9:
                    goto Label_00C6;

                case 10:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    goto Label_039B;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0392;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 15;
        }
    }
}

