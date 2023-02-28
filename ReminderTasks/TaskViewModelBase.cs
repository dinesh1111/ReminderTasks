using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderTasks
{
    public enum GetItemsType
    {
        All,
        AliasLinkWithOnlyPassingKey,
        TodayAliasLink,
        TomorrowAliasLink,
        AllWithPassingText
    }
    public enum GetRemindersType
    {
        All,
        Today,
        Tomorrow,
        Name
    }
    public class TaskViewModelBase:ITaskViewModelBase
    {
        public bool ApplicationError
        { 
            get; set; 
        }        
    }
}
