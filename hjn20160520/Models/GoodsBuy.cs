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
        //单位
        public string unit { get; set; }

        //原价
        public string orig { get; set; }
        //零售价
        public string lsPrice { get; set; }

        //总价=数量*零售价
        private float sum;

        public float Sum
        {
            get { return  float.Parse(countNum.ToString()) * float.Parse(lsPrice); }
            set { sum = value; }
        }
        //拼音
        public string pinYin { get; set; }

        //商品促销信息
        public string goodsDes { get; set; }

        //营业员
        public string salesClerk { get; set; }





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
