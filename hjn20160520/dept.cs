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
    
    public partial class dept
    {
        public byte id { get; set; }
        public string cname { get; set; }
        public Nullable<byte> parent_id { get; set; }
        public Nullable<bool> cant_del { get; set; }
        public Nullable<int> cid { get; set; }
        public Nullable<System.DateTime> ctime { get; set; }
        public Nullable<int> uid { get; set; }
        public Nullable<System.DateTime> utime { get; set; }
        public byte del_flag { get; set; }
        public byte sort { get; set; }
        public byte iLevel { get; set; }
    }
}
