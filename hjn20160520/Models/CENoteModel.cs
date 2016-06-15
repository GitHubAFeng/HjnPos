using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    /// <summary>
    /// 结算单
    /// </summary>
   public class CENoteModel
    {

       //单据ID
        //public int ID { get; set; }
       //单据号
        public string vCode { get; set; }
       //结算类型
        public int jsType { get; set; }
        public string jsTypeStr { get; set; }
       //应收金额
        public float YSJE { get; set; }
       //收取金额
        public float JE { get; set; }
       //实收金额 = 收取金额 + 找零
        public float SSJE { get; set; }
       //状态 是否结算 0否 1是
        public int status { get; set; }
       //删除标记
        public int delFlag { get; set; }
       //备注说明
        public string reMark { get; set; }
       //创建人ID
        public int CID { get; set; }
       //创建时间
        public DateTime? cTime { get; set; }

        public CENoteModel()
        {
            switch (jsType)
            {
                case 0:
                    jsTypeStr = "现金支付";
                    break;
                case 1:
                    jsTypeStr = "银联卡";
                    break;
                case 2:
                    jsTypeStr = "购物劵";
                    break;
                case 3:
                    jsTypeStr = "其它";
                    break;
                default:
                    jsTypeStr = "现金支付";
                    break;
            }

        }

    }
}
