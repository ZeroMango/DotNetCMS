using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Code;
using System.IO;
using Web.Server.Models;
namespace Web.Controllers
{
    public class DefaultController : Controller
    {
        #region 登录后的首页+Index()
        //
        // GET: /Default/
        /// <summary>
        /// 登录后的首页
        /// </summary>
        /// <returns></returns>
        [CheckinLogin]
        public ActionResult Index()
        {
            //判断是否登录
            User loginUser = Tool.GetLoginUser();
            if (loginUser == null)
            {
                loginUser = new User();
                loginUser.ID = 0;
                loginUser.s_RealName = "尚未登录";
            }
            return View(loginUser);
        } 
        #endregion


        public ActionResult FrontPage()
        {
            string indexPath=Server.MapPath("index.html");
            string resultStr=System.IO.File.ReadAllText(indexPath);
            return Content(resultStr);
        }
    }
}
