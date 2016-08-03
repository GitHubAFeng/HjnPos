using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    /// <summary>
    /// 购物车商品
    /// </summary>
    public class GoodsBuy
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
        public int countNum { get; set; }
        //单位编号，客户端不需要显示
        public int unit { get; set; }
        //单位名称，显示这个
        public string unitStr { get; set; }
        //进价  , 客户端不需要显示
        public decimal? jjPrice { get; set; }
        //零售价
        public decimal? lsPrice { get; set; }
        //会员价
        public decimal? hyPrice { get; set; }


        //总价=数量*零售价
        public decimal? Sum
        {
            //get { return (Cashiers.GetInstance.VipID == 0) ? countNum * lsPrice : countNum * hyPrice; }
            get { return isVip ? countNum * hyPrice : countNum * lsPrice; }

            //set { }

            //get;
            //set;
        }

        //拼音
        public string pinYin { get; set; }

        //商品促销信息
        public string goodsDes { get; set; }

        //营业员
        public string salesClerk { get; set; }

        //装数  , 客户端不需要显示
        public decimal? hpackSize { get; set; }
        //商品状态  2为停止销售, 客户端不需要显示
        public byte? status { get; set; }

        //单品折扣率
        public decimal ZKDP { get; set; }
        //批发价
        public decimal? pfPrice { get; set; }
        //是否赠送的活动商品
        public bool isZS { get; set; }

        //是否会员价
        public bool isVip { get; set; }

        //限购,数量等于限购就为true
        public bool isXG { get; set; }
        //活动类型
        public int vtype { get; set; }


        public GoodsBuy()
        {
            countNum = 1;

        }



        //浅拷贝
        //public GoodsBuy Clone()
        //{
        //    return this.MemberwiseClone() as GoodsBuy;
        //}


    }
}
