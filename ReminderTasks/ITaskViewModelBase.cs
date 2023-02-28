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
        public void FillReminders()
        {
            foreach (var item in TaskViewModel.Instance.DictTasks)
            {
                if (DateTime.Now >= item.Value.TimeToRun)
                {
                    TaskViewModel.Instance.UpdateTimeToRun(item.Value);
                    if (!TaskViewModel.Instance.ReminderItems.ContainsKey(item.Key))
                    {
                        TaskViewModel.Instance.ReminderItems.TryAdd(item.Key, TaskViewModel.Instance.GetItemsAsText(GetItemsType.AliasLinkWithOnlyPassingKey, string.Empty, item.Key).Item1);
                    }
                    TaskViewModel.Instance.StartProcess(item.Value.Link);
                }
            }
        }
        public void ShowReminders()
        {
            string result = string.Empty;
            foreach (var item in TaskViewModel.Instance.ReminderItems)
            {
                result += item.Value + "\r\n";
            }
            if (result.Trim() != string.Empty)
            {
                File.WriteAllText(ShowTodoPath, result);
                TaskViewModel.Instance.StartProcess(ShowTodoPath);
            }
        }
        public void ClearReminders()
        {
            TaskViewModel.Instance.ReminderItems.Clear();
        }
        public bool NotInPause()
        {
            if (TaskViewModel.Instance.PauseCountInSeconds <= 0)
            {
                return true;
            }
            TaskViewModel.Instance.PauseCountInSeconds--;
            return false;
        }
    }
}
