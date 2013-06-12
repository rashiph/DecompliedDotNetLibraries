namespace System.IO.Compression
{
    using System;

    internal class Deflater
    {
        private const double BadCompressionThreshold = 1.0;
        private const int CleanCopySize = 0xf88;
        private CopyEncoder copyEncoder = new CopyEncoder();
        private FastEncoder deflateEncoder = new FastEncoder();
        private DeflateInput input = new DeflateInput();
        private DeflateInput inputFromHistory;
        private const int MaxHeaderFooterGoo = 120;
        private const int MinBlockSize = 0x100;
        private OutputBuffer output = new OutputBuffer();
        private DeflaterState processingState = DeflaterState.NotStarted;

        public int Finish(byte[] outputBuffer)
        {
            if (this.processingState == DeflaterState.NotStarted)
            {
                return 0;
            }
            this.output.UpdateBuffer(outputBuffer);
            if (((this.processingState == DeflaterState.CompressThenCheck) || (this.processingState == DeflaterState.HandlingSmallData)) || (this.processingState == DeflaterState.SlowDownForIncompressible1))
            {
                this.deflateEncoder.GetBlockFooter(this.output);
            }
            this.WriteFinal();
            return this.output.BytesWritten;
        }

        private void FlushInputWindows()
        {
            this.deflateEncoder.FlushInput();
        }

        public int GetDeflateOutput(byte[] outputBuffer)
        {
            this.output.UpdateBuffer(outputBuffer);
            switch (this.processingState)
            {
                case DeflaterState.NotStarted:
                {
                    DeflateInput.InputState state = this.input.DumpState();
                    OutputBuffer.BufferState state2 = this.output.DumpState();
                    this.deflateEncoder.GetBlockHeader(this.output);
                    this.deflateEncoder.GetCompressedData(this.input, this.output);
                    if (this.UseCompressed(this.deflateEncoder.LastCompressionRatio))
                    {
                        this.processingState = DeflaterState.CompressThenCheck;
                    }
                    else
                    {
                        this.input.RestoreState(state);
                        this.output.RestoreState(state2);
                        this.copyEncoder.GetBlock(this.input, this.output, false);
                        this.FlushInputWindows();
                        this.processingState = DeflaterState.CheckingForIncompressible;
                    }
                    goto Label_023A;
                }
                case DeflaterState.SlowDownForIncompressible1:
                    this.deflateEncoder.GetBlockFooter(this.output);
                    this.processingState = DeflaterState.SlowDownForIncompressible2;
                    break;

                case DeflaterState.SlowDownForIncompressible2:
                    break;

                case DeflaterState.StartingSmallData:
                    this.deflateEncoder.GetBlockHeader(this.output);
                    this.processingState = DeflaterState.HandlingSmallData;
                    goto Label_0223;

                case DeflaterState.CompressThenCheck:
                    this.deflateEncoder.GetCompressedData(this.input, this.output);
                    if (!this.UseCompressed(this.deflateEncoder.LastCompressionRatio))
                    {
                        this.processingState = DeflaterState.SlowDownForIncompressible1;
                        this.inputFromHistory = this.deflateEncoder.UnprocessedInput;
                    }
                    goto Label_023A;

                case DeflaterState.CheckingForIncompressible:
                {
                    DeflateInput.InputState state3 = this.input.DumpState();
                    OutputBuffer.BufferState state4 = this.output.DumpState();
                    this.deflateEncoder.GetBlock(this.input, this.output, 0xf88);
                    if (!this.UseCompressed(this.deflateEncoder.LastCompressionRatio))
                    {
                        this.input.RestoreState(state3);
                        this.output.RestoreState(state4);
                        this.copyEncoder.GetBlock(this.input, this.output, false);
                        this.FlushInputWindows();
                    }
                    goto Label_023A;
                }
                case DeflaterState.HandlingSmallData:
                    goto Label_0223;

                default:
                    goto Label_023A;
            }
            if (this.inputFromHistory.Count > 0)
            {
                this.copyEncoder.GetBlock(this.inputFromHistory, this.output, false);
            }
            if (this.inputFromHistory.Count == 0)
            {
                this.deflateEncoder.FlushInput();
                this.processingState = DeflaterState.CheckingForIncompressible;
            }
            goto Label_023A;
        Label_0223:
            this.deflateEncoder.GetCompressedData(this.input, this.output);
        Label_023A:
            return this.output.BytesWritten;
        }

        public bool NeedsInput()
        {
            return ((this.input.Count == 0) && (this.deflateEncoder.BytesInHistory == 0));
        }

        public void SetInput(byte[] inputBuffer, int startIndex, int count)
        {
            this.input.Buffer = inputBuffer;
            this.input.Count = count;
            this.input.StartIndex = startIndex;
            if ((count > 0) && (count < 0x100))
            {
                switch (this.processingState)
                {
                    case DeflaterState.CompressThenCheck:
                        this.processingState = DeflaterState.HandlingSmallData;
                        break;

                    case DeflaterState.CheckingForIncompressible:
                    case DeflaterState.NotStarted:
                        this.processingState = DeflaterState.StartingSmallData;
                        return;

                    default:
                        return;
                }
            }
        }

        private bool UseCompressed(double ratio)
        {
            return (ratio <= 1.0);
        }

        private void WriteFinal()
        {
            this.copyEncoder.GetBlock(null, this.output, true);
        }

        internal enum DeflaterState
        {
            NotStarted,
            SlowDownForIncompressible1,
            SlowDownForIncompressible2,
            StartingSmallData,
            CompressThenCheck,
            CheckingForIncompressible,
            HandlingSmallData
        }
    }
}

