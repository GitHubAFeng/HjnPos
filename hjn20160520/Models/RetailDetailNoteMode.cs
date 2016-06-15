using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    /// <summary>
    /// 零售单明细
    /// </summary>
    public class RetailDetailNoteMode
    {
        //单据号
        public string vCode { get; set; }
        //货号
        public int itemID { get; set; }
        //条码
        public string TM { get; set; }
        //名称
        public string cName { get; set; }
        //规格
        public string spec { get; set; }
        //数量
        public int count { get; set; }
        //原零售价
        public float ylsPrice { get; set; }
        //金额
        public float TotalMoney { get; set; }


        //
        public float hpackSize { get; set; }
        //折扣
        public float ZK { get; set; }
        //是否赠送
        public int ISZS { get; set; }
        //零售号
        public int CID { get; set; }
        //出单时间
        public DateTime? ctime { get; set; }

    }
}
