using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    //员工岗位
    public enum RoleType
    {
        RoleAdmin = 1,  //管理员
        RoleMaster = 2,  //后台维护
        RoleFront = 3,  //前台维护
        RoleClerk = 4,  //业务员
        RoleTest = 5  //测试人员

    }


   public class RolesModel
    {

       //员工编号
        public int id { get; set; }
       //员工姓名
        public string name { get; set; }
       //入职日期
        public DateTime? cTime { get; set; }
       //岗位
        public RoleType roleType { get; set; }
       //取出岗位中文名字
        public string roleTypeStr { get; set; }
       //性别
        public string sex { get; set; }


       //传入员工类型识别码12345就可以产出对应的职务中文名称
        public RolesModel()
        {
            switch (roleType)
            {
                case RoleType.RoleAdmin:
                    roleTypeStr = "系统管理员";
                    break;
                case RoleType.RoleMaster:
                    roleTypeStr = "后台操作员";
                    break;
                case RoleType.RoleFront:
                    roleTypeStr = "前台操作员";
                    break;
                case RoleType.RoleClerk:
                    roleTypeStr = "业务员";
                    break;
                case RoleType.RoleTest:
                    roleTypeStr = "测试人员";
                    break;
                default:
                    roleTypeStr = "业务员";
                    break;
            }
        }


    }
}
