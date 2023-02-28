using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderTasks
{
    public interface ITaskViewModel
    {
        public void ParseAndFillTimeToRun(TaskModel taskModel);
        public bool ValidateTimeToRun(string whenToRun);
        public void ApplyPause(string mins);
    }
}
