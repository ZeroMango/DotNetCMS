using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Commons.Collections;
using NVelocity;
using NVelocity.App;
using NVelocity.Context;
using NVelocity.Runtime;

namespace Web.Code
{
    public class NVelocityHelper
    {
        private VelocityEngine velocity = null;
        private IContext context = null;
        /// 
        /// 无参数构造函数
        /// 
        public NVelocityHelper()
        { }

        ///    
        /// 构造函数
        ///templatDir模板文件夹路径  
        ///
        public NVelocityHelper(string templateDir)
        {
            Init(templateDir);
        }

        /// 
        /// 初始化NVelocity模块
        /// <param name="templateDir">模版文件所在的物理文件夹</param>
        /// 模板文件夹路径
        public void Init(string templateDir)
        {
            //创建VelocityEngine实例对象
            velocity = new VelocityEngine();

            //使用设置初始化VelocityEngine
            ExtendedProperties props = new ExtendedProperties();
            props.AddProperty(RuntimeConstants.RESOURCE_LOADER, "file");
            props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, templateDir);
            props.AddProperty(RuntimeConstants.INPUT_ENCODING, "utf-8");
            props.AddProperty(RuntimeConstants.OUTPUT_ENCODING, "utf-8");
            velocity.Init(props);

            //为模板变量赋值
            context = new VelocityContext();
        }

        /// 给模板变量赋值
        /// 模板变量
        ///模板变量值 
        public void Put(string key, object value)
        {
            if (context == null)
            {
                context = new VelocityContext();
                context.Put(key, value);
            }

            else
            {
                context.Put(key, value);
            }
        }

        public IContext Context
        {
            set { context = value; }
            get { return context; }
        }

        public StringWriter GetResultString(string templateFileName)
        {
            //从文件中读取模板
            Template template = velocity.GetTemplate(templateFileName);
            //合并模板
            StringWriter writer = new StringWriter();
            template.Merge(context, writer);
            return writer;
        }

        /// <summary>
        /// 显示结果内容
        /// </summary>
        /// <param name="templatFileName"></param>
        public void Display(string templateFileName)
        {
            StringWriter writer = GetResultString(templateFileName);
            //输出
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write(writer.ToString());
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }

        ///
        ///生成 .html 文件, 生成成功返回 true 不成功返回 false
        ///<summary>
        /// 生成.html 文件
        /// </summary
        /// <param name="fileName">
        /// .html的文件名(*.html)
        /// </param>
        /// <param name="targetFolder">
        /// .html文件的存放路径, 注意：这里需要的是服务器端的一个绝对路径,Server.MapPaht(para)方法获得，或直接给出.
        /// </param>
        /// <param name="templatFileName">模版文件名</param>
        public bool GenerateShtml(string templateFileName, string targetFolder, string fileName)
        {
            StringWriter writer = GetResultString(templateFileName);
            try
            {
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }
                File.WriteAllText(targetFolder + "\\" + fileName, writer.ToString(), new System.Text.UTF8Encoding(true));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}