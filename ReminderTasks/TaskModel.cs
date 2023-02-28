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
}
