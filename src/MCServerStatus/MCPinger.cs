using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MCServerStatus
{
    public class MCPinger
    {
        private string address { get; set; }
        private short port { get; set; }

        public MCPinger(string address, short port)
        {
            this.address = address;
            this.port = port;
        }

        private int ReadVarInt(byte[] input)
        {
            int count;
            return ReadVarInt(input, out count);
        }

        private int ReadVarInt(BinaryReader reader)
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

        private int ReadVarInt(byte[] input, out int count)
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

        private byte[] GetVarInt(int paramInt)
        {
            List<byte> output = new List<byte>();
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

        private byte[] GetString(string content)
        {
            List<byte> output = new List<byte>();

            output.AddRange(GetVarInt(content.Length));
            output.AddRange(Encoding.UTF8.GetBytes(content));

            return output.ToArray();
        }

        private void Handshake(TcpClient tcpclient)
        {
            MemoryStream handshakeStream = new MemoryStream();
            BinaryWriter handshakewriter = new BinaryWriter(handshakeStream);

            handshakewriter.Write((byte)0x00);  // Packet ID
            handshakewriter.Write(GetVarInt(4));  // Protocol version, 4 for 1.7.1-pre to 1.7.5
            handshakewriter.Write(GetString(address));  // hostname or IP
            handshakewriter.Write(port); // Port
            handshakewriter.Write(GetVarInt(0x01)); // Next state, 1 for `status'
            handshakewriter.Flush();

            ArraySegment<byte> handshakeStreamBuffer;
            handshakeStream.TryGetBuffer(out handshakeStreamBuffer);

            NetworkStream ns = tcpclient.GetStream();
            BinaryWriter writer = new BinaryWriter(ns);

            writer.Write(GetVarInt((int)handshakeStream.Length));
            writer.Write(handshakeStreamBuffer.ToArray(), 0, (int)handshakeStream.Length);
            writer.Flush();
        }

        public async Task<Status> Request()
        {
            using (var tcpclient = new TcpClient())
            {
                await tcpclient.ConnectAsync(address, port);

                if (!tcpclient.Connected)
                    return null;

                Handshake(tcpclient);

                NetworkStream ns = tcpclient.GetStream();

                BinaryWriter writer = new BinaryWriter(ns);
                BinaryReader reader = new BinaryReader(ns);

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


    }
}
