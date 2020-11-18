using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomColumnLibrary;
using LineBOT.Models.DataBase;

namespace LineBOT.Models.LineDevelopers
{
    /// <summary>
    /// 使用者相關資訊
    /// </summary>
    public class LineUserInfoModels : DataBaseModels
    {
        public static string[] Class = { "user" };

        /// <summary>
        /// 使用者ID
        /// </summary>
        [Column(Name = "使用者ID")]
        public string UserID { get; set; }
        /// <summary>
        /// 使用者名稱
        /// 需透過設定方式才能填入這欄位值
        /// 透過LineEvent方式無法取得預設名稱
        /// </summary>
        [Column(Name = "使用者名稱")]
        public string Name { get; set; }
        /// <summary>
        /// 新增時間
        /// </summary>
        [Column(Name = "新增時間")]
        public DateTime? CreateTime { get; set; }

        public static string TableName => "LineUserInfo";

        internal override string GetTableName => "LineUserInfo";
    }
}
