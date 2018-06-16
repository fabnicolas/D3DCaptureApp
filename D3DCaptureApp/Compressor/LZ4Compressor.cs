using Serilog.Core;
using SerilogLoggerSystem;

namespace D3DCaptureApp {
    /// <summary>
    /// LZ4Compressor is a class used to compress and decompress data, such as byte arrays, using LZ4 compression algorithms.
    /// 
    /// The aim of this class is to ease the usage of LZ4 for simple use-cases; for example,
    /// compressing a 1366x768 screenshot of my desktop and decompressing it averagely
    /// reduced its byte array size from 4 MB to just 200-300 KB without lose quality.
    /// 
    /// Requires NuGet package: lz4net (More info: https://github.com/MiloszKrajewski/lz4net)
    /// </summary>
    class LZ4Compressor {
        private static readonly Logger logger = SerilogFactory.GetLogger();

        /// <summary>
        /// Compress byte array using LZ4 algorithm (LZ4.LZ4Codec.Encode).
        /// </summary>
        /// <param name="input_buffer">The input byte array to compress.</param>
        /// <returns>The compressed version of the input byte array.</returns>
        public static byte[] Compress(byte[] input_buffer) {
            var input_buffer_length = input_buffer.Length;
            var input_buffer_max_length = LZ4.LZ4Codec.MaximumOutputLength(input_buffer_length);
            var output_buffer = new byte[input_buffer_max_length];
            
            var output_length = LZ4.LZ4Codec.Encode(
                input_buffer,0,input_buffer.Length,
                output_buffer,0,output_buffer.Length
            );

            var compressed_buffer = new byte[output_length];
            for(int i = 0;i<output_length;i++)
                compressed_buffer[i]=output_buffer[i];

            logger.Information("LZ4 compression done; before: "+input_buffer_length+" bytes, after: "+output_length+" bytes.");

            return compressed_buffer;
        }

        /// <summary>
        /// Decompress byte array that was compressed with Compress method. Uses LZ4.LZ4Codec.Decode.
        /// </summary>
        /// <param name="input_buffer">The input byte array to decompress.</param>
        /// <returns>The decompressed version of the input byte array.</returns>
        public static byte[] Decompress(byte[] input_buffer) {
            var input_buffer_length = input_buffer.Length;
            var input_buffer_max_length = input_buffer_length*30;
            var output_buffer = new byte[input_buffer_max_length];

            var output_length = LZ4.LZ4Codec.Decode(
                input_buffer,0,input_buffer.Length,
                output_buffer,0,input_buffer_max_length
            );

            var compressed_buffer = new byte[output_length];
            for(int i = 0;i<output_length;i++)
                compressed_buffer[i]=output_buffer[i];

            logger.Information("LZ4 decompression done; before: "+input_buffer_length+" bytes, after: "+output_length+" bytes.");

            return output_buffer;
        }
    }
}
