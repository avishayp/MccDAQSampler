using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceInterface
{
    public interface IDevice : IDisposable
    {
        Object ReadNext();
        Object Read(int samples);
        bool Write(Object o);
        string DisplayName
        {
            get;
        }
        bool IsConnected
        {
            get;
        }
    }
}
