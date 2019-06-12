using BlockNotifyMonitor.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BlockNotifyMonitor.task
{
    class ApiMonitorTask
    {
        private string name = "ApiMonitorTask";
        private EmailHelper emailHelper { get; set; }
        private int interval { get; set; } = 3;  // 轮询时间间隔
        private int threshold { get; set; } = 15; // 时间阈值(单位：秒)
        private int powRelay { get; set; } = 2; // 延迟底数
        private long nowTime => TimeHelper.GetTimeStamp();
        private List<UrlInfo> apiUrlInfoList;
        class UrlInfo
        {
            public string url { get; set; }
            public long lastUpdateTime { get; set; }
            public int errorSendCount { get; set; }
        }

        public static ApiMonitorTask create(
            string[] apis,
            int interval,
            int threshold,
            int powRelay,
            EmailHelper emailHelper
            ) => new ApiMonitorTask()
            {
                emailHelper = emailHelper,
                interval = interval,
                threshold = threshold,
                powRelay = powRelay,
                apiUrlInfoList = apis.Select(p => new UrlInfo { url = p, lastUpdateTime = TimeHelper.GetTimeStamp(), errorSendCount = 0 }).ToList()
            };
        
        public void loop()
        {
            while(true)
            {
                Thread.Sleep(interval);
                try
                {
                    process();
                } catch (Exception ex)
                {
                    Console.WriteLine("{0} failed, errMsg:{1}, errStack:{2}\n", name, ex.Message, ex.StackTrace);
                }
            }
        }
        private void process()
        {
            // 更新lastUpdateTime
            apiUrlInfoList = apiUrlInfoList.Select(p => {
                if (IsNormal(p.url, out long lastUpdateTime))
                {
                    p.lastUpdateTime = lastUpdateTime;
                    p.errorSendCount = 0;
                }
                return p;
            }).ToList();

            // 查询到达阈值的数据
            var res = apiUrlInfoList.Where(p => {
                return nowTime - p.lastUpdateTime > threshold * (int)Math.Pow(powRelay, p.errorSendCount);
            }).ToList();

            if (res != null && res.Count > 0)
            {
                res.ForEach(p => {
                    string message = string.Format("{0} API接口服务连不上的时间超过阈值{1}(s),apiURL={2}", DateTime.Now.ToString("u"), threshold, p.url);
                    if (sendData(message))
                    {
                        apiUrlInfoList.Remove(p);
                        ++p.errorSendCount;
                        apiUrlInfoList.Add(p);
                    }
                });
            }
        }

        private bool sendData(string message)
        {
            try
            {
                emailHelper.send(message);
                return true;
            } catch
            {
                return false;
            }
            
        }
        private bool IsNormal(string url, out long lastUpdateTime)
        {
            string method = "getnodetype";
            JObject postData = new JObject();
            postData.Add("jsonrpc", "2.0");
            postData.Add("method", method);
            postData.Add("params", new JArray() { });
            postData.Add("id", 1);

            string data = postData.ToString();
            string res = null;
            try
            {
                res = HttpHelper.Post(url, data, Encoding.UTF8, 1);
            } catch
            {

            }
            if(string.IsNullOrEmpty(res) || JObject.Parse(res)["result"] == null)
            {
                lastUpdateTime = 0;
                return false;
            }
            lastUpdateTime = nowTime;
            return true;
        }
    }
}
