using BlockNotifyMonitor.task;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockNotifyMonitor
{
    class TaskRunner
    {
        public static void run(string config, string mainName)
        {
            var root = new ConfigurationBuilder().AddJsonFile(config).Build();

            //
            MongoDBHelper mh = new MongoDBHelper();
            string block_mongodbConnStr_mainnet = root["block_mongodbConnStr_mainnet"];
            string block_mongodbDatabase_mainnet = root["block_mongodbDatabase_mainnet"];
            string notify_mongodbConnStr_mainnet = root["notify_mongodbConnStr_mainnet"];
            string notify_mongodbDatabase_mainnet = root["notify_mongodbDatabase_mainnet"];
            string analy_mongodbConnStr_mainnet = root["analy_mongodbConnStr_mainnet"];
            string analy_mongodbDatabase_mainnet = root["analy_mongodbDatabase_mainnet"];
            string block_mongodbConnStr_testnet = root["block_mongodbConnStr_testnet"];
            string block_mongodbDatabase_testnet = root["block_mongodbDatabase_testnet"];
            string notify_mongodbConnStr_testnet = root["notify_mongodbConnStr_testnet"];
            string notify_mongodbDatabase_testnet = root["notify_mongodbDatabase_testnet"];
            string analy_mongodbConnStr_testnet = root["analy_mongodbConnStr_testnet"];
            string analy_mongodbDatabase_testnet = root["analy_mongodbDatabase_testnet"];
            //
            AlySmsHelper smsHelper = new AlySmsHelper
            {
                config = new AlySmsConfig
                {
                    accessKeyId = root["accessKeyId"],
                    accessKeySecret = root["accessKeySecret"],
                    phoneNumbers = root["phoneNumbers"],
                    dailyTime = root["dailyTime"]
                }
            };
            //
            EmailHelper emailHelper = new EmailHelper
            {
                config = new EmailConfig
                {
                    mailFrom = root["mailFrom"],
                    mailPwd = root["mailPwd"],
                    smtpHost = root["smtpHost"],
                    smtpPort = int.Parse(root["smtpPort"]),
                    smtpEnableSsl = bool.Parse(root["smtpEnableSsl"]),
                    subject = root["subject"],
                    body = root["body"],
                    listener = root["listener"],
                }
            };


            //
            bool isBlock = root["isBlock"] == "1";
            bool isTx = root["isTx"] == "1";
            bool isNotify = root["isNotify"] == "1";
            bool isAnaly = root["isAnaly"] == "1";
            bool isCN = root["isCN"] == "1";
            int loop_interval_seconds = int.Parse(root["loop_interval_seconds"]);
            int height_warn_threshold = int.Parse(root["height_warn_threshold"]);
            int time_warn_threshold = int.Parse(root["time_warn_threshold"]);
            Console.WriteLine(mainName + ".isBlock:" + isBlock);
            Console.WriteLine(mainName + ".isTx:" + isTx);
            Console.WriteLine(mainName + ".isNotify:" + isNotify);
            Console.WriteLine(mainName + ".isAnaly:" + isAnaly);
            Console.WriteLine(mainName + ".loop_interval_seconds:" + loop_interval_seconds);
            Console.WriteLine(mainName + ".height_warn_threshold:" + height_warn_threshold);
            Console.WriteLine(mainName + ".time_warn_threshold:" + time_warn_threshold);

            //
            string mainnetStr = "主网"; // "mainnet";//
            string testnetStr = "测试网"; // "testnet";//
            string blockMonitor = "节点进程"; //"blockMonitor"; // 
            string txMonitor = "爬虫进程"; //"txMonitor"; // 
            string notifyMonitor = "合约分析进程"; //"notifyMonitor"; // 
            string analyMonitor = "基础分析进程"; //"analyMonitor"; // 
            if(!isCN)
            {
                mainnetStr = "mainnet";
                testnetStr = "testnet";
                blockMonitor = "blockMonitor[节点进程]";
                txMonitor = "txMonitor[爬虫进程]";
                notifyMonitor = "notifyMonitor[合约分析进程]";
                analyMonitor = "analyMonitor[Neo分析进程]";
            }


            if (isBlock) Task.Run(() => BlockMonitorTask.create(mainnetStr, blockMonitor, mh, block_mongodbConnStr_mainnet, block_mongodbDatabase_mainnet, "system_counter", "block", loop_interval_seconds, time_warn_threshold, emailHelper).loop());
            if (isTx) Task.Run(() => NotifyMonitorTask.create(mainnetStr, txMonitor, mh, block_mongodbConnStr_mainnet, block_mongodbDatabase_mainnet, "system_counter", "block", block_mongodbConnStr_mainnet, block_mongodbDatabase_mainnet, "system_counter", "tx", loop_interval_seconds, height_warn_threshold, emailHelper).loop());
            if (isNotify) Task.Run(() => NotifyMonitorTask.create(mainnetStr, notifyMonitor, mh, block_mongodbConnStr_mainnet, block_mongodbDatabase_mainnet, "system_counter", "notify", notify_mongodbConnStr_mainnet, notify_mongodbDatabase_mainnet, "contractRecord", "notify", loop_interval_seconds, height_warn_threshold, emailHelper).loop());
            if (isAnaly) Task.Run(() => NotifyMonitorTask.create(mainnetStr, analyMonitor, mh, block_mongodbConnStr_mainnet, block_mongodbDatabase_mainnet, "system_counter", "utxo", analy_mongodbConnStr_mainnet, analy_mongodbDatabase_mainnet, "system_counter", "utxoBalance", loop_interval_seconds, height_warn_threshold, emailHelper).loop());
            
            if (isBlock) Task.Run(() => BlockMonitorTask.create(testnetStr, blockMonitor, mh, block_mongodbConnStr_testnet, block_mongodbDatabase_testnet, "system_counter", "block", loop_interval_seconds, time_warn_threshold, emailHelper).loop());
            if (isTx) Task.Run(() => NotifyMonitorTask.create(testnetStr, txMonitor, mh, block_mongodbConnStr_testnet, block_mongodbDatabase_testnet, "system_counter", "block", block_mongodbConnStr_testnet, block_mongodbDatabase_testnet, "system_counter", "tx", loop_interval_seconds, height_warn_threshold, emailHelper).loop());
            if (isNotify) Task.Run(() => NotifyMonitorTask.create(testnetStr, notifyMonitor, mh, block_mongodbConnStr_testnet, block_mongodbDatabase_testnet, "system_counter", "notify", notify_mongodbConnStr_testnet, notify_mongodbDatabase_testnet, "contractRecord", "notify", loop_interval_seconds, height_warn_threshold, emailHelper).loop());
            if (isAnaly) Task.Run(() => NotifyMonitorTask.create(testnetStr, analyMonitor, mh, block_mongodbConnStr_testnet, block_mongodbDatabase_testnet, "system_counter", "utxo", analy_mongodbConnStr_testnet, analy_mongodbDatabase_testnet, "system_counter", "utxoBalance", loop_interval_seconds, height_warn_threshold, emailHelper).loop());
            

            while (true)
            {
                Thread.Sleep(1000 * 15);
            }
        }
    }
}
