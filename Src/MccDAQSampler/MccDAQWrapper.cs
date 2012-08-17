using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using MccDaq;
using DeviceInterface;

namespace MccDAQSampler
{
    public class MccDAQWrapper : BaseSource
    {
        public struct MccDAQCfg
        {
            public MccDAQCfg(int _board, int _channels)
            {
                boardNum = _board;
                channelsNum = _channels;
            }

            public int BoardNum
            {
                get { return boardNum; }
            }

            public int ChannelsNum
            {
                get { return channelsNum; }
            }

            private int boardNum;
            private int channelsNum;
        }

        // future use
        public enum MccDAQBoardType
        {
            USB_2416,
            USB_2416_EXT,
            USB_TEMP
        }

        public MccDAQWrapper(string _id, MccDAQCfg _cfg) : base(_id, _cfg) { }

        protected override void StartUp(object o)
        {
            m_board = ((MccDAQCfg)o).BoardNum;
            m_channelsNum = ((MccDAQCfg)o).ChannelsNum;
        }

        protected override void Init()
        {
            m_buffer = new float[1];
            m_channels = new MccDAQChannel[m_channelsNum];
            m_status = MccService.ErrHandling(ErrorReporting.DontPrint, ErrorHandling.DontStop);
            DaqBoard = new MccDaq.MccBoard(m_board);
            m_status = DaqBoard.BoardConfig.GetUsesExps(out UsesEXPs);
            if (m_status.Value == ErrorInfo.ErrorCode.BadBoard)
            {
                throw new ArgumentException(String.Format("MccDAQBoard number {0} failed to init! Check Instacal for valid board numbers.", m_board));
            }
            m_units = MccDaq.TempScale.Celsius;
            m_options = 0;
        }

        public IDevice CreateAndSetChannel(string name, int pin)
        {
            if (pin < 0 || pin >= m_channels.Length)
            {
                throw new Exception(String.Format("Unsupported Mcc channel! Pin {0} cannot be assigned to board {1}", pin, BoardNumber));
            }

            m_channels[pin] = CreateDevice("MccDAQChannel", name, this) as MccDAQChannel;
            m_channels[pin].Pin = pin;
            return m_channels[pin];
        }

        public void ReadChannel(int pin)
        {
            Channels[pin].Status = DaqBoard.TInScan(pin, pin, m_units, m_buffer, m_options);
            Channels[pin].Write(m_buffer[0]);
        }

        public override bool IsConnected
        {
            get
            {
                return Status.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors;
            }
        }

        protected override void InternalRead()
        {
            for (int i = 0; i < m_channelsNum; i++)
            {
                if (Channels[i] != null)
                {
                    Channels[i].ReadNext();
                }                
            }
        }

        public ErrorInfo Status
        {
            get { return m_status; }
            set 
            {
                if (!m_status.Equals(value))
                {
                    OnStatusChanged(value);
                } 
                m_status = value;
            }
        }

        private void OnStatusChanged(MccDaq.ErrorInfo newStatus)
        {
            // TBD
        }

        public MccDAQChannel[] Channels
        {
            get { return m_channels; }
        }

        public int BoardNumber
        {
            get { return m_board; }
        }
        
        private MccDAQChannel[] m_channels;
        private ErrorInfo m_status;
        private int m_board;
        private int m_channelsNum;
        private MccBoard DaqBoard;
        private int UsesEXPs = 0;
        private float[] m_buffer; 
        private MccDaq.TempScale m_units;
        private MccDaq.ThermocoupleOptions m_options;
    }

    public class MccDAQChannel : BaseSource
    {
        public MccDAQChannel(string _id, MccDAQWrapper _cfg) : base(_id, _cfg) { }

        protected override void StartUp(object o)
        {
            m_parent = (MccDAQWrapper)o;
        }

        protected override void Init()
        {
            LastSample = (float)0;
            Status = (Parent != null ? Parent.Status : new ErrorInfo(1));
        }

        public override bool IsConnected
        {
            get
            {
                return Status.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors;
            }
        }

        protected override void InternalRead()
        {
            Parent.ReadChannel(Pin);
        }

        public MccDAQSampler.MccDAQWrapper Parent
        {
            get { return m_parent; }
        }

        public ErrorInfo Status
        {
            get { return m_status; }
            set { m_status = value; }
        }

        public int Pin
        {
            get { return m_pin; }
            set { m_pin = value; }
        }

        private MccDAQWrapper m_parent;
        private ErrorInfo m_status;
        private int m_pin;
    }

}