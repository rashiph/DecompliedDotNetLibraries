namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class AspExprRegexRunner5 : RegexRunner
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
            if (((runtextpos != base.runtextstart) || (2 > (runtextend - runtextpos))) || ((runtext[runtextpos] != '<') || (runtext[runtextpos + 1] != '%')))
            {
                goto Label_02B4;
            }
            runtextpos += 2;
            int start = runtextend - runtextpos;
            if (start > 0)
            {
                runtrack[--runtrackpos] = start - 1;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 2;
            }
        Label_00EE:
            if (runtextpos >= runtextend)
            {
                goto Label_02B4;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_02B4;
            }
            runstack[--runstackpos] = -1;
            runstack[--runstackpos] = 0;
            runtrack[--runtrackpos] = 3;
            goto Label_01B1;
        Label_0136:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = runtextend - runtextpos;
            if (start > 0)
            {
                runtrack[--runtrackpos] = start - 1;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 4;
            }
        Label_0181:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 5;
        Label_01B1:
            start = runstack[runstackpos++];
            runtrack[--runtrackpos] = num5;
            if ((((num5 = runstack[runstackpos++]) != runtextpos) || (start < 0)) && (start < 1))
            {
                runstack[--runstackpos] = runtextpos;
                runstack[--runstackpos] = start + 1;
                runtrack[--runtrackpos] = 6;
                if ((runtrackpos > 40) && (runstackpos > 30))
                {
                    goto Label_0136;
                }
                runtrack[--runtrackpos] = 7;
                goto Label_02B4;
            }
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
        Label_0245:
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '%')) || (runtext[runtextpos + 1] != '>'))
            {
                goto Label_02B4;
            }
            runtextpos += 2;
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 5;
        Label_02AB:
            base.runtextpos = runtextpos;
            return;
        Label_02B4:
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
                    goto Label_02B4;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"))
                    {
                        goto Label_02B4;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_00EE;

                case 3:
                    runstackpos += 2;
                    goto Label_02B4;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"))
                    {
                        goto Label_02B4;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 4;
                    }
                    goto Label_0181;

                case 5:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_02B4;

                case 6:
                    start = runstack[runstackpos++] - 1;
                    if (start < 0)
                    {
                        runstack[runstackpos] = runtrack[runtrackpos++];
                        runstack[--runstackpos] = start;
                        goto Label_02B4;
                    }
                    runtextpos = runstack[runstackpos++];
                    runtrack[--runtrackpos] = start;
                    runtrack[--runtrackpos] = 8;
                    goto Label_0245;

                case 7:
                    goto Label_0136;

                case 8:
                    start = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    runstack[--runstackpos] = start;
                    goto Label_02B4;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_02AB;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 10;
        }
    }
}

