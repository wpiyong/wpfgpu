using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ImageProcessing.Util
{
    public static class IQUtil
    {
        private static string TAG = "IQUtil";
	
	    public static bool s_bIsDebug = true;

        public static bool s_bSaveAsEnabled = true;


        //public static Context context;

        public static void IQAssert()
        {
            if (s_bIsDebug)
            {
                throw new Exception("IQAssert");
            }
        }

        public static void IQAssert(bool bTest)
        {
            if (s_bIsDebug)
            {
                if (bTest == false)
                {
                    throw new Exception("IQAssert");
                }
            }
        }

        public static void checkGlError(string op)
        {
            if (IQUtil.s_bIsDebug)
            {
                ErrorCode IError;

                while ((IError = GL.GetError()) != ErrorCode.NoError)
                {
                    // todo: add error info
                    Console.WriteLine("{0} error!", op);
                    IQUtil.IQAssert();
                }
            }
        }

        public static void IQLog(string szTage, string szMessage, bool bDisplayToScreen)
        {
            Console.WriteLine("{0}, {1}", szTage, szMessage);
        }
    }
}
