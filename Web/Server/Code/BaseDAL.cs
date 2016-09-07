using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Web.Server.Models;

namespace Web.Code
{
    public class BaseDAL<T> where T:class
    {
        //1.创建EF上下文
        EHECD_WebEntities db = new EHECD_WebEntities();

        public BaseDAL()
        {
            //关闭 ef 检查
            db.Configuration.ValidateOnSaveEnabled = false;
        }

        #region 0.0 批量更新EF容器数据到数据库 +int SaveChanges()
        /// <summary>
        /// 0.0 批量更新EF容器数据到数据库
        /// </summary>
        /// <returns>返回受影响行数</returns>
        public int SaveChanges()
        {
            return db.SaveChanges();
        } 
        #endregion

        #region 1.0 新增方法 +void Add(T model)
        /// <summary>
        /// 1.0 新增方法
        /// </summary>
        /// <param name="model"></param>
        public void Add(T model)
        {
            //1.直接通过EF上下文的 Set方法 获取一个 针对于 T类 做操作的 DbSet对象
            //var dbSet = db.Set<T>();
            //dbSet.Add(model);
            db.Set<T>().Add(model);
        } 
        #endregion

        #region 2.0 删除方法 +void Delete(T model)
        /// <summary>
        /// 2.0 删除方法
        /// </summary>
        /// <param name="model"></param>
        public void Delete(T model)
        {
            DbEntityEntry entry = db.Entry<T>(model);
            entry.State = System.Data.EntityState.Deleted;
        } 
        #endregion

        #region 2.1 条件删除方法 +void DeleteBy(System.Linq.Expressions.Expression<Func<T, bool>> delWhere)
        /// <summary>
        /// 2.1 条件删除方法
        /// </summary>
        /// <param name="delWhere">要删除的元素查询条件</param>
        public void DeleteBy(System.Linq.Expressions.Expression<Func<T, bool>> delWhere)
        {
            var delList = db.Set<T>().Where(delWhere);
            foreach (T model in delList)
            {
                Delete(model);
            }
        }
        #endregion

        #region 3.0 修改实体 + void Modify(T model, params string[] propertyNames)
        /// <summary>
        /// 3.0 修改实体
        /// </summary>
        /// <param name="model"></param>
        /// <param name="propertyNames"></param>
        public void Modify(T model, params string[] propertyNames)
        {
            DbEntityEntry entry = db.Entry<T>(model);
            entry.State = System.Data.EntityState.Unchanged;
            foreach (string proName in propertyNames)
            {
                entry.Property(proName).IsModified = true;
            }
        } 
        #endregion

        #region 4.0 查询方法 +IQueryable<T> Where(Expression<Func<T, bool>> whereLambda)
        /// <summary>
        /// 4.0 查询方法
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public IQueryable<T> Where(Expression<Func<T, bool>> whereLambda)
        {
            return db.Set<T>().Where(whereLambda);
        } 
        #endregion

        #region 4.1 查询方法 -带排序 +IQueryable<T> WhereOrder<TKey>
        /// <summary>
        /// 4.1 查询方法 -带排序
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="whereLambda"></param>
        /// <param name="keySelector"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public IQueryable<T> WhereOrder<TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> keySelector, bool isAsc = true)
        {
            if (isAsc)
                return db.Set<T>().Where(whereLambda).OrderBy(keySelector);
            else
                return db.Set<T>().Where(whereLambda).OrderByDescending(keySelector);
        } 
        #endregion

        #region 4.2 查询方法 -带Include +IQueryable<T> WhereInclude
        /// <summary>
        /// 4.2 查询方法 -带Include
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <param name="includePropertyNames">要进行连接查询的 属性名</param>
        /// <returns></returns>
        public IQueryable<T> WhereInclude(Expression<Func<T, bool>> whereLambda, params string[] includePropertyNames)
        {
            DbQuery<T> dbQuery = db.Set<T>();
            foreach (string includeName in includePropertyNames)
            {
                dbQuery = dbQuery.Include(includeName);
            }
            return dbQuery.Where(whereLambda);

            //DbQuery<T> dbSet = (DbQuery<T>)db.Set<T>().Where(whereLambda);
            //foreach (string includeName in includePropertyNames)
            //{
            //        dbSet = dbSet.Include(includeName);
            //}
            //return dbSet;
        }
        #endregion

        #region 4.3 查询方法 -带Include 和 排序 +IQueryable<T> WhereInclude<TKey>
        /// <summary>
        /// 4.3 查询方法 -带Include 和 排序
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="whereLambda"></param>
        /// <param name="keySelector"></param>
        /// <param name="isAsc"></param>
        /// <param name="includePropertyNames"></param>
        /// <returns></returns>
        public IQueryable<T> WhereInclude<TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> keySelector, bool isAsc = true, params string[] includePropertyNames)
        {
            DbQuery<T> dbQuery = db.Set<T>();
            foreach (string includeName in includePropertyNames)
            {
                dbQuery = dbQuery.Include(includeName);
            }
            IQueryable<T> query = dbQuery.Where(whereLambda);
            if (isAsc)
                return query.OrderBy(keySelector);
            else
                return query.OrderByDescending(keySelector);
        } 
        #endregion
        #region 4.4 查询方法 - 分页+Include+排序 + void WherePaged<TKey>
        /// <summary>
        /// 4.4 查询方法 - 分页+Include+排序
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="pagedData"></param>
        /// <param name="whereLambda"></param>
        /// <param name="keySelector"></param>
        /// <param name="isAsc"></param>
        /// <param name="includePropertyNames"></param>
        public void WherePaged<TKey>(Server.Models.PagedData pagedData, Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> keySelector, bool isAsc = true, params string[] includePropertyNames)
        {
            //0.获取 要操作的 数据表 对应的查询对象
            DbQuery<T> dbQuery = db.Set<T>();
            //1.include 属性
            foreach (string includeName in includePropertyNames)
            {
                dbQuery = dbQuery.Include(includeName);
            }
            IOrderedQueryable<T> orderQuery = null;
            //2.排序
            if (isAsc) orderQuery = dbQuery.OrderBy(keySelector);
            else orderQuery = dbQuery.OrderByDescending(keySelector);
            //3.分页查询
            pagedData.ListData = orderQuery.Where(whereLambda).Skip((pagedData.PageIndex - 1) * pagedData.PageSize).Take(pagedData.PageSize).ToList();
            //4.获取总行数
            pagedData.RowCount = orderQuery.Where(whereLambda).Count();
        }
        #endregion

    }
}
