using isRock.LineBot;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using LineBOT.Models.LineDevelopers;
using Newtonsoft.Json.Linq;
using LineBOT.Models.Configuration;
using Microsoft.Extensions.Options;

namespace LineBOT.Controllers
{
    [ApiController]
    [Route("linebot")]
    public class LineBotController : ControllerBase
    {
        isRock.LineBot.Bot bot;
        private string _LineToken;
        private string BASE_URL;
        private WebSocketConnectOptions.WebSocketConnectValue WebSocketApiServer;
        isRock.LineBot.MessageBase responseMsg = null;

        //message collection for response multi-message 
        //可製作多筆回覆訊息
        List<isRock.LineBot.MessageBase> responseMsgs = new List<isRock.LineBot.MessageBase>();

        HttpClient client = new HttpClient();
        
        [HttpPost("message")]
        public async Task<string> Index([FromServices] IOptions<LineBotSetting> LineSetting, [FromServices] IOptions<WebSocketConnectOptions> wsSetting)
        {
            WebSocketApiServer = wsSetting.Value.ApiService;
            BASE_URL = LineSetting.Value.BaseUrl;
            _LineToken = LineSetting.Value.LineBotToken;

            try
            {
                //create vot instance
                //建立機器人
                bot = new isRock.LineBot.Bot(_LineToken);
                isRock.LineBot.Demographic.GetFriendDemographicsResult getFriendDemographicsResult = bot.GetFriendDemographics();

                var body = "";

                using (StreamReader reader = new StreamReader(Request.Body, System.Text.Encoding.UTF8))
                {
                    body = await reader.ReadToEndAsync();
                }

                //剖析JSON
                var ReceivedMessage = isRock.LineBot.Utility.Parsing(body);
                //Get LINE Event
                var LineEvent = ReceivedMessage.events;


                foreach (var item in LineEvent)
                {
                    string Type = item.type.ToLower();
                    var menus = isRock.LineBot.Utility.GetRichMenuList(_LineToken);
                    string MessageResult = "";
                    LineUserInfo LineuserInfo = null;
                    var LineEventresult = await InsertLineEvent(item); //新增事件
                    LineUserInfoModels LineUserInforesult = await GetLineUserInfo(item.source.userId); //查詢會員
                    var MemberInGRresult = await GetMemberInGR(item);//查詢是否有存在在聊天室/房間內
                    LineuserInfo = bot.GetUserInfo(item.source.userId);
                    //responseMsg = new isRock.LineBot.TextMessage(
                    //                        $"OnMessage 訊息事件\n" +
                    //                        $"訊息類型: {MessageResult}\n"
                    //);
                    //responseMsgs.Add(responseMsg);

                    switch (Type)
                    {
                        //加入好友
                        case "follow": 
                            LineuserInfo = bot.GetUserInfo(item.source.userId);
                            if (LineUserInforesult == null)
                            {
                                await InsertLineUserInfo(LineuserInfo);
                            }
                            else
                            {
                                if (LineUserInforesult.Name != LineuserInfo.displayName)
                                {
                                    LineUserInforesult.Name = LineuserInfo.displayName;
                                    await PatchLineUserInfo(LineUserInforesult);
                                }
                            }
                            if (MemberInGRresult == null)
                            {
                                await InsertMemberInGR(item);
                            }
                            break;
                        //封鎖/刪除好友
                        case "unfollow":
                            if (MemberInGRresult != null)
                            {
                                MemberInGRresult.IsLeave = true;
                                MemberInGRresult.LeaveTime = DateTime.Now;
                                await PatchMemberInGR(MemberInGRresult);
                            }
                            break;
                        //訊息
                        case "message":
                            if (item.source.type.ToLower() == "room")
                            {
                                LineuserInfo = isRock.LineBot.Utility.GetRoomMemberProfile(
                                    item.source.roomId, item.source.userId, _LineToken);
                            }
                            if (item.source.type.ToLower() == "group")
                            {
                                LineuserInfo = isRock.LineBot.Utility.GetGroupMemberProfile(
                                    item.source.groupId, item.source.userId, _LineToken);
                            }
                            if (LineUserInforesult == null)
                            {
                                await InsertLineUserInfo(LineuserInfo);
                            }
                            if (MemberInGRresult == null)
                            {
                                await InsertMemberInGR(item);
                            }
                            var insertMsgresult = await InsertMessageInfo(item);
                            LineEventresult.MessageInfoID = insertMsgresult.ID; //儲存訊息
                            await PatchLineEvent(LineEventresult);
                            MessageResult = MessageHandle(item);
                            break;
                        //有成員加入
                        case "memberjoined":
                            foreach (var member in item.joined.members)
                            {
                                if (item.source.type.ToLower() == "room")
                                {
                                    LineuserInfo = isRock.LineBot.Utility.GetRoomMemberProfile(
                                        item.source.roomId, member.userId, _LineToken);
                                }
                                if (item.source.type.ToLower() == "group")
                                {
                                    LineuserInfo = isRock.LineBot.Utility.GetGroupMemberProfile(
                                    item.source.groupId, member.userId, _LineToken);
                                }
                                LineUserInfoModels userinfoR = await GetLineUserInfo(member.userId); //查詢會員
                                if (userinfoR == null)
                                {
                                    await InsertLineUserInfo(LineuserInfo);
                                }
                                if (MemberInGRresult == null)
                                {
                                    await InsertMemberInGR(item);
                                }
                            }
                            break;
                        //有成員離開
                        case "memberleft":
                            if (MemberInGRresult != null)
                            {
                                MemberInGRresult.IsLeave = true;
                                MemberInGRresult.LeaveTime = DateTime.Now;
                                await PatchMemberInGR(MemberInGRresult);
                            }
                            break;
                        case "join":
                            var GRInforesult = await GetGRInfo(item);
                            if (GRInforesult == null)
                            {
                                await InsertGRInfo(item);
                            }
                            break;
                        case "left":
                            break;
                        case "push":
                            var insertpushMsgresult = await InsertMessageInfo(item);
                            LineEventresult.MessageInfoID = insertpushMsgresult.ID; //儲存訊息
                            await PatchLineEvent(LineEventresult);
                            PushMessage(item, insertpushMsgresult.ID);
                            break;
                        case "postback": // 5個
                            var data = item.postback.data;
                            string[] postBackArr = data.Split('|');
                            string url = postBackArr[0].ToString();
                            string msgID = postBackArr[1].ToString();
                            string msginfoID = postBackArr[2].ToString();
                            string flowFlag = postBackArr[3].ToString();

                            LineEventresult.MessageInfoID = msginfoID;  //儲存訊息
                            await PatchLineEvent(LineEventresult);

                            var MsgResult = await GetMessageInfo(msginfoID);
                            if (MsgResult != null)
                            {
                                if (MsgResult.IsCheck != null & flowFlag == "Y" || flowFlag == "N")
                                {
                                    responseMsg = new isRock.LineBot.TextMessage($"此筆訊息已回覆過，請勿重複!");
                                    responseMsgs.Add(responseMsg);
                                }
                                else if (MsgResult.IsCheck == null & flowFlag == "Y" || flowFlag == "N")
                                {
                                    if (flowFlag == "Y")
                                    {
                                        MsgResult.IsCheck = true;
                                    }
                                    else
                                    {
                                        MsgResult.IsCheck = false;
                                    }
                                    await PatchMessageInfo(MsgResult);
                                    await PostRequest(url, item); //傳送回傳的訊息給後端
                                    responseMsg = new isRock.LineBot.TextMessage($"感謝您的回覆!");
                                    responseMsgs.Add(responseMsg);
                                }
                                else
                                {
                                    responseMsg = new isRock.LineBot.TextMessage($"感謝您的回覆!");
                                    responseMsgs.Add(responseMsg);
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    if (responseMsgs.Count < 1)
                        return "";
                    else
                        return bot.ReplyMessage(item.replyToken, responseMsgs);
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// 訊息內容處理
        /// </summary>
        /// <param name="LineEvent"></param>
        /// <returns></returns>
        private string MessageHandle(isRock.LineBot.Event LineEvent)
        {
            //responseMsg = new isRock.LineBot.TextMessage(
            //                        $"OnMessage 訊息事件\n" +
            //                        $"訊息類型: {LineEvent.message.type.ToString()}\n" +
            //                        $"來源類型: {LineEvent.source.type.ToString()}\n" +
            //                        $"用戶 ID: {LineEvent.source.userId}"
            //);
            //responseMsgs.Add(responseMsg);

            switch (LineEvent.message.type.ToLower())
            {
                case "text":
                    string text = LineEvent.message.text.ToLower();
                    //add ButtonsTemplate if user say "/Show ButtonsTemplate"
                    //判定下指令
                    if (text.Contains("選單"))
                    {
                        //define actions
                        //設定按鈕
                        var actions = new List<isRock.LineBot.TemplateActionBase>();
                        actions.Add(new isRock.LineBot.MessageAction() { label = "message action", text = "回覆文字1" });
                        actions.Add(new isRock.LineBot.UriAction() { label = "url action", uri = new Uri("https://mercuryfire.com.tw/") });

                        //按鈕上方圖片
                        var tmp = new isRock.LineBot.ButtonsTemplate()
                        {
                            title = "選單",
                            text = "您想做什麼？",
                            thumbnailImageUrl = new Uri("https://upload.cc/i1/2020/05/12/VJ7eGx.png"),
                            actions = actions
                        };

                        //add TemplateMessage into responseMsgs
                        responseMsgs.Add(new isRock.LineBot.TemplateMessage(tmp));

                    }
                    else if (text.Contains("輪播"))
                    {
                        var actions1 = new List<isRock.LineBot.TemplateActionBase>();
                        actions1.Add(new isRock.LineBot.MessageAction() { label = "actions1-1", text = "回覆文字1" });
                        actions1.Add(new isRock.LineBot.MessageAction() { label = "actions1-2", text = "回覆文字2" });
                        actions1.Add(new isRock.LineBot.UriAction() { label = "官網", uri = new Uri("https://mercuryfire.com.tw/") });

                        var actions2 = new List<isRock.LineBot.TemplateActionBase>();
                        actions2.Add(new isRock.LineBot.MessageAction() { label = "actions2-1", text = "回覆文字1" });
                        actions2.Add(new isRock.LineBot.MessageAction() { label = "actions2-2", text = "回覆文字2" });
                        actions2.Add(new isRock.LineBot.UriAction() { label = "官網", uri = new Uri("https://mercuryfire.com.tw/") });

                        var tmp = new isRock.LineBot.CarouselTemplate()
                        {
                            columns = new List<Column>()
                            {
                                    new Column(){ title="test1",thumbnailImageUrl=new Uri("https://ithelp.ithome.com.tw/upload/images/20200106/20106865dA0ce7tJLA.png"),text="請選擇1",actions=actions1 },
                                    new Column(){ title="test2",thumbnailImageUrl=new Uri("https://ithelp.ithome.com.tw/upload/images/20200106/20106865q03SKAqv0U.png"),text="請選擇2",actions=actions2 }
                            }
                        };
                        //add TemplateMessage into responseMsgs
                        responseMsgs.Add(new isRock.LineBot.TemplateMessage(tmp));
                    }
                    else if (text.Contains("/changer1")) //更換RichMenu
                    {
                        SwitchMenuTo("r1", LineEvent);
                    }
                    else if (text.Contains("/changer2")) //更換RichMenu
                    {
                        SwitchMenuTo("r2", LineEvent);
                    }
                    else if (text.Contains("/fire")) //火警警報測試/fire 迴路+點位，例如/fire 002-004
                    {
                        text = text.Replace("/fire", "").Trim();

                        Console.WriteLine("F-" + text);

                        //使用API呼叫
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri("http://" + WebSocketApiServer.IP + ":" + WebSocketApiServer.Port + "/");
                            var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Post, "ws");

                            request.Content = new StringContent(JsonConvert.SerializeObject(text), Encoding.UTF8, "application/json");
                            var response = client.SendAsync(request).Result;

                            //判斷是否連線成功
                            var APIResult = response.Content.ReadAsAsync<string>().Result;
                            if (response.IsSuccessStatusCode)
                            {
                                //取回傳值
                                responseMsg = new TextMessage($"結果：成功=>" + APIResult);
                                
                            }
                            else
                            {
                                responseMsg = new TextMessage($"結果：失敗=>" + response.StatusCode);
                            }
                        };
                        responseMsgs.Add(responseMsg);
                    }
                    else if (text.Contains("r")) //初始化設置
                    {
                        if (isRock.LineBot.Utility.GetRichMenuList(_LineToken).richmenus.Count == 0)
                        {
                            CreateRichMenu(); //建立token選單
                        }
                        SwitchMenuTo("r1", LineEvent);
                    }
                    else if (text.Contains("取消")) //清空用戶的選單
                    {
                        isRock.LineBot.Utility.UnlinkRichMenuFromUser(LineEvent.source.userId, _LineToken);
                    }
                    else if (text.Contains("刪除")) //刪除token選單
                    {
                        DeleteRichMenu();
                    }
                    else
                    {
                        //add text response
                        //responseMsg = new isRock.LineBot.TextMessage($"you said : {text}");
                        //responseMsgs.Add(responseMsg);
                    }

                    break;
                case "sticker": //傳送貼圖(目前是固定的)
                    responseMsg =
                    new isRock.LineBot.StickerMessage(1, 2);
                    responseMsgs.Add(responseMsg);
                    break;
                case "video":
                    break;
                case "audio":
                    break;
                case "location":
                    break;
                //無法得知使用者的訊息使用甚麼型態
                default:
                    responseMsg = new isRock.LineBot.TextMessage($"None handled message type : { LineEvent.message.type}");
                    responseMsgs.Add(responseMsg);
                    break;
            }

            return "";
        }

        [HttpPost("webhook")]
        public int ChangeWebhook([FromBody] WebHook data, [FromServices] IOptions<LineBotSetting> LineSetting, [FromServices] IOptions<WebSocketConnectOptions> wsSetting) 
        {
            WebSocketApiServer = wsSetting.Value.ApiService;
            BASE_URL = LineSetting.Value.BaseUrl;
            _LineToken = LineSetting.Value.LineBotToken;

            isRock.LineBot.Utility.SetWebhookEndpointURL(_LineToken, new Uri(data.webhook + "/linebot/message"));
            var ret2 = isRock.LineBot.Utility.TestWebhookEndpoint(_LineToken, new Uri(data.webhook+ "/linebot/message"));
            return ret2.statusCode;
        }

        #region 新增 [事件]
        private async Task<LineEventModels> InsertLineEvent(isRock.LineBot.Event lineEvent)
        {
            try
            {
                var posturl = BASE_URL+ "lineDeveloper/event";
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("POST"),
                    RequestUri = new Uri(posturl),
                };

                LineEventModels newLE = new LineEventModels();
                newLE.Class = lineEvent.type.ToLower();
                newLE.SourceClass = lineEvent.source.type;
                newLE.MessageInfoID = null;
                newLE.TimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(lineEvent.timestamp).ToLocalTime();
                newLE.ReplyToken = lineEvent.replyToken;
                newLE.UserID = lineEvent.source.userId;
                newLE.RoomID = lineEvent.source.roomId;
                newLE.GroupID = lineEvent.source.groupId;

                var json = JsonConvert.SerializeObject(newLE);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                //requestMessage.Headers.Authorization =
                //        new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJlMDY5MTI1Yy0xM2I1LTQ3MTktODllYi0wNzQ4MzI2MDc0MzgiLCJzdWIiOiJtZjQ0IiwiZXhwIjoxNjA0NzM0OTgxLCJSb2xlIjoiVXNlciIsIm5iZiI6MTYwMzg3MDk4MSwiaWF0IjoxNjAzODcwOTgxLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjU5MTE5In0.z3u6g5lqEWwHHIyxdJ1v343bN570mDN5t8lsSAP86bk");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");
                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseResult = JObject.Parse(responseBody).GetValue("result").ToString();
               var result = JsonConvert.DeserializeObject<LineEventModels>(responseResult);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 更新 [事件] 
        private async Task<string> PatchLineEvent(LineEventModels lineEvent)
        {
            try
            {
                var posturl = BASE_URL + "lineDeveloper/event";
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("PATCH"),
                    RequestUri = new Uri(posturl),
                };

                var json = JsonConvert.SerializeObject(lineEvent);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");

                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseStatusCode + "//" + responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion

        #region 查詢[成員] 資料
        private async Task<LineUserInfoModels> GetLineUserInfo(string userid)
        {
            try
            {
                var posturl = BASE_URL + "lineDeveloper/userinfo/" + userid;
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri(posturl),
                };
                client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                var s = JObject.Parse(responseBody);
                var responseResult = JObject.Parse(responseBody).GetValue("result").ToString();
                var result = JsonConvert.DeserializeObject<LineUserInfoModels>(responseResult);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 新增 [成員] 資料
        private async Task<string> InsertLineUserInfo(LineUserInfo userinfo)
        {
            try
            {
                var posturl = BASE_URL + "lineDeveloper/userinfo";
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("POST"),
                    RequestUri = new Uri(posturl),
                };
                LineUserInfoModels newUser = new LineUserInfoModels();
                newUser.UserID = userinfo.userId;
                newUser.Name = userinfo.displayName;
                newUser.CreateTime = DateTime.Now;

                var json = JsonConvert.SerializeObject(newUser);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");

                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseStatusCode + "//" + responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion

        #region 更新 [成員] 資料
        private async Task<string> PatchLineUserInfo(LineUserInfoModels userinfo)
        {
            try
            {
                var posturl =BASE_URL+ "lineDeveloper/userinfo";
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("PATCH"),
                    RequestUri = new Uri(posturl),
                };
                
                var json = JsonConvert.SerializeObject(userinfo);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");

                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseStatusCode + "//" + responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion

        #region 查詢[聊天室/房間] 資料
        private async Task<LineGRInfoModels> GetGRInfo(isRock.LineBot.Event lineEvent)
        {
            try
            {
                var RoomID = lineEvent.source.roomId != null ? lineEvent.source.roomId : "0";
                var GroupID = lineEvent.source.groupId != null ? lineEvent.source.groupId : "0";

                var posturl = BASE_URL + "lineDeveloper/grinfo/" + GroupID+"/"+RoomID;
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri(posturl),
                };
                client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseResult = JObject.Parse(responseBody).GetValue("result").ToString();
                var result = JsonConvert.DeserializeObject<LineGRInfoModels>(responseResult);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 新增 [聊天室/房間] 資料
        private async Task<string> InsertGRInfo(isRock.LineBot.Event lineEvent)
        {
            try
            {
                var posturl = BASE_URL + "lineDeveloper/grinfo";
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("POST"),
                    RequestUri = new Uri(posturl),
                };
                LineGRInfoModels newGRs = new LineGRInfoModels();
                newGRs.RoomID = lineEvent.source.roomId;
                newGRs.GroupID = lineEvent.source.groupId;
                newGRs.CreateTime = DateTime.Now;

                var json = JsonConvert.SerializeObject(newGRs);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");

                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseStatusCode + "//" + responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion

        #region 查詢 成員存在[聊天室/群組]
        private async Task<LineMemberInGRInfo> GetMemberInGR(isRock.LineBot.Event  lineEvent)
        {
            try
            {
                var UserID = lineEvent.source.userId;
                var RoomID = lineEvent.source.roomId != null ? lineEvent.source.roomId : "0" ;
                var GroupID = lineEvent.source.groupId != null ? lineEvent.source.groupId : "0";
                if (lineEvent.type.ToLower() == "memberjoined")
                {
                    UserID = lineEvent.joined.members[0].userId;
                }
                if (lineEvent.type.ToLower() == "memberleft")
                {
                    UserID = lineEvent.left.members[0].userId;
                }

                var posturl = BASE_URL + "lineDeveloper/mingrinfo/" + UserID +"/"+GroupID+"/"+RoomID;
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri(posturl),
                };
                client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseResult = JObject.Parse(responseBody).GetValue("result").ToString();
                var result = JsonConvert.DeserializeObject<LineMemberInGRInfo>(responseResult);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 新增 成員存在[聊天室/群組]
        private async Task<string> InsertMemberInGR(isRock.LineBot.Event lineEvent)
        {
            try
            {
                var posturl = BASE_URL + "lineDeveloper/mingrinfo";
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("POST"),
                    RequestUri = new Uri(posturl),
                };

                LineMemberInGRInfo newMGR = new LineMemberInGRInfo();
                newMGR.UserID = lineEvent.source.userId;
                newMGR.RoomID = lineEvent.source.roomId;
                newMGR.GroupID = lineEvent.source.groupId;
                if (lineEvent.type.ToLower() == "memberjoined")
                {
                    newMGR.UserID = lineEvent.joined.members[0].userId;
                }
                if (lineEvent.type.ToLower() == "memberleft")
                {
                    newMGR.UserID = lineEvent.left.members[0].userId;
                }
                newMGR.CreateTime = DateTime.Now;
                var json = JsonConvert.SerializeObject(newMGR);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");

                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseStatusCode + "//" + responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion

        #region 更新 成員存在[聊天室/群組]
        private async Task<string> PatchMemberInGR(LineMemberInGRInfo MemberInGRInfo)
        {
            try
            {
                var posturl = BASE_URL + "lineDeveloper/mingrinfo";
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("PATCH"),
                    RequestUri = new Uri(posturl),
                };

                var json = JsonConvert.SerializeObject(MemberInGRInfo);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");

                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseStatusCode + "//" + responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion

        #region 新增 [訊息] 資料
        private async Task<LineMessageInfoModels> InsertMessageInfo(isRock.LineBot.Event lineEvent)
        {
            try
            {
                var posturl = BASE_URL + "lineDeveloper/msinfo";
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("POST"),
                    RequestUri = new Uri(posturl),
                };
                LineMessageInfoModels newLM = new LineMessageInfoModels();
                newLM.Title = lineEvent.message.type;
                newLM.Text = lineEvent.message.text;
                newLM.Address = lineEvent.message.address;
                newLM.Latitude = lineEvent.message.latitude;
                newLM.Longitude = lineEvent.message.longitude;
                newLM.PackageID = lineEvent.message.packageId;
                newLM.StickerID = lineEvent.message.stickerId;
                newLM.StickerResourceType = lineEvent.message.stickerResourceType;
                newLM.CreateTime = DateTime.Now;

                var json = JsonConvert.SerializeObject(newLM);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");

                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseResult = JObject.Parse(responseBody).GetValue("result").ToString();
                var result = JsonConvert.DeserializeObject<LineMessageInfoModels>(responseResult);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 查詢[訊息] 資料
        private async Task<LineMessageInfoModels> GetMessageInfo(string msgid)
        {
            try
            {
                var posturl = BASE_URL + "lineDeveloper/msinfo/" + msgid;
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri(posturl),
                };
                client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseResult = JObject.Parse(responseBody).GetValue("result").ToString();
                var result = JsonConvert.DeserializeObject<LineMessageInfoModels>(responseResult);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 更新 [訊息] 資料
        private async Task<string> PatchMessageInfo(LineMessageInfoModels userinfo)
        {
            try
            {
                var posturl = BASE_URL + "lineDeveloper/msinfo";
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("PATCH"),
                    RequestUri = new Uri(posturl),
                };

                var json = JsonConvert.SerializeObject(userinfo);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");

                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseStatusCode + "//" + responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion

        //伺服器推送消息
        private void PushMessage(Event LineEvent,string MessageInfoID) 
        {
            switch (LineEvent.message.type) 
            {
                case "leaveform":
                    var actions = new List<isRock.LineBot.TemplateActionBase>();
                    actions.Add(new isRock.LineBot.PostbackAction()
                    {
                        label = "核准",
                        data =
                        LineEvent.replyToken + "|" +  LineEvent.message.id + "|" + MessageInfoID + "|Y"
                    });
                    actions.Add(new isRock.LineBot.PostbackAction()
                    {
                        label = "退回",
                        data =
                        LineEvent.replyToken + "|" + LineEvent.message.id + "|" + MessageInfoID + "|N"
                    });
                    //actions.Add(new isRock.LineBot.PostbackAction() { label = "修改", data =
                    //    item.replyToken+"|"+item.source.userId+"|"+item.message.id+"|"+ MessageInfoID+ "|U" });
                    //actions.Add(new isRock.LineBot.PostbackAction() { label = "備註", data =
                    //    item.replyToken+"|"+item.source.userId+"|"+item.message.id+"|"+ MessageInfoID+ "|n" });

                    //按鈕上方圖片
                    var tmp = new isRock.LineBot.ButtonsTemplate()
                    {
                        title = "請假核准單",
                        text = LineEvent.message.text,
                        thumbnailImageUrl = new Uri("https://upload.cc/i1/2020/05/12/VJ7eGx.png"),
                        actions = actions
                    };
                    isRock.LineBot.Utility.PushTemplateMessage(LineEvent.source.userId, tmp, _LineToken);
                    break;
                case "purchaseorder":
                    break;
                case "worknotice":
                    isRock.LineBot.Utility.PushMessage(LineEvent.source.userId, LineEvent.message.text, _LineToken);
                    break;
                case "message":
                    break;
                default:
                    break;
            }
        }

        //回傳資料給後端
        private async Task<string> PostRequest(string url,isRock.LineBot.Event LineEvent)
        {
            try
            {
                var posturl = "http://192.168.88.65:59119" + url;
                var requestMessage = new HttpRequestMessage()
                {
                    Method = new HttpMethod("POST"),
                    RequestUri = new Uri(posturl),
                };
                var json = JsonConvert.SerializeObject(LineEvent);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJlMDY5MTI1Yy0xM2I1LTQ3MTktODllYi0wNzQ4MzI2MDc0MzgiLCJzdWIiOiJtZjQ0IiwiZXhwIjoxNjA0NzM0OTgxLCJSb2xlIjoiVXNlciIsIm5iZiI6MTYwMzg3MDk4MSwiaWF0IjoxNjAzODcwOTgxLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjU5MTE5In0.z3u6g5lqEWwHHIyxdJ1v343bN570mDN5t8lsSAP86bk");

                requestMessage.Content.Headers.TryAddWithoutValidation(
                        "x-custom-header", "value");

                var response = await client.SendAsync(requestMessage);
                var responseStatusCode = response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();
                return responseStatusCode + "//" + responseBody;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        //建立選單
        private string CreateRichMenu()
        {
            var richMenu1 = new isRock.LineBot.RichMenu.RichMenuItem()
            {
                size = { width = 2500, height = 1686 },
                selected = false,
                name = "r1",
                chatBarText = "richmenu1",
                areas = new List<isRock.LineBot.RichMenu.Area>()
                {
                    new isRock.LineBot.RichMenu.Area()
                    {
                        bounds = { x=0,y=0,width= 460, height=1686 },
                        action = new isRock.LineBot.MessageAction() { label = "/changer2", text = "/changer2" }
                    }
                }
            };
            //image 2500x1686 
            var one = isRock.LineBot.Utility.CreateRichMenu(richMenu1, new Uri("http://arock.blob.core.windows.net/blogdata201902/test01.png"), _LineToken);
            var richMenu2 = new isRock.LineBot.RichMenu.RichMenuItem()
            {
                size = { width = 2500, height = 1686 },
                selected = false,
                name = "r2",
                chatBarText = "richmenu2",
                areas = new List<isRock.LineBot.RichMenu.Area>()
                {
                    new isRock.LineBot.RichMenu.Area()
                    {
                        bounds = { x=2040,y=0,width=2040+460,height=1686 },
                        action = new isRock.LineBot.MessageAction() { label = "/changer1", text = "/changer1" }
                    }
                }
            };
            //image 2500x1686 
            isRock.LineBot.Utility.CreateRichMenu(richMenu2, new Uri("http://arock.blob.core.windows.net/blogdata201902/test01.png"), _LineToken);
            return one.richMenuId;
        }
        
        //設定使用者的選單
        private bool SwitchMenuTo(string MenuName, Event LineEvent)
        {
            //抓取所有選單
            var menus = isRock.LineBot.Utility.GetRichMenuList(_LineToken);
            //列舉每一個
            foreach (var item in menus.richmenus)
            {
                //如果選單名稱為 MenuName
                if (item.name == MenuName)
                {
                    isRock.LineBot.Utility.LinkRichMenuToUser(item.richMenuId, LineEvent.source.userId, _LineToken);
                    return true;
                }
            }
            return false;
        }

        //刪除所有選單
        protected void DeleteRichMenu()
        {
            //取消預設選單
            isRock.LineBot.Utility.CancelDefaultRichMenu(_LineToken);
            //抓取所有選單
            var menus = isRock.LineBot.Utility.GetRichMenuList(_LineToken);
            //刪除每一個
            foreach (var item in menus.richmenus)
            {
                isRock.LineBot.Utility.DeleteRichMenu(item.richMenuId, _LineToken);
            }
        }

        public class WebHook
        {
            public string webhook { get; set; }
        }
    }
}
