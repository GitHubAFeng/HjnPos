using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hjn20160520.Models
{
    /// <summary>
    /// 活动提醒模型
    /// </summary>
    public class HDTipModel
    {
        /// <summary>
        /// 活动类型
        /// </summary>
        public string vtypeStr { get; set; }
        /// <summary>
        /// 活动对象
        /// </summary>
        public string dxStr { get; set; }

        /// <summary>
        /// 会员类型等级
        /// </summary>
        public string vipTypeStr { get; set; }

        /// <summary>
        /// 限购数量
        /// </summary>
        public string xgStr { get; set; }
        /// <summary>
        /// 活动物品
        /// </summary>
        public string hdItemStr { get; set; }

        /// <summary>
        /// 数量条件
        /// </summary>
        public string countStr { get; set; }

        /// <summary>
        /// 活动价格
        /// </summary>
        public string lsStr { get; set; }

        /// <summary>
        /// 赠品
        /// </summary>
        public string zsStr { get; set; }


        /// <summary>
        /// 赠送数量
        /// </summary>
        public string zsCountStr { get; set; }

        /// <summary>
        /// 剩余数量
        /// </summary>
        public string zsSaveCountStr { get; set; }

        /// <summary>
        /// 活动开始时间
        /// </summary>
        public string beginTimeStr { get; set; }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        public string endTimeStr { get; set; }


    }
}
