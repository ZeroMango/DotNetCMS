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

namespace Web.Controllers
{
    public class ArticleController : Controller
    {
        //
        // GET: /Article/
        [CheckinLogin]
        public ActionResult Index()
        {
            return View();
        }

        #region 获取文章列表+List(int? iCategoryID, int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        /// <summary>
        /// 获取文章列表
        /// </summary>
        /// <param name="iCategoryID">文章分类id</param>
        /// <param name="page">当前页数默认第一页</param>
        /// <param name="rows">当前页数量默认10条</param>
        /// <param name="sort">通过哪个属性排序</param>
        /// <param name="order">升序还是降序</param>
        /// <param name="sField">字段列</param>
        /// <param name="sKeyValue">文章标题或者内容中的关键字</param>
        /// <returns></returns>
        public ActionResult List(int? iCategoryID, int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        {
            StringBuilder sResult = new StringBuilder();
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                //文章表和文章分类表，进行连接查询后，装入rightArticle
                var query = from a in entity.Articles
                            join b in entity.ArticleCategories on a.i_CategoryID equals b.ID into rightArticle
                            from b in rightArticle.DefaultIfEmpty()
                            //匿名类，查询出你自己需要的字段
                        
                            select new { a.ID, a.s_Author, a.i_UserID, a.s_Title, a.s_UpdateTime, a.s_ContentNoHtml, a.i_CategoryID, a.b_IsHot, a.b_IsSlide, a.b_IsTop, a.i_Status, s_CategoryName = b.s_Name };
                //根据排序方法进行排序
                if (order == "asc")
                {
                    switch (sort)
                    {
                        case "i_CategoryID"
                            : query = query.OrderBy(m => m.i_CategoryID);
                            break;
                        case "s_Title"
                            : query = query.OrderBy(m => m.s_Title);
                            break;
                        case "s_UpdateTime"
                        : query = query.OrderBy(m => m.s_UpdateTime);
                            break;
                        default:
                            //先通过b_IsTop字段降序排序然后再通过，b_IsHot字段降序排序，最后通过id降序排序
                            query = query.OrderByDescending(m => m.b_IsTop).ThenByDescending(m => m.b_IsHot).OrderByDescending(m => m.ID);
                            break;
                    }
                }
                   //判断是升序还是降序排序
                else if (order == "desc")
                {
                    switch (sort)
                    {
                        case "i_CategoryID"
                            : query = query.OrderByDescending(m => m.i_CategoryID);
                            break;
                        case "s_Title"
                            : query = query.OrderByDescending(m => m.s_Title);
                            break;
                        case "s_UpdateTime"
                            : query = query.OrderByDescending(m => m.s_UpdateTime);
                            break;
                        default
                            //先通过b_IsTop字段降序排序然后再通过，b_IsHot字段降序排序，最后通过id降序排序
                            : query = query.OrderByDescending(m => m.b_IsTop).ThenByDescending(m => m.b_IsHot).OrderByDescending(m => m.ID);
                            break;
                    }
                }
                //var query = entity.Templates.OrderByDescending(m => m.ID).Skip((int)(page - 1) * rows).Take(rows);
                if (!string.IsNullOrEmpty(sField) && !string.IsNullOrEmpty(sKeyValue))
                {
                    if (sField == "s_Title")
                    {
                        //通过标题里是否包含，sKeyValue关键字排序
                        query = query.Where(m => m.s_Title.Contains(sKeyValue));
                    }
                    else if (sField == "s_Content")
                    {
                        //通过s_ContentNoHtml里是否包含，sKeyValue关键字排序
                        query = query.Where(m => m.s_ContentNoHtml.Contains(sKeyValue));
                    }
                }
                if (iCategoryID != null && iCategoryID > 0)
                {
                    //查询出某一个文章分类的文章集合出来
                    query = query.Where(m => m.i_CategoryID == iCategoryID);
                }
                //拼接符合easyUi分页控件json格式的数据
                sResult.Append("{\"total\":" + query.Count() + ",\"rows\":");
                //分页查询
                query = query.Skip((int)(page - 1) * rows).Take(rows);
                //查询出user的文章
                var resultQuery = from a in query
                                  join c in entity.Users on a.i_UserID equals c.ID into leftArticle
                                  from c in leftArticle.DefaultIfEmpty()
                                  select new { a.ID, a.s_Author, a.i_UserID, a.s_Title, a.s_UpdateTime, a.s_ContentNoHtml, a.i_CategoryID, a.b_IsHot, a.b_IsSlide, a.b_IsTop, a.i_Status, a.s_CategoryName, s_RealName = c.s_RealName };
                //创建一个序列化的类
                JavaScriptSerializer json = new JavaScriptSerializer();
                sResult.Append(json.Serialize(resultQuery.ToList()) + "}");
            }
            catch (Exception ex)
            {
                //写入错误日志
                TTracer.WriteLog_App(ex.ToString());
            }
            return Content(sResult.ToString());
        }
        
        #endregion

        #region 获取文章分类+Access(int? iCategoryID)
        /// <summary>
        /// 获取文章分类
        /// </summary>
        /// <param name="iCategoryID">文章分类id</param>
        /// <returns></returns>
        public ActionResult Access(int? iCategoryID)
        {
            if (iCategoryID != null && iCategoryID > 0)
            {
                //把iCategoryID保存到viewdata中然后传递给view视图
                ViewData["iCategoryID"] = iCategoryID;
            }
            return View();
        } 
        #endregion

        #region 添加文章+ Add(Article item)
        /// <summary>
        /// 添加文章
        /// </summary>
        /// <param name="item">文章实体对象</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult Add(Article item)
        {
            if (item == null)
            {
                return Content("");
            }
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                if (!string.IsNullOrEmpty(item.s_EnName))
                {
                   
                    if (entity.Articles.Where(m => m.s_EnName == item.s_EnName).Count() > 0)
                    {
                        return Content("文章英文名称重复.");
                    }
                }
                if (item.s_Content != null)
                {
                    item.s_ContentNoHtml = TString.NoHTML(item.s_Content);
                }
                item.s_AddTime = TConvert.formatDateTime19(DateTime.Now);
                item.s_UpdateTime = item.s_AddTime;
                Web.Server.Models.User loginUser = Tool.GetLoginUser();
                if (loginUser != null)
                {
                    item.i_UserID = loginUser.ID;
                }
                //吧这个文章实体加入ef代理对象中
                entity.Articles.Add(item);
                //保存
                entity.SaveChanges();
                Generate generate = new Generate();
                generate.GenerateArticle(item);
                return Content("yes");
            }
                
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
            }
            return Content("添加文章失败.");
        } 
        #endregion

        #region 获取编辑信息+Edit(int? id)
        /// <summary>
        /// 获取编辑信息
        /// </summary>
        /// <param name="id">文章id</param>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            Article item = null;
            if (id > 0)
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                item = entity.Articles.Find(id);
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

        #region 确认编辑+Edit(Article item, int id)
        /// <summary>
        /// 确认编辑
        /// </summary>
        /// <param name="item"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult Edit(Article item, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    if (!string.IsNullOrEmpty(item.s_EnName))
                    {
                        if (entity.Articles.Where(m => m.s_EnName == item.s_EnName && m.ID != id).Count() > 0)
                        {
                            return Content("文章英文名称重复.");
                        }
                    }
                    if (item.s_Content != null)
                    {
                        item.s_ContentNoHtml = TString.NoHTML(item.s_Content);
                    }
                    Article originalItem = entity.Articles.Find(id);
                    if (originalItem == null)
                    {
                        return Content("所编辑的信息不存在或已被删除.");
                    }
                    originalItem.i_CategoryID = item.i_CategoryID;
                    originalItem.i_TemplateID = item.i_TemplateID;
                    originalItem.s_Author = item.s_Author;
                    originalItem.s_Content = item.s_Content;
                    originalItem.s_ContentNoHtml = item.s_ContentNoHtml;
                    originalItem.s_Description = item.s_Description;
                    originalItem.s_EnName = item.s_EnName;
                    originalItem.s_ImgPath = item.s_ImgPath;
                    originalItem.s_KeyWord = item.s_KeyWord;
                    originalItem.s_Summary = item.s_Summary;
                    originalItem.s_Title = item.s_Title;
                    originalItem.s_Attach = item.s_Attach;
                    originalItem.s_Lang = item.s_Lang;
                    originalItem.s_PublishTime = item.s_PublishTime;
                    originalItem.s_Size = item.s_Size;
                    originalItem.s_Version = item.s_Version;
                    originalItem.s_UpdateTime = TConvert.formatDateTime19(DateTime.Now);
                    entity.SaveChanges();
                    Generate generate = new Generate();
                    generate.GenerateArticle(originalItem);
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
                return Content("填写的文章信息有误.");
            }
        } 
        #endregion

        #region 删除文章+Delete(int? id)
        [CheckinLogin]
        /// <summary>
        /// 删除文章
        /// </summary>
        /// <param name="id">文章id</param>
        /// <returns></returns>
        public ActionResult Delete(int? id)
        {
            if (id > 0)
            {
                try
                {
                    EHECD_WebEntities wb = new EHECD_WebEntities();
                    Article item = wb.Articles.Find(id);
                    if (item == null)
                        return Content("该文章不存在或已被删除.");
                    wb.Articles.Remove(item);
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
        /// 增加点击数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddClickNum(int ?id)
        {
            BaseDAL<Article> artDal= new BaseDAL<Article>();
            Article artModel=artDal.Where(u => u.ID == id).FirstOrDefault();
            if (artModel!=null)
            {
                artModel.i_ViewCount = artModel.i_ViewCount + 1;
            }
            int articleId = 0;
            int.TryParse(Request["id"],out articleId);
            return EditByClick(artModel,articleId);

        }

        /// <summary>
        /// 点击的时候增加点击次数
        /// </summary>
        /// <param name="item"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditByClick(Article item, int id)
        {
           
                try
                {
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    if (!string.IsNullOrEmpty(item.s_EnName))
                    {
                        if (entity.Articles.Where(m => m.s_EnName == item.s_EnName && m.ID != id).Count() > 0)
                        {
                            return Content("文章英文名称重复.");
                        }
                    }
                    if (item.s_Content != null)
                    {
                        item.s_ContentNoHtml = TString.NoHTML(item.s_Content);
                    }
                    Article originalItem = entity.Articles.Find(id);
                    if (originalItem == null)
                    {
                        return Content("所编辑的信息不存在或已被删除.");
                    }
                    originalItem.i_CategoryID = item.i_CategoryID;
                    originalItem.i_TemplateID = item.i_TemplateID;
                    originalItem.s_Author = item.s_Author;
                    originalItem.s_Content = item.s_Content;
                    originalItem.s_ContentNoHtml = item.s_ContentNoHtml;
                    originalItem.s_Description = item.s_Description;
                    originalItem.s_EnName = item.s_EnName;
                    originalItem.s_ImgPath = item.s_ImgPath;
                    originalItem.s_KeyWord = item.s_KeyWord;
                    originalItem.s_Summary = item.s_Summary;
                    originalItem.s_Title = item.s_Title;
                    originalItem.s_Attach = item.s_Attach;
                    originalItem.s_Lang = item.s_Lang;
                    originalItem.s_PublishTime = item.s_PublishTime;
                    originalItem.s_Size = item.s_Size;
                    originalItem.s_Version = item.s_Version;
                    originalItem.i_ViewCount = item.i_ViewCount;
                    originalItem.s_UpdateTime = TConvert.formatDateTime19(DateTime.Now);
                    entity.SaveChanges();
                    Generate generate = new Generate();
                    generate.GenerateArticle(originalItem);
                    return Content("yes");
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                    return Content("编辑失败.");
                }
            

        }

        public ActionResult Test()
        {
            Web.Code.Generate.GenerateHelper g = new Generate.GenerateHelper();
            object obj= g.json();
            return Content(obj.ToString());
        }
    }
}
