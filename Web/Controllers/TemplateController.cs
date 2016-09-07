using System;
using System.Collections.Generic;
using System.IO;
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
    public class TemplateController : Controller
    {
        //
        // GET: /Template/
        [CheckinLogin]
        public ActionResult Index()
        {
            return View();
        }

        #region 分页获取模板列表+JsonList(int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        /// <summary>
        /// 分页获取模板列表
        /// </summary>
        /// <param name="page">当前页</param>
        /// <param name="rows">每一页的数量</param>
        /// <param name="sort">通过哪个字段进行排序</param>
        /// <param name="order">升序还是降序排序</param>
        /// <returns></returns>
        public ActionResult JsonList(int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        {
            StringBuilder sResult = new StringBuilder();
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                var query = entity.Templates.OrderByDescending(m => m.ID).Skip((int)(page - 1) * rows).Take(rows);
                if (!string.IsNullOrEmpty(sField) && !string.IsNullOrEmpty(sKeyValue))
                {
                    if (sField == "s_TemplateName")
                    {
                        query = query.Where(m => m.s_TemplateName.Contains(sKeyValue));
                    }
                    else if (sField == "s_TemplateType")
                    {
                        query = query.Where(m => m.s_TemplateType.Contains(sKeyValue));
                    }

                }
                if (order == "asc")
                {
                    switch (sort)
                    {
                        case "s_TemplateName"
                            : query = query.OrderBy(m => m.s_TemplateName);
                            break;
                        case "s_TemplateType"
                            : query = query.OrderBy(m => m.s_TemplateType);
                            break;
                    }
                }
                else if (order == "desc")
                {
                    switch (sort)
                    {
                        case "s_TemplateName"
                            : query = query.OrderByDescending(m => m.s_TemplateName);
                            break;
                        case "s_TemplateType"
                            : query = query.OrderByDescending(m => m.s_TemplateType);
                            break;
                    }
                }
                List<Template> list = query.ToList();
                sResult.Append("{\"total\":" + entity.Templates.Count() + ",\"rows\":");
                JavaScriptSerializer json = new JavaScriptSerializer();
                sResult.Append(json.Serialize(list) + "}");
            }
            catch (Exception ex)
            {
                TTracer.WriteLog_App(ex.ToString());
            }
            return Content(sResult.ToString());
        } 
        #endregion

        #region 下拉列表框+ComboBoxList(bool bSelectFirst = true)
        /// <summary>
        /// 下拉列表框
        /// </summary>
        /// <returns></returns>
        public ActionResult ComboBoxList(bool bSelectFirst = true)
        {
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                List<Template> list = entity.Templates.Where(m => m.s_TemplateType != "引用文件").ToList();
                if (list == null || list.Count == 0)
                {
                    return Content("");
                }
                StringBuilder sResult = new StringBuilder();
                sResult.Append("[");
                if (!bSelectFirst)
                {
                    sResult.Append("{");
                    sResult.Append("\"id\":0,");
                    sResult.Append("\"text\":\"无\",");
                    sResult.Append("\"selected\":true");
                    sResult.Append("}");
                    if (list.Count > 0)
                    {
                        sResult.Append(",");
                    }
                }
                foreach (Template item in list)
                {
                    sResult.Append("{");
                    sResult.Append("\"id\":" + item.ID + ",");
                    sResult.Append("\"text\":\"" + item.s_TemplateName + "-" + item.s_TemplateType + "\"");
                    if (bSelectFirst && item.ID == list.First().ID)
                    {
                        sResult.Append(",\"selected\":true");
                    }
                    sResult.Append("}");
                    if (item.ID != list.Last().ID)
                    {
                        sResult.Append(",");
                    }
                }
                sResult.Append("]");
                return Content(sResult.ToString());
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
                return Content("");
            }
        } 
        #endregion

        #region 新增加一个模板=Add(Template item)
        /// <summary>
        /// 新增加一个模板
        /// </summary>
        /// <param name="item">模板实体对象</param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        [CheckinLogin]
        public ActionResult Add(Template item)
        {

            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                if (entity.Templates.Where(m => m.s_TemplateName == item.s_TemplateName).Count() > 0)
                {
                    return Content("模板名重复.");
                }
                else if (entity.Templates.Where(m => m.s_TemplatePath == item.s_TemplatePath).Count() > 0)
                {
                    return Content("模板文件名重复.");
                }
                else
                {
                    entity.Templates.Add(item);
                }
                WriteTemplate(item);
                entity.SaveChanges();
                return Content("yes");
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
            }
            return Content("添加失败.");
        } 
        #endregion

        #region 写入模板文件+WriteTemplate(Template item)
        /// <summary>
        /// 写入模板文件
        /// </summary>
        /// <param name="item">模板实体对象</param>
        private void WriteTemplate(Template item)
        {
            using (StringWriter writer = new StringWriter())
            {
                writer.Write(item.s_TemplateContent);
                if (!Directory.Exists(Server.MapPath("/Template")))
                {
                    Directory.CreateDirectory(Server.MapPath("/Template"));
                }
                System.IO.File.WriteAllText(Server.MapPath("/Template/" + item.s_TemplatePath + ".html"), writer.ToString());
            }
        } 
        #endregion

        #region 获取编辑内容信息+Edit(int? id)
        /// <summary>
        /// 获取编辑内容信息
        /// </summary>
        /// <param name="id">模板id</param>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            Template item = null;
            if (id > 0)
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                item = entity.Templates.Find(id);
                if (item != null)
                {
                    string sTemplatePath = Server.MapPath("/Template/" + item.s_TemplatePath + ".html");
                    FileInfo fileInfo = new FileInfo(sTemplatePath);
                    if (fileInfo.Exists)
                    {//如果存在模板文件，则取模板文件的内容进行展示
                        using (StreamReader sr = new StreamReader(sTemplatePath))
                        {
                            item.s_TemplateContent = sr.ReadToEnd();
                        }
                    }
                    JavaScriptSerializer json = new JavaScriptSerializer();
                    return Content(json.Serialize(item));
                }
            }
            return Content("");
        } 
        #endregion

        #region 确认编辑模板+Edit(Template item, int id)
        /// <summary>
        /// 确认编辑模板
        /// </summary>
        /// <param name="item">模板实体对象</param>
        /// <param name="id">模板id</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult Edit(Template item, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    if (entity.Templates.Where(m => m.s_TemplateName == item.s_TemplateName && m.ID != id).Count() > 0)
                    {
                        return Content("模板名重复.");
                    }
                    if (entity.Templates.Where(m => m.s_TemplatePath == item.s_TemplatePath && m.ID != id).Count() > 0)
                    {
                        return Content("模板文件名重复.");
                    }
                    WriteTemplate(item);
                    entity.Templates.Attach(item);
                    entity.Entry<Template>(item).State = System.Data.EntityState.Modified;
                    entity.SaveChanges();
                    return Content("yes");
                }
                catch (Exception e)
                {
                    TTracer.WriteLog_App(e.ToString());
                    return Content("编辑失败."+e.ToString());
                }
            }
            else
            {
                return Content("填写的模板信息有误.");
            }
        } 
        #endregion

        #region 删除操作+Delete(int? id)
        /// <summary>
        /// 删除操作
        /// </summary>
        /// <param name="id">模板id</param>
        /// <returns></returns>
        [CheckinLogin]
        public ActionResult Delete(int? id)
        {
            if (id > 0)
            {
                try
                {
                    EHECD_WebEntities wb = new EHECD_WebEntities();
                    Template item = wb.Templates.Find(id);
                    if (item == null)
                        return Content("该模板不存在或已被删除.");
                    if (wb.ArticleCategories.Where(m => m.i_ContentTemplateID == id || m.i_TemplateID == id).Count() > 0
                        || wb.Articles.Where(m => m.i_TemplateID == id).Count() > 0)
                        return Content("该模板存在应用的文章，不能删除.");
                    //删除模板文件
                    FileInfo fileInfo = new FileInfo(Server.MapPath("/Template/" + item.s_TemplatePath + ".html"));
                    fileInfo.Delete();
                    wb.Templates.Remove(item);
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

        #region 生成单个模板+GenerateTeamplate(int? id)
        /// <summary>
        /// 生成模板
        /// </summary>
        /// <param name="id">模板id</param>
        /// <returns></returns>
        public ActionResult GenerateTeamplateSingle(int? id)
        {
            EHECD_WebEntities entity = new EHECD_WebEntities();
            Template templateModel = null;
            if (id>0)
            {
                templateModel=entity.Templates.Find(id);
                try
                {
                    Generate(templateModel.s_TemplateContent, templateModel.s_TemplatePath);
                    return Content("true");
                }
                catch (Exception)
                {

                    return Content("false");
                }
            }
            return Content("false");
        } 
        #endregion

        #region 生成全部模板+GenerateTeamplateAll()
        /// <summary>
        /// 生成全部模板
        /// </summary>
        /// <returns></returns>
        public ActionResult GenerateTeamplateAll()
        {
            //BaseDAL封装好的curd 链接查询 分页的泛型类,在code文件夹下
            BaseDAL<Template> temp = new BaseDAL<Template>();
            var templateList=temp.Where(u=>u.ID>0);
            try
            {
                foreach (var item in templateList)
                {
                    Generate(item.s_TemplateContent, item.s_TemplatePath);
                }
                return Content("true");
            }
            catch (Exception e)
            {
                
                return Content("false"+e.ToString());
            }
            
        } 
        #endregion

        #region 生成静态内容页复用方法 +Generate(string content, string templatePath)
        /// <summary>
        /// 生成静态内容页复用方法
        /// </summary>
        /// <param name="content">静态页内容</param>
        /// <param name="templatePath">静态页路径</param>
        public void Generate(string content, string templatePath)
        {
            using (StringWriter writer = new StringWriter())
            {
                writer.Write(content);
                if (!Directory.Exists(Server.MapPath("/Template")))
                {
                    Directory.CreateDirectory(Server.MapPath("/Template"));
                }
                System.IO.File.WriteAllText(Server.MapPath("/Template/" + templatePath + ".html"), writer.ToString());
            }
        } 
        #endregion
    }
}
