using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hjn20160520.Models
{
    public class VipItemModel
    {
        //货号
        public int itemid { get; set; }

        //条码
        public string tm { get; set; }

        //品名
        public string cname { get; set; }

        //数量
        public decimal count { get; set; }

        //会员卡
        public string vipcard { get; set; }
        //会员编号
        public int vipid { get; set; }

        //会员姓名
        public string vipName { get; set; }

        //分店号
        public int scode { get; set; }

        //操作员工
        public int cid { get; set; }
        //操作时间
        public DateTime ctime { get; set; }

    }
}
