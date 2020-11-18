using CustomColumnLibrary;
using LineBOT.Models.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBOT.Models.LineDevelopers
{
    public class LinePushMessage : DataBaseModels
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        [Column(Name = "使用者ID")]
        public string ToUser { get; set; }
        /// <summary>
        /// 類別
        /// </summary>
        [Column(Name = "類別")]
        public string Type { get; set; }
        /// <summary>
        /// 傳送訊息種類
        /// </summary>
        [Column(Name = "傳送訊息種類")]
        public string MessageType { get; set; }
        /// <summary>
        /// 請求來源的種類
        /// </summary>
        [Column(Name = "請求來源的種類")]
        public string SourceType { get; set; }
        /// <summary>
        /// 檢查訊息
        /// </summary>
        [Column(Name = "檢查訊息")]
        public string CheckMessage { get; set; }
        /// <summary>
        /// 訊息內容
        /// </summary>
        [Column(Name = "文字內容")]
        public string Message { get; set; }
        /// <summary>
        /// 事件發生時間
        /// </summary>
        [Column(Name = "發生時間")]
        public DateTime? TimeStamp { get; set; }

        public static string TableName => "LinePushMessage";

        internal override string GetTableName => "LinePushMessage";
    }
}
