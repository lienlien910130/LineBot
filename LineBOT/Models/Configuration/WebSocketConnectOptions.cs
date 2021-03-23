
using System.Collections.Generic;

namespace LineBOT.Models.Configuration
{
    /// <summary>
    /// WebSocket設定
    /// </summary>
    public class WebSocketConnectOptions
    {
        /// <summary>
        /// WS專屬API伺服器呼叫路徑
        /// </summary>
        public WebSocketConnectValue ApiService { get; set; }

        /// <summary>
        /// WS伺服器連接/Usually
        /// </summary>
        public WebSocketConnectValue WSC_Usually { get; set; }
        /// <summary>
        /// WS伺服器連接/GraphicFire
        /// </summary>
        public WebSocketConnectValue WSC_GraphicFire { get; set; }

        public class WebSocketConnectValue
        {
            public string IP { get; set; }
            public string Port { get; set; }
            public string Path { get; set; }
        }
    }
}
