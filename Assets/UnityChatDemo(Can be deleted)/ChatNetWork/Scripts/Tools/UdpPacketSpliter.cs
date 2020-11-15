using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UdpPacketSpliter : MonoBehaviour {

    /// <summary>
    /// 分割UDP数据包
    /// </summary>
    /// <param name="sequence">UDP数据包所持有的序号</param>
    /// <param name="datagram">被分割的UDP数据包</param>
    /// <param name="chunkLength">分割块的长度</param>
    /// <returns>
    /// 分割后的UDP数据包列表
    /// </returns>
    public static List<UdpPacket> Split(long sequence, byte[] datagram, int chunkLength)
    {
        if (datagram == null)Debug.LogError("datagram is null!");

        List<UdpPacket> packets = new List<UdpPacket>();

        if (datagram.Length <= chunkLength)
        {
            packets.Add(new UdpPacket(sequence, 1, 0, chunkLength, datagram));
        }
        else
        {
            int chunks = datagram.Length / chunkLength;
            int remainder = datagram.Length % chunkLength;
            int total = chunks;
            if (remainder > 0) total++;

            for (int i = 0; i < chunks; i++)
            {
                byte[] chunk = new byte[chunkLength];
                Buffer.BlockCopy(datagram, i * chunkLength, chunk, 0, chunkLength);
                packets.Add(new UdpPacket(sequence, total, i, chunkLength, chunk));
            }
            if (remainder > 0)
            {
                int length = datagram.Length - (chunkLength * chunks);
                byte[] chunk = new byte[length];
                Buffer.BlockCopy(datagram, chunkLength * chunks, chunk, 0, length);
                packets.Add(new UdpPacket(sequence, total, chunks, chunkLength, chunk));
            }
        }
        return packets;
    }
}
