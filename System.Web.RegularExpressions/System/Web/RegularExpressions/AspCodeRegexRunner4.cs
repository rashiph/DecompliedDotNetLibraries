namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class AspCodeRegexRunner4 : RegexRunner
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
            int num4;
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
            if (((runtextpos == base.runtextstart) && (2 <= (runtextend - runtextpos))) && ((runtext[runtextpos] == '<') && (runtext[runtextpos + 1] == '%')))
            {
                runtextpos += 2;
                runstack[--runstackpos] = base.runtrack.Length - runtrackpos;
                runstack[--runstackpos] = this.Crawlpos();
                runtrack[--runtrackpos] = 2;
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 3;
                if (runtextpos < runtextend)
                {
                    runtextpos++;
                    if (runtext[runtextpos] == '@')
                    {
                        runtrackpos = base.runtrack.Length - runstack[runstackpos++];
                        int num1 = runstack[runstackpos++];
                        if (num1 != this.Crawlpos())
                        {
                            do
                            {
                                this.Uncapture();
                            }
                            while (num1 != this.Crawlpos());
                        }
                    }
                }
            }
            goto Label_0280;
        Label_01E1:
            num4 = runstack[runstackpos++];
            this.Capture(1, num4, runtextpos);
            runtrack[--runtrackpos] = num4;
            runtrack[--runtrackpos] = 6;
            if (((2 > (runtextend - runtextpos)) || (runtext[runtextpos] != '%')) || (runtext[runtextpos + 1] != '>'))
            {
                goto Label_0280;
            }
            runtextpos += 2;
            num4 = runstack[runstackpos++];
            this.Capture(0, num4, runtextpos);
            runtrack[--runtrackpos] = num4;
            runtrack[--runtrackpos] = 6;
        Label_0277:
            base.runtextpos = runtextpos;
            return;
        Label_0280:
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
                    goto Label_0280;

                case 2:
                    runstackpos += 2;
                    goto Label_0280;

                case 3:
                    runtextpos = runtrack[runtrackpos++];
                    num4 = runstack[runstackpos++];
                    runtrackpos = base.runtrack.Length - runstack[runstackpos++];
                    runtrack[--runtrackpos] = num4;
                    runtrack[--runtrackpos] = 4;
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 1;
                    num4 = runtextend - runtextpos;
                    if (num4 > 0)
                    {
                        runtrack[--runtrackpos] = num4 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 5;
                    }
                    goto Label_01E1;

                case 4:
                {
                    int num10 = runtrack[runtrackpos++];
                    if (num10 != this.Crawlpos())
                    {
                        do
                        {
                            this.Uncapture();
                        }
                        while (num10 != this.Crawlpos());
                    }
                    goto Label_0280;
                }
                case 5:
                {
                    runtextpos = runtrack[runtrackpos++];
                    int num5 = runtrack[runtrackpos++];
                    runtextpos++;
                    if (!RegexRunner.CharInClass(runtext[runtextpos], "\0\x0001\0\0"))
                    {
                        goto Label_0280;
                    }
                    if (num5 > 0)
                    {
                        runtrack[--runtrackpos] = num5 - 1;
                        runtrack[--runtrackpos] = runtextpos;
                        runtrack[--runtrackpos] = 5;
                    }
                    goto Label_01E1;
                }
                case 6:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_0280;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0277;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 10;
        }
    }
}

