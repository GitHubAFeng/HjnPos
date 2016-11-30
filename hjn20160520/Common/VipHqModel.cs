using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hjn20160520.Common
{
    /// <summary>
    /// 会员还款欠款清单数据模型
    /// </summary>
    public class VipHqModel
    {
        public string jscode { get; set; }
        public decimal qkje { get; set; }
        public string scodeStr { get; set; }
        public string cidStr { get; set; }
        public string ctime { get; set; }

        ////这些隐藏
        //public int scode { get; set; }
        //public int cid { get; set; }
    }
}
