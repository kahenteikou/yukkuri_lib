using System.Reflection;


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
[assembly: AssemblyKeyFileAttribute("kokkiemouse.snk")]

namespace yukkuri_86_wrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1) //引数がないのは丘peopleなので...
            {
                Console.Error.WriteLine("Error!");
                return;
            }
            string[] argskun = args[0].Split('|');  //argsを分割。
            yukkuri_lib_interface.yukkuri_lib_interface yukkuri_inter;  //インタフェースを作成。
            Dictionary<string, string> channelproperty = new Dictionary<string, string>();  //チャンネル作成用。
            channelproperty.Add("portName", argskun[1] + "_yukkuri_lib_kokkiemouse_client_" + argskun[0]);
            channelproperty.Add("name", argskun[0]);
            IpcChannel servChannel = new IpcChannel(channelproperty,null,new BinaryServerFormatterSinkProvider //チャンネル作成。
            { 
                TypeFilterLevel=System.Runtime.Serialization.Formatters.TypeFilterLevel.Full,
            });
            ChannelServices.RegisterChannel(servChannel, true); //チャンネル登録。
            yukkuri_inter = Activator.GetObject(typeof(yukkuri_lib_interface.yukkuri_lib_interface),
                "ipc://" + argskun[1] + "_yukkuri_lib_kokkiemouse_"+ argskun[0] + '/' + argskun[0]) as yukkuri_lib_interface.yukkuri_lib_interface; //サーバー(64bit)のインタフェースを取得。
            AquesTalk aq=new AquesTalk();   //Aquestalkのクラスを取得。
            CountdownEvent cekun = new CountdownEvent(1);   //待機用。
            yukkuri_lib_interface.EventCallbackSink ebthink = new EventCallbackSink();  //コールバック関数
            ebthink.OnClose += new CloseDelegate(() =>  //Oncloseで呼ばれる。
            {
                cekun.Signal();//復帰。(終了)
            });
            ebthink.OnSpeak += new yukkuri_lib_interface.SpeakDelegate((yukkuri_lib_interface_EventArgs yukkuargs) =>   //OnSpeakを定義
            {
                int speed = yukkuargs.eventargs.speed;//speedを突っ込む。
                int size = 0;//サイズが入る。
                string koe = yukkuargs.eventargs.textdata;  //コピー。
                IntPtr wavPtr = aq.AquesTalk_Synthe(koe, speed, out size);  //Aquestalk呼び出し。ポインタが返ってくる。
                if (wavPtr == IntPtr.Zero)  //ぬるぽなら
                {
                    return new byte[] { 0 }; //ゼロを返す。
                }
                byte[] wavdata = new byte[size]; //C#側で配列を確保。
                Marshal.Copy(wavPtr, wavdata, 0, size);//ポインタの中身を配列にコピー。
                aq.AquesTalk_FreeWave(wavPtr);//ポインタはもういらないしトラブルの元なので即開放
                return wavdata;//コピーした配列を返す。
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
            ebthink.OnDllLoad += new Dll_load_delegate((yukkuri_lib_interface_dllload_args eargs) =>  //OnDllLoadで呼ばれる。
            {
                aq = new AquesTalk(eargs.dll_path); //パスをもとにAquestalkをロード。
                yukkuri_inter.dll_loaded();//ロード完了のイベント送信。
            });
            Dll_load_delegate dllldel = new Dll_load_delegate(ebthink.DllLoadtoClient); //delegateを定義。
            SpeakDelegate spd = new SpeakDelegate(ebthink.SpeakCallBackToClient);//delegateを定義。
            CloseDelegate cld = new CloseDelegate(ebthink.Close_toClient);//delegateを定義。
            yukkuri_inter.AddEventListener_Dllload(dllldel);//delegateを突っ込む。
            yukkuri_inter.AddEventListener_Speak(spd);//delegateを突っ込む。
            yukkuri_inter.AddEventListener_close(cld);//delegateを突っ込む。
            yukkuri_inter.inited();//初期化完了イベントを発行。
            cekun.Wait();//閉じないように。

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
