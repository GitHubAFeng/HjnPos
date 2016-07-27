using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
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
        //应交金额
        public decimal Money { get; set; }
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






    }
}
