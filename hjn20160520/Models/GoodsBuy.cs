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
        //进价
        public decimal? jjPrice { get; set; }
        //零售价
        public decimal? lsPrice { get; set; }
        //会员价
        public decimal? hyPrice { get; set; }
        
        private decimal? sum;
        //总价=数量*零售价
        public decimal? Sum
        {
            get { return (Cashiers.GetInstance.VipID == 0) ? countNum * lsPrice : countNum * hyPrice; }
            set { sum = value; }
        }

        //private decimal? vipsum;
        ////会员总价=数量*会员价
        //public decimal? VipSum
        //{
        //    get { return countNum * hyPrice; }
        //    set { vipsum = value; }
        //}


        //拼音
        public string pinYin { get; set; }

        //商品促销信息
        public string goodsDes { get; set; }

        //营业员
        public string salesClerk { get; set; }

        //装数
        public decimal? hpackSize { get; set; }



        //应收总金额
        //public float totalMoney { get; private set; }


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
