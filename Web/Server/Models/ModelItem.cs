//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Web.Server.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ModelItem
    {
        public int ID { get; set; }
        public int i_TableID { get; set; }
        public string s_ItemTitle { get; set; }
        public string s_ItemName { get; set; }
        public string s_Itemtype { get; set; }
        public bool b_IsRequired { get; set; }
        public int i_MinCount { get; set; }
        public int i_MaxCount { get; set; }
    }
}
