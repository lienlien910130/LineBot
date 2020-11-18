using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomColumnLibrary;
using LineBOT.Models.DataBase;

namespace LineBOT.Models.LineDevelopers
{
    /// <summary>
    /// LINE事件使用的房間屬性
    /// </summary>
    public class LineGRInfoModels : DataBaseModels
    {
        public static string[] Class = { "room", "group" };

        /// <summary>
        /// 聊天室ID
        /// </summary>
        [Column(Name = "聊天室ID")]
        public string RoomID { get; set; }
        /// <summary>
        /// 群組ID
        /// </summary>
        [Column(Name = "群組ID")]
        public string GroupID { get; set; }
        /// <summary>
        /// 新增時間
        /// </summary>
        [Column(Name = "新增時間")]
        public DateTime? CreateTime { get; set; }

        public static string TableName => "LineGRInfo";

        internal override string GetTableName => "LineGRInfo";
    }
}
