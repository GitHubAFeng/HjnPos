using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    /// <summary>
    /// 结算方式
    /// </summary>
    public enum JSType
    {
        Cash = 0, //现金
        UnionPay = 1, //银联卡
        Coupon = 2,  //购物劵
        Others = 3  //其它,储值卡
    }


    /// <summary>
    /// 收银员交班信息实体类/全局用户信息类
    /// 按逻辑此类是全局唯一的实例
    /// </summary>
    public class HandoverModel
    {
        private static HandoverModel _instance;

        public static HandoverModel GetInstance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HandoverModel();
                } 

                return _instance;
            }

        }

        private HandoverModel()
        {

        }



        //员工ID
        public int userID { get; set; }
        //员工名字
        public string userName { get; set; }
        //角色ID
        public int RoleID { get; set; }
        //角色名字
        public string RoleName { get; set; }
        //营业员/业务员ID
        public int YWYid { get; set; }
        //营业员/业务员 名字
        public string YWYStr { get; set; }
        //收银机ID
        public int MachineID { get; set; }
        //当班时间
        public DateTime? WorkingTime { get; set; }
        //当班金额
        public float WorkingMoney { get; set; }
        //交班时间
        public DateTime? ClosedTime { get; set; }
        //交易笔数
        public int OrderCount { get; set; }
        //退货金额
        public decimal RefundMoney { get; set; }
        //中途提款
        public decimal DrawMoney { get; set; }
        //现金交易金额
        public decimal CashMoney { get; set; }
        //银联卡交易金额
        public decimal paycardMoney { get; set; }
        //储值卡交易金额 , 不计入应交金额
        public decimal VipCardMoney { get; set; }
        //礼券消费金额
        public decimal LiQuanMoney { get; set; }
        //总共应收金额
        public decimal Money { get { return CashMoney + paycardMoney + LiQuanMoney; } set { } }

        //当班时钱箱余额
        public decimal SaveMoney { get; set; }

        //是否当班
        public bool isWorking { get; set; }
        //当班时间
        public DateTime workTime { get; set; }
        //分店编号
        public int scode { get; set; }
        //public bool isSetCode { get; set; }   //分店号是否读取成功
        //分店名字
        public string scodeName { get; set; }
        //本机编号 
        public int bcode { get; set; }

        //是否已经交班
        //public bool isJB { get; set; }

        //库存提醒报表默认保存路径
        public string istorePath { get; set; }

        public bool isPrint { get; set; }

        //会员ID
        public int VipID { get; set; }

        //会员卡号
        public string VipCard { get; set; }
        //会员名字
        public string VipName { get; set; }
        //会员等级
        public int VipLv { get; set; }
        //是否会员生日
        public bool isVipBirthday { get; set; }



        //小票注脚
        //客服专线
        public string Call { get; set; }
        //地址
        public string Address { get; set; }
        //备注1
        public string Remark1 { get; set; }
        //备注2
        public string Remark2 { get; set; }

        //打印字体
        private string printFont = "宋体";

        public string PrintFont
        {
            get { return printFont; }
            set { printFont = value; }
        }

        //打印字体大小
        private int fontSize = 8;

        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        //打印的宽度
        private int pageWidth = 240;

        public int PageWidth
        {
            get { return pageWidth; }
            set { pageWidth = value; }
        }


        //打印的高度（长）
        private int pageHeight = 600;

        public int PageHeight
        {
            get { return pageHeight; }
            set { pageHeight = value; }
        }

        //小票打印标题
        public string PrintTitle { get; set; }


    }
}
