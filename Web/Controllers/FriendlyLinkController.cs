using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Code;
using Web.Server.Models;
namespace Web.Controllers
{
    public class FriendlyLinkController : Controller
    {
        //
        // GET: /FriendlyLink/
        BaseDAL<Server.Models.FriendlyLink> frindlyDal=new BaseDAL<Server.Models.FriendlyLink>();
        public ActionResult Index()
        {
            return View();
        }
        //分页查询列表
        /// <summary>
        /// 分页查询列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <param name="sKeyValue"></param>
        /// <returns></returns>
        public ActionResult GetList(int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sKeyValue = "")
        {
            PagedData pagedata=new PagedData();
            pagedata.PageIndex=page;
            pagedata.PageSize = rows;
            frindlyDal.WherePaged(pagedata, u => u.ID > 0, u=>u.ID, order=="asc"?true:false);
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            var listData = new { total = pagedata.PageCount, rows=pagedata.ListData };
            return Content(js.Serialize(listData));
        }

    }
}
