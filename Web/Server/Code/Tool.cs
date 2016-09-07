using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Web.Server.Models;

namespace Web.Code
{
    public class Tool
    {
        /// <summary>
        /// 获取当前登陆者
        /// </summary>
        /// <returns></returns>
        public static User GetLoginUser()
        {
            if (HttpContext.Current.Session["user"] != null)
            {
                return (User)HttpContext.Current.Session["user"];
            }
            else
            {
                return null;
            }
        }


    }
}