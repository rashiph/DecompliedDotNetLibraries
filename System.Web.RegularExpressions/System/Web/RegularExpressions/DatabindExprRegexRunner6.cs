namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class DatabindExprRegexRunner6 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            if (base.runtextpos > base.runtextstart)
            {
                base.runtextpos = base.runtextend;
                return false;
            }
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
            if ((((runtextpos != base.runtextstart) || (3 > (runtextend - runtextpos))) || ((runtext[runtextpos] != '<') || (runtext[runtextpos + 1] != '%'))) || (runtext[runtextpos + 2] != '#'))
            {
                goto Label_0275;
            }
            runtextpos += 3;
            runstack[--runstackpos] = -1;
            runstack[--runstackpos] = 0;
            runtrack[--runtrackpos] = 2;
            goto Label_0172;
        Label_00F7:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            int start = runtextend - runtextpos;
            if (start > 0)
            {
                runtrack[--runtrackpos] = start - 1;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 3;
            }
        Label_0142:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 4;
        Label_0172:
            start = runstack[runstackpos++];
            runtrack[--runtrackpos] = num5;
            if ((((num5 = runstack[runstackpos++]) != runtextpos) || (start < 0)) && (start < 1))
            {
                runstack[--runstackpos] = runtextpos;
                runstack[--runstackpos] = start + 1;
                runtrack[--runtrackpos] = 5;
                if ((runtrackpos > 0x24) && (runstackpos > 0x1b))
                {
                    goto Label_00F7;
                }
                runtrack[--runtrackpos] = 6;
                goto Label_0275;
            }
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 7;
        Label_0206:
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '%')) || (runtext[runtextpos + 1] != '>'))
            {
                goto Label_0275;
            }
            runtextpos += 2;
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 4;
        Label_026C:
            base.runtextpos = runtextpos;
            return;
        Label_0275:
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
                    goto Label_0275;

                case 2:
                    runstackpos += 2;
                    goto Label_0275;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"))
                    {
                        goto Label_0275;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 3;
                    }
                    goto Label_0142;

                case 4:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0275;

                case 5:
                    start = runstack[runstackpos++] - 1;
                    if (start < 0)
                    {
                        runstack[runstackpos] = runtrack[runtrackpos++];
                        runstack[--runstackpos] = start;
                        goto Label_0275;
                    }
                    runtextpos = runstack[runstackpos++];
                    runtrack[--runtrackpos] = start;
                    runtrack[--runtrackpos] = 7;
                    goto Label_0206;

                case 6:
                    goto Label_00F7;

                case 7:
                    start = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    runstack[--runstackpos] = start;
                    goto Label_0275;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_026C;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 9;
        }
    }
}

