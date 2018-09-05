using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GzipBlockProcessorLib
{
    class ByteChunkQueue
    {
        public bool closed = false;

        private int idCounter = 0;
        private int queueCounter = 0;
        private Queue<ByteChunk> queue = new Queue<ByteChunk>();
        private int maxSize;


        public ByteChunkQueue(int maxSize)
        {
            this.maxSize = maxSize;
        }

        public void Close()
        {
            lock (queue)
            {
                closed = true;
                Monitor.PulseAll(queue);
            }
        }

        public void Enqueue(ByteChunk chunk)
        {
            int id = chunk.ID;
            lock (queue)
            {
                while (queueCounter >= maxSize || id != idCounter)
                {
                    Monitor.Wait(queue);
                }
                queue.Enqueue(chunk);
                idCounter++;
                queueCounter++;
                Monitor.PulseAll(queue);
            }
        }

        public void EnqueueBytes(byte[] buffer)
        {
            lock (queue)
            {
                while (queueCounter >= maxSize)
                {
                    Monitor.Wait(queue);
                }
                ByteChunk chunk = new ByteChunk(idCounter, buffer);
                queue.Enqueue(chunk);
                idCounter++;
                queueCounter++;
                Monitor.PulseAll(queue);
            }
        }

        public bool TryDequeue(out ByteChunk chunk)
        {
            lock (queue)
            {
                while (queueCounter == 0)
                {
                    if (closed)
                    {
                        chunk = new ByteChunk();
                        return false;
                    }
                    Monitor.Wait(queue);
                }
                chunk = queue.Dequeue();
                queueCounter--;

                Monitor.PulseAll(queue);

                return true;
            }
        }
    }
}
