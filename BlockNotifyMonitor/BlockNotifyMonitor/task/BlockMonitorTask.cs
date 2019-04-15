using System;
using System.Threading;

namespace BlockNotifyMonitor.task
{
    class BlockMonitorTask : NotifyMonitorTask
    {

        public static BlockMonitorTask create(

            string network,
            string taskname,
            //
            MongoDBHelper mh,
            string mongodbConnStr,
            string mongodbDatabase,
            string collection,
            string key,
            int interval,
            int threshold,
            //
            EmailHelper emailHelper
            ) => new BlockMonitorTask
            {

                network = network,
                taskname = taskname,
                //
                mh = mh,
                upmongodbConnStr = "",
                upmongodbDatabase = "",
                upcollection = "",
                upkey = "",
                mongodbConnStr = mongodbConnStr,
                mongodbDatabase = mongodbDatabase,
                collection = collection,
                key = key,
                interval = interval,
                threshold = threshold,
                //
                emailHelper = emailHelper
            };


        protected override void process()
        {
            var bh = getDataCounter();
            var tm = DateTime.Now;
            while (true)
            {
                ping();
                Thread.Sleep(1000 * interval);
                var nbh = getDataCounter();
                var ntm = DateTime.Now;
                if (bh != nbh)
                {
                    bh = nbh;
                    tm = ntm;
                    continue;
                }

                if ((ntm - tm).TotalSeconds > threshold)
                {
                    string message = string.Format("{0}  {1}_{2} 区块高度不变超过时间阈值: {3}(s), 此时高度: {4}", DateTime.Now.ToString("u"), network, taskname, threshold, nbh);
                    send(message);
                }
            }
        }
    }
}
