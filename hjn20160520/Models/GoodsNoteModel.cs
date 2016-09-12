using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace hjn20160520.Models
{
   public class GoodsNoteModel
    {
       //单号
        public int noNote { get; set; }

       //业务号
        public string salesMan { get; set; }

       //收银员
        public string cashier { get; set; }

       //挂单时间
        public string upDate { get; set; }

       //单子总额
        public decimal? totalM { get; set; }

       //单子的物品清单


        public GoodsNoteModel()
        {
            this.salesMan = "业务员";
        }


    }
}
