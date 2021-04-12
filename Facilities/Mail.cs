using Newtonsoft.Json;
using System;
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

        public static void SendSuccessMail(Config config)
        {
            foreach (var recipients in config.Recipients)
            {
                SendSuccessMail(config, recipients);
            }
        }

        public static void SendSuccessMail(Config config, string recipient)
        {
            Logger.Log($"Vaccino disponibile => invio mail to {recipient}");

            SendMail(config, recipient,
                "Vaccino disponibile",
                $"Il vaccino è disponibile per la prenotazione dalle {DateTime.Now}\n\n{Logger.logHistory}");
        }

        public static void SendLogMail(Config config)
        {
            SendMail(config, config.AdminEmail,
                "Log WebRefresherC19",
                Logger.logHistory);
        }

        public static void SendMailConfigIsChanged(Config config, int updateIndex)
        {
            SendMail(config, config.AdminEmail,
                $"Config has been changed #{updateIndex}",
                ConfigManager.PrettyJson(JsonConvert.SerializeObject(config)));
        }
    }
}
