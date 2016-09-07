using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Common;
using Web.Code;
using Web.Server.Models;

namespace Web.Controllers
{
    public class VelocityController : Controller
    {
        //
        // GET: /Velocity/
        [CheckinLogin]
        public ActionResult Index()
        {
            return View();
        }

        #region 生成选中栏目下文章的静态页面+GenerateArticleByCIDS(string sID)
        /// <summary>
        /// 生成选中栏目下文章的静态页面
        /// </summary>
        /// <param name="sID">栏目ID字符串</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GenerateArticleByCIDS(string sID)
        {
            if (string.IsNullOrEmpty(sID))
            {
                return Content("参数错误.");
            }
            List<string> idList = sID.Split(',').ToList();
            if (idList == null || idList.Count == 0)
            {
                return Content("参数错误.");
            }
            try
            {
                Generate genarate = new Generate();
                foreach (string id in idList)
                {
                    genarate.GenerateCategoryArticle(TConvert.toInt32(id));
                }
                return Content("yes");
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
                return Content("生成错误.");
            }
        } 
        #endregion

        #region 生成选中栏目静态页面+GenerateCategoryByIDS(string sID)
        /// <summary>
        /// 生成选中栏目静态页面
        /// </summary>
        /// <param name="sID">栏目ID字符串</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GenerateCategoryByIDS(string sID)
        {
            if (string.IsNullOrEmpty(sID))
            {
                return Content("参数错误.");
            }
            List<string> idList = sID.Split(',').ToList();
            if (idList == null || idList.Count == 0)
            {
                return Content("参数错误.");
            }
            try
            {
                Generate genarate = new Generate();
                foreach (string id in idList)
                {
                    genarate.GenerateCategoryByID(TConvert.toInt32(id));
                }
                return Content("yes");
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
                return Content("生成错误.");
            }
        } 
        #endregion

        #region 生成首页+GenerateDefault()
        /// <summary>
        /// 生成首页
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GenerateDefault()
        {
            try
            {
                Generate genarate = new Generate();
                genarate.GenerateDeafult();
                return Content("yes");
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
                return Content("生成错误." + e.ToString());
            }
        } 
        #endregion

        #region 生成静态页+Generate()
        /// <summary>
        /// 生成静态页
        /// </summary>
        /// <returns></returns>
        public ActionResult Generate()
        {
            Generate generate = new Code.Generate();
            generate.GenerateCateGory();
            return View();
        } 
        #endregion

    }
}
