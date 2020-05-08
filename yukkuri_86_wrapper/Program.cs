using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;
using yukkuri_lib_interface;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Threading;

namespace yukkuri_86_wrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.Error.WriteLine("Error!");
                return;
            }
            string[] argskun = args[0].Split('|');
            yukkuri_lib_interface.yukkuri_lib_interface yukkuri_inter;
            Dictionary<string, string> channelproperty = new Dictionary<string, string>();
            channelproperty.Add("portName", argskun[1] + "_yukkuri_lib_kokkiemouse_client_" + argskun[0]);
            channelproperty.Add("name", argskun[0]);
            IpcChannel servChannel = new IpcChannel(channelproperty,null,new BinaryServerFormatterSinkProvider
            { 
                TypeFilterLevel=System.Runtime.Serialization.Formatters.TypeFilterLevel.Full,
            });
            ChannelServices.RegisterChannel(servChannel, true);
            yukkuri_inter = Activator.GetObject(typeof(yukkuri_lib_interface.yukkuri_lib_interface),
                "ipc://" + argskun[1] + "_yukkuri_lib_kokkiemouse_"+ argskun[0] + '/' + argskun[0]) as yukkuri_lib_interface.yukkuri_lib_interface;
            bool close_disable = true;
            bool loaded = false;
            AquesTalk aq=new AquesTalk();
            CountdownEvent cekun = new CountdownEvent(1);
            yukkuri_lib_interface.EventCallbackSink ebthink = new EventCallbackSink();
            ebthink.OnClose += new CloseDelegate(() =>
            {
                cekun.Signal();
            });
            ebthink.OnSpeak += new yukkuri_lib_interface.SpeakDelegate((yukkuri_lib_interface_EventArgs yukkuargs) => 
            {
                int speed = yukkuargs.eventargs.speed;
                int size = 0;
                string koe = yukkuargs.eventargs.textdata;
                IntPtr wavPtr = aq.AquesTalk_Synthe(koe, speed, out size);
                if (wavPtr == IntPtr.Zero)
                {
                    return new byte[] { 0 };
                }
                byte[] wavdata = new byte[size];
                Marshal.Copy(wavPtr, wavdata, 0, size);
                aq.AquesTalk_FreeWave(wavPtr);
                return wavdata;
            });
            /*
            yukkuri_inter._run_speak += new yukkuri_lib_interface.yukkuri_lib_interface.CallEventHandler((ref byte[] wav, yukkuri_lib_interface.yukkuri_lib_interface.yukkuri_lib_interface_EventArgs e) =>
            {
                if (!loaded) return;
                int speed = e.speed;
                int size = 0;
                string koe = e.textdata;
                IntPtr wavPtr = aq.AquesTalk_Synthe(koe, speed, out size);
                if(wavPtr==IntPtr.Zero )
                {
                    wav= new byte[] { 0 };
                    return;
                }
                wav = new byte[size];
                Marshal.Copy(wavPtr, wav, 0, size);
                aq.AquesTalk_FreeWave(wavPtr);
            });
            */
            ebthink.OnDllLoad += new Dll_load_delegate((yukkuri_lib_interface_dllload_args eargs) => 
            {
                aq = new AquesTalk(eargs.dll_path);
                yukkuri_inter.dll_loaded();
            });
            Dll_load_delegate dllldel = new Dll_load_delegate(ebthink.DllLoadtoClient);
            SpeakDelegate spd = new SpeakDelegate(ebthink.SpeakCallBackToClient);
            CloseDelegate cld = new CloseDelegate(ebthink.Close_toClient);
            yukkuri_inter.AddEventListener_Dllload(dllldel);
            yukkuri_inter.AddEventListener_Speak(spd);
            yukkuri_inter.AddEventListener_close(cld);
            yukkuri_inter.inited();
            cekun.Wait();

        }
    }
    public class AquesTalk : UnManagedDll
    {
        private class Functions
        {
            public delegate System.IntPtr AquesTalk_Synthe(string koe, int speed, out int size);
            public delegate void AquesTalk_FreeWave(System.IntPtr wav);

        }
        /// <summary>
        /// Aquestalk Obj
        /// </summary>
        /// <param name="dllPath">aquestalk path</param>
        public AquesTalk(string dllPath) : base(dllPath) { }
        public AquesTalk() : base() { }
        /// <summary>
        /// 音声記号列から音声波形を生成します。
        /// </summary>
        /// <param name="koe">音声記号列</param>
        /// <param name="speed">発話速度（50-300、デフォルトは100）</param>
        /// <param name="size">生成した音声データのサイズ</param>
        /// <returns>WAV フォーマットのデータ</returns>
        public System.IntPtr AquesTalk_Synthe(string koe, int speed, out int size)
        {
            return GetProcAddress<Functions.AquesTalk_Synthe>()(koe, speed, out size);
        }
        /// <summary>
        /// 音声データの領域を開放します。
        /// </summary>
        /// <param name="wav">WAV フォーマットのデータ</param>
        public void AquesTalk_FreeWave(System.IntPtr wav)
        {
            GetProcAddress<Functions.AquesTalk_FreeWave>()(wav);
        }
    }
}
