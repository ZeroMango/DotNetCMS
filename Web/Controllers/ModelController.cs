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
    public class ModelController : Controller
    {
        //
        // GET: /Model/
        [CheckinLogin]
        public ActionResult Index()
        {
            return View();
        }

        #region 增加一个模型+Add(ModelTable item)
        /// <summary>
        /// 增加一个模型
        /// </summary>
        /// <param name="item">一个模型实体对象</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult Add(ModelTable item)
        {
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                if (entity.ModelTables.Where(m => m.s_ModelName == item.s_ModelName).Count() > 0)
                {
                    return Content("模型名称重复.");
                }
                else if (entity.ModelTables.Where(m => m.s_ModelIdentity == item.s_ModelIdentity).Count() > 0)
                {
                    return Content("模型标识重复.");
                }
                else
                {
                    entity.ModelTables.Add(item);
                }
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

        #region 分页获取模型的数据+List(int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        /// <summary>
        /// 分页获取模型的数据
        /// </summary>
        /// <param name="page">当前页数</param>
        /// <param name="rows">每页的数量</param>
        /// <param name="sort">通过哪个属性来排序</param>
        /// <param name="order">升学宴还是降序排序</param>
        /// <returns></returns>
        [CheckinLogin]
        public ActionResult List(int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        {
            StringBuilder sResult = new StringBuilder();
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                var query = entity.ModelTables.Where(m => 1 == 1);
                if (!string.IsNullOrEmpty(sField) && !string.IsNullOrEmpty(sKeyValue))
                {
                    if (sField == "s_ModelName")
                    {
                        query = query.Where(m => m.s_ModelName.Contains(sKeyValue));
                    }
                    else if (sField == "s_ModelIdentity")
                    {
                        query = query.Where(m => m.s_ModelIdentity.Contains(sKeyValue));
                    }
                }
                if (order == "asc")
                {
                    switch (sort)
                    {
                        case "s_ModelName"
                            : query = query.OrderBy(m => m.s_ModelName);
                            break;
                        case "s_ModelIdentity"
                            : query = query.OrderBy(m => m.s_ModelIdentity);
                            break;
                        default:
                            query = query.OrderBy(m => m.ID);
                            break;
                    }
                }
                else if (order == "desc")
                {
                    switch (sort)
                    {
                        case "s_ModelName"
                            : query = query.OrderByDescending(m => m.s_ModelName);
                            break;
                        case "s_ModelIdentity"
                            : query = query.OrderByDescending(m => m.s_ModelIdentity);
                            break;
                        default:
                            query = query.OrderByDescending(m => m.ID);
                            break;
                    }
                }
                List<ModelTable> list = query.Skip((int)(page - 1) * rows).Take(rows).ToList();
                sResult.Append("{\"total\":" + entity.ModelTables.Count() + ",\"rows\":");
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

        #region 获取编辑信息+Edit(int? id)
        /// <summary>
        /// 获取编辑信息
        /// </summary>
        /// <param name="id">Model Id</param>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            ModelTable item = null;
            if (id > 0)
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                item = entity.ModelTables.Find(id);
                if (item != null)
                {
                    JavaScriptSerializer json = new JavaScriptSerializer();
                    return Content(json.Serialize(item));
                }
            }
            return Content("");
        } 
        #endregion

        #region 确认编辑+Edit(ModelTable item, int id)
        /// <summary>
        /// 确认编辑
        /// </summary>
        /// <param name="item">Model实体对象</param>
        /// <param name="id">Model Id</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult Edit(ModelTable item, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    if (entity.ModelTables.Where(m => m.s_ModelName == item.s_ModelName && m.ID != id).Count() > 0)
                    {
                        return Content("模型名称重复.");
                    }
                    if (entity.ModelTables.Where(m => m.s_ModelIdentity == item.s_ModelIdentity && m.ID != id).Count() > 0)
                    {
                        return Content("模型标识重复.");
                    }
                    entity.ModelTables.Attach(item);
                    entity.Entry<ModelTable>(item).State = System.Data.EntityState.Modified;
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
                return Content("填写的模型信息有误.");
            }
        } 
        #endregion

        #region 删除操作+Delete(int? id)
        /// <summary>
        /// 删除操作
        /// </summary>
        /// <param name="id">Model id</param>
        /// <returns></returns>
        [CheckinLogin]
        public ActionResult Delete(int? id)
        {
            if (id > 0)
            {
                try
                {
                    EHECD_WebEntities wb = new EHECD_WebEntities();
                    ModelTable item = wb.ModelTables.Find(id);
                    if (item == null)
                        return Content("该模型不存在或已被删除.");
                    wb.ModelTables.Remove(item);
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

        #region 获取模型字段列表+ItemList(int iTableID = 0, int page = 1, int rows = 10, string sort = "ID", string order = "asc")
        /// <summary>
        /// 获取模型字段列表
        /// </summary>
        /// <param name="iTableID">ModelTable Id</param>
        /// <param name="page">当前页数</param>
        /// <param name="rows">每页的数量</param>
        /// <param name="sort">通过哪个字段来排序</param>
        /// <param name="order">升序还是降序排序</param>
        /// <returns></returns>
        [CheckinLogin]
        public ActionResult ItemList(int iTableID = 0, int page = 1, int rows = 10, string sort = "ID", string order = "asc")
        {
            StringBuilder sResult = new StringBuilder();
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                var query = entity.ModelItems.Where(m => m.i_TableID == iTableID);
                if (order == "asc")
                {
                    switch (sort)
                    {
                        case "s_ItemTitle"
                            : query = query.OrderBy(m => m.s_ItemTitle);
                            break;
                        case "s_ItemName"
                            : query = query.OrderBy(m => m.s_ItemName);
                            break;
                        case "s_Itemtype"
                            : query = query.OrderBy(m => m.s_Itemtype);
                            break;
                        default:
                            query = query.OrderBy(m => m.ID);
                            break;
                    }
                }
                else if (order == "desc")
                {
                    switch (sort)
                    {
                        case "s_ItemTitle"
                            : query = query.OrderByDescending(m => m.s_ItemTitle);
                            break;
                        case "s_ItemName"
                            : query = query.OrderByDescending(m => m.s_ItemName);
                            break;
                        case "s_Itemtype"
                            : query = query.OrderByDescending(m => m.s_Itemtype);
                            break;
                        default:
                            query = query.OrderByDescending(m => m.ID);
                            break;
                    }
                }
                List<ModelItem> list = query.Skip((int)(page - 1) * rows).Take(rows).ToList();
                sResult.Append("{\"total\":" + entity.ModelItems.Count() + ",\"rows\":");
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

        #region 添加操作+AddItem(ModelItem item, int iTableID)
        /// <summary>
        /// 添加操作
        /// </summary>
        /// <param name="item">Model实体对象</param>
        /// <param name="iTableID">Modeltable Id</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult AddItem(ModelItem item, int iTableID)//添加字段
        {
            try
            {
                if (item == null)
                {
                    return Content("请填写字段信息.");
                }
                item.i_TableID = iTableID;
                EHECD_WebEntities entity = new EHECD_WebEntities();
                if (entity.ModelItems.Where(m => m.s_ItemTitle == item.s_ItemTitle && m.i_TableID == item.i_TableID).Count() > 0)
                {
                    return Content("字段标题重复.");
                }
                else if (entity.ModelItems.Where(m => m.s_ItemName == item.s_ItemName && m.i_TableID == item.i_TableID).Count() > 0)
                {
                    return Content("字段英文名称重复.");
                }
                else
                {
                    entity.ModelItems.Add(item);
                }
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

        #region 获取编辑信息+EditItem(int? id)
        /// <summary>
        /// 获取编辑信息
        /// </summary>
        /// <param name="id">Model id</param>
        /// <returns></returns>
        public ActionResult EditItem(int? id)
        {
            ModelItem item = null;
            if (id > 0)
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                item = entity.ModelItems.Find(id);
                if (item != null)
                {
                    JavaScriptSerializer json = new JavaScriptSerializer();
                    return Content(json.Serialize(item));
                }
            }
            return Content("");
        } 
        #endregion

        /// <summary>
        /// 通过模型唯一标示符获取id
        /// </summary>
        /// <returns></returns>
        public ActionResult GetIdByModelIdentity()
        {
            string identity = Request["s_ModelIdentity"];
            ModelTable item = null;
            if (!string.IsNullOrEmpty(identity))
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                item=entity.ModelTables.Find(identity);
            }
            if (item != null)
            {
                return Content(item.ID.ToString());
            }
            else
            {
                return Content("false");
            }
        }
        #region 确认编辑+EditItem(ModelItem item, int id, int iTableID)
        /// <summary>
        /// 确认编辑
        /// </summary>
        /// <param name="item">Model实体独享</param>
        /// <param name="id">Model id</param>
        /// <param name="iTableID">ModelTable Id</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [CheckinLogin]
        public ActionResult EditItem(ModelItem item, int id, int iTableID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    item.i_TableID = iTableID;
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    if (entity.ModelItems.Where(m => m.s_ItemTitle == item.s_ItemTitle && m.i_TableID == item.i_TableID && m.ID != id).Count() > 0)
                    {
                        return Content("字段标题重复.");
                    }
                    if (entity.ModelItems.Where(m => m.s_ItemName == item.s_ItemName && m.i_TableID == item.i_TableID && m.ID != id).Count() > 0)
                    {
                        return Content("字段英文名称重复.");
                    }
                    entity.ModelItems.Attach(item);
                    entity.Entry<ModelItem>(item).State = System.Data.EntityState.Modified;
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
                return Content("填写的字段信息有误.");
            }
        } 
        #endregion

        #region 删除操作+DeleteItem(int? id)
        /// <summary>
        /// 删除操作
        /// </summary>
        /// <param name="id">Model Id</param>
        /// <returns></returns>
        [CheckinLogin]
        public ActionResult DeleteItem(int? id)
        {
            if (id > 0)
            {
                try
                {
                    EHECD_WebEntities wb = new EHECD_WebEntities();
                    ModelItem item = wb.ModelItems.Find(id);
                    if (item == null)
                        return Content("该字段不存在或已被删除.");
                    wb.ModelItems.Remove(item);
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
    }
}
