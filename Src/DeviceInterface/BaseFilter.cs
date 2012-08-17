using System;
using System.Collections.Generic;
using System.Text;

namespace MccDAQSampler
{
    public abstract class BaseFilter : BaseSource
    {
        public struct FilterCfg
        {
            public FilterCfg(IDevice _filt, int _len) { filt = _filt; length = _len; }

            private IDevice filt;
            public IDevice Filt
            {
                get { return filt; }
            }

            private int length;
            public int Length
            {
                get { return length; }
            }
        }

        public BaseFilter(string _id, FilterCfg _cfg) : base(_id, _cfg) { }

        protected override void StartUp(object o)
        {
            m_target = ((FilterCfg)o).Filt;
            m_length = ((FilterCfg)o).Length;
        }

        protected override void Init()
        {
            m_buffer = new CircularBuffer<float>(m_length);
            this.DisplayName = this.Target.DisplayName + " + " + DisplayName;
            this.ID = ((BaseSource)(this.Target)).ID;
        }

        public override bool IsConnected
        {
            get
            {
                return true;
            }
        }

        protected virtual void ApplyFilter(object o)
        {
            Buffer.Push((float)o);
        }

        public override object ReadNext()
        {
            ApplyFilter(m_target.ReadNext());
            return base.ReadNext();
        }

        protected override void InternalRead()
        {
            LastSample = Execute();
        }

        protected abstract object Execute();

        private IDevice m_target;
        protected MccDAQSampler.IDevice Target
        {
            get { return m_target; }
        }
        private int m_length;
        protected int Length
        {
            get { return m_length; }
        }
        private CircularBuffer<float> m_buffer;
        protected CircularBuffer<float> Buffer
        {
            get { return m_buffer; }
            set { m_buffer = value; }
        }
    }

    public class AverageFilter : BaseFilter
    {
        public AverageFilter(string _id, FilterCfg _cfg) : base(_id, _cfg) { }

        protected override object Execute()
        {
            float retVal = 0;
            foreach (float f in Buffer)
            {
                retVal += f;
            }
            return retVal / Buffer.Count;
        }
    }

    public class MaxFilter : BaseFilter
    {
        public MaxFilter(string _id, FilterCfg _cfg) : base(_id, _cfg) { }
        protected override object Execute()
        {
            float retVal = float.MinValue;
            foreach (float f in Buffer)
            {
                retVal = f > retVal ? f : retVal;
            }
            return retVal;
        }
    }

    public class MedianFilter : BaseFilter
    {
        public MedianFilter(string _id, FilterCfg _cfg) : base(_id, _cfg) { }

        protected override object Execute()
        {
            int i = 0;
            float retVal = 0;
            float[] dummy = new float[Buffer.Count];
            foreach (float f in Buffer)
            {
                if (i >= Buffer.Count)
                {
                    break;
                }
                dummy[i++] = f;
            }
            Array.Sort(dummy);
            retVal = dummy[(int)(Math.Ceiling(dummy.Length / 2.0) - 1)];
            return retVal;
        }
    }

}


