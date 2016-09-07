using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Web.Controllers
{
    public class UploadController : Controller
    {
        #region 上传+UploadRealName()
        //
        // GET: /Upload/

        /// <summary>
        /// 服务器上保存的文件名，和客户端上传上来的名字保持一致
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadRealName()
        {
            String aspxUrl = string.Empty;

            //文件保存目录路径
            String savePath = "/attached/";

            //文件保存目录URL
            String saveUrl = aspxUrl + "/attached/";

            //定义允许上传的文件扩展名
            Hashtable extTable = new Hashtable();
            extTable.Add("image", "gif,jpg,jpeg,png,bmp");
            extTable.Add("flash", "swf,flv");
            extTable.Add("media", "swf,flv,mp3,wav,wma,wmv,mid,avi,mpg,asf,rm,rmvb,exe,zip,rar");
            extTable.Add("file", "doc,docx,xls,xlsx,ppt,htm,html,txt,gz,bz2");

            //最大文件大小
            int maxSize = 2048 * 1000000;

            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            if (imgFile == null || imgFile.ContentLength == 0)
            {
                return Content("请选择文件.");
            }

            String dirPath = Server.MapPath(savePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            String dirName = Request.QueryString["dir"];
            if (String.IsNullOrEmpty(dirName))
            {
                dirName = "image";
            }
            if (!extTable.ContainsKey(dirName))
            {
                return Content("目录名不正确.");
            }

            String fileName = imgFile.FileName;
            String fileExt = Path.GetExtension(fileName).ToLower();

            if (imgFile.InputStream == null || imgFile.InputStream.Length > maxSize)
            {
                return Content("上传文件大小超过限制.");
            }

            if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(((String)extTable[dirName]).Split(','), fileExt.Substring(1).ToLower()) == -1)
            {
                return Content("上传文件扩展名是不允许的扩展名.");
            }

            //创建文件夹
            dirPath += dirName + "/";
            saveUrl += dirName + "/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            String ymd = DateTime.Now.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);
            dirPath += ymd + "/";
            saveUrl += ymd + "/";
         
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            //String newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo) + fileExt;
            string newFileName = fileName;
            String filePath = dirPath + newFileName;

            imgFile.SaveAs(filePath);

            String fileUrl = saveUrl + newFileName;

            Hashtable hash = new Hashtable();
            hash["error"] = 0;
            hash["url"] = fileUrl;
            JavaScriptSerializer json = new JavaScriptSerializer();
            return Content(json.Serialize(hash));
        } 
        #endregion

        #region 上传+Upload()
        //
        // GET: /Upload/

        /// <summary>
        /// 上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Upload()
        {
            String aspxUrl = string.Empty;

            //文件保存目录路径
            String savePath = "/attached/";

            //文件保存目录URL
            String saveUrl = aspxUrl + "/attached/";

            //定义允许上传的文件扩展名
            Hashtable extTable = new Hashtable();
            extTable.Add("image", "gif,jpg,jpeg,png,bmp");
            extTable.Add("flash", "swf,flv");
            extTable.Add("media", "swf,flv,mp3,wav,wma,wmv,mid,avi,mpg,asf,rm,rmvb");
            extTable.Add("file", "doc,docx,xls,xlsx,ppt,htm,html,txt,gz,bz2,exe,rar,zip,gif,jpg,jpeg,png,bmp");

            //最大文件大小
            int maxSize = 2048 * 1000000;

            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            if (imgFile == null || imgFile.ContentLength == 0)
            {
                return Content("请选择文件.");
            }

            String dirPath = Server.MapPath(savePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            String dirName = Request.QueryString["dir"];
            if (String.IsNullOrEmpty(dirName))
            {
                dirName = "image";
            }
            if (!extTable.ContainsKey(dirName))
            {
                return Content("目录名不正确.");
            }

            String fileName = imgFile.FileName;
            String fileExt = Path.GetExtension(fileName).ToLower();

            if (imgFile.InputStream == null || imgFile.InputStream.Length > maxSize)
            {
                return Content("上传文件大小超过限制.");
            }

            if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(((String)extTable[dirName]).Split(','), fileExt.Substring(1).ToLower()) == -1)
            {
                return Content("上传文件扩展名是不允许的扩展名.");
            }

            //创建文件夹
            dirPath += dirName + "/";
            saveUrl += dirName + "/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            String ymd = DateTime.Now.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);
            dirPath += ymd + "/";
            saveUrl += ymd + "/";

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            String newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo) + fileExt;
            //string newFileName = fileName;
            String filePath = dirPath + newFileName;

            imgFile.SaveAs(filePath);

            String fileUrl = saveUrl + newFileName;

            Hashtable hash = new Hashtable();
            hash["error"] = 0;
            hash["url"] = fileUrl;
            JavaScriptSerializer json = new JavaScriptSerializer();
            return Content(json.Serialize(hash));
        }
        #endregion
    }
}
