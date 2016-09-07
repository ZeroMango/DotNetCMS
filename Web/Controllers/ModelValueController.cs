using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using Common;
using Web.Server.Models;

namespace Web.Controllers
{
    public class ModelValueController : Controller
    {
        #region Modelvalue 首页取8条数据到前台+Index(int? iTableID)
        //
        // GET: /ModelValue/
        /// <summary>
        /// Modelvalue 首页取8条数据到前台
        /// </summary>
        /// <param name="iTableID">Model Table id</param>
        /// <returns></returns>
        public ActionResult Index(int? iTableID)
        {
            if (iTableID == null || iTableID == 0)
            {
                return Content("");
            }
            try
            {
                ViewData["iTableID"] = iTableID;
                EHECD_WebEntities entity = new EHECD_WebEntities();
                List<ModelItem> itemList = entity.ModelItems.Where(m => m.i_TableID == iTableID).Take(8).ToList();
                StringBuilder sColumns = new StringBuilder();
                sColumns.Append("[[");
                foreach (ModelItem item in itemList)
                {
                    sColumns.Append("{ field: '" + item.s_ItemName + "', title: '" + item.s_ItemTitle + "', width: " + GetColumnWidth(item.s_Itemtype) + " }");
                    if (item.ID != itemList.Last().ID)
                    {//最后一个不加逗号
                        sColumns.Append(",");
                    }
                }
                sColumns.Append("]]");
                ViewData["columns"] = sColumns.ToString();
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
            }
            return View();
        } 
        #endregion

        #region 获取模型内容列表+List(int iTableID, int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        /// <summary>
        /// 获取模型内容列表
        /// </summary>
        /// <param name="iTableID">Model table id</param>
        /// <param name="page">当前页数</param>
        /// <param name="rows">每页的数量</param>
        /// <param name="sort">通过哪个字段排序</param>
        /// <param name="order">升序还是降序排序</param>
        /// <param name="sField">ModelValue字段</param>
        /// <param name="sKeyValue">包含的关键字</param>
        /// <returns></returns>
        public ActionResult List(int iTableID, int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        {
            StringBuilder sResult = new StringBuilder();
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                //var query = entity.ModelItem_Value.Where(m => m.i_TableID == iTableID);
                var query = entity.ModelItem_Value.Where(m => m.i_TableID == iTableID);
                if (!string.IsNullOrEmpty(sKeyValue))
                {
                    query = query.Where(m => m.s_ContentNoHtml.Contains(sKeyValue));
                }
                List<ModelItem_Value> list = query.OrderBy(m => m.i_Order).ThenByDescending(m => m.ID).Skip((int)(page - 1) * rows).Take(rows).ToList();
                sResult.Append("{\"total\":" + query.Count() + ",\"rows\":[");
                foreach (ModelItem_Value item in list)
                {
                    XmlDocument xContent = TXml.LoadFromXml(item.s_Content);
                    if (xContent == null)
                    {
                        continue;
                    }
                    sResult.Append("{");
                    sResult.Append("\"ID\":" + item.ID + ",");
                    XmlNode xOutput = TXmlResults.getOutput(xContent);
                    foreach (XmlNode node in xOutput.ChildNodes)
                    {
                        sResult.Append("\"" + node.Name + "\":\"" + TXml.getCData(node) + "\",");
                    }
                    sResult.Append("\"s_AddTime\":\"" + item.s_AddTime + "\"");
                    sResult.Append("},");
                }
                if (sResult[sResult.Length - 1] == ',')
                {
                    sResult.Remove(sResult.Length - 1, 1);
                }
                sResult.Append("]}");
            }
            catch (Exception ex)
            {
                TTracer.WriteLog_App(ex.ToString());
            }
            return Content(sResult.ToString());
        } 
        #endregion

        public ActionResult GetList(int iTableid)
        {

            return null;
        }

        public ActionResult SearchModel()
        {
            return View();
        }

        #region 设置列高度+GetColumnWidth(string sItemType)
        /// <summary>
        /// 设置列高度
        /// </summary>
        /// <param name="sItemType">文本框类型</param>
        /// <returns></returns>
        private int GetColumnWidth(string sItemType)
        {
            int iResult = 150;
            if (sItemType == "多行文本框")
            {
                iResult = 300;
            }
            return iResult;
        } 
        #endregion


        #region 通过modeltable id 获取数据ModelValue数据+ActionResult Access(int? iTableID)
        /// <summary>
        /// 通过modeltable id 获取数据ModelValue数据
        /// </summary>
        /// <param name="iTableID">ModelTable Id</param>
        /// <returns></returns>
        public ActionResult Access(int? iTableID)
        {
            List<ModelItem> itemList = new List<ModelItem>();
            try
            {
                ViewData["iTableID"] = iTableID == null ? 0 : iTableID;
                EHECD_WebEntities entity = new EHECD_WebEntities();
                itemList = entity.ModelItems.Where(m => m.i_TableID == iTableID).ToList();
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
            }
            return View(itemList);
        } 
        #endregion

        #region 添加模型内容
        /// <summary>
        /// 添加模型内容
        /// </summary>
        /// <param name="iTableID">ModelTable id</param>
        /// <returns></returns>
        public ActionResult Add(int iTableID)
        {
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                string sType = Request["type"];
                if (sType == "messageBox")
                {
                    string sCode = Session["VerifyCode"].ToString();
                    if (sCode != Request["validateCode"])
                    {
                        return Content("验证码错误!");
                    }
                }
                else { }
                ModelItem_Value item = new ModelItem_Value();
                item.i_Order = 0;
                item.i_TableID = iTableID;
                item.s_AddTime = DateTime.Now.ToString();
                XmlDocument xContent = TXml.Create();
                XmlNode xOutput = TXmlResults.getOutput(xContent);

                //查询已注册会员的人数
                int listcount = entity.RegistAndLogins.Count(m => m.s_UserName != "null");
                int VipCount = 10000;//初始化VIPcode
                int VIPCode = VipCount + listcount + 1;

                if (sType == "messageBox1" || sType == "messageBox2")
                {
                    for (int i = 0; i < Request.Form.Count - 2; i++)
                    {
                        TXml.addCData(xOutput, Request.Form.AllKeys[i], Request.Form[i].ToString());
                        item.s_ContentNoHtml += TString.NoHTML(Request.Form[i].ToString());
                    }
                    TXml.addCData(xOutput, "VIPCode", VIPCode.ToString());
                    item.s_ContentNoHtml += TString.NoHTML(VIPCode.ToString());
                }
                else
                {
                    for (int i = 0; i < Request.Form.Count; i++)
                    {

                        TXml.addCData(xOutput, Request.Form.AllKeys[i], Request.Form[i].ToString());
                        item.s_ContentNoHtml += TString.NoHTML(Request.Form[i].ToString());
                    }
                    TXml.addCData(xOutput, "VIPCode", VIPCode.ToString());
                    item.s_ContentNoHtml += TString.NoHTML(VIPCode.ToString());
                }
                item.s_Content = xContent.OuterXml;
                entity.ModelItem_Value.Add(item);
                if (sType == "messageBox1" || iTableID == 48)
                {
                    var name = Request.Form[0];
                    var psw = Request.Form[1];
                    string sAdress="";
                    if (Request["sAdderss"] != null)
                    {
                        sAdress = Request["sAdderss"].ToString();
                    }
                    var namePsw = name + psw;
                    RegistAndLogin regist = new RegistAndLogin();
                    regist.s_UserName = name;
                    regist.s_UserPassword = psw;
                    if (entity.RegistAndLogins.Where(n => (n.s_UserName + n.s_UserPassword) == namePsw).Count() == 0)
                    {
                        if (entity.RegistAndLogins.Where(m => m.s_UserName == name).Count() > 0)
                        {
                            return Content("该用户名已经被注册.");
                        }
                        else
                        {
                            regist.s_UserName = name;
                            regist.s_UserPassword = psw;
                            regist.s_VIPCode = VIPCode.ToString();
                            regist.sAdderss = sAdress;
                            entity.RegistAndLogins.Add(regist);
                        }
                    }
                    else
                    {
                        return Content("该用户名已经被注册.");
                    }
                }
                else if (sType == "messageBox2")
                {
                    //登录
                    string name = Request.Form[0];
                    var password = Request.Form[1];
                    
                    List<RegistAndLogin> queryList = entity.RegistAndLogins.Where(m => m.s_UserName == name && m.s_UserPassword == password).ToList();
                    if (queryList == null || queryList.Count == 0)
                    {
                        return Content("用户名或密码错误.");
                    }
                }
                entity.SaveChanges();
                return Content("yes");
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
                return Content(e.ToString());
            }  
        } 
        #endregion

        #region 获取编辑模型内容+Edit(int? id)
        /// <summary>
        /// 获取编辑模型内容
        /// </summary>
        /// <param name="id">ModelValue id</param>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            ModelItem_Value item = null;
            if (id > 0)
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                item = entity.ModelItem_Value.Find(id);
                if (item != null)
                {
                    StringBuilder sResult = new StringBuilder();
                    sResult.Append("{");
                    XmlDocument xContent = TXml.LoadFromXml(item.s_Content);
                    if (xContent == null)
                    {
                        return Content("");
                    }
                    XmlNode xOutput = TXmlResults.getOutput(xContent);
                    foreach (XmlNode node in xOutput.ChildNodes)
                    {
                        sResult.Append("\"" + node.Name + "\":\"" + TXml.getCData(node) + "\",");
                    }
                    if (sResult[sResult.Length - 1] == ',')
                    {
                        sResult.Remove(sResult.Length - 1, 1);
                    }
                    sResult.Append("}");
                    return Content(sResult.ToString());
                }
            }
            return Content("");
        } 
        #endregion

        #region 确认编辑模型内容+Edit(int id)
        /// <summary>
        /// 确认编辑模型内容
        /// </summary>
        /// <param name="id">ModelTable id</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int id,string VIPCode)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ModelItem_Value item = new ModelItem_Value();
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    ModelItem_Value originalItem = entity.ModelItem_Value.Find(id);
                    if (originalItem == null)
                    {
                        return Content("所编辑的信息不存在或已被删除.");
                    }
                    XmlDocument xContent = TXml.Create();
                    XmlNode xOutput = TXmlResults.getOutput(xContent);
                    for (int i = 0; i < Request.Form.Count; i++)
                    {
                        TXml.addCData(xOutput, Request.Form.AllKeys[i], Request.Form[i].ToString());
                        item.s_ContentNoHtml += TString.NoHTML(Request.Form[i].ToString());
                    }
                    TXml.addCData(xOutput, "VIPCode", VIPCode.ToString());
                    item.s_ContentNoHtml += TString.NoHTML(VIPCode.ToString());

                    item.s_Content = xContent.OuterXml;
                    originalItem.s_Content = item.s_Content;
                    originalItem.s_ContentNoHtml = item.s_ContentNoHtml;
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
                return Content("填写的信息有误.");
            }
        } 
        #endregion

        #region 删除模型内容+Delete(int? id)
        /// <summary>
        /// 删除模型内容
        /// </summary>
        /// <param name="id">ModelTable id</param>
        /// <returns></returns>
        public ActionResult Delete(int? id)
        {
            if (id > 0)
            {
                try
                {
                    EHECD_WebEntities wb = new EHECD_WebEntities();
                    ModelItem_Value item = wb.ModelItem_Value.Find(id);
                    if (item == null)
                        return Content("该信息不存在或已被删除.");
                    wb.ModelItem_Value.Remove(item);
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
