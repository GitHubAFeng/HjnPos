﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace hjn20160520
{
    using hjn20160520.Common;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class hjnbhEntities : DbContext
    {
        public hjnbhEntities()
            : base(MyEFDB.GetEntityConnectionString())
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Code_Lib> Code_Lib { get; set; }
        public virtual DbSet<com_oper> com_oper { get; set; }
        public virtual DbSet<download> download { get; set; }
        public virtual DbSet<hd_bh_detail> hd_bh_detail { get; set; }
        public virtual DbSet<hd_bh_info> hd_bh_info { get; set; }
        public virtual DbSet<hd_client_tikun> hd_client_tikun { get; set; }
        public virtual DbSet<hd_cxtj> hd_cxtj { get; set; }
        public virtual DbSet<hd_cxtj_detail> hd_cxtj_detail { get; set; }
        public virtual DbSet<hd_dborjb> hd_dborjb { get; set; }
        public virtual DbSet<hd_hs_js> hd_hs_js { get; set; }
        public virtual DbSet<hd_hs_jsd> hd_hs_jsd { get; set; }
        public virtual DbSet<hd_in> hd_in { get; set; }
        public virtual DbSet<hd_in_b> hd_in_b { get; set; }
        public virtual DbSet<hd_in_detail> hd_in_detail { get; set; }
        public virtual DbSet<hd_in_detail_b> hd_in_detail_b { get; set; }
        public virtual DbSet<hd_istore> hd_istore { get; set; }
        public virtual DbSet<hd_istore_detail> hd_istore_detail { get; set; }
        public virtual DbSet<hd_item_db> hd_item_db { get; set; }
        public virtual DbSet<hd_item_info> hd_item_info { get; set; }
        public virtual DbSet<hd_item_lb> hd_item_lb { get; set; }
        public virtual DbSet<hd_js> hd_js { get; set; }
        public virtual DbSet<hd_js_b> hd_js_b { get; set; }
        public virtual DbSet<hd_js_type> hd_js_type { get; set; }
        public virtual DbSet<hd_js_type_b> hd_js_type_b { get; set; }
        public virtual DbSet<hd_ls> hd_ls { get; set; }
        public virtual DbSet<hd_ls_b> hd_ls_b { get; set; }
        public virtual DbSet<hd_ls_detail> hd_ls_detail { get; set; }
        public virtual DbSet<hd_ls_detail_b> hd_ls_detail_b { get; set; }
        public virtual DbSet<hd_out> hd_out { get; set; }
        public virtual DbSet<hd_out_b> hd_out_b { get; set; }
        public virtual DbSet<hd_out_detail> hd_out_detail { get; set; }
        public virtual DbSet<hd_out_detail_b> hd_out_detail_b { get; set; }
        public virtual DbSet<hd_pdd> hd_pdd { get; set; }
        public virtual DbSet<hd_pdd_detail> hd_pdd_detail { get; set; }
        public virtual DbSet<hd_rkd_fk> hd_rkd_fk { get; set; }
        public virtual DbSet<hd_sup_info> hd_sup_info { get; set; }
        public virtual DbSet<hd_syd_detail> hd_syd_detail { get; set; }
        public virtual DbSet<hd_sys_kq> hd_sys_kq { get; set; }
        public virtual DbSet<hd_sys_qx> hd_sys_qx { get; set; }
        public virtual DbSet<hd_t_bank> hd_t_bank { get; set; }
        public virtual DbSet<hd_tjd> hd_tjd { get; set; }
        public virtual DbSet<hd_tjd_detail> hd_tjd_detail { get; set; }
        public virtual DbSet<hd_vip_cz> hd_vip_cz { get; set; }
        public virtual DbSet<hd_vip_fq> hd_vip_fq { get; set; }
        public virtual DbSet<hd_vip_info> hd_vip_info { get; set; }
        public virtual DbSet<hd_vip_item> hd_vip_item { get; set; }
        public virtual DbSet<hd_vip_item_detail> hd_vip_item_detail { get; set; }
        public virtual DbSet<hd_vip_memo> hd_vip_memo { get; set; }
        public virtual DbSet<hd_vip_qk> hd_vip_qk { get; set; }
        public virtual DbSet<hd_vip_type> hd_vip_type { get; set; }
        public virtual DbSet<hd_vip_zs_history> hd_vip_zs_history { get; set; }
        public virtual DbSet<hd_yh_detail> hd_yh_detail { get; set; }
        public virtual DbSet<hd_yh_history> hd_yh_history { get; set; }
        public virtual DbSet<hdh_sy_db> hdh_sy_db { get; set; }
        public virtual DbSet<menu_func> menu_func { get; set; }
        public virtual DbSet<mtc_t> mtc_t { get; set; }
        public virtual DbSet<setting> setting { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<area> area { get; set; }
        public virtual DbSet<city> city { get; set; }
        public virtual DbSet<dept> dept { get; set; }
        public virtual DbSet<hd_Bank> hd_Bank { get; set; }
        public virtual DbSet<hd_bhd> hd_bhd { get; set; }
        public virtual DbSet<hd_bhd_detail> hd_bhd_detail { get; set; }
        public virtual DbSet<hd_bz> hd_bz { get; set; }
        public virtual DbSet<hd_cgdh_detail> hd_cgdh_detail { get; set; }
        public virtual DbSet<hd_dept_info> hd_dept_info { get; set; }
        public virtual DbSet<hd_item_fz> hd_item_fz { get; set; }
        public virtual DbSet<hd_kf_price> hd_kf_price { get; set; }
        public virtual DbSet<hd_sys_bz> hd_sys_bz { get; set; }
        public virtual DbSet<hd_vippoint_flow> hd_vippoint_flow { get; set; }
        public virtual DbSet<hd_xsdh_detail> hd_xsdh_detail { get; set; }
        public virtual DbSet<hd_yc_price> hd_yc_price { get; set; }
        public virtual DbSet<MTC> MTC { get; set; }
        public virtual DbSet<province> province { get; set; }
        public virtual DbSet<role_menu> role_menu { get; set; }
        public virtual DbSet<roles> roles { get; set; }
        public virtual DbSet<send_msg> send_msg { get; set; }
        public virtual DbSet<SolarData> SolarData { get; set; }
        public virtual DbSet<usr_role> usr_role { get; set; }
        public virtual DbSet<user_role_view> user_role_view { get; set; }
        public virtual DbSet<v_deptLevelName> v_deptLevelName { get; set; }
        public virtual DbSet<v_hd_in> v_hd_in { get; set; }
        public virtual DbSet<v_hd_in_detail> v_hd_in_detail { get; set; }
        public virtual DbSet<v_hd_istore> v_hd_istore { get; set; }
        public virtual DbSet<v_hd_item_info> v_hd_item_info { get; set; }
        public virtual DbSet<v_hd_js> v_hd_js { get; set; }
        public virtual DbSet<v_hd_js_type> v_hd_js_type { get; set; }
        public virtual DbSet<v_hd_ls> v_hd_ls { get; set; }
        public virtual DbSet<v_hd_ls_detail> v_hd_ls_detail { get; set; }
        public virtual DbSet<v_hd_out> v_hd_out { get; set; }
        public virtual DbSet<v_hd_out_detail> v_hd_out_detail { get; set; }
        public virtual DbSet<v_item_pack> v_item_pack { get; set; }
        public virtual DbSet<v_item_packsn> v_item_packsn { get; set; }
        public virtual DbSet<v_xs_item_info> v_xs_item_info { get; set; }
        public virtual DbSet<v_yh_detail> v_yh_detail { get; set; }
    }
}
