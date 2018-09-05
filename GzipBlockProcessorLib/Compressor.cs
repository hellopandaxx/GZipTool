namespace GzipBlockProcessorLib
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;

    /// <summary>
    /// Contains logic for perform Gzip comression.
    /// </summary>
    internal class Compressor : ProcessorBase
    {
        public override void Execute(string input, string output)
        {
            inputFilePath = input;
            outputFilePath = output;

            Thread reader = new Thread(new ThreadStart(Read));
            reader.Start();

            Thread[] compressors = new Thread[compressionThreads];
            exitCompressionThread = new ManualResetEvent[compressionThreads];
            for (int i = 0; i < compressionThreads; i++)
            {
                compressors[i] = new Thread(new ParameterizedThreadStart(Compress));
                exitCompressionThread[i] = new ManualResetEvent(false);
                compressors[i].Start(i);
            }

            Thread writer = new Thread(new ThreadStart(Write));
            writer.Start();

            //Close writeBuffer
            WaitHandle.WaitAll(exitCompressionThread);
            writeBuffer.Close();
        }

        protected override void Read()
        {
            int bytesRead;
            byte[] buffer = new byte[bufferSize];

            using (FileStream input = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                while ((bytesRead = input.Read(buffer, 0, bufferSize)) > 0 && !cancel)
                {
                    byte[] lastBuffer = new byte[bytesRead];
                    Buffer.BlockCopy(buffer, 0, lastBuffer, 0, bytesRead);

                    readBuffer.EnqueueBytes(lastBuffer);
                }
                readBuffer.Close();
            }
        }

        private void Compress(object threadNumber)
        {
            ByteChunk inputChunk;

            while (readBuffer.TryDequeue(out inputChunk) && !cancel)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress))
                    using (BinaryWriter bw = new BinaryWriter(gz))
                    {
                        bw.Write(inputChunk.Content, 0, inputChunk.Content.Length);
                    }

                    byte[] outBuffer = ms.ToArray();
                    ByteChunk outputChunk = new ByteChunk(inputChunk.ID, outBuffer);

                    writeBuffer.Enqueue(outputChunk);
                }
            }

            ManualResetEvent exitThread = exitCompressionThread[(int)threadNumber];
            exitThread.Set();
        }

        protected override void Write()
        {
            using (FileStream output = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ByteChunk chunk;

                while (writeBuffer.TryDequeue(out chunk) && !cancel)
                {
                    formatter.Serialize(output, chunk);
                }
            }

            if (!cancel) success = true;
        }
    }
}
