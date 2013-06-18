namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class SimpleDirectiveRegexRunner14 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            // This item is obfuscated and can not be translated.
            string runtext = base.runtext;
            int runtextend = base.runtextend;
            for (int i = base.runtextpos + 1; i < runtextend; i = 2 + i)
            {
                int num2 = runtext[i];
                if (num2 == '%')
                {
                    num2 = i;
                    if (runtext[--num2] == '<')
                    {
                        base.runtextpos = num2;
                        return true;
                    }
                }
                else
                {
                    num2 -= 0x25;
                    if (num2 <= 0x17)
                    {
                    }
                }
            }
            base.runtextpos = base.runtextend;
            return false;
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
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '<')) || (runtext[runtextpos + 1] != '%'))
            {
                goto Label_0CBF;
            }
            runtextpos += 2;
            int start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_00EF;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_00EF:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_0122:
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (runtext[runtextpos] != '@')
            {
                goto Label_0CBF;
            }
            runstack[--runstackpos] = -1;
            runtrack[--runtrackpos] = 1;
            goto Label_0BAA;
        Label_015E:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_01B7;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_01B7:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 3;
            }
        Label_01EA:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
            {
                goto Label_0CBF;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_026A;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0002\t:;\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
            runtextpos--;
        Label_026A:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 4;
            }
        Label_029D:
            runstack[--runstackpos] = base.runtrack.Length - runtrackpos;
            runstack[--runstackpos] = this.Crawlpos();
            runtrack[--runtrackpos] = 5;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (!RegexRunner.CharInClass(runtext[runtextpos], "\x0001\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
            {
                goto Label_0CBF;
            }
            runtrack[--runtrackpos] = runtextpos = runstack[runstackpos++];
            runtrack[--runtrackpos] = 6;
            start = runstack[runstackpos++];
            runtrackpos = base.runtrack.Length - runstack[runstackpos++];
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 7;
            start = runstack[runstackpos++];
            this.Capture(3, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            runtrack[--runtrackpos] = runtextpos;
            runtrack[--runtrackpos] = 9;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0408;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0408:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 10;
            }
        Label_043B:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0CBF;
            }
            start = runstack[runstackpos++];
            this.Capture(4, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_04E3;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_04E3:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 11;
            }
        Label_0516:
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0CBF;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0580;
                }
                runtextpos++;
            }
            while (runtext[runtextpos] != '"');
            runtextpos--;
        Label_0580:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 12;
            }
        Label_05B3:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0CBF;
            }
            goto Label_0B4A;
        Label_0660:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 14;
            }
        Label_0693:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0CBF;
            }
            start = runstack[runstackpos++];
            this.Capture(4, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_073B;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_073B:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 15;
            }
        Label_076E:
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (runtext[runtextpos] != '\'')
            {
                goto Label_0CBF;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_07D8;
                }
                runtextpos++;
            }
            while (runtext[runtextpos] != '\'');
            runtextpos--;
        Label_07D8:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x10;
            }
        Label_080B:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (runtext[runtextpos] != '\'')
            {
                goto Label_0CBF;
            }
            goto Label_0B4A;
        Label_08B8:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x12;
            }
        Label_08EB:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CBF;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0CBF;
            }
            start = runstack[runstackpos++];
            this.Capture(4, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0993;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0993:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x13;
            }
        Label_09C6:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0A1F;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\x0001\b\x0001\"#%&'(>?d"));
            runtextpos--;
        Label_0A1F:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 20;
            }
        Label_0A52:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            goto Label_0B4A;
        Label_0B1A:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
        Label_0B4A:
            start = runstack[runstackpos++];
            this.Capture(2, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
        Label_0BAA:
            runtrack[--runtrackpos] = start;
            if ((start = runstack[runstackpos++]) != runtextpos)
            {
                runtrack[--runtrackpos] = runtextpos;
                runstack[--runstackpos] = runtextpos;
                runtrack[--runtrackpos] = 0x16;
                if ((runtrackpos > 0xcc) && (runstackpos > 0x99))
                {
                    goto Label_015E;
                }
                runtrack[--runtrackpos] = 0x17;
                goto Label_0CBF;
            }
            runtrack[--runtrackpos] = 0x18;
        Label_0C1D:
            if ((start = runtextend - runtextpos) > 0)
            {
                runtrack[--runtrackpos] = start - 1;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 0x19;
            }
        Label_0C50:
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '%')) || (runtext[runtextpos + 1] != '>'))
            {
                goto Label_0CBF;
            }
            runtextpos += 2;
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
        Label_0CB6:
            base.runtextpos = runtextpos;
            return;
        Label_0CBF:
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
                    goto Label_0CBF;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_0122;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 3;
                    }
                    goto Label_01EA;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 4;
                    }
                    goto Label_029D;

                case 5:
                    runstackpos += 2;
                    goto Label_0CBF;

                case 6:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    goto Label_0CBF;

                case 7:
                {
                    int num1 = runtrack[runtrackpos++];
                    if (num1 != this.Crawlpos())
                    {
                        do
                        {
                            this.Uncapture();
                        }
                        while (num1 != this.Crawlpos());
                    }
                    goto Label_0CBF;
                }
                case 8:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0CBF;

                case 9:
                    runtextpos = runtrack[runtrackpos++];
                    runtrack[--runtrackpos] = runtextpos;
                    runtrack[--runtrackpos] = 13;
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_0660;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
                    runtextpos--;
                    goto Label_0660;

                case 10:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 10;
                    }
                    goto Label_043B;

                case 11:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 11;
                    }
                    goto Label_0516;

                case 12:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 12;
                    }
                    goto Label_05B3;

                case 13:
                    runtextpos = runtrack[runtrackpos++];
                    runtrack[--runtrackpos] = runtextpos;
                    runtrack[--runtrackpos] = 0x11;
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_08B8;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
                    runtextpos--;
                    goto Label_08B8;

                case 14:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 14;
                    }
                    goto Label_0693;

                case 15:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 15;
                    }
                    goto Label_076E;

                case 0x10:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x10;
                    }
                    goto Label_080B;

                case 0x11:
                    runtextpos = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    start = runstack[runstackpos++];
                    this.Capture(4, start, runtextpos);
                    runtrack[--runtrackpos] = start;
                    runtrack[--runtrackpos] = 8;
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    start = runtextend - runtextpos;
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 0x15;
                    }
                    goto Label_0B1A;

                case 0x12:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x12;
                    }
                    goto Label_08EB;

                case 0x13:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x13;
                    }
                    goto Label_09C6;

                case 20:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 20;
                    }
                    goto Label_0A52;

                case 0x15:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"))
                    {
                        goto Label_0CBF;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 0x15;
                    }
                    goto Label_0B1A;

                case 0x16:
                {
                    runtextpos = runtrack[runtrackpos++];
                    int num10 = runstack[runstackpos++];
                    runtrack[--runtrackpos] = 0x18;
                    goto Label_0C1D;
                }
                case 0x17:
                    goto Label_015E;

                case 0x18:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    goto Label_0CBF;

                case 0x19:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"))
                    {
                        goto Label_0CBF;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 0x19;
                    }
                    goto Label_0C50;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0CB6;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 0x33;
        }
    }
}

