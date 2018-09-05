namespace GzipBlockProcessorLib
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;

    /// <summary>
    /// Contains logic for perform Gzip decomression.
    /// </summary>
    internal class Decompressor : ProcessorBase
    {
        public override void Execute(string input, string output)
        {
            inputFilePath = input;
            outputFilePath = output;

            Thread reader = new Thread(new ThreadStart(Read));
            reader.Start();

            Thread[] decompressors = new Thread[compressionThreads];
            exitCompressionThread = new ManualResetEvent[compressionThreads];
            for (int i = 0; i < compressionThreads; i++)
            {
                decompressors[i] = new Thread(new ParameterizedThreadStart(Decompress));
                exitCompressionThread[i] = new ManualResetEvent(false);
                decompressors[i].Start(i);
            }

            Thread writer = new Thread(new ThreadStart(Write));
            writer.Start();

            //Close writeBuffer
            WaitHandle.WaitAll(exitCompressionThread);
            writeBuffer.Close();
        }


        protected override void Read()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            ByteChunk chunk;

            using (FileStream input = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                while (input.Position < input.Length && !cancel)
                {
                    chunk = (ByteChunk)formatter.Deserialize(input);
                    readBuffer.Enqueue(chunk);
                }
                readBuffer.Close();
            }
        }

        private void Decompress(object threadNumber)
        {
            ByteChunk inputChunk;

            while (readBuffer.TryDequeue(out inputChunk) && !cancel)
            {
                using (MemoryStream ms = new MemoryStream(inputChunk.Content))
                {
                    using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        int bytesRead;
                        byte[] buffer = new byte[bufferSize];

                        bytesRead = gz.Read(buffer, 0, buffer.Length);

                        byte[] lastBuffer = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, lastBuffer, 0, bytesRead);
                        ByteChunk outputChunk = new ByteChunk(inputChunk.ID, lastBuffer);

                        writeBuffer.Enqueue(outputChunk);
                    }
                }
            }

            ManualResetEvent exitThread = exitCompressionThread[(int)threadNumber];
            exitThread.Set();
        }

        protected override void Write()
        {
            using (FileStream output = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                ByteChunk chunk;
                while (writeBuffer.TryDequeue(out chunk) && !cancel)
                {
                    byte[] buffer = chunk.Content;

                    output.Write(buffer, 0, buffer.Length);
                }
            }

            if (!cancel) success = true;
        }
    }
}
