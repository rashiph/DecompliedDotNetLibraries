namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class DirectiveRegexRunner2 : RegexRunner
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
                goto Label_0CCC;
            }
            runtextpos += 2;
            int start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_00FC;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_00FC:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_012F:
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (runtext[runtextpos] != '@')
            {
                goto Label_0CCC;
            }
            runstack[--runstackpos] = -1;
            runtrack[--runtrackpos] = 1;
            goto Label_0BB7;
        Label_016B:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_01C4;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_01C4:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 3;
            }
        Label_01F7:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
            {
                goto Label_0CCC;
            }
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0277;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\x0002\t:;\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
            runtextpos--;
        Label_0277:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 4;
            }
        Label_02AA:
            runstack[--runstackpos] = base.runtrack.Length - runtrackpos;
            runstack[--runstackpos] = this.Crawlpos();
            runtrack[--runtrackpos] = 5;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (!RegexRunner.CharInClass(runtext[runtextpos], "\x0001\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"))
            {
                goto Label_0CCC;
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
                    goto Label_0415;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0415:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 10;
            }
        Label_0448:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0CCC;
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
                    goto Label_04F0;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_04F0:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 11;
            }
        Label_0523:
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0CCC;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_058D;
                }
                runtextpos++;
            }
            while (runtext[runtextpos] != '"');
            runtextpos--;
        Label_058D:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 12;
            }
        Label_05C0:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (runtext[runtextpos] != '"')
            {
                goto Label_0CCC;
            }
            goto Label_0B57;
        Label_066D:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 14;
            }
        Label_06A0:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0CCC;
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
                    goto Label_0748;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_0748:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 15;
            }
        Label_077B:
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (runtext[runtextpos] != '\'')
            {
                goto Label_0CCC;
            }
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_07E5;
                }
                runtextpos++;
            }
            while (runtext[runtextpos] != '\'');
            runtextpos--;
        Label_07E5:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x10;
            }
        Label_0818:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (runtext[runtextpos] != '\'')
            {
                goto Label_0CCC;
            }
            goto Label_0B57;
        Label_08C5:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x12;
            }
        Label_08F8:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if (runtextpos >= runtextend)
            {
                goto Label_0CCC;
            }
            runtextpos++;
            if (runtext[runtextpos] != '=')
            {
                goto Label_0CCC;
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
                    goto Label_09A0;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
            runtextpos--;
        Label_09A0:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 0x13;
            }
        Label_09D3:
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0A2C;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(runtext[runtextpos], "\x0001\b\x0001\"#%&'(>?d"));
            runtextpos--;
        Label_0A2C:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 20;
            }
        Label_0A5F:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            goto Label_0B57;
        Label_0B27:
            start = runstack[runstackpos++];
            this.Capture(5, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
        Label_0B57:
            start = runstack[runstackpos++];
            this.Capture(2, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
            start = runstack[runstackpos++];
            this.Capture(1, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
        Label_0BB7:
            runtrack[--runtrackpos] = start;
            if ((start = runstack[runstackpos++]) != runtextpos)
            {
                runtrack[--runtrackpos] = runtextpos;
                runstack[--runstackpos] = runtextpos;
                runtrack[--runtrackpos] = 0x16;
                if ((runtrackpos > 0xcc) && (runstackpos > 0x99))
                {
                    goto Label_016B;
                }
                runtrack[--runtrackpos] = 0x17;
                goto Label_0CCC;
            }
            runtrack[--runtrackpos] = 0x18;
        Label_0C2A:
            if ((start = runtextend - runtextpos) > 0)
            {
                runtrack[--runtrackpos] = start - 1;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 0x19;
            }
        Label_0C5D:
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '%')) || (runtext[runtextpos + 1] != '>'))
            {
                goto Label_0CCC;
            }
            runtextpos += 2;
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 8;
        Label_0CC3:
            base.runtextpos = runtextpos;
            return;
        Label_0CCC:
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
                    goto Label_0CCC;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_012F;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 3;
                    }
                    goto Label_01F7;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 4;
                    }
                    goto Label_02AA;

                case 5:
                    runstackpos += 2;
                    goto Label_0CCC;

                case 6:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    goto Label_0CCC;

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
                    goto Label_0CCC;
                }
                case 8:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0CCC;

                case 9:
                    runtextpos = runtrack[runtrackpos++];
                    runtrack[--runtrackpos] = runtextpos;
                    runtrack[--runtrackpos] = 13;
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_066D;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
                    runtextpos--;
                    goto Label_066D;

                case 10:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 10;
                    }
                    goto Label_0448;

                case 11:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 11;
                    }
                    goto Label_0523;

                case 12:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 12;
                    }
                    goto Label_05C0;

                case 13:
                    runtextpos = runtrack[runtrackpos++];
                    runtrack[--runtrackpos] = runtextpos;
                    runtrack[--runtrackpos] = 0x11;
                    start = (num5 = runtextend - runtextpos) + 1;
                    do
                    {
                        if (--start <= 0)
                        {
                            goto Label_08C5;
                        }
                        runtextpos++;
                    }
                    while (RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"));
                    runtextpos--;
                    goto Label_08C5;

                case 14:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 14;
                    }
                    goto Label_06A0;

                case 15:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 15;
                    }
                    goto Label_077B;

                case 0x10:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x10;
                    }
                    goto Label_0818;

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
                    goto Label_0B27;

                case 0x12:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x12;
                    }
                    goto Label_08F8;

                case 0x13:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 0x13;
                    }
                    goto Label_09D3;

                case 20:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 20;
                    }
                    goto Label_0A5F;

                case 0x15:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"))
                    {
                        goto Label_0CCC;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 0x15;
                    }
                    goto Label_0B27;

                case 0x16:
                {
                    runtextpos = runtrack[runtrackpos++];
                    int num10 = runstack[runstackpos++];
                    runtrack[--runtrackpos] = 0x18;
                    goto Label_0C2A;
                }
                case 0x17:
                    goto Label_016B;

                case 0x18:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    goto Label_0CCC;

                case 0x19:
                    runtextpos = runtrack[runtrackpos++];
                    num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\0\x0001d"))
                    {
                        goto Label_0CCC;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 0x19;
                    }
                    goto Label_0C5D;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0CC3;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 0x33;
        }
    }
}

