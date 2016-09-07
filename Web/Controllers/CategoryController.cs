using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Common;
using Web.Code;
using Web.Server.Models;
using Web.App_Start;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Web.Controllers
{
    public class CategoryController : Controller
    {
        private BaseDAL<ArticleCategory> cateDal = new BaseDAL<ArticleCategory>();
        private BaseDAL<Article> articleDal = new BaseDAL<Article>(); 
        // GET: /Category/
        [CheckinLogin]
        public ActionResult Index()
        {
            return View();
        }

        #region 获取文章栏目列表+List()
        /// <summary>
        /// 获取文章栏目列表
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                //对文章栏目列表进行排序
                List<ArticleCategory> allList = entity.ArticleCategories.OrderBy(m => m.i_Order).ThenBy(m => m.ID).ToList();
                if (allList == null || allList.Count == 0)
                {
                    return Content("");
                }

                List<ArticleCategory> topList = allList;
                if (topList == null || topList.Count == 0)
                {
                    return Content("");
                }
               // JavaScriptSerializer json = new JavaScriptSerializer();
                StringBuilder sResult = new StringBuilder();
                //拼接json格式数据给easyuI的分页控件
                sResult.Append("{\"total\":" + topList.Count + ",\"rows\":");
                sResult.Append(JsonConvert.SerializeObject(topList));
                sResult.Append("}");
                sResult.Replace("\"i_PID\"", "\"_parentId\"");
                //sResult.Replace("\"ID\"", "\"id\"");
                return Content(sResult.ToString());
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
                return Content("");
            }
        }
        #endregion

        [CheckinLogin]
        public ActionResult Access()
        {
            return View();
        }

        #region 新增文章分类+Add(ArticleCategory item)
        /// <summary>
        /// 新增文章分类
        /// </summary>
        /// <param name="item">文章分类实体对象</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult Add(ArticleCategory item)
        {
            if (item == null)
            {
                return Content("");
            }
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                if (entity.ArticleCategories.Where(m => m.s_Name == item.s_Name).Count() > 0)
                {
                    return Content("栏目名称重复.");
                }
                else if (entity.ArticleCategories.Where(m => m.s_EnName == item.s_EnName).Count() > 0)
                {
                    return Content("栏目英文名称重复.");
                }
                else
                {
                    if (item.s_Content != null)
                    {
                        item.s_ContentNoHtml = TString.NoHTML(item.s_Content);
                    }
                    entity.ArticleCategories.Add(item);
                }
                entity.SaveChanges();
                Generate genareta = new Generate();
                genareta.GenerateCategoryByID(item.ID);//添加了栏目则生成栏目页
                return Content("yes");
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
            }
            return Content("添加栏目失败.");
        }
        #endregion

        #region 获取树形下拉菜单数据+TreeJsonList(bool bShowTop = true)
        /// <summary>
        /// 获取树形下拉菜单数据
        /// </summary>
        /// <returns></returns>
        public ActionResult TreeJsonList(bool bShowTop = true)
        {
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                List<ArticleCategory> list = entity.ArticleCategories.OrderBy(m => m.i_Order).ThenBy(m => m.ID).ToList();
                if (list == null || list.Count == 0)
                {
                    return Content("");
                }
                StringBuilder sResult = new StringBuilder();
                if (bShowTop)
                {
                    sResult.Append("[{");
                    sResult.Append("\"id\":0,");
                    sResult.Append("\"text\":\"顶级栏目\",");
                    sResult.Append("\"selected\":true,");
                    sResult.Append("\"children\":");
                }
                GetTreeString(list.Where(m => m.i_PID == 0).ToList(), ref sResult, list);
                if (bShowTop)
                {
                    sResult.Append("}]");
                }
                return Content(sResult.ToString());
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
                return Content("");
            }
        }
        #endregion

        #region 树形下拉菜单数据+GetTreeString(List<ArticleCategory> list, ref StringBuilder sResult, List<ArticleCategory> allList)
        /// <summary>
        /// 树形下拉菜单数据
        /// </summary>
        /// <param name="list">文章分类集合</param>
        /// <param name="sResult">stringbuilder一个对象</param>
        /// <param name="allList">文章分类集合</param>
        private void GetTreeString(List<ArticleCategory> list, ref StringBuilder sResult, List<ArticleCategory> allList)
        {
            if (!(list != null && list.Count > 0))
            {
                return;
            }
            sResult.Append("[");
            foreach (ArticleCategory item in list)
            {
                sResult.Append("{");
                sResult.Append("\"id\":" + item.ID + ",");
                sResult.Append("\"text\":\"" + item.s_Name + "\"");
                List<ArticleCategory> tempList = allList.Where(m => m.i_PID == item.ID).ToList();
                if (tempList != null && tempList.Count > 0)
                {
                    sResult.Append(",\"children\":");
                    GetTreeString(tempList, ref sResult, allList);
                }
                sResult.Append("}");
                if (item.ID != list.Last().ID)
                {//不是最后一个则加上,
                    sResult.Append(",");
                }
            }
            sResult.Append("]");
        }

        #endregion

        #region 获取编辑信息+Edit(int? id)
        /// <summary>
        /// 获取编辑信息
        /// </summary>
        /// <param name="id">文章分类id</param>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            ArticleCategory item = null;
            if (id > 0)
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                item = entity.ArticleCategories.Find(id);
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

        #region 确认编辑+Edit(ArticleCategory item, int id)
        /// <summary>
        /// 确认编辑
        /// </summary>
        /// <param name="item">文章分类实体对象</param>
        /// <param name="id">文章分类id</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult Edit(ArticleCategory item, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    if (entity.ArticleCategories.Where(m => m.s_Name == item.s_Name && m.ID != id).Count() > 0)
                    {
                        return Content("栏目名称重复.");
                    }
                    if (entity.ArticleCategories.Where(m => m.s_EnName == item.s_EnName && m.ID != id).Count() > 0)
                    {
                        return Content("栏目英文名称重复.");
                    }
                    if (!string.IsNullOrEmpty(item.s_Content))
                    {
                        item.s_ContentNoHtml = TString.NoHTML(item.s_Content);
                    }
                    entity.ArticleCategories.Attach(item);
                    entity.Entry<ArticleCategory>(item).State = System.Data.EntityState.Modified;
                    entity.SaveChanges();
                    Generate genareta = new Generate();
                    genareta.GenerateCategoryByID(item.ID);//修改了栏目则生成栏目页
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
                return Content("填写的栏目信息有误.");
            }
        }

        #endregion

        #region 删除文章分类栏目+Delete(int? id)
        /// <summary>
        /// 删除文章分类栏目
        /// </summary>
        /// <param name="id">文章分类id</param>
        /// <returns></returns>
        [CheckinLogin]
        public ActionResult Delete(int? id)
        {
            if (id > 0)
            {
                try
                {
                    EHECD_WebEntities wb = new EHECD_WebEntities();
                    ArticleCategory item = wb.ArticleCategories.Find(id);
                    if (item == null)
                        return Content("该栏目不存在或已被删除.");
                    if (wb.ArticleCategories.Where(m => m.i_PID == id).ToList().Count > 0)
                        return Content("该栏目存在子栏目，不能删除.");
                    wb.ArticleCategories.Remove(item);
                    wb.SaveChanges();
                    return Content("yes");
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                    return Content("删除失败.");
                }
            }
            else
            {
                return Content("删除失败.");
            }
        }
        #endregion

        /// <summary>
        /// 把菜单生成到微信公众平台上去
        /// </summary>
        /// <returns></returns>
        public ActionResult GenerateMenuToWeiXin()
        {
            List<ArticleCategory> allCate = cateDal.Where(u => u.i_PID==0).ToList();
            string sResult = GetMenuJsonList(allCate);
            Web.Server.Models.User user=new Web.Server.Models.User();
            Web.Server.Models.User sessionUser=Session["user"] as Web.Server.Models.User;
            string sWeixinAccount, sAppId, sAppSecret;
            bool isSucess= WeixinUser.GetUserInfo(sessionUser.ID, out sWeixinAccount, out sAppId, out sAppSecret);
            if (isSucess)
            {
                var access_token = new Access_Token(sWeixinAccount, sAppId, sAppSecret);
                string sAcess_Token = access_token.sToken;
                isSucess = new HandleMenu().Create(sAcess_Token, sResult);
            }
            
            return Content(isSucess?"公众号菜单创建成功！":"公众号自定义菜单创建失败！"+sResult);
        }
        public string GetMenuJsonList(List<ArticleCategory> list)
        {
            Web.Code.Generate.GenerateHelper helper = new Generate.GenerateHelper();
            List<ArticleCategory> cateList = new List<ArticleCategory>();
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"button\":[");
            foreach (var item in list)
            {
                sb.Append("{");
                //sb.Append(item.s_Name);
                cateList = cateDal.Where(u => u.i_PID == item.ID).ToList();
                if (cateList.Count > 0)
                {
                    sb.Append("\"name\":\""+item.s_Name+"\",");
                    sb.Append("\"sub_button\":[");
                    foreach (var cate in cateList)
                    {
                        sb.Append("{");
                        sb.Append("\"type\":\"view\",");
                        sb.Append("\"name\":\""+cate.s_Name+"\",");
                        //http://v.qq.com/
                        //sb.Append("\"url\":\""+helper.GetCategoryLinkByEn(cate.s_EnName)+"\"");
                        sb.Append("\"url\":\""+cate.s_RedirectUrl+"\"");
                        sb.Append("},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("]");
                    //sb.Append(GetMenuJsonList(cateList));
                }
                else
                {
                    sb.Append("\"type\":\"view\",");
                    sb.Append("\"name\":\""+item.s_Name+"\",");
                    //sb.Append("\"url\":\"" + helper.GetCategoryLinkByEn(item.s_EnName)+ "\"");
                    sb.Append("\"url\":\""+item.s_RedirectUrl+"\"");
                }
                sb.Append("},");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 根据栏目父栏目英文名获取下面的所有子栏目
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSonCateListByparEn()
        {
            string sEnName=Request["enname"];
            List<Article> artList = articleDal.Where(u => u.ID > 0).ToList();
            List<ArticleCategory> cateList= cateDal.Where(u => u.s_EnName == sEnName).ToList();
            //cateList.Join(artList,cate=>cate.ID,art=>art.i_CategoryID,(art,cate)=>new {cateID=cate.ID,}
            return Content("");
        }
    }
}
