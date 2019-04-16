using Newtonsoft.Json.Linq;
using System;
using System.Threading;

namespace BlockNotifyMonitor.task
{
    class NotifyMonitorTask
    {
        protected string network { get; set; }
        protected string taskname { get; set; }
        //
        protected MongoDBHelper mh { get; set; }
        protected string upmongodbConnStr { get; set; }
        protected string upmongodbDatabase { get; set; }
        protected string upcollection { get; set; }
        protected string upkey { get; set; }
        protected string mongodbConnStr { get; set; }
        protected string mongodbDatabase { get; set; }
        protected string collection { get; set; }
        protected string key { get; set; }
        protected int interval { get; set; } = 15;
        protected int threshold { get; set; } = 15; // 时间阈值(单位：秒)
        protected int powRelay { get; set; } = 2; // 延迟底数
        //
        protected EmailHelper emailHelper { get; set; }


        private DateTime? nowTime => DateTime.Now;
        private DateTime? lastPingTime = null;
        private DateTime? lastSendTime = null;
        private int firstSendIntervalSeconds = 300; // 默认5分钟
        private int continueSendCount = 0;

        public static NotifyMonitorTask create(
            string network,
            string taskname,
            //
            MongoDBHelper mh,
            string upmongodbConnStr,
            string upmongodbDatabase,
            string upcollection,
            string upkey,
            string mongodbConnStr,
            string mongodbDatabase,
            string collection,
            string key,
            int interval,
            int threshold,
            int powRelay,
            //
            EmailHelper emailHelper
            ) => new NotifyMonitorTask
            {
                network = network,
                taskname = taskname,
                //
                mh = mh,
                upmongodbConnStr = upmongodbConnStr,
                upmongodbDatabase = upmongodbDatabase,
                upcollection = upcollection,
                upkey = upkey,
                mongodbConnStr = mongodbConnStr,
                mongodbDatabase = mongodbDatabase,
                collection = collection,
                key = key,
                interval = interval,
                threshold = threshold,
                powRelay = powRelay,
                //
                emailHelper = emailHelper
            };

        public void loop()
        {
            head();
            while (true)
            {
                try
                {
                    ping();
                    process();
                }
                catch (Exception ex)
                {
                    error(ex);
                }
            }
        }

        protected virtual void process()
        {
            long upHeight = getUpDataCounter();
            long height = getDataCounter();
            if (upHeight - height > threshold)
            {
                // 相差高度大于阈值时, 排除掉程序重启正在同步数据情况, 然后发送通知
                Thread.Sleep(1000);
                var th = getDataCounter();
                if (th > height)
                {
                    reset();
                    return;
                }

                //
                string message = string.Format("{0}  {1}_{2} 任务与上游高度相差超过阈值: {3}(个), 实际高度: {4}/{5}", DateTime.Now.ToString("u"), network, taskname, threshold, height, upHeight);
                send(message);
                return;
            }
            lastSendTime = null;
            continueSendCount = 0;
        }

        protected long getDataCounter()
        {
            string findStr = new JObject { { "counter", key } }.ToString();
            var queryRes = mh.GetData(mongodbConnStr, mongodbDatabase, collection, findStr);
            if (queryRes == null || queryRes.Count == 0) return -1;
            return long.Parse(queryRes[0]["lastBlockindex"].ToString());
        }

        protected long getUpDataCounter()
        {
            string findStr = new JObject { { "counter", upkey } }.ToString();
            var queryRes = mh.GetData(upmongodbConnStr, upmongodbDatabase, upcollection, findStr);
            if (queryRes == null || queryRes.Count == 0) return -1;
            return long.Parse(queryRes[0]["lastBlockindex"].ToString());
        }

        protected void send(string message)
        {
            DateTime? now = nowTime;
            if (lastSendTime != null)
            {
                int range = (int)(now - lastSendTime).Value.TotalSeconds;
                int limit = firstSendIntervalSeconds * (int)Math.Pow(2, continueSendCount);
                if (range < limit) return;
            }

            emailHelper.send(message);
            lastSendTime = now;
            ++continueSendCount;
            Console.WriteLine(message);
        }
        protected void reset()
        {
            lastSendTime = null;
            continueSendCount = 0;
        }


        protected void head()
        {
            Console.WriteLine("{0} {1}_{2} start", DateTime.Now.ToString("u"), network, taskname);
        }
        protected void ping()
        {
            Thread.Sleep(1000 * interval);
            DateTime? now = nowTime;
            if (lastPingTime != null && (now - lastPingTime).Value.TotalMinutes < 60/*60分钟*/) return;

            lastPingTime = now;
            Console.WriteLine("{0} {1}_{2} is running...", DateTime.Now.ToString("u"), network, taskname);
        }
        protected void error(Exception ex)
        {
            Console.WriteLine("{0} {1}_{2} failed, errMsg:{3}, errStack:{4}\n", DateTime.Now.ToString("u"), network, taskname, ex.Message, ex.StackTrace);
        }
    }
}
