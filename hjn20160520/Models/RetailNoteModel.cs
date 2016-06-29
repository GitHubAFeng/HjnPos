﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
    /// <summary>
    /// 零售主单
    /// </summary>
    public class RetailNoteModel
    {

        //单据号
        public string vCode { get; set; }
        //VIP会员号-  0为普通零售
        public int VIP { get; set; }
        //总折扣
        public float ZZK { get; set; }
        //删除标记
        public int DelFlag { get; set; }

        //(收银员)零售员工号
        public int? CID { get; set; }
        //业务员工ID
        public int? ywyID { get; set; }
        //分店号ID
        public int scode { get; set; }
        //生成时间
        public DateTime? cTiem { get; set; }

        //Code First 下用虚拟属性 
        //主单下属的商品清单
        //public RetailDetailNoteMode RDNote { get; set; }
        ////主单下属的结算单
        //public CENoteModel CENote { get; set; }




    }
}
