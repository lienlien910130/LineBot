using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomColumnLibrary;
using LineBOT.Models.DataBase;

namespace LineBOT.Models.LineDevelopers
{
    /// <summary>
    /// LINE事件
    /// </summary>
    public class LineEventModels : DataBaseModels
    {
        /// <summary>
        /// 事件的類型
        /// message
        /// memberJoined
        /// memberLeft
        /// join
        /// left
        /// </summary>
        [Column(Name = "事件類型")]
        public string Class { get; set; }
        /// <summary>
        /// 事件發生時間
        /// </summary>
        [Column(Name = "發生時間")]
        public DateTime? TimeStamp { get; set; }
        /// <summary>
        /// 回覆用的Token
        /// </summary>
        [Column(Name = "回覆用TOKEN")]
        public string ReplyToken { get; set; }
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
        /// 使用者ID
        /// </summary>
        [Column(Name = "使用者ID")]
        public string UserID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Column(Name = "來源")]
        public string SourceClass { get; set; }
        /// <summary>
        /// LineMessageInfo的ID
        /// </summary>
        [Column(Name = "對應訊息ID")]
        public string MessageInfoID { get; set; }

        public static string TableName => "LineEvent";

        internal override string GetTableName => "LineEvent";

        #region 判斷Event事件用
        /// <summary>
        /// 判斷是否被追蹤
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public static bool isFollow(string Event) => Event.ToLower().Equals("follow");
        /// <summary>
        /// 判斷是否被封鎖
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public static bool isUnFollow(string Event) => Event.ToLower().Equals("unfollow");
        /// <summary>
        /// 判斷是否為成員加入
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public static bool isMemberJoin(string Event) => Event.ToLower().Equals("memberjoined");
        /// <summary>
        /// 判斷是否為成員離開
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public static bool isMemberLeft(string Event) => Event.ToLower().Equals("memberleft");
        /// <summary>
        /// 判斷是否為訊息
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public static bool isMessage(string Event) => Event.ToLower().Equals("message");
        /// <summary>
        /// 判斷是否為BOT 加入 群組 或 聊天室
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public static bool isBOTJoin(string Event) => Event.ToLower().Equals("join");
        /// <summary>
        /// 判斷是否為BOT 離開 群組 或 聊天室
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public static bool isBOTLeft(string Event) => Event.ToLower().Equals("left");
        /// <summary>
        /// 判斷是否為postback
        /// </summary>
        /// <param name="Event"></param>
        /// <returns></returns>
        public static bool isPostBack(string Event) => Event.ToLower().Equals("postback");
        #endregion
    }
}
