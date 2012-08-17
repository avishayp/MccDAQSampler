using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace DeviceInterface
{

    public abstract class BaseSource : IDevice
    {
        protected static int counter;

        public static IDevice CreateDevice()
        {
            return CreateDevice("", "", "");
        }

        public static IDevice CreateDevice(string _type, string _id, object _cfg)
        {
            counter++;

            IDevice retVal = new NULLSource("", "");
            string typeName = Assembly.GetExecutingAssembly().GetName().Name + "." +
                                _type.Replace(" ", "");

            if (typeName.Contains(retVal.DisplayName))
            {
                return retVal;
            }

            try
            {
                retVal = (IDevice)Activator.CreateInstance(Type.GetType(typeName), new Object[] { _id, _cfg });
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Factory method failed: type={0}, id={1}. ",
                        _type, _id) + ex.ToString());
            }
            return retVal;
        }

        protected BaseSource() { }

        protected BaseSource(string _id, Object _cfg)
        {
            ID = _id;
            m_name = this.GetType().Name;
            StartUp(_cfg);
            Init();
        }

        public virtual Object ReadNext() { InternalRead(); return LastSample; }

        public virtual Object Read(int n) { return null; }

        public virtual bool Write(Object o)
        {
            if (IsConnected)
            {
                return InternalWrite(o);
            }
            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    FreeUnmanagedResources();
                }

                // Dispose any unmanaged resources here:
                disposed = true;
            }
        }

        protected virtual void FreeUnmanagedResources() { }

        protected virtual void StartUp(Object o)
        {
            // populate fields (configuration)
        }

        protected virtual void Init()
        {
            // do stuff
        }

        ~BaseSource()
        {
            Dispose(false);
        }

        protected virtual void InternalRead()
        {
            // LastSample = ?
        }

        protected virtual bool InternalWrite(Object o)
        {
            if (o != null)
            {
                LastSample = o;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return DisplayName + ": " + ID;
        }

        protected object LastSample
        {
            get { return m_lastSample; }
            set { m_lastSample = value; }
        }

        public virtual bool IsConnected
        {
            get { return false; }
            protected set { }
        }

        public virtual string DisplayName
        {
            get { return m_name; }
            protected set { m_name = value; }
        }

        public virtual string ID
        {
            get { return m_id; }
            protected set { m_id = value; }
        }

        public override bool Equals(object obj)
        {
            BaseSource bs = obj as BaseSource;
            return (bs != null) && (this.ID.Equals(bs.ID));
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        private object m_lastSample;
        protected string m_name;
        protected string m_id;
        private bool disposed = false;
    }

    public class NULLSource : BaseSource
    {
        public NULLSource(string _id, string _dummy) : base(_id, _dummy) { }
    }

}
