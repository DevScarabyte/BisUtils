using BisUtils.Core.Compression.Externals;
using BisUtils.Core.Compression.Options;

namespace BisUtils.Core.Compression; 

public class BisLZOCompressionAlgorithms : IBisCompressionAlgorithm<BisLZOCompressionOptions>, IBisDecompressionAlgorithm<BisLZODecompressionOptions> {
    public long Compress(byte[] input, BinaryWriter output, BisLZOCompressionOptions options) 
        => throw new NotImplementedException();
    
    public long Decompress(Stream input, BinaryWriter output, BisLZODecompressionOptions options) {
        var startPos = output.BaseStream.Position;

        var isCompressed = !(!options.ForceDecompression && input.Length > 1024);
        var reader = new BinaryReader(input);

        if (options.UseCompressionFlag) isCompressed = reader.ReadBoolean();

        output.Write(
            isCompressed 
                ? BFF_LZO.ReadLZO(reader.BaseStream, (uint) options.ExpectedSize) 
                : reader.ReadBytes((int) (reader.BaseStream.Length - reader.BaseStream.Position))
                );
        return output.BaseStream.Position - startPos;
    }
}
