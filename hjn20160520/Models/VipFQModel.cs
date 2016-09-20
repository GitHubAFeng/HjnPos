using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hjn20160520.Models
{
    /// <summary>
    /// 会员分期金额实体模型
    /// </summary>
    public class VipFQModel
    {
        //序号
        public int indexid { get; set; }
        //是否使用
        public bool Used { get; set; }

        //会员名字
        public string vipName { get; set; }

        //每期金额
        public decimal MQJE { get; set; }

        //失效时间
        public string ValiTime { get; set; }


        //以下不需要显示的数据
        //数据库主键ID ， 用于识别同一条分期
        public int id { get; set; }
        //剩余分期数量
        public decimal amount { get; set; }
        //会员编号
        public int vipCode { get; set; }
    }
}
