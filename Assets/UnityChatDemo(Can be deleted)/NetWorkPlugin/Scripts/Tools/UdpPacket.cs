public class UdpPacket
{
    public long Sequence;
    public int Total;
    public int Index;
    public int ChunkLength;
    public byte[] Chunk;
    public UdpPacket()
    {
  
    }
    public UdpPacket(long sequence, int total, int index, int chunkLength, byte[] chunk)
    {
        Sequence = sequence;
        Total = total;
        Index = index;
        ChunkLength = chunkLength;
        Chunk = chunk;
    }
}