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

namespace Web.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        [CheckinLogin]
        public ActionResult Index()
        {
            return View();
        }

        #region 注销登录+ Logout()
        /// <summary>
        /// 注销登录
        /// </summary>
        public void Logout()
        {
            Session.Clear();
            Response.Redirect("Login");
        } 
        #endregion

        #region 登录 +Login()
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public ActionResult Login()
        {
            return View();
        } 
        #endregion

        #region 登录提示 +LoginTip()
        /// <summary>
        /// 登录提示
        /// </summary>
        /// <returns></returns>
        public ActionResult LoginTip()
        {

            return Content("<script>window.parent.location.href='../user/login'</script>");
        } 
        #endregion

        #region post提交过来验证登录+Login(string sUserName, string sPassword)
        /// <summary>
        /// post提交过来验证登录
        /// </summary>
        /// <param name="sUserName">用户名</param>
        /// <param name="sPassword">密码</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(string sUserName, string sPassword)
        {
            if (string.IsNullOrEmpty(sUserName) || string.IsNullOrEmpty(sPassword))
            {
                return Content("请输入用户名和密码.");
            }
            EHECD_WebEntities entity = new EHECD_WebEntities();
            List<User> queryList = entity.Users.Where(m => m.s_UserName == sUserName && m.s_Password == sPassword).ToList();
            if (queryList == null || queryList.Count == 0)
            {
                return Content("用户名或密码错误.");
            }
            Session["user"] = queryList[0];
            return Content("yes");
        } 
        #endregion

        [CheckinLogin]
        public ActionResult List()
        {
            return View();
        }

        #region 请求user的分页数据+JsonList(int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        /// <summary>
        /// 请求user的分页数据
        /// </summary>
        /// <param name="page">第几页的</param>
        /// <param name="rows">每页的数据</param>
        /// <param name="sort">通过哪个属性排序</param>
        /// <param name="order">升序还是降序排列</param>
        /// <returns></returns>
        [CheckinLogin]
        public ActionResult JsonList(int page = 1, int rows = 10, string sort = "ID", string order = "asc", string sField = "", string sKeyValue = "")
        {
            StringBuilder sResult = new StringBuilder();
            try
            {
                //创建一个EF上下文
                EHECD_WebEntities entity = new EHECD_WebEntities();
                //id为降序的用户分页查询出来的结合
                var query = entity.Users.OrderByDescending(m => m.ID).Skip((int)(page - 1) * rows).Take(rows);
                if (!string.IsNullOrEmpty(sField) && !string.IsNullOrEmpty(sKeyValue))
                {
                    if (sField == "sUserName")
                    {
                        //过滤出query结合里包含sKeyValue
                        query = query.Where(m => m.s_UserName.Contains(sKeyValue));
                    }
                    else if (sField == "sRealName")
                    {
                        query = query.Where(m => m.s_RealName.Contains(sKeyValue));
                    }

                }
                //排序方法
                if (order == "asc")
                {
                    switch (sort)
                    {
                        case "s_UserName"
                            : query = query.OrderBy(m => m.s_UserName);
                            break;
                        case "i_Status"
                            : query = query.OrderBy(m => m.i_Status);
                            break;
                    }
                }
                else if (order == "desc")
                {
                    switch (sort)
                    {
                        case "s_UserName"
                            : query = query.OrderByDescending(m => m.s_UserName);
                            break;
                        case "i_Status"
                            : query = query.OrderByDescending(m => m.i_Status);
                            break;
                    }
                }
                List<User> list = query.ToList();
                //拼接一个符合easyUi格式的json数据
                sResult.Append("{\"total\":" + entity.Users.Count() + ",\"rows\":");
                //创建出一个序列化对象出来
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

        #region 注册用户+AddUser(User user)
        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="user">userModel对象</param>
        /// <returns></returns>
        [HttpPost]
        [CheckinLogin]
        public ActionResult AddUser(User user)
        {
            user.s_Token = WeixinUser.GenerateToken();
            User sessionUser=Session["user"] as User;
            if (sessionUser!=null)
            {
                user.s_Url = WeixinUser.GenerateUrl(user.s_UserName);
            }
            // 尝试注册用户
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                //判断注册用户是否重复
                if (entity.Users.Where(m => m.s_UserName == user.s_UserName).Count() > 0)
                {
                    return Content("用户名重复.");
                }
                else
                {
                    entity.Users.Add(user);
                }
                entity.SaveChanges();
                
                return Content("yes");
            }
            catch (Exception e)
            {
                //写入错误日志
                TTracer.WriteLog_App(e.ToString());
            }
            return Content("添加失败.");
        } 
        #endregion

        #region 开始编辑+Edit(int? id)
        /// <summary>
        /// 开始编辑
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            User item = null;
            if (id > 0)
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                //查找user的一个对象By Id
                item = entity.Users.Find(id);
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

        #region 确认编辑+Edit(User user, int id, bool bAllowAccessAdmin = false)
        /// <summary>
        /// 确认编辑
        /// </summary>
        /// <param name="user">user实体对象</param>
        /// <param name="id">用户id</param>
        /// <param name="bAllowAccessAdmin">是否允许编辑admin用户</param>
        /// <returns></returns>
        [HttpPost]
        [CheckinLogin]
        public ActionResult Edit(User user, int id, bool bAllowAccessAdmin = false)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (!bAllowAccessAdmin && user.s_UserName == "admin")
                    {
                        return Content("不能编辑admin用户.");
                    }
                    EHECD_WebEntities entity = new EHECD_WebEntities();
                    if (entity.Users.Where(m => m.s_UserName == user.s_UserName && m.ID != id).Count() > 0)
                    {
                        return Content("用户名重复.");
                    }
                    //把user实体对象附加到ef里
                    entity.Users.Attach(user);
                    //标记为修改
                    entity.Entry<User>(user).State = System.Data.EntityState.Modified;
                    //保存
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
                return Content("填写的用户信息有误.");
            }
        } 
        #endregion

        #region 删除操作+Delete(int? id)
        /// <summary>
        /// 删除操作
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        [CheckinLogin]
        public ActionResult Delete(int? id)
        {
            if (id > 0)
            {
                try
                {
                    EHECD_WebEntities wb = new EHECD_WebEntities();
                    //通过id把这个user对象查找出来
                    User item = wb.Users.Find(id);
                    if (item == null)
                        return Content("该用户不存在或已被删除.");
                    if (TString.Compare2String(item.s_UserName, "admin"))
                        return Content("admin用户不能被删除.");
                    //删除
                    wb.Users.Remove(item);
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

        #region PC端模糊查询VIP账号
        /// <summary>
        /// 根据用户名和金卡号查询
        /// </summary>
        /// <returns></returns>/User/QueryList
        public JsonResult QueryList(string sUserName, string VIPCode)
        {
            try
            {
                EHECD_WebEntities entity = new EHECD_WebEntities();
                //获取查询列表
                List<RegistAndLogin> register = null;
                if (string.IsNullOrEmpty(sUserName)&&!string.IsNullOrEmpty(VIPCode))
                {
                    register = entity.RegistAndLogins.Where(m =>m.s_VIPCode.Contains(VIPCode)).OrderByDescending(m => m.ID)
                                       .ToList();
                }
                else if (string.IsNullOrEmpty(VIPCode)&&!string.IsNullOrEmpty(sUserName))
                {
                    register = entity.RegistAndLogins.Where(m => m.s_UserName.Contains(sUserName)).OrderByDescending(m => m.ID)
                                                          .ToList();
                }
                else if (!string.IsNullOrEmpty(sUserName) && !string.IsNullOrEmpty(VIPCode))
                {
                    register = entity.RegistAndLogins.Where(m => m.s_UserName.Contains(sUserName)
                        && m.s_VIPCode.Contains(VIPCode)).OrderByDescending(m => m.ID)
                        .ToList();
                }
                //获取三个最近新注册的会员
                List<RegistAndLogin> newMember = entity.RegistAndLogins.OrderByDescending(m => m.ID).Skip(0).Take(3).ToList();
                return Json(new { List = register, memberlist = newMember});
            }
            catch (Exception e)
            {
                TTracer.WriteLog_App(e.ToString());
                return null;
            }
        }
        #endregion
    }
}
