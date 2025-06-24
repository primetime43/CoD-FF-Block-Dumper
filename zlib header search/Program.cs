using System;
using System.IO;
using Ionic.Zlib;

namespace FFZlibScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter path to FF file:");
            string path = Console.ReadLine();

            // Remove leading and trailing quotes
            if (!string.IsNullOrWhiteSpace(path))
                path = path.Trim().Trim('"');

            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            string concatOutPath = $"all_blocks_concat_{Path.GetFileNameWithoutExtension(path)}.bin";
            using (var concatStream = new FileStream(concatOutPath, FileMode.Create, FileAccess.Write))
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                // Skip header (12 bytes)
                reader.BaseStream.Seek(0xC, SeekOrigin.Begin);
                int blockNum = 0;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // Read 2-byte big-endian block length
                    byte[] lenBytes = reader.ReadBytes(2);
                    if (lenBytes.Length < 2) break;

                    int blockLen = (lenBytes[0] << 8) | lenBytes[1];
                    // Many FFs use 0x0001 as a terminator or EOF marker—handle that here
                    if (blockLen == 1) break;
                    if (blockLen == 0) continue;

                    byte[] compressedBlock = reader.ReadBytes(blockLen);
                    if (compressedBlock.Length < blockLen) break; // EOF

                    try
                    {
                        using (var msIn = new MemoryStream(compressedBlock))
                        using (var msOut = new MemoryStream())
                        using (var inflater = new Ionic.Zlib.DeflateStream(msIn, CompressionMode.Decompress))
                        {
                            inflater.CopyTo(msOut);
                            byte[] decompressed = msOut.ToArray();

                            // Write individual bin file
                            string outPath = $"block_{blockNum:D4}_at_{reader.BaseStream.Position - blockLen:X}.bin";
                            File.WriteAllBytes(outPath, decompressed);
                            Console.WriteLine($"Decompressed block {blockNum} at 0x{reader.BaseStream.Position - blockLen:X} ({decompressed.Length} bytes) -> {outPath}");

                            // Also append to concatenated output
                            concatStream.Write(decompressed, 0, decompressed.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  [Error decompressing block {blockNum} at 0x{reader.BaseStream.Position - blockLen:X}]: {ex.Message}");
                    }

                    blockNum++;
                }
            }

            Console.WriteLine($"Wrote concatenated output: {concatOutPath}");
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
