using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hjn20160520.Models
{
    ///退货商品
    public class TuiHuoItemModel
    {

        //货号
        public int noCode { get; set; }

        //条码
        public string barCodeTM { get; set; }

        //商名
        public string goods { get; set; }

        //规格
        public string spec { get; set; }
        //数量
        public decimal countNum { get; set; }
        //单位编号，客户端不需要显示
        public int unit { get; set; }
        //单位名称，显示这个
        public string unitStr { get; set; }

        ////零售价
        //public decimal? lsPrice { get; set; }
        ////会员价
        //public decimal? hyPrice { get; set; }
        //售价
        public decimal Sum { get; set; }

        //该商品购买时候以什么活动类型购买的
        public int vtype { get; set; }








    }
}
