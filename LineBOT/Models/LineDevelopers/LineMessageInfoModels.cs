using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomColumnLibrary;
using LineBOT.Models.DataBase;

namespace LineBOT.Models.LineDevelopers
{
    /// <summary>
    /// LINE訊息詳細屬性
    /// </summary>
    public class LineMessageInfoModels : DataBaseModels
    {
        /// <summary>
        /// 
        /// </summary>
        [Column(Name = "標題?")]
        public string Title { get; set; }
        /// <summary>
        /// 訊息文字內容
        /// </summary>
        [Column(Name = "訊息內容")]
        public string Text { get; set; }
        /// <summary>
        /// 回覆
        /// </summary>
        [Column(Name = "回覆")]
        public bool? IsCheck { get; set; }
        /// <summary>
        /// 詳細地址訊息
        /// </summary>
        [Column(Name = "詳細地址")]
        public string Address { get; set; }
        /// <summary>
        /// 緯度
        /// </summary>
        [Column(Name = "緯度")]
        public double Latitude { get; set; }
        /// <summary>
        /// 經度
        /// </summary>
        [Column(Name = "經度")]
        public double Longitude { get; set; }
        /// <summary>
        /// 目前用途不明
        /// </summary>
        [Column(Name = "用途不明")]
        public int? PackageID { get; set; }
        /// <summary>
        /// 目前用途不明
        /// </summary>
        [Column(Name = "用途不明")]
        public int? StickerID { get; set; }
        /// <summary>
        /// 目前用途不明
        /// </summary>
        [Column(Name = "用途不明")]
        public string StickerResourceType { get; set; }

        /// <summary>
        /// 事件發生時間
        /// </summary>
        [Column(Name = "發生時間")]
        public DateTime? CreateTime { get; set; }

        public static string TableName => "LineMessageInfo";

        internal override string GetTableName => "LineMessageInfo";
    }
}
