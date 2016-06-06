using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{
   public class NoteGoodsListModel
    {

       //货号
        public int GNo { get; set; }

       //条码
        public string GTm { get; set; }

       //品名
        public string  Gname { get; set; }

       //数量 
        public int Gcount { get; set; }

       //单价
        public string Gpr { get; set; }

       //总金额
        public float Gtot { get; set; }






    }
}
