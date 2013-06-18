namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class ConnectionUtilities
    {
        internal static void CloseNoThrow(IConnection connection, TimeSpan timeout)
        {
            bool flag = false;
            try
            {
                connection.Close(timeout, false);
                flag = true;
            }
            catch (TimeoutException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
            catch (CommunicationException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
            finally
            {
                if (!flag)
                {
                    connection.Abort();
                }
            }
        }

        internal static void ValidateBufferBounds(ArraySegment<byte> buffer)
        {
            ValidateBufferBounds(buffer.Array, buffer.Offset, buffer.Count);
        }

        internal static void ValidateBufferBounds(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            ValidateBufferBounds(buffer.Length, offset, size);
        }

        internal static void ValidateBufferBounds(int bufferSize, int offset, int size)
        {
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", offset, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            if (offset > bufferSize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", offset, System.ServiceModel.SR.GetString("OffsetExceedsBufferSize", new object[] { bufferSize })));
            }
            if (size <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, System.ServiceModel.SR.GetString("ValueMustBePositive")));
            }
            int num = bufferSize - offset;
            if (size > num)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, System.ServiceModel.SR.GetString("SizeExceedsRemainingBufferSpace", new object[] { num })));
            }
        }
    }
}

