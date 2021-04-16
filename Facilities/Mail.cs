using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using WebPageRefresherC19.config;

namespace WebPageRefresherC19.Facilities
{
    static class Mail
    {
        public static void SendMail(Config config, string recipient, string subject, string body)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(config.MailCredential.SenderUsername);
                mail.To.Add(recipient);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(config.MailCredential.SenderUsername, config.MailCredential.SenderPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

        public static void SendBroadcastMail(Config config, string broadcastMessage)
        {
            foreach (var recipients in config.Recipients)
            {
                SendBroadcastMail(config, recipients, broadcastMessage);
            }
        }

        private static void SendBroadcastMail(Config config, string recipient, string broadcastMessage)
        {
            Logger.Log($"{broadcastMessage} => invio mail a {recipient}");

            SendMail(config, recipient,
                "News Vaccino Regione Toscana",
                $"{broadcastMessage} - {DateTime.Now}\n\n{Logger.logHistory}");
        }

        public static void SendLogMail(Config config)
        {
            SendMail(config, config.AdminEmail,
                "Log WebRefresherC19",
                Logger.logHistory);
        }

        public static void SendMailConfigIsChanged(Config config, Config previousConfig, int updateIndex)
        {
            SendMail(config, config.AdminEmail,
                $"Config has been changed #{updateIndex}",
                ConfigManager.PrettyJson(JsonConvert.SerializeObject(config)));

            var prevRecipients = previousConfig.Recipients;
            var currRecipients = config.Recipients;
            var diffRecipients = currRecipients.Where(cr => !prevRecipients.Contains(cr)).ToList();
            foreach (var newRecipient in diffRecipients)
            {
                SendMail(config, newRecipient, 
                    $"New Vaccino Regione Toscana", 
                    $"Sei stato aggiunto alla mailing list per ricevere notifiche relative alla prenotazione del vaccino anti covid sul portale @{config.VaccineUrl}");
            }
        }
    }
}
