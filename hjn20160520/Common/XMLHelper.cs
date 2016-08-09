using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Common
{
    /// <summary>
    /// XML与DataTable转换的工具类
    /// </summary>
    public class XMLHelper
    {
        /// <summary>
        /// 把DataTable转换为XML文件，成功则返回DataTable，失败则返回null
        /// </summary>
        /// <param name="xmlPath">XML文件路径，包含文件名+后缀</param>
        /// <returns></returns>
        public static DataTable XmlToDataTable(string xmlPath)
        {
            if (File.Exists(xmlPath))
            {
                try
                {
                    using (DataSet dt = new DataSet())
                    {
                        dt.ReadXml(xmlPath);
                        return dt.Tables[0];
                    }
                }
                catch (Exception e)
                {
                    LogHelper.WriteLog("DataTable转换为XML文件发生异常:", e);  //输出异常日志
                    return null;
                }
            }
            else
            {
                return null;
            }

        }


        /// <summary>
        /// 把DataTable保存为XML文件，成功则返回true，失败则返回false
        /// </summary>
        /// <param name="data">DataTable数据</param>
        /// <param name="XmlPath">保存路径，包含文件名+后缀</param>
        /// <returns></returns>
        public static bool DataTableToXml(object data ,string XmlPath)
        {
            if (!string.IsNullOrEmpty(XmlPath))
            {
                StreamWriter StrStream = null;
                XmlWriter writer = null;
                try
                {                
                    StrStream = new StreamWriter(XmlPath);
                    writer = XmlWriter.Create(StrStream);
                    XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
                    serializer.Serialize(writer, data as DataTable);
                    return true;
                }
                catch (Exception e)
                {
                    LogHelper.WriteLog("把DataTable保存为XML文件发生异常:", e);  //输出异常日志
                    return false;
                }
                finally
                {
                    //释放资源  
                    writer.Close();
                    StrStream.Close();
                    StrStream.Dispose();
                 }
            }
            else
            {
                return false;
            }
        }











    }
}
