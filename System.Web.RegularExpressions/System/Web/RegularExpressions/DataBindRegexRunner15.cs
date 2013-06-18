namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class DataBindRegexRunner15 : RegexRunner
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
            if (runtextpos != base.runtextstart)
            {
                goto Label_03A5;
            }
            int start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_00C6;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_00C6:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_00F9:
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '<')) || (runtext[runtextpos + 1] != '%'))
            {
                goto Label_03A5;
            }
            runtextpos += 2;
            start = runtextend - runtextpos;
            if (start > 0)
            {
                runtrack[--runtrackpos] = start - 1;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 3;
            }
        Label_0162:
            if (runtextpos >= runtextend)
            {
                goto Label_03A5;
            }
            runtextpos++;
            if (runtext[runtextpos] != '#')
            {
                goto Label_03A5;
            }
            runstack[--runstackpos] = -1;
            runstack[--runstackpos] = 0;
            runtrack[--runtrackpos] = 4;
            goto Label_0225;
        Label_01AA:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = runtextend - runtextpos;
            if (start > 0)
            {
                runtrack[--runtrackpos] = start - 1;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 5;
            }
        Label_01F5:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
        Label_0225:
            start = runstack[runstackpos++];
            runtrack[--runtrackpos] = num5;
            if ((((num5 = runstack[runstackpos++]) != runtextpos) || (start < 0)) && (start < 1))
            {
                runstack[--runstackpos] = runtextpos;
                runstack[--runstackpos] = start + 1;
                runtrack[--runtrackpos] = 7;
                if ((runtrackpos > 0x30) && (runstackpos > 0x24))
                {
                    goto Label_01AA;
                }
                runtrack[--runtrackpos] = 8;
                goto Label_03A5;
            }
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 9;
        Label_02B9:
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '%')) || (runtext[runtextpos + 1] != '>'))
            {
                goto Label_03A5;
            }
            runtextpos += 2;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0330;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0330:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 10;
            }
        Label_0363:
            if (runtextpos < runtextend)
            {
                goto Label_03A5;
            }
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
        Label_039C:
            base.runtextpos = runtextpos;
            return;
        Label_03A5:
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
                    goto Label_03A5;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_00F9;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"))
                    {
                        goto Label_03A5;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 3;
                    }
                    goto Label_0162;

                case 4:
                    runstackpos += 2;
                    goto Label_03A5;

                case 5:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"))
                    {
                        goto Label_03A5;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 5;
                    }
                    goto Label_01F5;

                case 6:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_03A5;

                case 7:
                    start = runstack[runstackpos++] - 1;
                    if (start < 0)
                    {
                        runstack[runstackpos] = runtrack[runtrackpos++];
                        runstack[--runstackpos] = start;
                        goto Label_03A5;
                    }
                    runtextpos = runstack[runstackpos++];
                    runtrack[--runtrackpos] = start;
                    runtrack[--runtrackpos] = 9;
                    goto Label_02B9;

                case 8:
                    goto Label_01AA;

                case 9:
                    start = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    runstack[--runstackpos] = start;
                    goto Label_03A5;

                case 10:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 10;
                    }
                    goto Label_0363;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_039C;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 12;
        }
    }
}

