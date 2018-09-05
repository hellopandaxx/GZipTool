namespace GzipBlockProcessorLib
{
    using System;
    using System.Threading;

    /// <summary>
    /// Class is responsible for compression/decompression file using Gzip format.
    /// </summary>
    public static class GzipBlockProcessor
    {
        static ProcessorBase processor;

        /// <summary>
        /// Perform compress operation.
        /// </summary>
        /// <param name="inputFileName">Path to the input file.</param>
        /// <param name="outputFileName">Path to the output file.</param>
        public static void Compress(string inputFileName, string outputFileName)
        {
            processor = new Compressor();
            processor.Execute(inputFileName, outputFileName);
        }

        /// <summary>
        /// Perform decompress operation.
        /// </summary>
        /// <param name="inputFileName">Path to the input file.</param>
        /// <param name="outputFileName">Path to the output file.</param>
        public static void Decompress(string inputFileName, string outputFileName)
        {
            //Decompressor.ParallelDecompress(inputFileName, outputFileName);
            processor = new Decompressor();
            processor.Execute(inputFileName, outputFileName);
        }
    }
}
