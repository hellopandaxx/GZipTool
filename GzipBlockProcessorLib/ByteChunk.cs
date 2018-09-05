using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GzipBlockProcessorLib
{
    [Serializable]
    internal class ByteChunk
    {
        int id;
        byte[] buffer;

        public int ID { get { return id; } }
        public byte[] Content { get { return buffer; } }

        public ByteChunk()
            : this(0, new byte[0])
        {

        }

        public ByteChunk(int id, byte[] buffer)
        {
            this.id = id;
            this.buffer = buffer;
        }
    }
}
