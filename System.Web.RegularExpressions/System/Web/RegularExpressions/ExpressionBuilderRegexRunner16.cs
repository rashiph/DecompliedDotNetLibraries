namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class ExpressionBuilderRegexRunner16 : RegexRunner
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
                goto Label_049B;
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
                goto Label_049B;
            }
            runtextpos += 2;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0170;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0170:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 3;
            }
        Label_01A3:
            if (runtextpos >= runtextend)
            {
                goto Label_049B;
            }
            runtextpos++;
            if (runtext[runtextpos] != '$')
            {
                goto Label_049B;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0203;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0203:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 4;
            }
        Label_0236:
            runstack[--runstackpos] = -1;
            runstack[--runstackpos] = 0;
            runtrack[--runtrackpos] = 5;
            goto Label_031B;
        Label_025F:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_02B8;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"));
            runtextpos--;
        Label_02B8:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 6;
            }
        Label_02EB:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 7;
        Label_031B:
            start = runstack[runstackpos++];
            runtrack[--runtrackpos] = num5;
            if ((((num5 = runstack[runstackpos++]) != runtextpos) || (start < 0)) && (start < 1))
            {
                runstack[--runstackpos] = runtextpos;
                runstack[--runstackpos] = start + 1;
                runtrack[--runtrackpos] = 8;
                if ((runtrackpos > 0x34) && (runstackpos > 0x27))
                {
                    goto Label_025F;
                }
                runtrack[--runtrackpos] = 9;
                goto Label_049B;
            }
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 10;
        Label_03AF:
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '%')) || (runtext[runtextpos + 1] != '>'))
            {
                goto Label_049B;
            }
            runtextpos += 2;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0426;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0426:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 11;
            }
        Label_0459:
            if (runtextpos < runtextend)
            {
                goto Label_049B;
            }
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 7;
        Label_0492:
            base.runtextpos = runtextpos;
            return;
        Label_049B:
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
                    goto Label_049B;

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
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 3;
                    }
                    goto Label_01A3;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 4;
                    }
                    goto Label_0236;

                case 5:
                    runstackpos += 2;
                    goto Label_049B;

                case 6:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 6;
                    }
                    goto Label_02EB;

                case 7:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_049B;

                case 8:
                    start = runstack[runstackpos++] - 1;
                    if (start < 0)
                    {
                        runstack[runstackpos] = runtrack[runtrackpos++];
                        runstack[--runstackpos] = start;
                        goto Label_049B;
                    }
                    runtextpos = runstack[runstackpos++];
                    runtrack[--runtrackpos] = start;
                    runtrack[--runtrackpos] = 10;
                    goto Label_03AF;

                case 9:
                    goto Label_025F;

                case 10:
                    start = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    runstack[--runstackpos] = start;
                    goto Label_049B;

                case 11:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 11;
                    }
                    goto Label_0459;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0492;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 13;
        }
    }
}

