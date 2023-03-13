using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderTasks
{
    public class TaskModel
    {
        public int Key
        {
            get; private set;
        }
        public string Alias
        {
            get; private set;
        }
        public string Link
        {
            get; private set;
        }
        public string WhenToRun
        {
            get; private set;
        }
        public DateTime? TimeToRun
        {
            get; private set;
        }        

        public TaskModel(string alias, string link, string whenToRun)
        {
            Alias = alias;
            Link = link;
            WhenToRun = whenToRun;                     
        }

        public void FillTimeToRun(DateTime? timeToRun)
        {
            TimeToRun = timeToRun;
        }
    }

    public class SettingsModel
    {
        public int Key
        {
            get; private set;
        }
        public string Email
        {
            get; private set;
        }
        public int? EmailFreequency
        {
            get; private set;
        }
        public int? NotesFreequency
        {
            get; private set;
        }
        public bool AutoOpenLink
        {
            get;private set;
        }

        public SettingsModel(string email,int? emailFreequency, int? notesFreequency, bool autoOpenLink)
        {
            Email = email;
            EmailFreequency = emailFreequency;
            NotesFreequency = notesFreequency;
            AutoOpenLink = autoOpenLink;
        }
    }
}
