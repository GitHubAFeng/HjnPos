using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{

    /// <summary>
    /// 订单明细查询datagridview2所用的数据源
    /// </summary>
    public class DetailNoteDataModel
    {
        //这个只是为了留空，方便显示序号
        public string null_temp = "";
        //货号
        public int itemID { get; set; }
        //条码
        public string TM { get; set; }
        //名称
        public string cName { get; set; }
        //规格
        public string spec { get; set; }
        //单位
        public string StrDw { get; set; }
        //数量
        public decimal? count { get; set; }
        //原零售价
        public decimal? ylsPrice { get; set; }
        //金额
        public decimal? TotalMoney { get; set; }

    }
}
