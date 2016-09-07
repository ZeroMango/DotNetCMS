using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.App_Start
{
    /// <summary>
    /// 主菜单
    /// </summary>
    public class Menu
    {
        private List<object> _button;
        /// <summary>
        /// 主菜单列表
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<object> button
        {
            get
            {
                if (null == _button)
                {
                    _button = new List<object>();
                }
                return _button;
            }
            set
            {
                _button = value;
            }
        }
    }
}