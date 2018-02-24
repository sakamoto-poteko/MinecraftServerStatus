using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MCServerStatus.Models;
using Newtonsoft.Json;

namespace MCServerStatus
{
    public class MinecraftPinger : IMinecraftPinger
    {
        private string Address { get; }
        private short Port { get; }

        public MinecraftPinger(string address, short port)
        {
            Address = address;
            Port = port;
        }

        private static int ReadVarInt(IEnumerable<byte> input)
        {
            return ReadVarInt(input, out var count);
        }

        private static int ReadVarInt(BinaryReader reader)
        {
            var s = reader;
            int i = 0;
            int j = 0;

            while (true)
            {
                int k = s.ReadByte();

                i |= (k & 0x7F) << j++ * 7;
                if (j > 5) return 0;

                if ((k & 0x80) != 128) break;
            }
            return i;
        }

        private static int ReadVarInt(IEnumerable<byte> input, out int count)
        {
            var s = input.ToList();
            int i = 0;
            int j = 0;

            count = 0;
            while (true)
            {
                ++count;
                int k = s.First();
                s.RemoveAt(0);

                i |= (k & 0x7F) << j++ * 7;
                if (j > 5)
                {
                    return 0;
                }
                if ((k & 0x80) != 128) break;
            }
            return i;
        }

        private static byte[] GetVarInt(int paramInt)
        {
            var output = new List<byte>();
            while (true)
            {
                if ((paramInt & 0xFFFFFF80) == 0)
                {
                    output.Add((byte)paramInt);
                    return output.ToArray();
                }

                output.Add((byte)(paramInt & 0x7F | 0x80));
                paramInt = paramInt >> 7;
            }
        }

        private static byte[] GetString(string content)
        {
            var output = new List<byte>();

            output.AddRange(GetVarInt(content.Length));
            output.AddRange(Encoding.UTF8.GetBytes(content));

            return output.ToArray();
        }

        private void Handshake(TcpClient tcpclient)
        {
            var handshakeStream = new MemoryStream();
            var handshakewriter = new BinaryWriter(handshakeStream);

            handshakewriter.Write((byte)0x00);  // Packet ID
            handshakewriter.Write(GetVarInt(4));  // Protocol version, 4 for 1.7.1-pre to 1.7.5
            handshakewriter.Write(GetString(Address));  // hostname or IP
            handshakewriter.Write(Port); // Port
            handshakewriter.Write(GetVarInt(0x01)); // Next state, 1 for `status'
            handshakewriter.Flush();

            handshakeStream.TryGetBuffer(out var handshakeStreamBuffer);

            var ns = tcpclient.GetStream();
            var writer = new BinaryWriter(ns);

            writer.Write(GetVarInt((int)handshakeStream.Length));
            writer.Write(handshakeStreamBuffer.ToArray(), 0, (int)handshakeStream.Length);
            writer.Flush();
        }

        public async Task<Status> RequestAsync()
        {
            using (var tcpclient = new TcpClient())
            {
                await tcpclient.ConnectAsync(Address, Port);

                if (!tcpclient.Connected)
                    return null;

                Handshake(tcpclient);

                var ns = tcpclient.GetStream();

                var writer = new BinaryWriter(ns);
                var reader = new BinaryReader(ns);

                writer.Write((short)0x0001);   // BE: 0x0100, Length and 
                                               //writer.Write((byte)0x00);   // ID for `Request'
                writer.Flush();

                var packetLen = ReadVarInt(reader);
                var packetId = ReadVarInt(reader);
                var packetJsonLen = ReadVarInt(reader);

                var response = reader.ReadBytes(packetJsonLen);

                var json = Encoding.UTF8.GetString(response);

                try
                {
                    var jsonobj = JsonConvert.DeserializeObject<Status>(json);
                    return jsonobj;
                }
                catch (Exception)
                {
                    return null;
                }
            }

        }

        public Task<Status> PingAsync()
        {
            return RequestAsync();
        }
    }
}
