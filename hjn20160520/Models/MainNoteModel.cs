using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    /// <summary>
    /// 查询明细窗口所使用的数据源实体
    /// </summary>
    public class MainNoteModel
    {
        //内部零售单号
        public string ID { get; set; }
        //小票单据号（结算单）
        public string JSDH { get; set; }
        //业务员工号
        public int? YMID { get; set; }
        //业务员工名字
        public string YwyStr { get; set; }
        //(收银员)零售员工号
        public int? CID { get; set; }
        //收银员工名字
        public string cidStr { get; set; }
        //生成时间
        public DateTime? cTiem { get; set; }
        //应收金额
        public decimal? YSJE { get; set; }
        //抹零
        public decimal? MoLing { get; set; }

    }
}
