namespace GzipBlockProcessorLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    abstract class ProcessorBase
    {
        protected static bool cancel = false;
        protected static bool success = false;

        protected static int bufferSize = 32 * 1024;
        protected static int queueSize = 20;

        protected ByteChunkQueue readBuffer = new ByteChunkQueue(queueSize);
        protected ByteChunkQueue writeBuffer = new ByteChunkQueue(queueSize);

        protected string inputFilePath;
        protected string outputFilePath;

        // Compress/Decompress threads = ProcessorCount - 2 (Read and Write)
        protected static int compressionThreads = (Environment.ProcessorCount - 2) > 0 ? Environment.ProcessorCount - 2 : 1;

        protected ManualResetEvent[] exitCompressionThread;

        public int GetSuccessValue()
        {
            return success ? 1 : 0;
        }

        public void Cancel()
        {
            cancel = true;
        }

        abstract public void Execute(string input, string output);
        abstract protected void Read();
        abstract protected void Write();
    }
}
