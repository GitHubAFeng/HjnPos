using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    /// <summary>
    /// 补货申请明细列表商品模型
    /// </summary>
    public class RRGoodsModel
    {
        //商品货号
        public int noCode { get; set; }
        //占位符，序号,GUID
        public string id { get; set; }

        //条码
        public string barCodeTM { get; set; }

        //商名
        public string goods { get; set; }

        //规格
        public string spec { get; set; }
        //单位
        public string unit { get; set; }
        //数量
        public decimal? countNum { get; set; }

        //件数
        public decimal? JianShu { get; set; }
        //零售价
        public decimal? lsPrice { get; set; }
        //拼音
        public string PinYin { get; set; }
        //现存
        public float Extant { get; set; }

        //下面这些不需要在客户端显示 
        //仓库
        public int? scode { get; set; }
        //对应的补货单号
        //public string Bno { get; set; }


    }
}
