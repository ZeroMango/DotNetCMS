using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Server.Code;

namespace Web.Controllers
{
    public class CodeController : Controller
    {
        //
        // GET: /Code/

        public ActionResult GetValidateCode()
        {
            ValidateCode validateCode = new ValidateCode();
            string code = validateCode.CreateValidateCode(5);
            Session["VerifyCode"] = code;
            byte[] bytes = validateCode.CreateValidateGraphic(code);
            return File(bytes, @"image/jpeg");
        }

    }
}
