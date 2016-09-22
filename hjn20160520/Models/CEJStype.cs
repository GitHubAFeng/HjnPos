using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hjn20160520.Models
{
    /// <summary>
    /// 用于记录付款方式  , 0现金，1银行，2礼券，3储卡，4移动支付
    /// </summary>
    public class CEJStype
    {
        //付款方式
        public int cetype { get; set; }

        //付款金额
        public decimal ceJE { get; set; }
    }
}
