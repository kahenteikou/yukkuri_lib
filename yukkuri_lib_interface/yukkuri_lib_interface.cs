using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace yukkuri_lib_interface
{
    public delegate byte[] SpeakDelegate(yukkuri_lib_interface_EventArgs eventargs);
    public delegate void CloseDelegate();
    public delegate void Dll_load_delegate(yukkuri_lib_interface_dllload_args dllargs);
    public delegate void init_delegate();
    public delegate void dll_loaded_delegate();
    public class EventCallbackSink : MarshalByRefObject
    {
        public event SpeakDelegate OnSpeak;
        public event Dll_load_delegate OnDllLoad;
        public event CloseDelegate OnClose;
        public EventCallbackSink()
        {

        }
        public byte[] SpeakCallBackToClient(yukkuri_lib_interface_EventArgs evargs)
        {
            return OnSpeak?.Invoke(evargs);
        }
        public void DllLoadtoClient(yukkuri_lib_interface_dllload_args dargs)
        {
            OnDllLoad?.Invoke(dargs);
        }
        public void Close_toClient()
        {
            OnClose?.Invoke();
        }

    }
    public class yukkuri_lib_interface_EventClass
    {
        public string textdata { get; set; }
        public int speed { get; set; }
        public yukkuri_lib_interface_EventClass(string txtdata, int speedkun)
        {
            speed = speedkun;
            textdata = txtdata;
        }
        public yukkuri_lib_interface_EventClass()
        {
            textdata = "";
            speed = 100;
        }
    }
    [Serializable]
    public class yukkuri_lib_interface_dllload_args : ISerializable
    {
        public string dll_path;
        public yukkuri_lib_interface_dllload_args(string dllpath)
        {
            dll_path = dllpath;
        }
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected yukkuri_lib_interface_dllload_args(SerializationInfo info,StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = info.GetString("dll_path");
            dll_path = ser.Deserialize<string>(json);
        }
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info,StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = ser.Serialize(dll_path);
            info.AddValue("dll_path", json);
        }
    }
    [Serializable]
    public class yukkuri_lib_interface_EventArgs : ISerializable
    {
        public yukkuri_lib_interface_EventClass eventargs;
        public yukkuri_lib_interface_EventArgs(yukkuri_lib_interface_EventClass evebtar)
        {
            this.eventargs = evebtar;
        }
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected yukkuri_lib_interface_EventArgs(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = info.GetString("eventargs");
            eventargs = ser.Deserialize<yukkuri_lib_interface_EventClass>(json);
        }
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = ser.Serialize(eventargs);
            info.AddValue("eventargs", json);
        }
    }
    public class yukkuri_lib_interface : MarshalByRefObject
    {
        private List<SpeakDelegate> eventListeners_speak = new List<SpeakDelegate>();
        private List<CloseDelegate> closeListeners = new List<CloseDelegate>();
        private List<Dll_load_delegate> eventListeners_dllload = new List<Dll_load_delegate>();
        public event init_delegate Oninit;
        public event dll_loaded_delegate OnDllLoaded;
        public void inited()
        {
            Oninit?.Invoke();
        }
        public void dll_loaded()
        {
            OnDllLoaded?.Invoke();
        }
        public void AddEventListener_Speak(SpeakDelegate listener)
        {
            eventListeners_speak.Add(listener);
        }
        public void AddEventListener_Dllload(Dll_load_delegate listener)
        {
            eventListeners_dllload.Add(listener);
        }
        public void AddEventListener_close(CloseDelegate cl)
        {
            closeListeners.Add(cl);
        }
        public byte[] Speak_to_client(yukkuri_lib_interface_EventClass paramkun)
        {
            yukkuri_lib_interface_EventArgs evt = new yukkuri_lib_interface_EventArgs(paramkun);
            foreach(SpeakDelegate listener in eventListeners_speak)
            {
                return listener(evt);
            }
            return new byte[] { 0 };
        }
        public void Close_to_client()
        {
            /*yukkuri_lib_close_args evt = new yukkuri_lib_close_args(cle);
            foreach(CloseDelegate clel in closeListeners)
            {
                cle=clel(evt);
            }
            */
            foreach(CloseDelegate clel in closeListeners)
            {
                clel();
            }
        }

        public void DllLoad_to_client(string dllpath)
        {
            yukkuri_lib_interface_dllload_args evt = new yukkuri_lib_interface_dllload_args(dllpath);
            foreach (Dll_load_delegate listener in eventListeners_dllload)
            {
                listener(evt);
            }
        }
        public override object InitializeLifetimeService()
        {
            return null;
        }

    }
}
