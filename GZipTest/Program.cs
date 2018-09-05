namespace GZipTest
{
    using System;
    using System.IO;
    using GzipBlockProcessorLib;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ValidateArgs(args);

                string inputFileName = args[1];

                string outputFileName = args.Length == 3 ? args[2] : string.Empty;

                switch (args[0])
                {
                    case "compress":
                        GzipBlockProcessor.Compress(inputFileName, outputFileName);
                        break;
                    case "decompress":
                        GzipBlockProcessor.Decompress(inputFileName, outputFileName);
                        break;
                    default:
                        DisplayHelp();
                        break;
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                DisplayHelp();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Input file not found.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Unable access a file.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("Usage: GZipTest.exe compress/decompress [input file name] [output file name]");
        }

        private static void ValidateArgs(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("Command-line must contain 3 arguments", "args");
            }

            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
            {
                throw new ArgumentException("First argument must be 'compress' or 'decompress'", "args[0]");
            }

            if (!File.Exists(args[1]))
            {
                throw new ArgumentException("File [" + args[1] + "] doesn't exist", "args[1]");
            }

            if (args[1] == args[2])
            {
                throw new ArgumentException("Input and output files have same names");
            }

            FileInfo fileIn = new FileInfo(args[1]);
            if (fileIn.Length == 0)
            {
                throw new ArgumentException("File [" + args[1] + "] has 0 bytes size", "args[1]");
            }
            if (fileIn.Extension == ".gz" && args[0] == "compress")
            {
                throw new ArgumentException("File [" + args[1] + "] already compressed", "args[1]");
            }

            if (fileIn.Length < 11 && args[0] == "decompress")
            {
                throw new Exception("Minimal file size to decompress = 11 bytes (10 for header)");
            }
        }
    }
}
