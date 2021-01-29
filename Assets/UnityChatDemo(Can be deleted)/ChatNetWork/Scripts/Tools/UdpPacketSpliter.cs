using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UdpPacketSpliter : MonoBehaviour {

    /// <summary>
    /// Split UDP packets
    /// </summary>
    /// <param name="sequence">Sequence number of UDP packet</param>
    /// <param name="data">Splited UDP packet</param>
    /// <param name="chunkLength">The length of the chunk</param>
    /// <returns>
    /// List of splited UDP packets
    /// </returns>
    public static List<UdpPacket> Split(long sequence, byte[] data, int chunkLength)
    {
        if (data == null)Debug.LogError("data is null!"); 

        List<UdpPacket> packets = new List<UdpPacket>();

        if (data.Length <= chunkLength)
        {
            packets.Add(new UdpPacket(sequence, 1, 0, chunkLength, data));
        }
        else
        {
            int chunks = data.Length / chunkLength;
            int remainder = data.Length % chunkLength;
            int total = chunks;
            if (remainder > 0) total++;

            for (int i = 0; i < chunks; i++)
            {
                byte[] chunk = new byte[chunkLength];
                Buffer.BlockCopy(data, i * chunkLength, chunk, 0, chunkLength);
                packets.Add(new UdpPacket(sequence, total, i, chunkLength, chunk));
            }
            if (remainder > 0)
            {
                int length = data.Length - (chunkLength * chunks);
                byte[] chunk = new byte[length];
                Buffer.BlockCopy(data, chunkLength * chunks, chunk, 0, length);
                packets.Add(new UdpPacket(sequence, total, chunks, chunkLength, chunk));
            }
        }
        return packets;
    }
}
