using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Server.Models
{
    public class PagedData
    {
        public PagedData() { }

        public PagedData(int pageIndex, int pageSize)
        {
            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
        }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex = 1;

        /// <summary>
        /// 页容量
        /// </summary>
        public int PageSize = 10;

        /// <summary>
        /// 总行数
        /// </summary>
        public int RowCount;

        /// <summary>
        /// 总页数 - 根据总行数计算得知
        /// </summary>
        public int PageCount
        {
            get
            {
                decimal pageCount = Math.Ceiling(Convert.ToDecimal(RowCount) / Convert.ToDecimal(PageSize));
                return (int)pageCount;
            }
        }

        /// <summary>
        /// 当前页码 的数据 集合
        /// </summary>
        public object ListData;
    }
}