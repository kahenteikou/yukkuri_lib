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
    /// <summary>
    /// <see cref="yukkuri_lib"/>のコア部分。
    /// usingで使うといいよ。
    /// <see cref="IDisposable"/>を継承している。
    /// </summary>
    public class yukkuri_lib : IDisposable
    {
        private yukkuri_lib_interface.yukkuri_lib_interface yukkui=null;
        private bool _disposed = false;
        /// <summary>
        /// <see cref="yukkuri_lib"/> のコンストラクタ
        /// </summary>
        /// <param name="dll_path">使うAquestalkのDLLパス</param>
        /// <param name="application_id"> 独自のid。競合防止のため。</param>
        public yukkuri_lib(string dll_path,string application_id)
        {
            if (!File.Exists(dll_path))
            {
                throw new System.IO.FileNotFoundException("DLL Load failed! " + dll_path + " is not found!");
            }
            string dll_name = Path.GetFileName(Path.GetDirectoryName(dll_path));    //dllのフォルダ名を取得。
            ProcessStartInfo psinfo = new ProcessStartInfo();   //子プロセス用。
            psinfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\yukkuri_86_wrapper.exe";  //子プロセスのファイル名。
            psinfo.UseShellExecute = false; //シェルは使わん。
            psinfo.CreateNoWindow = true;   //ウィンドウも作らん。
            string dtnow = DateTime.Now.ToString("ddHHmmssfff");    //現在時刻を取得
            psinfo.Arguments = dll_name + '|' + application_id + dtnow; //現在時刻をもとに作り、競合を回避。。
            System.Collections.IDictionary properties =
                new System.Collections.Hashtable(); //ハッシュテーブルを作成。
            properties["portName"] = application_id + dtnow + "_yukkuri_lib_kokkiemouse_" + dll_name;   //ポートネーム生成。
            properties["exclusiveAddressUse"] = false;
            properties["name"] = dll_name;

            IpcChannel serverChannel = new IpcChannel(properties, null,
                new BinaryServerFormatterSinkProvider
                {
                    TypeFilterLevel=System.Runtime.Serialization.Formatters.TypeFilterLevel.Full,
                });//チャンネル生成。
            ChannelServices.RegisterChannel(serverChannel, true);   //チャンネル登録
            /*yukkui = (yukkuri_lib_interface.yukkuri_lib_interface)Activator.GetObject(typeof(yukkuri_lib_interface.yukkuri_lib_interface),
                "ipc://yukkuri_lib_kokkiemouse/" + dll_name);
            */
            CountdownEvent inited_ev = new CountdownEvent(1);   //待機用。
            yukkui = new yukkuri_lib_interface.yukkuri_lib_interface(); //インターフェースを生成。
            yukkui.Oninit += new yukkuri_lib_interface.init_delegate(() =>          //初期化後に実行される
            {
                inited_ev.Signal(); //処理を再開。
            }
            );
            CountdownEvent loaded_dll_ev = new CountdownEvent(1);   //dllを読み込むまでの待機用。
            yukkui.OnDllLoaded += new dll_loaded_delegate(() => //dllが読み込まれると実行
            {
                loaded_dll_ev.Signal(); //処理を再開。
            });
            RemotingServices.Marshal(yukkui, dll_name, typeof(yukkuri_lib_interface.yukkuri_lib_interface));    //定義したオブジェクトを登録。
            System.Runtime.Remoting.Channels.ChannelDataStore channelData =
           (System.Runtime.Remoting.Channels.ChannelDataStore)
           serverChannel.ChannelData;
            foreach (string uri in channelData.ChannelUris)
            {
                Debug.WriteLine("The channel URI is {0}.", uri);    //デバッグ用。
            }
            Process.Start(psinfo);//子プロセス起動。
            inited_ev.Wait();   //初期化が完了するまで待機。
            try
            {
                yukkui.DllLoad_to_client(dll_path); //dllを読み込み。
            }catch (Exception)
            {
                throw;
            }
            

        }
        /// <summary>
        /// Aquestalkに話させるやつ。
        /// <see cref="byte"/>配列で帰ってくるよ。
        /// </summary>
        /// <param name="speed">スピード</param>
        /// <param name="textd">テキスト</param>
        /// <param name="pitch">ピッチ(100で標準)</param>
        /// <returns>wavファイル</returns>
        public byte[] speak_wav(int speed,string textd,int pitch) 
        {
            yukkuri_lib_interface_EventClass evc = new yukkuri_lib_interface_EventClass(textd, speed);  //32bitの方に与えるパラメータを初期化。
            yukkuri_lib_interface.SPEAK_RETURN spr = yukkui.Speak_to_client(evc); //32bit側を呼び出し。byte配列でwavファイルが返ってくる。
            if (spr.error.err_code.Equals(yukkuri_lib_interface.DLL_ERR_CODE.NULLPOINTER_OTHER)){
                throw new Wave_NULLException(spr.error.message,new System.NullReferenceException());
            }
            byte[] wavd = spr.wavdata;
            uint samplingRate = BitConverter.ToUInt32(wavd, 24);//サンプリングレートを算出。
            uint dataRate = BitConverter.ToUInt32(wavd, 28);//データレートを算出。
            float pct_to = (float)pitch / 100;  //ピッチの割合を計算。
            float samplingRatef = samplingRate; //サンプリング周波数をfloatに。
            samplingRatef *= pct_to;    //サンプリング周波数をpitchで変化させる。
            samplingRate = (uint)samplingRatef;//floatなサンプリング周波数をintへコンバート。
            float dataRatef = dataRate; //データレートをfloatへ。
            dataRatef *= pct_to;//データレートをpitchで変化させる。
            dataRate = (uint)dataRatef;//データレートをfloatからintへ。
            byte[] samplingRateBytes = BitConverter.GetBytes(samplingRate);
            byte[] dataRateBytes = BitConverter.GetBytes(dataRate);
            Array.Copy(samplingRateBytes, 0, wavd, 24, 4);
            Array.Copy(dataRateBytes, 0, wavd, 28, 4);
            return wavd;    //改造版wavを返す。

        }
        /// <summary>
        /// 開放用。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 開放用。
        /// </summary>
        /// <param name="disposing">使わない。</param>
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed)
            {
                if (yukkui!=null)
                {
                    yukkui.Close_to_client();   //clientを落とす。
                }
                _disposed = true;


            }
        }
        /// <summary>
        /// 開放用。
        /// </summary>
        ~yukkuri_lib()
        {
            Dispose(false);
        }


    }
}
