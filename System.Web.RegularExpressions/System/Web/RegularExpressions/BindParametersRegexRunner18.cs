namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class BindParametersRegexRunner18 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            int runtextpos = base.runtextpos;
            string runtext = base.runtext;
            int num3 = base.runtextend - runtextpos;
            if (num3 <= 0)
            {
                return false;
            }
            num3--;
            runtextpos++;
            if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\x0004\x0001\"#'(d"))
            {
            }
            runtextpos--;
            base.runtextpos = runtextpos;
            return (num3 > 0);
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
            int start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_00B9;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_00B9:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_00EC:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            runtrack[--runtrackpos] = runtextpos;
            runtrack[--runtrackpos] = 3;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0E4C;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0E4C;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            runtrack[--runtrackpos] = runtextpos;
            runtrack[--runtrackpos] = 4;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (1 > (runtextend - runtextpos))
            {
                goto Label_0E4C;
            }
            runtextpos++;
            start = 1;
            do
            {
                start--;
                if (!RegexRunner.CharInClass(runtext[runtextpos - start], "\0\x0002\t./\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
                {
                    goto Label_0E4C;
                }
            }
            while (start > 0);
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0235;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0002\t./\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
            runtextpos--;
        Label_0235:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 5;
            }
        Label_0268:
            start = runstack[runstackpos++];
            this.Capture(4, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            goto Label_03D8;
        Label_0356:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 7;
            }
        Label_0389:
            if (runtextpos >= runtextend)
            {
                goto Label_0E4C;
            }
            runtextpos++;
            if (runtext[runtextpos] != ']')
            {
                goto Label_0E4C;
            }
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
        Label_03D8:
            start = runstack[runstackpos++];
            this.Capture(3, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            start = runstack[runstackpos++];
            this.Capture(14, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            if (runtextpos >= runtextend)
            {
                goto Label_0E4C;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0E4C;
            }
            start = runstack[runstackpos++];
            this.Capture(2, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            goto Label_07F7;
        Label_05A5:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 9;
            }
        Label_05D8:
            start = runstack[runstackpos++];
            this.Capture(8, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            goto Label_0748;
        Label_06C6:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 10;
            }
        Label_06F9:
            if (runtextpos >= runtextend)
            {
                goto Label_0E4C;
            }
            runtextpos++;
            if (runtext[runtextpos] != ']')
            {
                goto Label_0E4C;
            }
            start = runstack[runstackpos++];
            this.Capture(9, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
        Label_0748:
            start = runstack[runstackpos++];
            this.Capture(7, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            start = runstack[runstackpos++];
            this.Capture(14, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            if (runtextpos >= runtextend)
            {
                goto Label_0E4C;
            }
            runtextpos++;
            if (runtext[runtextpos] != '\'')
            {
                goto Label_0E4C;
            }
            start = runstack[runstackpos++];
            this.Capture(6, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
        Label_07F7:
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0868;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0868:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 11;
            }
        Label_089B:
            runstack[--runstackpos] = -1;
            runstack[--runstackpos] = 0;
            runtrack[--runtrackpos] = 12;
            goto Label_0CFC;
        Label_08C4:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0E4C;
            }
            runtextpos++;
            if (runtext[runtextpos] != ',')
            {
                goto Label_0E4C;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_093C;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_093C:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 13;
            }
        Label_096F:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            runtrack[--runtrackpos] = runtextpos;
            runtrack[--runtrackpos] = 14;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0E4C;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0E4C;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0A2F;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"));
            runtextpos--;
        Label_0A2F:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 15;
            }
        Label_0A62:
            start = runstack[runstackpos++];
            this.Capture(15, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            if (runtextpos >= runtextend)
            {
                goto Label_0E4C;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0E4C;
            }
            start = runstack[runstackpos++];
            this.Capture(12, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            goto Label_0C28;
        Label_0B76:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x10;
            }
        Label_0BA9:
            start = runstack[runstackpos++];
            this.Capture(15, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            if (runtextpos >= runtextend)
            {
                goto Label_0E4C;
            }
            runtextpos++;
            if (runtext[runtextpos] != '\'')
            {
                goto Label_0E4C;
            }
            start = runstack[runstackpos++];
            this.Capture(13, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
        Label_0C28:
            start = runstack[runstackpos++];
            this.Capture(11, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0C99;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0C99:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x11;
            }
        Label_0CCC:
            start = runstack[runstackpos++];
            this.Capture(10, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
        Label_0CFC:
            start = runstack[runstackpos++];
            runtrack[--runtrackpos] = num5;
            if ((((num5 = runstack[runstackpos++]) != runtextpos) || (start < 0)) && (start < 1))
            {
                runstack[--runstackpos] = runtextpos;
                runstack[--runstackpos] = start + 1;
                runtrack[--runtrackpos] = 0x12;
                if ((runtrackpos > 0xec) && (runstackpos > 0xb1))
                {
                    goto Label_08C4;
                }
                runtrack[--runtrackpos] = 0x13;
                goto Label_0E4C;
            }
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 20;
        Label_0D96:
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0DD7;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0DD7:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x15;
            }
        Label_0E0A:
            if (runtextpos < runtextend)
            {
                goto Label_0E4C;
            }
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 6;
        Label_0E43:
            base.runtextpos = runtextpos;
            return;
        Label_0E4C:
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
                    goto Label_0E4C;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_00EC;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    if (runtextpos >= runtextend)
                    {
                        goto Label_0E4C;
                    }
                    runtextpos++;
                    if (runtext[runtextpos] != '\'')
                    {
                        goto Label_0E4C;
                    }
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    runtrack[--runtrackpos] = runtextpos;
                    runtrack[--runtrackpos] = 8;
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    if (1 > (runtextend - runtextpos))
                    {
                        goto Label_0E4C;
                    }
                    runtextpos++;
                    start = 1;
                    do
                    {
                        start--;
                        if (!RegexRunner.CharInClass(runtext[runtextpos - start], "\0\x0002\t./\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
                        {
                            goto Label_0E4C;
                        }
                    }
                    while (start > 0);
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_05A5;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0002\t./\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
                    runtextpos--;
                    goto Label_05A5;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    if (runtextpos >= runtextend)
                    {
                        goto Label_0E4C;
                    }
                    runtextpos++;
                    if ((runtext[runtextpos] != '[') || (1 > (runtextend - runtextpos)))
                    {
                        goto Label_0E4C;
                    }
                    runtextpos++;
                    start = 1;
                    do
                    {
                        start--;
                        if (!RegexRunner.CharInClass(runtext[runtextpos - start], "\0\x0001\0\0"))
                        {
                            goto Label_0E4C;
                        }
                    }
                    while (start > 0);
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_0356;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"));
                    runtextpos--;
                    goto Label_0356;

                case 5:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 5;
                    }
                    goto Label_0268;

                case 6:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0E4C;

                case 7:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 7;
                    }
                    goto Label_0389;

                case 8:
                    runtextpos = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    if (runtextpos >= runtextend)
                    {
                        goto Label_0E4C;
                    }
                    runtextpos++;
                    if ((runtext[runtextpos] != '[') || (1 > (runtextend - runtextpos)))
                    {
                        goto Label_0E4C;
                    }
                    runtextpos++;
                    start = 1;
                    do
                    {
                        start--;
                        if (!RegexRunner.CharInClass(runtext[runtextpos - start], "\0\x0001\0\0"))
                        {
                            goto Label_0E4C;
                        }
                    }
                    while (start > 0);
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_06C6;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"));
                    runtextpos--;
                    goto Label_06C6;

                case 9:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 9;
                    }
                    goto Label_05D8;

                case 10:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 10;
                    }
                    goto Label_06F9;

                case 11:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 11;
                    }
                    goto Label_089B;

                case 12:
                    runstackpos += 2;
                    goto Label_0E4C;

                case 13:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 13;
                    }
                    goto Label_096F;

                case 14:
                    runtextpos = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    if (runtextpos >= runtextend)
                    {
                        goto Label_0E4C;
                    }
                    runtextpos++;
                    if (runtext[runtextpos] != '\'')
                    {
                        goto Label_0E4C;
                    }
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_0B76;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"));
                    runtextpos--;
                    goto Label_0B76;

                case 15:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 15;
                    }
                    goto Label_0A62;

                case 0x10:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x10;
                    }
                    goto Label_0BA9;

                case 0x11:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x11;
                    }
                    goto Label_0CCC;

                case 0x12:
                    start = runstack[runstackpos++] - 1;
                    if (start < 0)
                    {
                        runstack[runstackpos] = runtrack[runtrackpos++];
                        runstack[--runstackpos] = start;
                        goto Label_0E4C;
                    }
                    runtextpos = runstack[runstackpos++];
                    runtrack[--runtrackpos] = start;
                    runtrack[--runtrackpos] = 20;
                    goto Label_0D96;

                case 0x13:
                    goto Label_08C4;

                case 20:
                    start = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    runstack[--runstackpos] = start;
                    goto Label_0E4C;

                case 0x15:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x15;
                    }
                    goto Label_0E0A;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0E43;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 0x3b;
        }
    }
}

