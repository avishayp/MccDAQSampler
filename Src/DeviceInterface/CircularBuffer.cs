using System;
using System.Collections;
using System.Text;

namespace MccDAQSampler
{
    public class CircularBuffer<T> : IEnumerable
    {
        private int m_max;   // max size
        private int fp = 0;  // front pointer
        private T[] arr;    // actual collection
        private int m_count;

        public CircularBuffer(int size)
        {
            m_max = size;
            fp = 0;
            m_count = 0;
            arr = new T[m_max];
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public bool IsFull
        {
            get { return Count == m_max; }
        }

        public int Count
        {
            get { return m_count; }
            private set { m_count = value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return arr.GetEnumerator();
        }

        public void Push(T val)
        {
            arr[fp] = val;
            Count = Count < m_max ? Count + 1 : m_max;
            fp = (fp + 1) % m_max;            
        }
    }
}
