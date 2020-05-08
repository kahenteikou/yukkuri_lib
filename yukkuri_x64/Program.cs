using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace yukkuri_x64
{
    class Program
    {
        static void Main(string[] args)
        {
            using (yukkuri_lib.yukkuri_lib yukkuri_f1 = new yukkuri_lib.yukkuri_lib("Aquestalk\\f1\\Aquestalk.dll","test"))
            {
                using (yukkuri_lib.yukkuri_lib yukkuri_f2 = new yukkuri_lib.yukkuri_lib("Aquestalk\\f2\\Aquestalk.dll","test"))
                {
                    using (yukkuri_lib.yukkuri_lib yukkuri_m1 = new yukkuri_lib.yukkuri_lib("Aquestalk\\m1\\Aquestalk.dll", "test"))
                    {
                        using (yukkuri_lib.yukkuri_lib yukkuri_r1 = new yukkuri_lib.yukkuri_lib("Aquestalk\\r1\\Aquestalk.dll", "test"))
                        {
                            using (yukkuri_lib.yukkuri_lib yukkuri_jgr = new yukkuri_lib.yukkuri_lib("Aquestalk\\jgr\\Aquestalk.dll", "test"))
                            {
                                playaq(100, 100, "はげ", yukkuri_f1);
                                playaq(100, 100, "はげ", yukkuri_f2);
                                playaq(100, 100, "はげ", yukkuri_m1);
                                playaq(100, 100, "はげ", yukkuri_r1);
                                playaq(100, 100, "はげ", yukkuri_jgr);
                            }
                        }
                    }
                }


            }
        }
        static void playaq(int speed,int pitch,string text,yukkuri_lib.yukkuri_lib yklib)
        {
            byte[] wavdata = yklib.speak_wav(speed, text, pitch);
            if (wavdata.Equals(new byte[] { 0 }))
            {
                return;
            }
            using (MemoryStream memstr = new MemoryStream(wavdata))
            {
                using (SoundPlayer sp = new SoundPlayer(memstr))
                {
                    sp.PlaySync();
                }
            }
        }
    }
}
