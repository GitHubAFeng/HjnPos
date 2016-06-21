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
        //主键
        public int id { get; set; }
        //单号---目前用GUid代替
        public string Bno { get; set; }
        //制作人ID
        public int CID { get; set; }
        //制作时间
        public DateTime CTime { get; set; }
        //审核时间
        public DateTime ATime { get; set; }
        //经办人/制单
        public int OID { get; set; }
        //审核人
        public int AID { get; set; }
        //状态
        public int Bstatus { get; set; }

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

    }
}
