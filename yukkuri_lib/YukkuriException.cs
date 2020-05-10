using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace yukkuri_lib
{
    /// <summary>
    /// Waveファイルに指定した物が丘peopleな時に発生する例外です。
    /// </summary>
    public class WaveArgsException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WaveArgsException() : base()
        {

        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message">メッセージ</param>
        public WaveArgsException(string message) : base(message)
        {

        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="innerException">例外</param>
        public WaveArgsException(string message,Exception innerException) : base(message,innerException)
        {

        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="serializationInfo"><see cref="SerializationInfo"/></param>
        /// <param name="context"><see cref="StreamingContext"/></param>
        protected WaveArgsException(SerializationInfo serializationInfo,StreamingContext context) : base(serializationInfo,context)
        {

        }
    }
    /// <summary>
    /// 丘peopleな時に発生する例外です。
    /// </summary>
    public class Wave_NULLException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Wave_NULLException() : base()
        {

        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message">メッセージ</param>
        public Wave_NULLException(string message) : base(message)
        {

        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="innerException">例外</param>
        public Wave_NULLException(string message, Exception innerException) : base(message, innerException)
        {

        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="serializationInfo"><see cref="SerializationInfo"/></param>
        /// <param name="context"><see cref="StreamingContext"/></param>
        protected Wave_NULLException(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context)
        {

        }
    }
}
