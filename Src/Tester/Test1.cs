using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MccDAQSampler;
using DeviceInterface;
using System.Threading;

namespace Tester
{
    class Test1
    {
        public Test1()
        {
            InitHW();
        }

        public void RunTest()
        {
            RunTest(10, 1000);
        }

        public void RunTest(int iterations, int interval)
        {
            while (iterations-- > 0)
            {
                for (int i = 0; i < Devices.Length; i++)
                {
                    TestChannel(i);
                }
                Thread.Sleep(interval);
            }
        }

        private void TestChannel(int channel)
        {
            double val = (double) Devices[channel].ReadNext();
            String name = Devices[channel].DisplayName;
            String res = String.Format("{0}: {1}", name, val);
            Console.WriteLine(res);
        }

        private void InitBoard(int board, int channels)
        {
            try
            {
               Board = (MccDAQWrapper)BaseSource.CreateDevice("MccDAQWrapper", "Board " + board, new MccDAQWrapper.MccDAQCfg(board, channels));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void SetChannels(int channels)
        {
            Devices = new IDevice[channels];
            for (int i = 0; i < channels; i++)
            {
                try
                {
                    Devices[i] = Board.CreateAndSetChannel(i.ToString(), i);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        private void InitHW()
        {
            InitBoard(BOARD, CHANNELS);
            SetChannels(CHANNELS);  // each sampling channels is a 'device' by itself.
        }

        public IDevice[] Devices { get; private set; }      // array of channels
        public MccDAQWrapper Board { get; private set; }    // the board itself

        private const int CHANNELS = 16;    // test MccDAQ USB-2416 board with 16 channels    
        private const int BOARD = 0;        // board number as appears in InstaCal

    }
}
