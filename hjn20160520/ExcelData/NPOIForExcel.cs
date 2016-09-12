using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelData
{
    /// <summary>
    /// 导出导入Excel表格工具类
    /// </summary>
    public class NPOIForExcel
    {
        /// <summary>
        /// 弹窗保存Excel，使用NPOI把DataTable数据源导出Excel表格，现设置导出格式为2003版本，返回true则导出成功，否则失败。
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="sheetName">表格中工作薄名，默认值为"MySheet"</param>
        /// <param name="isHasColumnName">是否输出数据源中的列名，默认值为true</param>
        /// <returns>返回保存路径则导出成功，否则失败</returns>
        public static string ToExcelWrite(object data, string fileName = "报表", string sheetName = "MySheet", string txtPath = "", bool isHasColumnName = true)
        {

            try
            {
                if (txtPath == "")
                {

                    FolderBrowserDialog BrowDialog = new FolderBrowserDialog();
                    BrowDialog.ShowNewFolderButton = true;
                    BrowDialog.Description = "请选择保存位置";
                    DialogResult result = BrowDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        txtPath = BrowDialog.SelectedPath;

                        using (ExcelHelper excelHelper = new ExcelHelper(txtPath + "\\" + fileName + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls"))
                        {
                            int count = excelHelper.DataTableToExcel((DataTable)data, sheetName, isHasColumnName);
                            if (count > 0)
                            {
                                return txtPath; //导出成功
                            }

                        }
                    }
                }
                else
                {
                    using (ExcelHelper excelHelper = new ExcelHelper(txtPath + "\\" + fileName + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls"))
                    {
                        int count = excelHelper.DataTableToExcel((DataTable)data, sheetName, isHasColumnName);
                        if (count > 0)
                        {
                            return txtPath; //导出成功
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("导出Excel表格时发生异常:", ex);
            }
            return ""; //导出失败
        }

        /// <summary>
        /// 弹窗导入Excel，使用NPOI读取Excel表格，成功则返回DataTable数据源，失败则返回null空。
        /// </summary>
        /// <param name="sheetName">需要读取的工作薄名，默认值为"MySheet"，如果找不到则读取第一个工作薄</param>
        /// <param name="isFirstRowColumn">是否把表格首行设置为列名，默认值为true</param>
        /// <returns>成功则返回DataTable数据源，失败则返回null空</returns>
        public static DataTable ToExcelRead(string sheetName = "MySheet", bool isFirstRowColumn = true)
        {
            try
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Title = "请选择需要导入的表格文件";
                openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                openFile.Filter = "Excel文件|*.xls";
                //openFile.FilterIndex = 0;
                openFile.RestoreDirectory = true;
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(openFile.FileName))
                    {
                        string txtFile = openFile.FileName;
                        using (ExcelHelper excelHelper = new ExcelHelper(txtFile))
                        {
                            DataTable dt = excelHelper.ExcelToDataTable(sheetName, isFirstRowColumn);
                            return dt;
                        }
                    }
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
