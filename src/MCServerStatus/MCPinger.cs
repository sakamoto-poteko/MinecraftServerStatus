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
        private Socket socket { get; set; }
        private string address { get; set; }
        private short port { get; set; }

        public MCPinger(string addr, short port)
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.address = addr;
            this.port = port;
        }

        private void ConnectToServer()
        {
            socket.Connect(address, port);
        }

        public int ReadVarInt(byte[] input)
        {
            int count;
            return ReadVarInt(input, out count);
        }

        public int ReadVarInt(BinaryReader reader)
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

        public int ReadVarInt(byte[] input, out int count)
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

        public byte[] WriteVarInt(int paramInt)
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

        public Status Ping(string addr, short port)
        {
            if (socket.Connected == false)
            {
                ConnectToServer();
            }

            if (!socket.Connected == false)
                return null;
            
            NetworkStream ns = new NetworkStream(socket);

            BinaryWriter writer = new BinaryWriter(ns);
            BinaryReader reader = new BinaryReader(ns);

            MemoryStream handshakeStream = new MemoryStream();
            BinaryWriter handshakewriter = new BinaryWriter(handshakeStream);
            handshakewriter.Write((byte)0x00);
            handshakewriter.Write(WriteVarInt(4));

            handshakewriter.Write(WriteVarInt(addr.Length));
            handshakewriter.Write(Encoding.UTF8.GetBytes(addr));
            handshakewriter.Write((short)25565);
            handshakewriter.Write(WriteVarInt(0x01));
            handshakewriter.Flush();
            ArraySegment<byte> bas;
            var gotBuffer = handshakeStream.TryGetBuffer(out bas);
            if (!gotBuffer)
                return null;
            var handshake = bas.ToArray();
            writer.Write(WriteVarInt((int)handshakeStream.Length));
            writer.Write(handshake, 0, (int)handshakeStream.Length);
            writer.Flush();

            writer.Write((byte)0x01);
            writer.Write((byte)0x00);
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
