using System;
using System.Collections.Generic;
using System.Text;

#if WINDOWS_UWP
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace UnpiNet
{
#if WINDOWS_UWP
    public class Unpi : IUnpi
    {
        private readonly string id;
        private SerialDevice device;
        private Stream inputStream;
        private Stream outputStream;
        public Stream InputStream
        {
            get { return inputStream; }
        }
        public Stream OutputStream
        {
            get { return outputStream; }
        }
        public object Parity { get; private set; }

        public Unpi(string name)
        {
            var selector = SerialDevice.GetDeviceSelector(name);
            var devices = Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(selector, null).AsTask().Result;
            if (!devices.Any())
                throw new ArgumentOutOfRangeException(nameof(name), name, "Serialport not found");

            id = devices.First().Id;
        }

        public Unpi(ushort vendorId, ushort productId)
        {
            var selector = SerialDevice.GetDeviceSelectorFromUsbVidPid(vendorId, productId);
            var devices = Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(selector, null).AsTask().Result;
            if (!devices.Any())
                throw new ArgumentOutOfRangeException("Serialport not found, invalid vendorId or productId");

            id = devices.First().Id;
        }

        public void Open()
        {
            device = SerialDevice.FromIdAsync(id).AsTask().Result;
            if (device == null)
            {
                throw new Exception("Error opening serial device.");
            }

            device.BaudRate = 115200;
            device.Parity = SerialParity.None;
            device.DataBits = 8;
            device.StopBits = SerialStopBitCount.One;
            inputStream = new SerialReadStream(device.InputStream);
            outputStream = new SerialWriteStream(device.OutputStream);
        }

        public void Close()
        {
            if (device != null)
            {
                inputStream = null;
                outputStream = null;
                device.Dispose();
                device = null;
            }
        }

        class SerialReadStream : Stream
        {
            private readonly IInputStream _input;

            public SerialReadStream(IInputStream input)
            {
                _input = input;
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override long Length
            {
                get { throw new NotSupportedException(); }
            }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                try
                {
                    var bytes = new byte[1024];
                    _input.ReadAsync(bytes.AsBuffer(), (uint)count, InputStreamOptions.None).AsTask().Wait();
                    bytes.CopyTo(0, buffer.AsBuffer(), (uint)offset, count);
                    return count;
                }
                catch (Exception ex)
                {
                    throw new IOException("Read failed", ex);
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }


        class SerialWriteStream : Stream
        {
            private readonly IOutputStream _output;

            public SerialWriteStream(IOutputStream output)
            {
                _output = output;
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override long Length
            {
                get { throw new NotSupportedException(); }
            }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                try
                {
                    _output.WriteAsync(buffer.Skip(offset).Take(count).ToArray().AsBuffer()).AsTask().Wait();
                }
                catch (Exception ex)
                {
                    throw new IOException("Write failed", ex);
                }
            }
        }
    }

#endif
}
