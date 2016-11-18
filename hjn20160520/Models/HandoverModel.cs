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
    /// 全局信息类，上下文
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
        //整单 营业员/业务员ID
        public int YWYid { get; set; }
        //整单 营业员/业务员 名字
        public string YWYStr { get; set; }

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
        //移动支付金额
        public decimal ModbilePayMoney { get; set; }
        //储值卡交易金额 , 不计入应交金额
        public decimal VipCardMoney { get; set; }
        //礼券消费金额
        public decimal LiQuanMoney { get; set; }
        //VIP充值金额
        public decimal CZVipJE { get; set; }
        //VIP还款金额
        public decimal HKVipJE { get; set; }
        //总共应收金额
        public decimal Money { get { return CashMoney + paycardMoney + LiQuanMoney + ModbilePayMoney + CZVipJE + HKVipJE; } set { } }

        //当班时金额
        public decimal SaveMoney { get; set; }

        //目前钱箱余额
        public decimal QianxiangMoney { get { return Money + SaveMoney - DrawMoney; } set{} }


        //是否当班
        public bool isWorking { get; set; }
        //当班时间
        public DateTime workTime { get; set; }
        //分店编号
        public int scode { get; set; }

        //下标
        public int scodeIndex { get; set; }

        //分店名字
        public string scodeName { get; set; }
        //本机编号 收银机ID
        public int bcode { get; set; }

        //本机机械码（网卡ID）
        public string pc_code { get; set; }

        //计算机名字
        public string pc_name { get; set; }

        //当班期间售出商品总数量
        public decimal AllCount { get; set; }

        //当班期间售出商品总金额
        public decimal AllJe { get; set; }

        //库存提醒报表默认保存路径
        public string istorePath { get; set; }

        public bool isPrint { get; set; }

        //会员ID
        public int VipID { get; set; }

        //会员卡号
        public string VipCard { get; set; }
        //会员名字
        public string VipName { get; set; }

        //积分
        public decimal vipjfnum { get; set; }
        //定金余额
        public decimal vipydje { get; set; }
        //分期余额
        public decimal vipfqje { get; set; }
        //卡余额
        public decimal vipczk_ye { get; set; }
        //电话
        public string viptel { get; set; }


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
        //备注3
        public string Remark3 { get; set; }
        //备注4
        public string Remark4 { get; set; }


        private short _printcopies = 1;
        //打印份数
        public short PrintCopies
        {
            get { return _printcopies; }
            set { _printcopies = value; }
        }

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

        //传递退货结算单号给结算时 计算退货明细
        public string TuiHuoJSDH { get; set; }
        //退货零售明细单号 
        public string TuiHuoLSDH { get; set; }
        //

        //是否练习模式
        public bool isLianxi { get; set; }

    }
}
