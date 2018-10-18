using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace UnpiNet
{
    /// <summary>
    /// TI Unified NPI Packet Format
    /// SOF(1) + Length(2/1) + Type/Sub(1) + Cmd(1) + Payload(N) + FCS(1)
    ///  
    /// Source: http://processors.wiki.ti.com/index.php/Unified_Network_Processor_Interface
    /// </summary>
    public class Packet
    {
      
        public static byte SOF = 0xfe;

        public Packet()
        {
            
        }

        public Packet(MessageType type, SubSystem subSystem, byte commandId, byte[] payload = null)
        {
            Type = type;
            SubSystem = subSystem;
            Cmd1 = commandId;
            Payload = payload != null ? payload : new byte[0];
        }

        public Task WriteAsync(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var buffer = new List<byte>();
            buffer.Add(SOF);
            buffer.Add((byte)Payload.Length);
            buffer.Add(Cmd0);
            buffer.Add(Cmd1);
            buffer.AddRange(Payload);
            buffer.Add(buffer.Skip(1).Aggregate((byte) 0xFF, (total, next) => total ^= next));

            return stream.WriteAsync(buffer.ToArray(), 0, buffer.Count);
        }

        public static async Task<Packet> ReadAsync(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var buffer = new byte[1024];
            await stream.ReadAsyncExact(buffer, 0, 1);

            if (buffer[0] == SOF)
            {
                await stream.ReadAsyncExact(buffer, 1, 1);
                var length = buffer[1];
                await stream.ReadAsyncExact(buffer, 2, length + 2);

                
                var type = (MessageType) (buffer[2] & 0xe0);
                var subsystem = (SubSystem) (buffer[2] & 0x1f);
                var cmd1 = buffer[3];
                var payload = buffer.Skip(4).Take(length - 2).ToArray();

                if (buffer.Skip(1).Take(buffer.Length - 2).Aggregate((byte)0xFF, (total, next) => (byte)(total ^ next)) != buffer[buffer.Length - 1])
                    throw new InvalidDataException("checksum error");

                return new Packet(type, subsystem, cmd1, payload);
            }

            throw new InvalidDataException("unable to decode packet");
        }

        public MessageType Type { get; set; }

        public SubSystem SubSystem { get; set; }

        /// <summary>
        /// CMD0 is a 1 byte field that contains both message type and subsystem information 
        /// Bits[8-6]: Message type, see the message type section for more info.
        /// Bits[5-1]: Subsystem ID field, used to help NPI route the message to the appropriate place.
        /// 
        /// Source: http://processors.wiki.ti.com/index.php/NPI_Type_SubSystem
        /// </summary>
        public byte Cmd0
        {
            get
            {
                return (byte)(((int)Type << 5) | ((int)SubSystem));
            }
            private set
            {
                Type = (MessageType)(value & 0xE0);
                SubSystem = (SubSystem)(value & 0x1F);
            }
        }

        /// <summary>
        /// CMD1 is a 1 byte field that contains the opcode of the command being sent
        /// </summary>

        public byte Cmd1 { get; set; }

        /// <summary>
        /// Payload is a variable length field that contains the parameters defined by the 
        /// command that is selected by the CMD1 field. The length of the payload is defined by the length field.
        /// </summary>
        public byte[] Payload { get; set; }

        /// <summary>
        /// Frame Check Sequence (FCS) is calculated by doing a XOR on each bytes of the frame in the order they are 
        /// send/receive on the bus (the SOF byte is always excluded from the FCS calculation): 
        ///     FCS = LEN_LSB XOR LEN_MSB XOR D1 XOR D2...XOR Dlen
        /// </summary>
        public byte FrameCheckSequence { get; set; }


    }
}
