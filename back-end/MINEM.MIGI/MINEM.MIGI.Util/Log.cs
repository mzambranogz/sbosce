using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MINEM.MIGI.Util
{
    public static class Log
    {
        public static void Error(Exception ex)
        {
            try
            {
                String file = "";
                if (!Directory.Exists(HttpContext.Current.Request.PhysicalApplicationPath + "\\Log\\"))
                {
                    Directory.CreateDirectory(HttpContext.Current.Request.PhysicalApplicationPath + "\\Log");
                }
                file = HttpContext.Current.Request.PhysicalApplicationPath + "\\Log\\" + "Sistema" + DateTime.Today.Date.ToString("yyyyMMdd") + ".log";

                StreamWriter sw = new StreamWriter(file, true);
                StackTrace st = new StackTrace(ex, true);
                for (int i = 0; i < st.FrameCount; i++)
                {
                    StackFrame sf = st.GetFrame(i);
                    sw.WriteLine(DateTime.Now + ": [Método: " + sf.GetMethod() + ", Línea: " + sf.GetFileLineNumber() + "] - " + ex.Message);
                }
                sw.Close();
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
    }
}
