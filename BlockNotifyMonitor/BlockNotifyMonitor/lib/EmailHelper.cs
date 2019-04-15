using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockNotifyMonitor
{
    class EmailHelper
    {
        public EmailConfig config { get; set; }
        public void send(string messsage) => send(messsage, config);
        public void send(string message, EmailConfig config)
        {
            //初始化
            var
            client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.Connect(config.smtpHost, config.smtpPort, config.smtpEnableSsl);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(config.mailFrom, config.mailPwd);

            // 构造msg
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("sccot", config.mailFrom));

            string toEmail = config.listener;
            if (toEmail.IndexOf(",") >= 0)
            {
                var toArr = toEmail.Split(",");
                toEmail = toArr[0];
                if (toArr.Length > 1)
                {
                    foreach (var addr in toArr.Skip(1).ToArray())
                    {
                        msg.Cc.Add(new MailboxAddress("", addr));
                    }
                }
            }
            msg.To.Add(new MailboxAddress("listener", toEmail));

            msg.Subject = config.subject;
            msg.Body = new TextPart("plain") { Text = string.Format(config.body, message) };

            // 发送
            client.Send(msg);
        }
    }
    class EmailConfig
    {
        public string mailFrom { get; set; }
        public string mailPwd { get; set; }
        public string smtpHost { get; set; }
        public int smtpPort { get; set; }
        public bool smtpEnableSsl { get; set; } = false;
        public string subject { get; set; }
        public string body { get; set; }
        public string listener { get; set; }
    }
}
