using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    /// <summary>
    /// 会员信息类
    /// </summary>
    public class VIPmodel
    {
        /// <summary>
        /// 会员类型
        /// </summary>
        public enum VipType
        {
            All=0, //全部
            General=1, //普通
            Gold=2,  //黄金
            Diamond =3,  //钻石
            Locked =101  //锁定
        }
        /// <summary>
        /// 使用状态
        /// </summary>
        public enum UseStatus
        {
            Normal= 0,  //正常使用
            Expire= -1,  //过期
            Cancel= -2  //作废
        }

        //会员编号
        public int vipCode { get; set; }
        //会员卡号
        public string vipCard { get; set; }
        //会员姓名
        public string vipName { get; set; }
        //身份证号
        public string id_No { get; set; }
        //联系电话
        public string Tel { get; set; }
        //单位地址
        public string address { get; set; }
        //发行日期
        public string cTime { get; set; }
        //累计消费额
        public float LJXF { get; set; }
        //
        //public int ZKH { get; set; }
        //发行人的ID
        public int cID { get; set; }
        //修改日期
        public string uTime { get; set; }
        //修改人的ID
        public int uID { get; set; }
        //发行分店
        public int dept_ID { get; set; }
        //折扣率
        public int ZKL { get; set; }
        //密码
        public int password { get; set; }
        //到期日期
        public string end_Date { get; set; }
        //公历生日
        public string birthday { get; set; }
        //电子邮件
        public string email { get; set; }
        //是否储值
        public int iczk_Void { get; set; }
        //储值余额
        public float czk_YE { get; set; }
        //累计积分
        public float JFnum { get; set; }
        //保底金额
        public float BDJE { get; set; }
        //内卡号
        public string other1 { get; set; }
        //欠款消费
        public string other4 { get; set; }
        //农历生日
        public string DTbirthday { get; set; }
        //信誉额度
        public float DcMaxQK { get; set; }
        //会员有效期
        public string valiDate { get; set; }

    }
}
