using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using yukkuri_lib;

namespace yukkuri_x64
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            using (yukkuri_lib.yukkuri_lib yukkuri_f1 = new yukkuri_lib.yukkuri_lib("Aquestalk\\f1\\Aquestalk.dll","test"))
            {
                using (yukkuri_lib.yukkuri_lib yukkuri_f2 = new yukkuri_lib.yukkuri_lib("Aquestalk\\f2\\Aquestalk.dll","test"))
                {
                    using (yukkuri_lib.yukkuri_lib yukkuri_m1 = new yukkuri_lib.yukkuri_lib("Aquestalk\\m1\\Aquestalk.dll", "test"))
                    {
                        using (yukkuri_lib.yukkuri_lib yukkuri_imd1 = new yukkuri_lib.yukkuri_lib("Aquestalk\\imd1\\Aquestalk.dll", "test"))
                        {
                            using (yukkuri_lib.yukkuri_lib yukkuri_jgr = new yukkuri_lib.yukkuri_lib("Aquestalk\\jgr\\Aquestalk.dll", "test"))
                            {
                                playaq(100, 100, "はげ", yukkuri_f1);
                                playaq(100, 100, "はげ", yukkuri_f2);
                                playaq(100, 100, "aaa", yukkuri_m1);
                                playaq(100, 100, "はげ", yukkuri_imd1);
                                playaq(100, 100, "はげ", yukkuri_jgr);
                            }
                        }
                    }
                }


            }
            */
            try
            {
                using (yukkuri_lib.yukkuri_lib yukkuri_qa = new yukkuri_lib.yukkuri_lib("Aquestalk\\jgr\\Aquestalk.dll", "test"))
                {
                    playaq(100, 100, "ほも", yukkuri_qa); 
                    /*
                    playaq(100, 100, "<hage", yukkuri_qa);
                    playaq(100, 100, "ほも", yukkuri_qa);
                    playaq(100, 100, "ほも", yukkuri_qa);
                    */
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error! " + e.ToString());
            }
        }
        static void playaq(int speed,int pitch,string text,yukkuri_lib.yukkuri_lib yklib)
        {
            try
            {
                byte[] wavdata = yklib.speak_wav(speed, text, pitch);
                if (wavdata.Length == 1)
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
            }catch (Exception e)
            {

                Console.Error.WriteLine("Error! " + e.ToString());
            }
        }
    }
}
