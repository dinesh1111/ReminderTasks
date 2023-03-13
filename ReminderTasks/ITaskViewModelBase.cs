using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReminderTasks
{
    public abstract class ITaskViewModelBase:Messages
    {        
        public void UpdateReminderTime()
        {
            foreach (var item in TaskViewModel.Instance.DictTasks)
            {
                if (DateTime.Now >= item.Value.TimeToRun)
                {
                    TaskViewModel.Instance.UpdateTimeToRun(item.Value);
                }
            }
        }

        public void StartLinkIfTimeElapsed()
        {
            foreach (var item in TaskViewModel.Instance.DictTasks)
            {
                if (DateTime.Now >= item.Value.TimeToRun)
                {
                    TaskViewModel.Instance.StartProcess(item.Value.Link);
                }
            }
        }

        public void ShowReminders()
        {
            string now = "----Today----\r\n";
            string later = "----Later----\r\n";
            int count = 0;
            foreach (var item in TaskViewModel.Instance.DictTasks)
            {
                count = count + 1;
                if (DateTime.Now >= item.Value.TimeToRun || DateTime.Now.ToShortDateString() == Convert.ToDateTime(item.Value.TimeToRun).ToShortDateString())
                {
                    now += count + "." + item.Value.Alias + "\r\n"+item.Value.Link+"\r\n";
                }
                else
                {
                    later += count + "."+ item.Value.Alias + "\r\n" + item.Value.Link+"\r\n";
                }
            }
            if (now.Trim() != string.Empty)
            {
                File.WriteAllText(ShowTodoPath, now+"\r\n"+"\r\n"+later);
                TaskViewModel.Instance.StartProcess(ShowTodoPath);
            }
        }

        public void SendRemindersEmail(string toEmail)
        {
            try
            {

                var email = new MimeMessage();

                email.From.Add(new MailboxAddress("Sender Name", "noreply@gmail.com"));
                email.To.Add(new MailboxAddress("Receiver Name", toEmail));

                email.Subject = "Reminders";
                string today = "-----Today-----<br/>";
                string later = "-----Later-----<br/>";
                foreach (var item in TaskViewModel.Instance.DictTasks)
                {
                    if (item.Value.TimeToRun?.ToShortDateString() == DateTime.Now.ToShortDateString())
                    {
                        today += "<br/>" + item.Value.Alias + "<br/>";
                    }
                    else
                    {
                        later += "<br/>" + item.Value.Alias + "<br/>";
                    }                    
                }
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = today + "\r\n"+ later
                };

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    smtp.Connect("smtp.gmail.com", 587, false);

                    // Note: only needed if the SMTP server requires authentication
                    smtp.Authenticate("dinesh.ms.net.add@gmail.com", "hrajwynoyxyytxft");

                    smtp.Send(email);
                    smtp.Disconnect(true);
                }
            }
            catch
            {

            }
        }
    }
}
