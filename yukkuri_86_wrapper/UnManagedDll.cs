using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yukkuri_86_wrapper
{
    public class UnManagedDll : IDisposable
    {
        private static class NativeMethods
        {
            /// <summary>
            /// 指定された実行可能モジュールを、呼び出し側プロセスのアドレス空間内にマップします。
            /// </summary>
            /// <param name="lpLibFileName">実行可能モジュールの名前を保持する null で終わる文字列へのポインタ</param>
            /// <returns>モジュールのハンドル</returns>
            [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "LoadLibrary", BestFitMapping = false, ThrowOnUnmappableChar = true)]
            public static extern System.IntPtr LoadLibrary([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)] string lpLibFileName);

            /// <summary>
            /// ダイナミックリンクライブラリ（DLL）が持つ、指定されたエクスポート済み関数のアドレスを取得します。
            /// </summary>
            /// <param name="hModule">希望の関数を保持する DLL モジュールのハンドル</param>
            /// <param name="lpProcName">関数名を保持する null で終わる文字列へのポインタ</param>
            /// <returns>DLL のエクスポート済み関数のアドレス</returns>
            [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "GetProcAddress", BestFitMapping = false, ThrowOnUnmappableChar = true)]
            public static extern System.IntPtr GetProcAddress(System.IntPtr hModule, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)] string lpProcName);

            /// <summary>
            /// ロード済みのダイナミックリンクライブラリ（DLL）モジュールの参照カウントを 1 つ減らします。
            /// 参照カウントが 0 になると、モジュールは呼び出し側プロセスのアドレス空間からマップ解除され、そのモジュールのハンドルは無効になります。
            /// </summary>
            /// <param name="hModule">ロード済みの DLL モジュールのハンドル</param>
            /// <returns>関数が成功した場合は 0 以外</returns>
            [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
            [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool FreeLibrary(System.IntPtr hModule);
        }

        /// <summary>
        /// DLL のパスを取得、設定します。
        /// </summary>
        public string DllPath;

        /// <summary>
        /// アンマネージ DLL が使用可能かどうかを取得、設定します。
        /// </summary>
        public bool IsAvailable;

        /// <summary>
        /// リソースが解放されているかどうかを取得、設定します。
        /// </summary>
        protected bool Disposed;

        /// <summary>
        /// DLL モジュールのハンドルを取得、設定します。
        /// </summary>
        private System.IntPtr ModuleHandle;

        /// <summary>
        /// 関数名とその <see cref="System.Delegate"/> のコレクションを取得、設定します。
        /// </summary>
        private System.Collections.Generic.Dictionary<string, System.Delegate> Functions;
        /// <summary>
        /// <see cref="UnManagedDll"/> オブジェクトを生成します。
        /// </summary>
        public UnManagedDll()
        {
            Initialize();
        }
        ///<summary>
        ///<see cref="UnManagedDll"/> オブジェクトを生成します。
        ///</summary>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="dllPath"/> に DLL が存在しない場合、スローされます。</exception>
        /// <param name="dllPath">DLL のパス</param>
        public UnManagedDll(string dllPath)
        {
            Initialize();
            try
            {
                this.Load(dllPath);
            }catch (System.IO.FileNotFoundException e)
            {
                throw;
            }
        }
        /// <summary>
        /// <see cref="UnManagedDll"/> のデストラクタ
        /// </summary>
        ~UnManagedDll()
        {
            Dispose(false);
        }
        /// <summary>
        /// アンマネージ DLL を読み込みます。
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException"><paramref name="dllPath"/> に DLL が存在しない場合、スローされます。</exception>
        /// <param name="dllPath">DLL のパス</param>
        /// <returns>読み込みが成功した場合は true</returns>
        public bool Load(string dllPath)
        {
            this.DllPath = dllPath;

            if (!System.IO.File.Exists(this.DllPath))
            {
                this.IsAvailable = false;
                throw new System.IO.FileNotFoundException(this.DllPath + " は存在しません。");
            }

            this.ModuleHandle = NativeMethods.LoadLibrary(this.DllPath);
            this.IsAvailable = (this.ModuleHandle != System.IntPtr.Zero && this.ModuleHandle != null);

            return this.IsAvailable;
        }

        /// <summary>
        /// 指定されたエクスポート済み関数を取得します。
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException">DLL が存在しない場合、スローされます。</exception>
        /// <exception cref="System.NotImplementedException">DLL に指定された関数が存在しない場合、スローされます。</exception>
        /// <typeparam name="T">エクスポート済み関数の定義</typeparam>
        /// <returns>エクスポート済み関数</returns>
        public T GetProcAddress<T>() where T : class
        {
            return GetProcAddress<T>("");
        }

        /// <summary>
        /// 指定されたエクスポート済み関数を取得します。
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException">DLL が存在しない場合、スローされます。</exception>
        /// <exception cref="System.NotImplementedException">DLL に指定された関数が存在しない場合、スローされます。</exception>
        /// <typeparam name="T">エクスポート済み関数の定義</typeparam>
        /// <param name="alias">関数名</param>
        /// <returns>エクスポート済み関数</returns>
        public T GetProcAddress<T>(string alias) where T : class
        {
            if (!this.IsAvailable)
            {
                throw new System.IO.FileNotFoundException(this.DllPath + " は存在しません。");
            }

            string funcName = System.String.IsNullOrEmpty(alias) ? typeof(T).Name : alias;

            if (!this.Functions.ContainsKey(funcName))
            {
                System.IntPtr procAddress = NativeMethods.GetProcAddress(this.ModuleHandle, funcName);

                if (procAddress == System.IntPtr.Zero || procAddress == null)
                {
                    throw new System.NotImplementedException(this.DllPath + " に " + funcName + " は見つかりません。");
                }

                this.Functions[funcName] = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(procAddress, typeof(T));
            }

            return this.Functions[funcName] as T;
        }

        /// <summary>
        /// 割り当てられたリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 割り当てられたリソースを解放します。
        /// </summary>
        /// <param name="disposing">マネージドリソースの解放をする場合は true</param>
        protected void Dispose(bool disposing)
        {
            if (this.Disposed)
            {
                return;
            }

            this.Disposed = true;

            if (disposing)
            {
                this.Functions = null;
            }

            if (this.ModuleHandle != System.IntPtr.Zero || this.ModuleHandle != null)
            {
                NativeMethods.FreeLibrary(this.ModuleHandle);
                this.ModuleHandle = System.IntPtr.Zero;
            }
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        private void Initialize()
        {
            this.IsAvailable = false;
            this.Disposed = false;
            this.Functions = new System.Collections.Generic.Dictionary<string, System.Delegate>();
            this.ModuleHandle = System.IntPtr.Zero;
        }
    }
}
