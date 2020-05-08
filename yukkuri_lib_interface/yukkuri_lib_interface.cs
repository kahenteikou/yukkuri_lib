using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// <summary>
    /// Server(64bit側)からのCallのインターフェース
    /// <see cref="MarshalByRefObject"/>を継承してる。
    /// </summary>
    public class EventCallbackSink : MarshalByRefObject
    {
        /// <summary>
        /// Serverから<see cref="yukkuri_lib_interface.Speak_to_client(yukkuri_lib_interface_EventClass)"/>を呼ぶと呼ばれるイベント。
        /// </summary>
        public event SpeakDelegate OnSpeak;
        /// <summary>
        /// Serverから<see cref="DllLoadtoClient(yukkuri_lib_interface_dllload_args)"/>を呼ぶと呼ばれるイベント。
        /// </summary>
        public event Dll_load_delegate OnDllLoad;
        /// <summary>
        /// Serverから<see cref="Close_toClient"/>を呼ぶと呼ばれるイベント。
        /// </summary>
        public event CloseDelegate OnClose;
        /// <summary>
        /// <see cref="EventCallbackSink"/>のコンストラクタ。何もしないよ。
        /// </summary>
        public EventCallbackSink()
        {

        }
        /// <summary>
        /// Serverから呼ばれるやつ。
        /// パラメータに指定したものでwavを生成するよ。
        /// </summary>
        /// <param name="evargs">パラメータのオブジェクト</param>
        /// <returns>wavファイルの<see cref="byte"/>配列</returns>
        public byte[] SpeakCallBackToClient(yukkuri_lib_interface_EventArgs evargs)
        {
            return OnSpeak?.Invoke(evargs); //OnSpeakイベントを呼び出し。
        }
        /// <summary>
        /// Serverから呼ばれるやつ。
        /// Dllをロードする際に呼ばれる。
        /// </summary>
        /// <param name="dargs">DLLファイルに関するオブジェクト</param>
        public void DllLoadtoClient(yukkuri_lib_interface_dllload_args dargs)
        {
            OnDllLoad?.Invoke(dargs);   //OnDllLoadイベントを呼び出し。
        }
        /// <summary>
        /// Serverから終了する際に呼ばれるやつ。
        /// これを呼ぶと32bitの方が終了する。
        /// </summary>
        public void Close_toClient()
        {
            OnClose?.Invoke();  //OnCloseイベントを呼び出し。
        }

    }
    /// <summary>
    /// <see cref="yukkuri_lib_interface_dllload_args"/>で使われるやつ。
    /// スピード、テキストデータが入るよ。
    /// </summary>
    public class yukkuri_lib_interface_EventClass
    {
        /// <summary>
        ///音声記号列 
        /// </summary>
        public string textdata { get; set; }
        /// <summary>
        /// スピード(標準は100)
        /// </summary>
        public int speed { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="txtdata">音声記号列</param>
        /// <param name="speedkun">スピード(標準は100)</param>
        public yukkuri_lib_interface_EventClass(string txtdata, int speedkun)
        {
            speed = speedkun;
            textdata = txtdata; //突っ込んでるだけ。
        }
        /// <summary>
        /// 無いとエラーが起きた。
        /// 深い意味はない。
        /// </summary>
        public yukkuri_lib_interface_EventClass()
        {
            textdata = "";
            speed = 100;
        }
    }
    /// <summary>
    /// 通信用。
    /// <see cref="yukkuri_lib_interface.DllLoad_to_client(string)"/>で使われるよ。
    /// 
    /// </summary>
    [Serializable]
    public class yukkuri_lib_interface_dllload_args : ISerializable
    {
        /// <summary>
        /// dllのパス。
        /// 
        /// </summary>
        public string dll_path;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dllpath">dllのパス。</param>
        public yukkuri_lib_interface_dllload_args(string dllpath)
        {
            dll_path = dllpath;
        }
        /// <summary>
        /// カスタムデシリアライズ用。
        /// </summary>
        /// <param name="info">データが入るらしい。</param>
        /// <param name="context">使わん。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected yukkuri_lib_interface_dllload_args(SerializationInfo info,StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = info.GetString("dll_path");  //シリアライズされたJSONデータを取得。
            dll_path = ser.Deserialize<string>(json);   //jsonデータをデシリアライズして突っ込む。
        }
        /// <summary>
        /// シリアライズ用。
        /// </summary>
        /// <param name="info">シリアライズ用のデータ。</param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info,StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();  //シリアライザーを作成。
            var json = ser.Serialize(dll_path); //dll_pathをシリアライズ。
            info.AddValue("dll_path", json);    //シリアライズしたものを突っ込む。
        }
    }
    /// <summary>
    /// 通信用。
    /// <see cref="yukkuri_lib_interface.Speak_to_client(yukkuri_lib_interface_EventClass)"/>で使われる。
    /// </summary>
    [Serializable]
    public class yukkuri_lib_interface_EventArgs : ISerializable
    {
        /// <summary>
        /// データが入ってるオブジェクト。
        /// </summary>
        public yukkuri_lib_interface_EventClass eventargs;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="evebtar">データが入った<see cref="yukkuri_lib_interface_EventClass"/>オブジェクト</param>
        public yukkuri_lib_interface_EventArgs(yukkuri_lib_interface_EventClass evebtar)
        {
            this.eventargs = evebtar;
        }
        /// <summary>
        /// デシリアライズ用。
        /// </summary>
        /// <param name="info">データ処理先。</param>
        /// <param name="context">使わない。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected yukkuri_lib_interface_EventArgs(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = info.GetString("eventargs");
            eventargs = ser.Deserialize<yukkuri_lib_interface_EventClass>(json);
        }
        /// <summary>
        /// シリアライズ用
        /// </summary>
        /// <param name="info">データ</param>
        /// <param name="context">使わん。</param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var json = ser.Serialize(eventargs);
            info.AddValue("eventargs", json);
        }
    }
    /// <summary>
    /// 32bitと64bitをつなぐインターフェース。
    /// <see cref="MarshalByRefObject"/>を継承してる。
    /// </summary>
    public class yukkuri_lib_interface : MarshalByRefObject
    {
        private List<SpeakDelegate> eventListeners_speak = new List<SpeakDelegate>();
        private List<CloseDelegate> closeListeners = new List<CloseDelegate>();
        private List<Dll_load_delegate> eventListeners_dllload = new List<Dll_load_delegate>();
        /// <summary>
        /// 初期化完了後にクライアント(32bit)が呼び出すイベント。
        /// </summary>
        public event init_delegate Oninit;
        /// <summary>
        /// DLLが読み込まれた後にClient(32bit)が呼び出すイベント。
        /// </summary>
        public event dll_loaded_delegate OnDllLoaded;
        /// <summary>
        /// Clientが呼び出す。
        /// <see cref="yukkuri_lib_interface.Oninit"/>を呼び出すだけ。
        /// </summary>
        public void inited()
        {
            Oninit?.Invoke();
        }
        /// <summary>
        /// Clientが呼び出す。
        /// <see cref="OnDllLoaded"/>を呼び出す。
        /// </summary>
        public void dll_loaded()
        {
            OnDllLoaded?.Invoke();
        }
        /// <summary>
        /// <see cref="Speak_to_client"/>のListenerに追加。
        /// Client側が使う。
        /// </summary>
        /// <param name="listener"><see cref="SpeakDelegate"/>型の関数。ラムダ式を使うと楽。</param>
        public void AddEventListener_Speak(SpeakDelegate listener)
        {
            eventListeners_speak.Add(listener);
        }

        /// <summary>
        /// <see cref="DllLoad_to_client"/>のListenerに追加。
        /// Client側が使う。
        /// </summary>
        /// <param name="listener"><see cref="Dll_load_delegate"/>型の関数。ラムダ式を使うと楽。</param>
        public void AddEventListener_Dllload(Dll_load_delegate listener)
        {
            eventListeners_dllload.Add(listener);
        }

        /// <summary>
        /// <see cref="Close_to_client"/>のListenerに追加。
        /// Client側が使う。
        /// </summary>
        /// <param name="cl"><see cref="CloseDelegate"/>型の関数。ラムダ式を使うと楽。</param>
        public void AddEventListener_close(CloseDelegate cl)
        {
            closeListeners.Add(cl);
        }
        /// <summary>
        /// Serverが呼び出す。
        /// 指定した内容でwavを生成して返す。
        /// </summary>
        /// <param name="paramkun">指定する内容。</param>
        /// <returns>wavファイル</returns>
        public byte[] Speak_to_client(yukkuri_lib_interface_EventClass paramkun)
        {
            yukkuri_lib_interface_EventArgs evt = new yukkuri_lib_interface_EventArgs(paramkun);    //引数を生成
            foreach(SpeakDelegate listener in eventListeners_speak) 
            {
                return listener(evt);   //実行する。
            }
            return new byte[] { 0 };    //エラー時。
        }
        /// <summary>
        /// Serverが使用。
        /// Clientを落とすときに使用。
        /// </summary>
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
                clel(); //実行
            }
        }
        /// <summary>
        /// Serverが使用。
        /// Dllをロードさせるときに使用。
        /// </summary>
        /// <param name="dllpath"></param>
        public void DllLoad_to_client(string dllpath)
        {
            yukkuri_lib_interface_dllload_args evt = new yukkuri_lib_interface_dllload_args(dllpath);   //イベントの引数を作成。
            foreach (Dll_load_delegate listener in eventListeners_dllload)
            {
                listener(evt);//実行
            }
        }
        /// <summary>
        /// 勝手に消されないように。
        /// </summary>
        /// <returns>しらん。</returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }

    }
}
