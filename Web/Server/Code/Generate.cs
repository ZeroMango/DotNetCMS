using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Common;
using Web.Server.Models;

namespace Web.Code
{
    /// <summary>
    /// 页面生成辅助类
    /// </summary>
    public class Generate
    {
        /// <summary>
        /// entity辅助对象
        /// </summary>
        private EHECD_WebEntities wb;

        /// <summary>
        /// 网站配置信息
        /// </summary>
        private WebConfig webConfig;

        /// <summary>
        /// 网站栏目信息
        /// </summary>
        private List<ArticleCategory> categoryList;

        private List<ModelItem_Value> modelItem_valueList;
        /// <summary>
        /// velocity辅助类
        /// </summary>
        private NVelocityHelper velocityHelper;

        /// <summary>
        /// 初始生成页面所必须的内容
        /// </summary>
        public Generate()
        {
            wb = new EHECD_WebEntities();
            try
            {
                BaseDAL<ModelItem_Value> modelItem_value = new BaseDAL<ModelItem_Value>();//通用curd类
                webConfig = wb.WebConfigs.Take(1).SingleOrDefault();
                BaseDAL<Article> article = new BaseDAL<Article>();//通用curd类
                List<Article> afterServiceList = article.Where(u => u.i_CategoryID == 34).ToList();//查询售后服务问题列表
                categoryList = wb.ArticleCategories.OrderBy(m => m.i_Order).ToList();
                if (categoryList == null || categoryList.Count == 0)
                {
                    return;
                }
                velocityHelper = new NVelocityHelper();
                velocityHelper.Init(HttpContext.Current.Server.MapPath("/Template/"));
                velocityHelper.Put("webConfig", webConfig);//置入网站配置对象
                velocityHelper.Put("categoryList", categoryList);//置入栏目列表对象
                velocityHelper.Put("afterServiceList", afterServiceList);//置入售后服务列表对象
                velocityHelper.Put("GenerateHelper", new GenerateHelper(categoryList));//置入页面生成辅助对象   
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
            }
        }

        /// <summary>
        /// 生成首页
        /// </summary>
        public void GenerateDeafult()
        {
            Template template = wb.Templates.Where(m => m.s_TemplateName == "首页").SingleOrDefault();
            if (template != null)
            {
                velocityHelper.GenerateShtml(template.s_TemplatePath + ".html", HttpContext.Current.Server.MapPath("/"), "index.html");
            }
        }
        /// <summary>
        /// 首页官方动态
        /// </summary>
        public void IndexOfficial()
        {
            BaseDAL<Article> artBase = new BaseDAL<Article>();
            artBase.Where(u => u.s_EnName == "Official");
            velocityHelper.Put("articleList", "");
        }
        /// <summary>
        /// 生成栏目页
        /// </summary>
        public void GenerateCateGory()
        {
            string sDir = "";
            GenerateCateGory(categoryList.Where(m => m.i_PID == 0).ToList(),
                ref sDir, categoryList);

        }

        /// <summary>
        /// 根据英文名生成栏目页 不生成栏目下的子栏目 及文章
        /// </summary>
        /// <param name="sEnName"></param>
        public void GenerateCategoryByEn(string sEnName)
        {
            if (string.IsNullOrEmpty(sEnName))
            {
                return;
            }
            ArticleCategory item = categoryList.Where(m => m.s_EnName == sEnName).SingleOrDefault();
            if (item == null)
            {
                return;
            }
            StringBuilder sPath = new StringBuilder();
            GetCategoryPath(item, categoryList, ref sPath);
            if (sPath.Length > 0)
            {
                sPath.Replace(item.s_EnName + "/", "");//本级的目录舍去，只生成在父目录的文件夹中
                GenerateCategory(sPath.ToString(), item);
            }
        }

        /// <summary>
        /// 根据ID生成栏目页 不生成栏目下的子栏目 及文章
        /// </summary>
        /// <param name="sEnName"></param>
        public void GenerateCategoryByID(int iID)
        {
            if (iID == 0)
            {
                return;
            }
            ArticleCategory item = categoryList.Where(m => m.ID == iID).SingleOrDefault();
            if (item == null)
            {
                return;
            }
            StringBuilder sPath = new StringBuilder();
            GetCategoryPath(item, categoryList, ref sPath);
            if (sPath.Length > 0)
            {
                sPath.Replace(item.s_EnName + "/", "");//本级的目录舍去，只生成在父目录的文件夹中
                GenerateCategory(sPath.ToString(), item);
            }
        }

        /// <summary>
        /// 查找栏目页面对应的路径
        /// </summary>
        /// <param name="item">栏目实体</param>
        /// <param name="categoryList">所有的栏目列表</param>
        /// <param name="sPath">用于返回值的路径</param>
        private void GetCategoryPath(ArticleCategory item, List<ArticleCategory> categoryList, ref StringBuilder sPath)
        {
            if (item == null || categoryList.Count == 0)
            {
                return;
            }
            sPath.Insert(0, item.s_EnName + "/");
            if (item.i_PID != 0)
            {//如果没有到最顶级，则继续递归
                ArticleCategory parentItem = categoryList.Where(m => m.ID == item.i_PID).SingleOrDefault();
                if (parentItem == null)
                {//如果没有找到父级节点 则返回
                    return;
                }
                GetCategoryPath(parentItem, categoryList, ref sPath);
            }
        }
        /// <summary>
        /// 生成栏目及子栏目、文章
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sDir"></param>
        /// <param name="allList"></param>
        private void GenerateCateGory(List<ArticleCategory> list, ref string sDir, List<ArticleCategory> allList)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }
            foreach (ArticleCategory item in list)
            {
                GenerateCategory(sDir, item);
                GenerateCategoryArticle(item.ID);
                List<ArticleCategory> tempList = allList.Where(m => m.i_PID == item.ID).ToList();
                if (tempList != null && tempList.Count > 0)
                {
                    sDir += item.s_EnName + "/";
                    GenerateCateGory(tempList, ref sDir, allList);
                }
                if (sDir.Contains("/"))
                {
                    ArticleCategory templateP = wb.ArticleCategories.Find(item.i_PID);
                    sDir = sDir.Replace(templateP.s_EnName + "/", "");
                }
            }
        }

        /// <summary>
        /// 生成栏目页
        /// </summary>
        /// <param name="sDir"></param>
        /// <param name="item"></param>
        private void GenerateCategory(string sDir, ArticleCategory item)
        {
            if (item.ID == 154)
            {
              List<Article> list= wb.Articles.Where(m => m.i_CategoryID == 121).Take(4).OrderByDescending(m=>m.s_PublishTime).ToList();
              velocityHelper.Put("List", list);
            }
            string sTemplateFile = "";
            Template template = wb.Templates.Find(item.i_TemplateID);
            sTemplateFile = template.s_TemplatePath + ".html";
            velocityHelper.Put("categoryItem", item);//置入栏目对象
            List<Article> articleList = wb.Articles.Where(m => m.i_CategoryID == item.ID && m.i_Status == 0).ToList();//获取栏目下文章列表

            int iPageCount = (articleList.Count - 1) / webConfig.i_PageSize + 1;
            velocityHelper.Put("pageCount", iPageCount);//置入栏目文章列表页面数量
            for (int iPageIndex = 1; iPageIndex <= iPageCount; iPageIndex++)
            {
                List<Article> tempArtList = articleList.OrderByDescending(m => m.b_IsTop).OrderByDescending(m => m.s_UpdateTime).OrderByDescending(m => m.ID)
                    .Skip((iPageIndex - 1) * webConfig.i_PageSize).Take(webConfig.i_PageSize).ToList();
                velocityHelper.Put("articleList", tempArtList);//置入栏目文章列表对象
                velocityHelper.Put("currentPage", iPageIndex);//置入栏目当前页面页码
                if (iPageIndex == 1)
                {//第一页不用加速页面前缀符号
                    velocityHelper.GenerateShtml(sTemplateFile, HttpContext.Current.Server.MapPath("/" + sDir), item.s_EnName + ".html");
                }
                else
                {
                    velocityHelper.GenerateShtml(sTemplateFile, HttpContext.Current.Server.MapPath("/" + sDir), item.s_EnName + iPageIndex + ".html");
                }
            }
        }

        /// <summary>
        /// 生成栏目下对应的文章
        /// </summary>
        /// <param name="iCategory">栏目ID</param>
        public void GenerateCategoryArticle(int iCategory)
        {
            try
            {
                List<Article> articleList = wb.Articles.Where(m => m.i_CategoryID == iCategory && m.i_Status == 0).OrderByDescending(m => m.ID).ToList();
                if (articleList == null || articleList.Count == 0)
                {
                    return;
                }
                foreach (Article item in articleList)
                {
                    GenerateArticle(item);
                }
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
            }
        }

        /// <summary>
        /// 生成文章页面
        /// </summary>
        /// <param name="item">文章实体</param>
        /// <param name="iTemplate">模板ID</param>
        /// <param name="sDir">生成页面所在目录</param>
        public void GenerateArticle(Article item)
        {
            try
            {
                ArticleCategory categoryItem = categoryList.Where(m => m.ID == item.i_CategoryID).SingleOrDefault();//所属栏目
                #region 取文章模板
                int iTemplate = 0;
                if (item.i_TemplateID != 0)
                {//如果为该文章单独设置过模板 则应用单独设置的
                    iTemplate = item.i_TemplateID;
                }
                else
                {//默认取栏目设置的内容模板
                    if (categoryItem != null)
                    {
                        iTemplate = categoryItem.i_ContentTemplateID;
                    }
                }
                Template template = wb.Templates.Find(iTemplate);
                #endregion
                string sTemplateFile = template.s_TemplatePath + ".html";
                velocityHelper.Put("articleItem", item);
                StringBuilder sPath = new StringBuilder();
                GetCategoryPath(categoryItem, categoryList, ref sPath);
                if (!string.IsNullOrEmpty(item.s_EnName))
                {//如果单独设置过英文名 则生成英文名的静态页面
                    velocityHelper.GenerateShtml(sTemplateFile, HttpContext.Current.Server.MapPath("/" + sPath), item.s_EnName + ".html");
                }
                else
                {
                    velocityHelper.GenerateShtml(sTemplateFile, HttpContext.Current.Server.MapPath("/" + sPath + TConvert.formatDate(TConvert.toDateTime(item.s_UpdateTime))), item.ID + ".html");
                }
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
            }
        }

        /// <summary>
        /// 查询典型用户的userName
        /// </summary>
        /// <param name="iTableID"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="sort"></param>
        /// <param name="order"></param>
        /// <param name="sField"></param>
        /// <param name="sKeyValue"></param>
        /// <returns></returns>
        public List<string> ModelItemValueUserNameList(int iTableID, string modelItemName, int page = 1, int rows = 10, string sort = "ID", string order = "asc")
        {
            List<string> modelItemValue = new List<string>() { };
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                var query = entity.ModelItem_Value.Where(m => m.i_TableID == iTableID);
                List<ModelItem_Value> list = query.OrderBy(m => m.i_Order).ThenByDescending(m => m.ID).Skip((int)(page - 1) * rows).Take(rows).ToList();
                foreach (ModelItem_Value item in list)
                {
                    XmlDocument xContent = TXml.LoadFromXml(item.s_Content);
                    if (xContent == null)
                    {
                        continue;
                    }
                    XmlNode xOutput = TXmlResults.getOutput(xContent);
                    foreach (XmlNode node in xOutput.ChildNodes)
                    {
                        if (node.Name == modelItemName)
                        {
                            modelItemValue.Add(TXml.getCData(node));
                        }

                    }
                }
                return modelItemValue;
            }
            catch (Exception ex)
            {
                TTracer.WriteLog_App(ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 页面生成辅助前端调用辅助
        /// </summary>
        public class GenerateHelper
        {
            /// <summary>
            /// 网站栏目信息 不含不显示在导航栏的
            /// </summary>
            private List<ArticleCategory> categoryList = null;

            /// <summary>
            /// 网站栏目信息 含不显示在导航栏的
            /// </summary>
            private List<ArticleCategory> categoryAllList = null;

            public GenerateHelper()
            {

            }
            /// <summary>
            /// 构造函数 
            /// </summary>
            /// <param name="categoryList">网站栏目信息</param>
            public GenerateHelper(List<ArticleCategory> categoryList)
            {
                this.categoryAllList = categoryList;
                this.categoryList = categoryList.Where(m => m.b_IsShowNav).ToList();
            }
            public string GetCategory(string ff)
            {
                return "func" + ff;
            }
            /// <summary>
            /// 通过栏目英文名，得到下面的子栏目
            /// </summary>
            /// <param name="sEnName"></param>
            /// <returns></returns>
            public List<ArticleCategory> GetChildeCategoryByEnName(string sEnName)
            {
                BaseDAL<ArticleCategory> dal = new BaseDAL<ArticleCategory>();
                List<ArticleCategory> resList = new List<ArticleCategory>();
                List<ArticleCategory> allList = new List<ArticleCategory>();
                allList = dal.Where(u => u.ID > 0).ToList();
                ArticleCategory artc = dal.Where(m => m.s_EnName == sEnName).SingleOrDefault();
                if (artc != null)
                {
                    resList = allList.Where(m => m.i_PID == artc.ID).ToList();
                }
                return resList;
            }

            /// <summary>
            /// 通过栏目的英文名获取下面的文章数量
            /// </summary>
            /// <returns></returns>
            public int GetArticleCountByEnName(string sEnName)
            {
                BaseDAL<ArticleCategory> dal = new BaseDAL<ArticleCategory>();
                BaseDAL<Article> artDal = new BaseDAL<Article>();
                ArticleCategory cate = dal.Where(u => u.s_EnName == sEnName).SingleOrDefault();
                List<Article> artList = new List<Article>();
                if (cate != null)
                {
                    artList = artDal.Where(u => u.i_CategoryID == cate.ID).ToList();
                }
                return artList.Count;
            }
            /// <summary>
            /// 加载文章列表通过父栏目
            /// </summary>
            /// <param name="sEnName"></param>
            /// <returns></returns>
            public List<Article> GetArticleByParentEnName(string sEnName)
            {
                BaseDAL<ArticleCategory> dal = new BaseDAL<ArticleCategory>();
                BaseDAL<Article> artDal = new BaseDAL<Article>();
                ArticleCategory cate = dal.Where(u => u.s_EnName == sEnName).SingleOrDefault();
                List<Article> artList = new List<Article>();
                List<int> cateList = new List<int>();
                if (cate != null)
                {
                    cateList = dal.Where(u => u.i_PID == cate.ID).Select(u => u.ID).ToList();
                    artList = artDal.Where(u => u.ID > 0).ToList();
                    artList = artList.Where(u => cateList.Contains(u.i_CategoryID)).ToList();
                }
                return artList;
            }
            /// <summary>
            /// 判断并获取子栏目列表
            /// </summary>
            /// <param name="list"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public List<ArticleCategory> CheckHasChild(object list, int id)
            {
                List<ArticleCategory> resultList = new List<ArticleCategory>();
                try
                {
                    if (list != null && id > 0)
                    {
                        List<ArticleCategory> categoryList = (List<ArticleCategory>)list;
                        resultList = categoryAllList.Where(m => m.i_PID == id).ToList();
                    }
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                }
                return resultList;
            }

            /// <summary>
            /// 显示在导航栏中
            /// </summary>
            /// <param name="list"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public List<ArticleCategory> CheckHasNavChild(object list, int id)
            {
                List<ArticleCategory> resultList = new List<ArticleCategory>();
                try
                {
                    if (list != null && id > 0)
                    {
                        List<ArticleCategory> categoryList = (List<ArticleCategory>)list;
                        resultList = categoryList.Where(m => m.i_PID == id && m.b_IsShowNav).ToList();
                    }
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                }
                return resultList;
            }

            /// <summary>
            /// 判断并获取子栏目列表
            /// </summary>
            /// <param name="list"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public List<ArticleCategory> CheckHasChildBysEnName(string sEnName)
            {
                BaseDAL<ArticleCategory> baseDal = new BaseDAL<ArticleCategory>();
                List<ArticleCategory> resultList = new List<ArticleCategory>();
                try
                {
                    if (!string.IsNullOrEmpty(sEnName))
                    {
                        List<ArticleCategory> categoryList = baseDal.Where(m => m.ID > 0).ToList();
                        ArticleCategory cate = categoryList.Where(m => m.s_EnName == sEnName).FirstOrDefault();
                        if (cate != null)
                        {
                            resultList = categoryList.Where(m => m.i_PID == cate.ID).ToList();
                        }

                    }
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                }
                return resultList;
            }

            /// <summary>
            /// 获取顶级栏目列表
            /// </summary>
            /// <param name="list"></param>
            /// <returns></returns>
            public List<ArticleCategory> GetTopCategory(object list)
            {
                List<ArticleCategory> resultList = new List<ArticleCategory>();
                if (list != null)
                {
                    List<ArticleCategory> categoryList = (List<ArticleCategory>)list;
                    resultList = categoryList.Where(m => m.i_PID == 0).ToList();
                }
                return resultList;
            }

            /// <summary>
            /// 获取栏目列表中的文章链接
            /// </summary>
            /// <param name="articleItem"></param>
            /// <param name="sCategoryEnName"></param>
            /// <returns></returns>
            public string GetArticleLink(object articleItem, string sCategoryEnName)
            {
                string sResult = string.Empty;
                if (articleItem != null)
                {
                    Article item = articleItem as Article;
                    StringBuilder sPath = new StringBuilder();
                    ArticleCategory categoryItem = categoryAllList.Where(m => m.ID == item.i_CategoryID).SingleOrDefault();
                    GetCategoryPath(categoryItem, categoryList, ref sPath);
                    if (!string.IsNullOrEmpty(item.s_EnName))
                    {//如果单独设置过 英文名字  则文件路径规则为：{栏目}/英文名字.html
                        sResult = "/" + sPath.ToString() + item.s_EnName + ".html";
                    }
                    else
                    {
                        sResult = TConvert.formatDate(TConvert.toDateTime(item.s_UpdateTime)) + "/" + item.ID + ".html";
                        if (!string.IsNullOrEmpty(sCategoryEnName))
                        {
                            sResult = "/" + sPath.ToString() + sResult;
                        }
                    }
                }
                return sResult;
            }


            /// <summary>
            /// 查询文章的标题
            /// </summary>
            /// <param name="articleItem"></param>
            /// <param name="sCategoryEnName"></param>
            /// <returns></returns>
            public string GetArticleTitle(object articleItem, string sCategoryEnName)
            {
                string sResult = string.Empty;
                if (articleItem != null)
                {
                    Article item = articleItem as Article;
                    sResult = item.s_Title;
                }
                return sResult;
            }

            /// <summary>
            /// 查找栏目页面对应的路径
            /// </summary>
            /// <param name="item">栏目实体</param>
            /// <param name="categoryList">所有的栏目列表</param>
            /// <param name="sPath">用于返回值的路径</param>
            private void GetCategoryPath(ArticleCategory item, List<ArticleCategory> categoryList, ref StringBuilder sPath)
            {
                if (item == null || categoryList == null || categoryList.Count == 0)
                {
                    return;
                }
                sPath.Insert(0, item.s_EnName + "/");
                if (item.i_PID != 0)
                {//如果没有到最顶级，则继续递归
                    ArticleCategory parentItem = categoryList.Where(m => m.ID == item.i_PID).SingleOrDefault();
                    if (parentItem == null)
                    {//如果没有找到父级节点 则返回
                        return;
                    }
                    GetCategoryPath(parentItem, categoryList, ref sPath);
                }
            }

            /// <summary>
            /// 获取栏目链接
            /// </summary>
            /// <param name="item">栏目实体</param>
            /// <returns></returns>
            public string GetCategoryLink(object item)
            {
                string sResult = string.Empty;
                if (item != null)
                {
                    ArticleCategory categoryitem = item as ArticleCategory;
                    if (categoryitem.b_IsRedirect && !string.IsNullOrEmpty(categoryitem.s_RedirectUrl))
                    {
                        sResult = categoryitem.s_RedirectUrl;
                    }
                    else
                    {
                        StringBuilder sPath = new StringBuilder();
                        GetCategoryPath(categoryitem, categoryList, ref sPath);
                        if (sPath.Length > 0)
                        {
                            sPath.Replace(categoryitem.s_EnName + "/", "");
                        }
                        sResult = "/" + sPath.ToString() + categoryitem.s_EnName + ".html";
                    }
                }
                return sResult;
            }

            /// <summary>
            /// 根据英文名获取栏目链接地址
            /// </summary>
            /// <param name="sEnName"></param>
            /// <returns></returns>
            public string GetCategoryLinkByEn(string sEnName)
            {
                string sResult = string.Empty;
                ArticleCategory categoryItem = GetCategoryByEn(sEnName);
                if (categoryItem != null)
                {
                    sResult = GetCategoryLink(categoryItem);
                }
                return sResult;
            }


            /// <summary>
            /// 根据栏目id 获取子栏目
            /// </summary>
            /// <param name="iCategory"></param>
            /// <returns></returns>
            public List<ArticleCategory> GetChildrenCategory(int iCategory)
            {
                List<ArticleCategory> result = new List<ArticleCategory>();
                result = categoryAllList.Where(m => m.i_PID == iCategory).ToList();
                return result;
            }

            /// <summary>
            /// 根据英文名获取文章
            /// </summary>
            /// <param name="sEnName"></param>
            /// <returns></returns>
            public Article GetArticleByEnName(string sEnName)
            {
                Article result = new Article();
                EHECD_WebEntities entity = new EHECD_WebEntities();
                result = entity.Articles.Where(m => m.s_EnName == sEnName).SingleOrDefault();
                return result;
            }

            /// <summary>
            /// 根据英文名获取栏目
            /// </summary>
            /// <param name="sEnName"></param>
            /// <returns></returns>
            public ArticleCategory GetCategoryByEn(string sEnName)
            {
                ArticleCategory result = new ArticleCategory();
                //result = categoryAllList.Where(m => m.s_EnName == sEnName).SingleOrDefault();
                BaseDAL<ArticleCategory> cateBase = new BaseDAL<ArticleCategory>();
                result = cateBase.Where(m => m.s_EnName == sEnName).SingleOrDefault();
                return result;
            }

            /// <summary>
            /// 根据栏目英文名获取栏目下文章
            /// </summary>
            /// <param name="sEnName"></param>
            /// <param name="iCount"></param>
            /// <param name="sSortType"></param>
            /// <returns></returns>
            public List<Article> GetArticleByCateEn(string sEnName, int iCount, string sSortType = "asc")
            {
                List<Article> result = new List<Article>();
                try
                {
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    ArticleCategory categoryItem = GetCategoryByEn(sEnName);
                    if (categoryItem != null)
                    {
                        var query = entity.Articles.Where(m => m.i_CategoryID == categoryItem.ID && m.i_Status == 0).OrderByDescending(m => m.b_IsTop).ThenByDescending(m => m.b_IsHot);
                        if (TString.Compare2String(sSortType, "asc"))
                        {//id升序
                            query = query.ThenBy(m => m.ID);
                        }
                        else
                        {//id降序
                            query = query.ThenByDescending(m => m.ID);
                        }
                        result = query.Take(iCount).ToList();
                    }
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                }
                return result;
            }

            /// <summary>
            /// 下一篇文章
            /// </summary>
            /// <param name="sEnName">栏目的英文名</param>
            /// <param name="iCount">读取好多篇文章</param>
            /// <param name="id">当前文章的id</param>
            /// <returns></returns>
            public Article NextArticle(string sEnName, int iCount, int iId)
            {
                BaseDAL<ArticleCategory> artDal = new BaseDAL<ArticleCategory>();
                BaseDAL<Article> articleDal = new BaseDAL<Article>();
                ArticleCategory artItem = artDal.Where(u => u.s_EnName == sEnName).FirstOrDefault();
                List<Article> artList = new List<Article>();
                if (artItem != null)
                {
                    artList = articleDal.Where(u => u.i_CategoryID == artItem.ID).ToList();
                }
                Article art = artList.Where(s => s.ID > iId).OrderBy(s => s.ID).FirstOrDefault() as Article;
                return art;
            }

            /// <summary>
            /// 上一篇文章
            /// </summary>
            /// <param name="sEnName"></param>
            /// <param name="iCount"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public Article PreArticle(string sEnName, int iCount, int iId)
            {
                BaseDAL<ArticleCategory> artDal = new BaseDAL<ArticleCategory>();
                BaseDAL<Article> articleDal = new BaseDAL<Article>();
                ArticleCategory artItem=artDal.Where(u => u.s_EnName == sEnName).FirstOrDefault();
                List<Article> artList=new List<Article>();
                if (artItem!=null)
                {
                    artList=articleDal.Where(u => u.i_CategoryID == artItem.ID).ToList();
                }
                //List<Article> artList = GetArticleByCateEn(sEnName, iCount);
                Article art = artList.Where(s => s.ID < iId).OrderByDescending(s => s.ID).FirstOrDefault() as Article;
                return art;
            }
            /// <summary>
            /// 最后一篇文章
            /// </summary>
            /// <param name="sEnName"></param>
            /// <param name="iCount"></param>
            /// <returns></returns>
            public Article LastArticle(string sEnName, int iCount)
            {
                List<Article> artList = GetArticleByCateEn(sEnName, iCount);
                Article art = artList.Where(s => s.ID > 0).Take(1).SingleOrDefault() as Article;
                return art;
            }
            /// <summary>
            /// 获取下一个栏目
            /// </summary>
            /// <param name="sEnName"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public ArticleCategory NextCategory(string sEnName, int id)
            {
                List<ArticleCategory> cateList = GetChildeCategoryByEnName(sEnName);
                ArticleCategory cate = cateList.Where(s => s.ID > id).Take(1).SingleOrDefault() as ArticleCategory;
                return cate;
            }
            /// <summary>
            /// 获取上一个栏目
            /// </summary>
            /// <param name="sEnName"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public ArticleCategory PreCategory(string sEnName, int id)
            {
                List<ArticleCategory> cateList = GetChildeCategoryByEnName(sEnName);
                ArticleCategory cate = cateList.Where(s => s.ID < id).Take(1).SingleOrDefault() as ArticleCategory;
                return cate;
            }
            /// <summary>
            /// 获取父栏目下的最后一个子栏目
            /// </summary>
            /// <param name="sEnName"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public ArticleCategory LastCategory(string sEnName, int id)
            {
                List<ArticleCategory> cateList = GetChildeCategoryByEnName(sEnName);
                ArticleCategory cate = cateList.Where(s => s.ID > 0).OrderByDescending(s => s.ID).Take(1).SingleOrDefault() as ArticleCategory;
                return cate;
            }
            /// <summary>
            /// 截取字符串
            /// </summary>
            /// <param name="sStr"></param>
            /// <param name="iLength"></param>
            /// <returns></returns>
            public string Substring(string sStr, int iLength)
            {
                string sResult = sStr.Trim();
                if (sResult.Length > iLength)
                {
                    sResult = sResult.Substring(0, iLength);
                    sResult = TString.NoHTML(sResult);
                    sResult += "...";
                }
                return sResult;
            }

            /// <summary>
            /// 截取字符串没有省略的点
            /// </summary>
            /// <param name="sStr"></param>
            /// <param name="iLength"></param>
            /// <returns></returns>
            public string SubstringNoDot(string sStr, int iLength)
            {
                string sResult = sStr.Trim();
                if (sResult.Length > iLength)
                {
                    sResult = sResult.Substring(0, iLength);
                }
                return sResult;
            }
            /// <summary>
            /// 获取模型内容
            /// </summary>
            /// <param name="sModelIdentity">模型表标识</param>
            /// <param name="iCount">获取的条数</param>
            /// <param name="sSortType">排序方式</param>
            /// <returns></returns>
            public ArrayList GetModelValueList(string sModelIdentity, int iCount, string sSortType = "asc")
            {
                ArrayList result = new ArrayList();
                try
                {
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    ModelTable tempTable = entity.ModelTables.Where(m => m.s_ModelIdentity == sModelIdentity).SingleOrDefault();
                    if (tempTable != null)
                    {
                        var query = entity.ModelItem_Value.Where(m => m.i_TableID == tempTable.ID);
                        if (TString.Compare2String(sSortType, "asc"))
                        {//ID升序
                            query = query.OrderBy(m => m.ID);
                        }
                        else
                        {//id降序
                            query = query.OrderByDescending(m => m.ID);
                        }
                        List<ModelItem_Value> valueList = query.Take(iCount).ToList();
                        foreach (ModelItem_Value valueItem in valueList)
                        {//遍历每项模型内容
                            XmlDocument xContent = TXml.LoadFromXml(valueItem.s_Content);
                            if (xContent == null)
                            {
                                continue;
                            }
                            XmlNode xOutput = TXmlResults.getOutput(xContent);
                            if (xOutput.ChildNodes == null)
                            {
                                continue;
                            }
                            Dictionary<string, string> key_ValueList = new Dictionary<string, string>();
                            foreach (XmlNode node in xOutput.ChildNodes)
                            {//获取每项值中的每个字段及其值
                                key_ValueList.Add(node.Name, TXml.getCData(node));
                            }
                            result.Add(key_ValueList);
                        }
                    }
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                }
                return result;
            }

            public string GetValueFromDic(object itemList, string sKey)
            {
                string sResult = string.Empty;
                try
                {
                    Dictionary<string, string> tempList = itemList as Dictionary<string, string>;
                    if (tempList.Keys.Contains(sKey))
                    {
                        sResult = tempList[sKey];
                    }
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                }
                return sResult;
            }

            //通过父栏目的英文名得到子栏目的英文文章
            /// <summary>
            /// 通过父栏目的英文名得到子栏目的英文文章
            /// </summary>
            /// <param name="sEname"></param>
            /// <returns></returns>
            public List<Article> GetSonArticleByPaCateEn(string sEname,int iCount,string sSortType="asc")
            {
                BaseDAL<ArticleCategory> cateDal = new BaseDAL<ArticleCategory>();
                BaseDAL<Article> artDal = new BaseDAL<Article>();
                ArticleCategory cateItem = cateDal.Where(u => u.s_EnName == sEname).FirstOrDefault();//通过栏目英文名把栏目item取出来
                List<ArticleCategory> artCate = new List<ArticleCategory>();
                List<Article> artList = new List<Article>();
                List<int> cateIdList = new List<int>();
                if (cateItem != null)
                {
                    cateIdList = cateDal.Where(u => u.i_PID == cateItem.ID).Select(u => u.ID).ToList();//找出父栏目下的所有子栏目id
                }
                if (cateIdList.Count > 0)
                {
                    int id = 0;
                    for (int i = 0; i < cateIdList.Count; i++)
                    {
                        id = cateIdList[i];
                        List<Article> artItem = artDal.Where(u => u.i_CategoryID == id).ToList();//把栏目下的文章找出来
                        if (artItem.Count>0)
                        {
                            artList.AddRange(artItem);//合并集合
                        }
                    }
                }
                if (sSortType == "asc")//升序
                {
                    artList = artList.Take(iCount).OrderBy(m => m.ID).ToList();
                }
                else
                {
                    artList=artList.Take(iCount).OrderByDescending(m => m.ID).ToList();
                }
                
                return artList;
            }

            /// <summary>
            /// 根据文章id获取栏目英文名
            /// </summary>
            /// <returns></returns>
            public string getCateEnByartId(int iArtId)
            {
                BaseDAL<Article> artDal = new BaseDAL<Article>();
                BaseDAL<ArticleCategory> cateDal = new BaseDAL<ArticleCategory>();
                Article artItem= artDal.Where(u => u.ID == iArtId).FirstOrDefault();
                string sCateEnName = string.Empty;
                if (artItem!=null)
                {
                    ArticleCategory cateItem=cateDal.Where(u => u.ID == artItem.i_CategoryID).FirstOrDefault();
                    if (cateItem!=null)
                    {
                        sCateEnName = cateItem.s_EnName;
                    }
                }
                return sCateEnName;
            }

            #region 根据父栏目的英文名查询他的子栏目 getSonCateByParentSEn(string sEname)
            /// <summary>
            /// 根据父栏目的英文名查询他的子栏目
            /// </summary>
            /// <param name="sEname">父栏目英文名</param>
            /// <returns></returns>
            public List<ArticleCategory> getSonCateByParentSEn(string sEname)
            {
                BaseDAL<ArticleCategory> cateDal = new BaseDAL<ArticleCategory>();
                ArticleCategory artItem = cateDal.Where(u => u.s_EnName == sEname).FirstOrDefault();
                List<ArticleCategory> cateSonList = new List<ArticleCategory>();
                if (artItem != null)
                {
                    cateSonList = cateDal.Where(u => u.i_PID == artItem.ID).OrderBy(u=>u.i_Order).ToList();
                }
                return cateSonList;
            } 
            #endregion

            /// <summary>
            /// 通过栏目英文名获得文章列表
            /// </summary>
            /// <param name="sEnName"></param>
            /// <returns></returns>
            public List<Article> GetArticleListByCateEn(string sEnName,int iTakeCount=0)
            {
                List<Article> articleList = new List<Article>();
                BaseDAL<ArticleCategory> cateDal = new BaseDAL<ArticleCategory>();
                BaseDAL<Article> artilceDal = new BaseDAL<Article>();
                ArticleCategory cateModel = cateDal.Where(u => u.s_EnName == sEnName).FirstOrDefault();
                if (cateModel!=null)
                {
                    articleList=artilceDal.Where(u => u.i_CategoryID == cateModel.ID).Take(iTakeCount).ToList();
                }
                return articleList;
            }

            /// <summary>
            /// 根据栏目id获取文章列表
            /// </summary>
            /// <returns></returns>
            public List<Article> getArtListByCateId(int iCateId)
            {
                BaseDAL<Article> arDal = new BaseDAL<Article>();
                return arDal.Where(u => u.i_CategoryID == iCateId).ToList();
            }

            public object Test()
            {
                var obj = new { ID = 1, Name = "cj", Age = 12 };
                return obj;
            }

            public List<object> listObj()
            {
                List<object> objList = new List<object>(){
                    new {Nmae="cj",Age=12},
                    new {Nmae="cj",Age=12},
                    new {Nmae="cj",Age=12},
                    new {Nmae="cj",Age=12},
                    new {Nmae="cj",Age=12}
                };
                return objList;
            }

            public object json()
            {
                string json = "{'person':[{'name':'cj','age':12},{'name':'ccc','age':22}]}";
                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                return js.DeserializeObject(json);
            }

        }
    }
}