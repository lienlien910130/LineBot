
namespace LineBOT.Models.Configuration
{
    /// <summary>
    /// LINE BOT設定
    /// </summary>
    public class LineBotSetting
    {
        /// <summary>
        /// LINE BOT Token
        /// </summary>
        public string LineBotToken { get; set; }

        /// <summary>
        /// 存取資料庫的位置
        /// </summary>
        public string BaseUrl { get; set; }

    }
}
