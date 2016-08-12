using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Models
{

    /*
     * CREATE TABLE [dbo].[hd_bh_info](
	[id] [int] IDENTITY(1,1) NOT NULL,-- 主键
	[b_no] [varchar](100) NULL,-- 单号
	[cid] [int] NULL,			--制作人
	[ctime] [datetime] NULL,	--操作时间
	[bh_time] [datetime] NULL,	--补货时间
	[b_status] [int] NULL,		--状态
	[b_type] [int] NULL,		--单据类型
	[zd_time] [datetime] NULL,  --补货时间限制
	[bt_change_time] [datetime] NULL, --修改时间
	[o_id] [int] NULL,	--经办人
	[scode] [int] NULL, --仓库id
	[a_id] [int] NULL, --审核人
	[a_time] [datetime] NULL, --审核时间
	[del_flag] [tinyint] NULL, --是否删除
     */

    /// <summary>
    /// 补货信息主单
    /// </summary>
    public class BHInfoNoteModel
    {
        //主键,没用到
        public int id { get; set; }
        //单号--
        public string Bno { get; set; }
        //经办人/制单
        public int OID { get; set; }
        public string OidStr { get; set; }  //制单人中文UI

        //制作时间
        public DateTime? CTime { get; set; }
        //审核时间
        public DateTime? ATime { get; set; }
        //制作人ID
        public int CID { get; set; }
        public string CidStr { get; set; }  //制作人中文UI
        //审核人
        public int AID { get; set; }
        public string AidStr { get; set; }  //审核人中文UI
        //状态 0为未发送，1为发送
        public string Bstatus { get; set; }  //状态UI

        //以下客房端不需要显示的
        //补货时间
        public DateTime? BHtime { get; set; }
        //单据类型
        public int BHtype { get; set; }
        //补货时间限制
        public DateTime? ZDtime { get; set; }
        //修改时间
        public DateTime? changeTime { get; set; }
        //仓库ID
        public int? scode { get; set; }
        //是否删除
        public int? delFlag { get; set; }

        //明细表
        //public virtual ICollection<hd_bh_detail> BhDetailList { get; set; }

        //经办人
        /*  未签名       
            系统管理员
            后台操作员
            收银员
            业务员
         */

        public BHInfoNoteModel()
        {
            switch (OID)
            {
                case 0:
                    OidStr = "未签名";
                    break;
                case 1:
                    OidStr = "系统管理员";
                    break;
                case 2:
                    OidStr = "后台操作员";
                    break;
                case 3:
                    OidStr = "收银员";
                    break;
                case 4:
                    OidStr = "业务员";
                    break;
            }
        }


    }
}
