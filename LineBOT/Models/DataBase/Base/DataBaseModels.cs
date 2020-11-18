
using System;
using CustomColumnLibrary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LineBOT.Models.DataBase
{
    /// <summary>
    /// 資料庫資料表基層參考
    /// </summary>
    public abstract class DataBaseModels
    {
        /// <summary>
        /// 是否已刪除
        /// </summary>
        [Display(Name = "IsDelete")]
        [Column(Name = "刪除")]
        public virtual bool IsDelete { get; set; }

        /// <summary>
        /// 資料編號(資料表必加)
        /// 此欄位須放置在最下方，已面執行映射時，讀取Columns.Last()抓不到ID
        /// </summary>
        [Display(Name = "ID")]
        [Column(Name = "編號")]
        public virtual string ID { get; set; }

        /// <summary>
        /// 關聯資料表
        /// </summary>
       // internal virtual List<AssociationLinkTable> LinkList { get; set; }

        internal virtual string GetTableName => "";

        /// <summary>
        /// 角色清單
        /// </summary>
        internal virtual List<Claim> Roles => default;
    }
}
