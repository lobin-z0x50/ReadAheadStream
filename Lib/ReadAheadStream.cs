using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Lib
{
    public class ReadAheadStream : Stream
    {
        TwoWayStream tws = new TwoWayStream();

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => tws.Length;

        public override long Position 
        {
            get { return tws.Position; } 
            set { }
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) => tws.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Task ReadTask { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="s"></param>
        private ReadAheadStream(Stream s)
        {
            ReadTask = Task.Run(() =>
            {
                s.CopyTo(tws, 1024);
                tws.Close();
            });
        }

        /// <summary>
        /// Factory method
        /// </summary>
        /// <param name="readableStream"></param>
        /// <returns></returns>
        public static ReadAheadStream Start(Stream readableStream)
        {
            return new ReadAheadStream(readableStream);
        }
    }
}
