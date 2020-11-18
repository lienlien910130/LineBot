
namespace LineBOT.Models.Configuration
{
    /// <summary>
    /// 錯誤代碼集合
    /// </summary>
    public class ErrorMessageOptions
    {
        public static ErrorMessageOptions Options { get; set; }

        /// <summary>
        /// 處理控制要求時，發生意外狀況。
        /// </summary>
        public static ErrorCode NonException { get; set; }
        /// <summary>
        /// 登入失敗: 無法辨識的使用者名稱或密碼錯誤。
        /// </summary>
        public static ErrorCode LoginFail { get; set; }
        /// <summary>
        /// 項目已經存在。
        /// </summary>
        public static ErrorCode DataExists { get; set; }
        /// <summary>
        /// 項目找不到。
        /// </summary>
        public static ErrorCode DataNotFound { get; set; }
    }

    /// <summary>
    /// 子項目
    /// </summary>
    public class ErrorCode
    {
        /// <summary>
        /// 代碼
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 翻譯
        /// </summary>
        public string Translation { get; set; }
    }
}
