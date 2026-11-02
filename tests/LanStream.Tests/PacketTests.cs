using LanStream.Shared.Protocol;
using Xunit;

namespace LanStream.Tests;

/// <summary>
/// Kiểm thử đơn vị cho cấu trúc <see cref="PacketHeader"/> và <see cref="PacketSerializer"/>.
/// Phụ trách: TV3 + TV4.
/// </summary>
public class PacketTests
{
    [Fact]
    public void ProtocolConstants_MaxPayloadSize_EqualsDatagramMinusHeader()
    {
        Assert.Equal(24, ProtocolConstants.HeaderSize);
        Assert.Equal(1200, ProtocolConstants.MaxDatagramSize);
        Assert.Equal(1176, ProtocolConstants.MaxPayloadSize);
    }

    [Fact]
    public void PacketHeader_IsValid_ReturnsTrueForValidHeader()
    {
        var header = new PacketHeader(
            Magic: ProtocolConstants.Magic,
            Version: ProtocolConstants.CurrentVersion,
            Flags: ProtocolConstants.FlagData,
            StreamId: 1001,
            FrameId: 42,
            ChunkIndex: 0,
            ChunkCount: 3,
            TimestampMs: 1_762_000_000_000L);

        Assert.True(header.IsValid);
    }

    [Theory]
    [InlineData((ushort)0x0000, ProtocolConstants.CurrentVersion, (ushort)0, (ushort)1)] // Sai Magic (TC09)
    [InlineData(ProtocolConstants.Magic, (byte)99, (ushort)0, (ushort)1)]                // Sai Version (TC09)
    [InlineData(ProtocolConstants.Magic, ProtocolConstants.CurrentVersion, (ushort)0, (ushort)0)]   // ChunkCount = 0
    [InlineData(ProtocolConstants.Magic, ProtocolConstants.CurrentVersion, (ushort)2, (ushort)2)]   // ChunkIndex >= ChunkCount
    public void PacketHeader_IsValid_ReturnsFalseForInvalidHeader(
        ushort magic,
        byte version,
        ushort chunkIndex,
        ushort chunkCount)
    {
        var header = new PacketHeader(magic, version, 0, 1, 1, chunkIndex, chunkCount, 123456L);
        Assert.False(header.IsValid);
    }

    [Fact(Skip = "TODO [TV3]: Bỏ Skip sau khi hoàn thiện PacketSerializer.SerializePacket & TryDeserializePacket")]
    public void SerializeAndDeserialize_RoundTrip_PreservesHeaderAndPayload()
    {
        var expectedHeader = new PacketHeader(
            ProtocolConstants.Magic,
            ProtocolConstants.CurrentVersion,
            ProtocolConstants.FlagData,
            StreamId: 77,
            FrameId: 5,
            ChunkIndex: 1,
            ChunkCount: 4,
            TimestampMs: 1_762_000_123_456L);

        byte[] expectedPayload = [1, 2, 3, 4, 5, 255];

        byte[] datagram = PacketSerializer.SerializePacket(expectedHeader, expectedPayload);
        bool ok = PacketSerializer.TryDeserializePacket(datagram, out var actualHeader, out var actualPayload);

        Assert.True(ok);
        Assert.Equal(expectedHeader, actualHeader);
        Assert.Equal(expectedPayload, actualPayload.ToArray());
    }
}
