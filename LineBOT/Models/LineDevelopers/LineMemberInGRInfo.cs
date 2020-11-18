using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomColumnLibrary;
using LineBOT.Models.DataBase;

namespace LineBOT.Models.LineDevelopers
{
    /// <summary>
    /// 聊天室、群組內成員狀況
    /// </summary>
    public class LineMemberInGRInfo : DataBaseModels
    {
        /// <summary>
        /// 群組ID
        /// </summary>
        [Column(Name = "群組ID")]
        public string GroupID { get; set; }
        /// <summary>
        /// 聊天室ID
        /// </summary>
        [Column(Name = "聊天室ID")]
        public string RoomID { get; set; }
        /// <summary>
        /// 成員ID
        /// </summary>
        [Column(Name = "成員ID")]
        public string UserID { get; set; }
        /// <summary>
        /// 是否離開
        /// </summary>
        [Column(Name = "是否離開")]
        public bool IsLeave { get; set; }
        /// <summary>
        /// 加入時間
        /// </summary>
        [Column(Name = "加入時間")]
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 離開時間
        /// </summary>
        [Column(Name = "離開時間")]
        public DateTime? LeaveTime { get; set; }

        public static string TableName => "LineMemberInGRInfo";

        internal override string GetTableName => "LineMemberInGRInfo";
    }
}
