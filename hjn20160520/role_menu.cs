//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace hjn20160520
{
    using System;
    using System.Collections.Generic;
    
    public partial class role_menu
    {
        public int role_id { get; set; }
        public int menu_id { get; set; }
        public bool can_add { get; set; }
        public bool can_upd { get; set; }
        public bool can_del { get; set; }
        public Nullable<int> cid { get; set; }
        public Nullable<System.DateTime> ctime { get; set; }
        public Nullable<int> uid { get; set; }
        public Nullable<System.DateTime> utime { get; set; }
        public byte can_see { get; set; }
    }
}
