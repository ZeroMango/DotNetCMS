﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EHECD_WebEntities : DbContext
    {
        public EHECD_WebEntities()
            : base("name=EHECD_WebEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleCategory> ArticleCategories { get; set; }
        public DbSet<Download> Downloads { get; set; }
        public DbSet<FriendlyLink> FriendlyLinks { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ModelItem> ModelItems { get; set; }
        public DbSet<ModelItem_Value> ModelItem_Value { get; set; }
        public DbSet<ModelTable> ModelTables { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<RegistAndLogin> RegistAndLogins { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<WebConfig> WebConfigs { get; set; }
    }
}
