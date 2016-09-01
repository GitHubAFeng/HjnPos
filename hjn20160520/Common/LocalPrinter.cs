using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Common
{
    /// <summary>
    /// 获取本地打印机信息  
    /// </summary>
    public class LocalPrinter
    {
        private static PrintDocument fPrintDocument = new PrintDocument();


        [DllImport("winspool.drv")]
        public static extern bool SetDefaultPrinter(String Name); //调用win api将指定名称的打印机设置为默认打印机 


        /// <summary>  
        /// 获取本机默认打印机名称  
        /// </summary>  
        public static String DefaultPrinter
        {
            get { return fPrintDocument.PrinterSettings.PrinterName; }
        }
        /// <summary>  
        /// 获取本机的打印机列表。列表中的第一项就是默认打印机。  
        /// </summary>  
        public static List<String> GetLocalPrinters()
        {
            List<String> fPrinters = new List<string>();
            fPrinters.Add(DefaultPrinter); // 默认打印机始终出现在列表的第一项  
            foreach (String fPrinterName in PrinterSettings.InstalledPrinters)
            {
                if (!fPrinters.Contains(fPrinterName))
                    fPrinters.Add(fPrinterName);
            }
            return fPrinters;
        }





    }



}

