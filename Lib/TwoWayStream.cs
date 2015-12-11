using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lib
{
    /// <summary>
    /// An override of Stream that readable and writable
    /// </summary>
        public class TwoWayStream : Stream
    {
        readonly object X = new object();
        readonly object readWait = new object();

        Queue<byte[]> queue = new Queue<byte[]>();


        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        private long _length = 0;
        public override long Length => _length;

        private long _pos = 0;
        public override long Position
        {
            get { lock (X) { return _pos; } }
            set { throw new NotImplementedException(); }
        }

        public override void Flush() {}

        private bool closed = false;
        public override void Close()
        {
            base.Close();
            closed = true;
            lock (readWait)
            {
                Monitor.PulseAll(readWait);
            }
        }

        int rPos = 0;
        public override int Read(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                byte[] buff = null;
                lock (X)
                {
                    if (queue.Count != 0)
                    {
                        buff = queue.Peek();
                    }
                }
                if (buff == null)
                {
                    if (this.closed) return 0;
                    lock (readWait)
                    {
                        Monitor.Wait(readWait);
                    }
                    continue;
                }

                if (buff.Length - rPos > count)
                {
                    // 余る
                    Array.Copy(buffer, offset, buff, rPos, count);
                    rPos += count;
                    _pos += count;
                    return count;
                }
                else
                {
                    // 足らずor丁度
                    int ret = buff.Length - rPos;
                    Array.Copy(buff, rPos, buffer, offset, ret);
                    rPos = 0;
                    lock (X)
                    {
                        queue.Dequeue();
                    }
                    _pos += ret;
                    return ret;
                }
            }
        }

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
            byte[] buff2 = new byte[count - offset];
            Array.Copy(buffer, offset, buff2, 0, count);
            lock (X)
            {
                queue.Enqueue(buff2);
                _length += count;
            }
            lock (readWait)
            {
                Monitor.PulseAll(readWait);
            }
        }
    }


}
