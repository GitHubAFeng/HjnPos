using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace hjn20160520.Common
{
    /// <summary>
    /// 电脑硬件信息查询，添加引用---System.Management
    /// </summary>
    public class HardwareHandler
    {

        /// <summary>
        /// 获取网卡ID
        /// </summary>
        /// <returns></returns>
        public static string GetNetworkAdpaterID()
        {
            try
            {
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac += mo["MacAddress"].ToString() + " ";
                        break;
                    }
                moc = null;
                mc = null;
                return mac.Trim();
            }
            catch (Exception)
            {
                return "error";
            }
        }


        /// <summary>
        /// 获取计算机名
        /// </summary>
        /// <returns></returns>
        public static string GetMachineName()
        {
            try
            {
                return System.Environment.MachineName;
            }
            catch (Exception)
            {
                return "error";
            }
        }


        /// <summary>
        /// 获取硬盘ID代码  
        /// </summary>
        /// <returns></returns>
        public static string GetHardDiskID()
        {
            try
            {
                string hdInfo = "";//硬盘序列号  
                ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
                hdInfo = disk.Properties["VolumeSerialNumber"].Value.ToString();
                disk = null;
                return hdInfo.Trim();
            }
            catch (Exception)
            {
                return "error";
            }
        }

        /// <summary>
        /// 操作系统的登录用户名
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            try
            {
                return System.Environment.UserName;
            }
            catch (Exception)
            {
                return "error";
            }
        }



        /// <summary>
        /// 获取CPU序列号
        /// </summary>
        /// <returns></returns>
        public static string GetCpuID()
        {
            try
            {
                string cpuInfo = ""; 
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject item in moc)
                {
                    cpuInfo = item.Properties["ProcessorId"].Value.ToString();
                    break;
                }
                moc = null;
                mc = null;
                return cpuInfo.Trim();
            }
            catch (Exception)
            {

                return "error";
            }
        }


    }
}
