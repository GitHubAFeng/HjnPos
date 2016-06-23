using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelData
{
    public class NPOIForExcel
    {
        /// <summary>
        /// 使用NPOI把DataTable数据源导出Excel表格，现设置导出格式为2003版本，返回true则导出成功，否则失败。
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="filePath">导出的文件路径，需要包含文件名+后缀，例如 E:\\目录\\文件名.xls</param>
        /// <param name="sheetName">表格中工作薄名，默认值为"MySheet"</param>
        /// <param name="isHasColumnName">是否输出数据源中的列名，默认值为true</param>
        /// <returns>返回true则导出成功，否则失败</returns>
        public static bool TestExcelWrite(DataTable data, string filePath, string sheetName = "MySheet", bool isHasColumnName = true)
        {
            try
            {
                using (ExcelHelper excelHelper = new ExcelHelper(filePath))
                {
                    int count = excelHelper.DataTableToExcel(data, sheetName, isHasColumnName);
                    if (count > 0)
                    {
                        //Console.WriteLine("Number of imported data is {0} ", count);
                        //System.Windows.Forms.MessageBox.Show("Number of imported data is "+ count.ToString());
                        return true; //导出成功
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("导出Excel表格时发生异常:", ex);
            }
            return false; //导出失败
        }

        /// <summary>
        /// 使用NPOI读取Excel表格，成功则返回DataTable数据源，失败则返回null空。
        /// </summary>
        /// <param name="filePath">文件路径，需要包含文件名+后缀，例如 E:\\目录\\文件名.xls</param>
        /// <param name="sheetName">需要读取的工作薄名，默认值为"MySheet"，如果找不到则读取第一个工作薄</param>
        /// <param name="isFirstRowColumn">是否把表格首行设置为列名，默认值为true</param>
        /// <returns>成功则返回DataTable数据源，失败则返回null空</returns>
        public static DataTable TestExcelRead(string filePath, string sheetName = "MySheet", bool isFirstRowColumn = true)
        {
            try
            {
                using (ExcelHelper excelHelper = new ExcelHelper(filePath))
                {
                    DataTable dt = excelHelper.ExcelToDataTable(sheetName, isFirstRowColumn);
                    return dt;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("导入Excel表格时发生异常:", ex);
            }

            return null;
        }




    }
}
