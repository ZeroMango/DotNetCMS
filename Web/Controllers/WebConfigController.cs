using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using Common;
using Web.Code;
using Web.Server.Models;

namespace Web.Controllers
{
    public class WebConfigController : Controller
    {
        //
        // GET: /WebConfig/
        [CheckinLogin]
        public ActionResult Index()
        {
            return View();
        }
        #region 获取网站配置的相关信息+Edit(int? id)
        /// <summary>
        /// 获取网站配置的相关信息
        /// </summary>
        /// <param name="id">WebConfig id</param>
        /// <returns></returns>
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult Edit(int? id)
        {
            WebConfig item = null;
            if (id > 0)
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                item = entity.WebConfigs.Find(id);
                if (item != null)
                {
                    JavaScriptSerializer json = new JavaScriptSerializer();
                    return Content(json.Serialize(item));
                }
            }
            else
            {
                return Content("");
            }
            return Content("");
        } 
        #endregion

        #region 确认编辑网站配置+Edit(WebConfig item, int id)
        
        /// <summary>
        /// 确认编辑网站配置
        /// </summary>
        /// <param name="item">WebConfig 实体对象</param>
        /// <param name="id">WebConfig id</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult Edit(WebConfig item, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    entity.WebConfigs.Attach(item);
                    entity.Entry<WebConfig>(item).State = System.Data.EntityState.Modified;
                    entity.SaveChanges();
                    return Content("yes");
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                    return Content("编辑失败.");
                }
            }
            else
            {
                return Content("填写的站点信息有误.");
            }
        } 
        #endregion
    }
}
