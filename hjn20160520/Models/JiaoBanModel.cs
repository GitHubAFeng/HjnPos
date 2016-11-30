using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hjn20160520.Models
{
    /// <summary>
    /// 交班模型
    /// </summary>
    public class JiaoBanModel
    {
        //当班表ID(主键，内部流水)
        public int workid { get; set; }

        //员工ID
        public int userID { get; set; }
        //员工名字
        public string userName { get; set; }

        //分店编号
        public int scode { get; set; }

        //本机编号 收银机ID
        public int bcode { get; set; }

        //交易笔数
        public int OrderCount { get; set; }

        //当班时金额
        public decimal SaveMoney { get; set; }

        //退货金额
        public decimal RefundMoney { get; set; }
        //中途提款
        public decimal DrawMoney { get; set; }

        //目前钱箱余额(应交金额)
        public decimal QianxiangMoney { get; set; }

        //当班时间
        public string workTime { get; set; }

        //交班时间
        public string JTime { get; set; }


        /////////////////下面没有显示

        //现金交易金额
        public decimal CashMoney { get; set; }
        //银联卡交易金额
        public decimal paycardMoney { get; set; }

        //储值卡交易金额 , 不计入应交金额
        public decimal VipCardMoney { get; set; }
        //礼券消费金额
        public decimal LiQuanMoney { get; set; }

        //移动支付金额
        public decimal ModbilePayMoney { get; set; }

        //VIP充值金额
        public decimal CZVipJE { get; set; }
        //VIP还款金额
        public decimal HKVipJE { get; set; }

        //总共应收金额
        public decimal Money { get; set; }






        ////本机机械码（网卡ID）
        //public string pc_code { get; set; }

        ////当班期间售出商品总金额
        //public decimal AllJe { get; set; }
        ////当班期间售出商品总数量
        //public decimal AllCount { get; set; }













    }
}
