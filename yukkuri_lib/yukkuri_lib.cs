using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using yukkuri_lib_interface;

namespace yukkuri_lib
{
    public class yukkuri_lib : IDisposable
    {
        private yukkuri_lib_interface.yukkuri_lib_interface yukkui=null;
        private bool _disposed = false;
        /// <summary>
        /// <see cref="yukkuri_lib"/> のコンストラクタ
        /// </summary>
        /// <param name="dll_path">使うAquestalkのDLLパス</param>
        public yukkuri_lib(string dll_path,string application_id)
        {

            Thread thread_dllload = new Thread(new ThreadStart(() =>
            {
                while (true);
            }));

            Thread thread_prepare = new Thread(new ThreadStart(() =>
            {
                while (true)
                {

                }
            }));
            string dll_name = Path.GetFileName(Path.GetDirectoryName(dll_path));
            ProcessStartInfo psinfo = new ProcessStartInfo();
            psinfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\yukkuri_86_wrapper.exe";
            psinfo.UseShellExecute = false;
            psinfo.CreateNoWindow = true;
            psinfo.Arguments = dll_name + '|' + application_id;
            System.Collections.IDictionary properties =
                new System.Collections.Hashtable();
            properties["portName"] = application_id + "_yukkuri_lib_kokkiemouse_" + dll_name;
            properties["exclusiveAddressUse"] = false;
            properties["name"] = dll_name;

            IpcChannel serverChannel = new IpcChannel(properties, null,
                new BinaryServerFormatterSinkProvider
                {
                    TypeFilterLevel=System.Runtime.Serialization.Formatters.TypeFilterLevel.Full,
                });
            ChannelServices.RegisterChannel(serverChannel, true);
            /*yukkui = (yukkuri_lib_interface.yukkuri_lib_interface)Activator.GetObject(typeof(yukkuri_lib_interface.yukkuri_lib_interface),
                "ipc://yukkuri_lib_kokkiemouse/" + dll_name);
            */
            CountdownEvent inited_ev = new CountdownEvent(1);
            yukkui = new yukkuri_lib_interface.yukkuri_lib_interface();
            yukkui.Oninit += new yukkuri_lib_interface.init_delegate(() =>
            {
                inited_ev.Signal();
            }
            );
            CountdownEvent loaded_dll_ev = new CountdownEvent(1);
            yukkui.OnDllLoaded += new dll_loaded_delegate(() =>
            {
                loaded_dll_ev.Signal();
            });
            RemotingServices.Marshal(yukkui, dll_name, typeof(yukkuri_lib_interface.yukkuri_lib_interface));
            System.Runtime.Remoting.Channels.ChannelDataStore channelData =
           (System.Runtime.Remoting.Channels.ChannelDataStore)
           serverChannel.ChannelData;
            foreach (string uri in channelData.ChannelUris)
            {
                Debug.WriteLine("The channel URI is {0}.", uri);
            }
            Process.Start(psinfo);
            inited_ev.Wait();

            yukkui.DllLoad_to_client(dll_path);
            

        }
        /// <summary>
        /// Aquestalkに話させるやつ。
        /// byte型で帰ってくるよ。
        /// </summary>
        /// <param name="speed">スピード</param>
        /// <param name="textd">テキスト</param>
        /// <param name="pitch">ピッチ(100で標準)</param>
        /// <returns>wavファイル</returns>
        public byte[] speak_wav(int speed,string textd,int pitch)
        {
            yukkuri_lib_interface_EventClass evc = new yukkuri_lib_interface_EventClass(textd, speed);
            byte[] wavd= yukkui.Speak_to_client(evc);
            uint samplingRate = BitConverter.ToUInt32(wavd, 24);
            uint dataRate = BitConverter.ToUInt32(wavd, 28);
            float pct_to = (float)pitch / 100;
            float samplingRatef = samplingRate;
            samplingRatef *= pct_to;
            samplingRate = (uint)samplingRatef;
            float dataRatef = dataRate;
            dataRatef *= pct_to;
            dataRate = (uint)dataRatef;
            byte[] samplingRateBytes = BitConverter.GetBytes(samplingRate);
            byte[] dataRateBytes = BitConverter.GetBytes(dataRate);
            Array.Copy(samplingRateBytes, 0, wavd, 24, 4);
            Array.Copy(dataRateBytes, 0, wavd, 28, 4);
            return wavd;

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed)
            {
                yukkui.Close_to_client();
                _disposed = true;


            }
        }
        ~yukkuri_lib()
        {
            Dispose(false);
        }


    }
}
