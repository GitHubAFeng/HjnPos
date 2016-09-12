using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hjn20160520.Common
{
    //自定义一个连接
    //public partial class MyEFDB : DbContext
    /// <summary>
    /// 这么写与上面自定义的区别是把连接字符串传递给EF默认的实体类的构造函数里
    /// </summary>
    public class MyEFDB
    {

        //public MyEFDB() : base(GetEntityConnectionString(string connectionString))
        //{

        //}

        //Edmx名字，服务器域名，数据库名字，用户名，密码
        public static string edmxName = "DB", serverAdd, dadaBaseName, usrid, wd;

        /// <summary>
        /// 最好先让EF自动生成一次连接字符串，然后抄
        /// </summary>
        /// <param name="connectionString"> 传入完整的 data source</param>
        /// <returns></returns>
        public static string GetEntityConnectionString(string connectionString)
        {
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
            //不需要指定 name
            entityBuilder.Metadata = "res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl";  //这个Model1指你的edmx名字
            entityBuilder.ProviderConnectionString = connectionString;  //这个指 data source
            entityBuilder.Provider = "System.Data.SqlClient";  //这个一般不变
            return entityBuilder.ToString();
        }


        /// <summary>
        /// 得到Entity的连接字符串
        /// </summary>
        /// <param name="edmxFullName">Edmx的包括命名空间的全名称</param>
        /// <param name="server">服务器地址或名称</param>
        /// <param name="dadaBase">数据库</param>
        /// <param name="usr">用户</param>
        /// <param name="pswd">密码</param>
        /// <returns>Entity连接字符串</returns>
        public static string GetEntityConnectionString(string edmxFullName, string server, string dadaBase, string usr, string pswd)
        {
            EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder();
            entityConnectionStringBuilder.Metadata = @"res://*/" + edmxFullName + @".csdl|res://*/" + edmxFullName + @".ssdl|res://*/" + edmxFullName + @".msl";
            entityConnectionStringBuilder.Provider = "System.Data.SqlClient";
            
            //这段注释掉的，不知为何会报错……
            //SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
            //sqlConnectionStringBuilder.DataSource = server;
            //sqlConnectionStringBuilder.InitialCatalog = dadaBase;
            //sqlConnectionStringBuilder.IntegratedSecurity = true;
            //sqlConnectionStringBuilder.UserID = usr;
            //sqlConnectionStringBuilder.Password = pswd;
            //sqlConnectionStringBuilder.MultipleActiveResultSets = true;
            //sqlConnectionStringBuilder.ApplicationName = appName;
            //entityConnectionStringBuilder.ProviderConnectionString = sqlConnectionStringBuilder.ToString();
            //所以我改成了下面拼接字符串的方法解决
            string connectionString = string.Format("data source={0};initial catalog={1};persist security info=True;user id={2};password={3};MultipleActiveResultSets=True;App=EntityFramework", server, dadaBase, usr, pswd);
            entityConnectionStringBuilder.ProviderConnectionString = connectionString;

            return entityConnectionStringBuilder.ConnectionString;
        }

        /// <summary>
        /// 把相关的字段值通过本类静态字段传递
        /// </summary>
        /// <returns></returns>
        public static string GetEntityConnectionString()
        {
            EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder();
            entityConnectionStringBuilder.Metadata = @"res://*/" + edmxName + @".csdl|res://*/" + edmxName + @".ssdl|res://*/" + edmxName + @".msl";
            entityConnectionStringBuilder.Provider = "System.Data.SqlClient";

            string connectionString = string.Format("data source={0};initial catalog={1};persist security info=True;user id={2};password={3};MultipleActiveResultSets=True;App=EntityFramework", serverAdd, dadaBaseName, usrid, wd);
            entityConnectionStringBuilder.ProviderConnectionString = connectionString;

            return entityConnectionStringBuilder.ConnectionString;
        }


    }
}
