using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    /*  CREATE TABLE hd_sys_qx
  ( usr_id INT, -- 用户ID
  	zm int,     -- 权限密码
  	zk int,		-- 折扣
  	mw VARCHAR(30), -- 密文
  	mje NUMERIC(6,2),  -- 最大抹零金额
  	sIDcode VARCHAR(30), -- 授权码
  	utime DATETIME 
  )*/

    /// <summary>
    /// 抹零权限
    /// </summary>
    public class SysQXModel
    {
        //主键ID
        public int ID { get; set; }
        //用户ID
        public int userID { get; set; }
        //权限密码
        public int? ZM { get; set; }
        //折扣
        public int? ZK { get; set; }
        //密文
        public string MW { get; set; }
        //最大抹零金额
        public float? MJE { get; set; }
        //授权码
        public string SIDcode { get; set; }
        //时间
        public DateTime? Utime { get; set; }

    }
}
