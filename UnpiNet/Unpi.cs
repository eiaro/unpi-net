using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace UnpiNet
{
    /// <summary>
    /// The unpi is the packet builder and parser for Texas Instruments Unified Network Processor Interface (UNPI) 
    /// used in RF4CE, BluetoothSmart, and ZigBee wireless SoCs. As stated in TI's wiki page:
    ///     TI's Unified Network Processor Interface (NPI) is used for establishing a serial data link between a TI SoC and 
    ///     external MCUs or PCs. This is mainly used by TI's network processor solutions.
    /// 
    /// The UNPI packet consists of sof, length, cmd0, cmd1, payload, and fcs fields.The description of each field 
    /// can be found in Unified Network Processor Interface.
    /// 
    /// It is noted that UNPI defines the length field with 2 bytes wide, but some SoCs use NPI in their real transmission (physical layer), 
    /// the length field just occupies a single byte. (The length field will be normalized to 2 bytes in the transportation layer of NPI stack.)
    /// 
    /// Source: http://processors.wiki.ti.com/index.php/Unified_Network_Processor_Interface?keyMatch=Unified%20Network%20Processor%20Interface&tisearch=Search-EN-Support
    /// 
    /// /*************************************************************************************************/
    /// /*** TI Unified NPI Packet Format                                                              ***/
    /// /***     SOF(1) + Length(2/1) + Type/Sub(1) + Cmd(1) + Payload(N) + FCS(1)                     ***/
    /// /*************************************************************************************************/
    /// </summary>

    public class Unpi : IUnpi
    {
        public SerialPort Port { get; set; }
        public int LenBytes;

        public Stream InputStream
        {
            get { return Port.BaseStream; }
        }

        public Stream OutputStream
        {
            get { return Port.BaseStream; }
        }

        /// <summary>
        /// Create a new instance of the Unpi class.
        /// </summary>
        /// <param name="lenBytes">1 or 2 to indicate the width of length field. Default is 2.</param>
        /// <param name="stream">The transceiver instance, i.e. serial port, spi. It should be a duplex stream.</param>
        public Unpi(string port, int baudrate = 115200, int lenBytes = 2)
        {
            Port = new SerialPort(port, baudrate);

            LenBytes = lenBytes;
        }

        public void Open()
        {
            Port.Open();

            Port.DiscardInBuffer();
            Port.DiscardOutBuffer();
        }

        public void Close()
        {
            Port.Close();
        }
    }

}
