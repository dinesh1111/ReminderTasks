using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReminderTasks
{
    public class Messages
    {
        public const string ValidationMessageWhenToRun = "WhenToRun should be like following 1 min, 2 mins, 1 hour, 2 hours, dateandtime";
        public const string ValidationMessageKeyNumber = "Key should be number";
        public const string ValidationMessageMinsNumber = "Mins should be number";
        public const string ValidationMessageKeyNotExists = "Key not exists";
        public const string ValidationMessageAddKeyExists = "Key already exists";
        public const string NoItemsToShow = "No items to show";
        public const string AddMultipleItemsMessage = "Add items with line seperator in AddMultiple.txt file. Save & Close -> Enter";
        public const string ErrorFoundWhileAddingMultiple = "Error Found while adding multiple";
        public const string DBLoadingIssueFound = "DB Loading Issue...";
        public static string DBFolderPath = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Items");
        public const string DBFieldSeperator = ":";
        public static string ErrorLogPath = @"ErrorLog.txt";
        public static string AddMultiplePath = @"AddMultiple.txt";
        public static string HelpCommands = "Type commands followed by 'ENTER'\r\n" +
                "add {name}\r\n" +
                "quickadd {name}\r\n" +
                "update {key}\r\n" +
                "delete {key}\r\n" +
                "display {today/tomorrow/name/all}\r\n" +                
                "open {key}\r\n" +
                "deletecompleted\r\n" +
                "pause {mins}\r\n" +
                "Press CTL+C to Terminate";
        public const string OperationCanceled = "Operation Canceled";
        public const string MainMethod = "Main Method";
        public const string DoWorkMethod = "Do Work Method";
        public static string ShowTodoPath = @"Todo.txt";
    }
}
