using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Web.App_Start
{
    public class WeixinUser
    {
        /// <summary>
        /// 先取得guid，再通过加密最后得倒加密后的Token （该Token是用于绑定公众账号）
        /// 是存入数据库中的
        /// </summary>
        /// <returns></returns>
        public static string GenerateToken()
        {
            var sGuid = Guid.NewGuid().ToString().Replace("-", "");
            byte[] inputData = System.Text.Encoding.Default.GetBytes(sGuid);
            byte[] data = System.Security.Cryptography.MD5.Create().ComputeHash(inputData);
            StringBuilder sResult = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sResult.Append(data[i].ToString("X2"));
            }

            return sResult.ToString();
        }

        /// <summary>
        /// 得到绑定的URL
        /// </summary>
        /// <returns></returns>
        public static string GenerateUrl(string id)
        {
            return string.Format("/WeiXin/Index/{0}", id);
        }

        /// <summary>
        /// 得到公众号上的微信号， 开发者的 AppId和 AppSecret
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sWeiXinAccount"></param>
        /// <param name="sAppId"></param>
        /// <param name="sAppSecret"></param>
        /// <returns></returns>
        public static bool GetUserInfo(long id, out string sWeiXinAccount, out string sAppId, out string sAppSecret)
        {
            sWeiXinAccount = sAppId = sAppSecret = string.Empty;
            bool bIsSucess = false;
            Web.Code.BaseDAL<Web.Server.Models.User> userBase = new Code.BaseDAL<Server.Models.User>();
            Web.Server.Models.User user=userBase.Where(u => u.ID == id).FirstOrDefault();
            if (user!=null)
            {
                sWeiXinAccount = user.s_WeixinAccount;
                sAppId = user.s_AppId;
                sAppSecret = user.s_AppSecret;
                bIsSucess = true;
            }
            return bIsSucess;
        }
    }
}