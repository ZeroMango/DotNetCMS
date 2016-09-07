using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http.Filters;
using System.Web.Mvc;

namespace Web.Code
{
    public class CheckinLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["user"] == null)
            {
                filterContext.Result = new RedirectResult("/User/LoginTip");
                return;
            }
        }
    }

    
}