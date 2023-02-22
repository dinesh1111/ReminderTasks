using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReminderTasks
{
    public enum GetItemsType
    {
        All,
        AllWithPassingText,
        AliasLinkWithOnlyPassingKey,
        AliasLinkWhenToRun,
        TodayAliasLink,
        TomorrowAliasLink,
    }
    public enum GetRemindersType
    {
        All,
        Today,
        Tomorrow,
        Name
    }
    public sealed class TaskModel
    {
        private const string ValidationMessageWhenToRun = "WhenToRun should be like following 1 min, 2 mins, 1 hour, 2 hours, dateandtime";
        private const string ValidationMessageKeyNumber = "Key should be number";
        private const string ValidationMessageMinsNumber = "Mins should be number";
        private const string ValidationMessageKeyNotExists = "Key not exists";
        private const string ValidationMessageAddKeyExists = "Key already exists";
        private const string NoItemsToShow = "No items to show";
        private const string AddMultipleItemsMessage = "Add items with line seperator in AddMultiple.txt file. Save & Close -> Enter";
        private const string ErrorFoundWhileAddingMultiple = "Error Found while adding multiple";
        private const string DBLoadingError = "DB Loading issue";
        private const string DBLoadedSuccessfully = "DB Loaded Successfully";
        private static string DBPath = @"DB.txt";
        private const string DBItemsSeperator = "**********";        
        private const string DBFieldSeperator = ":";
        private static string ErrorLogPath = @"ErrorLog.txt";
        private static string AddMultiplePath = @"AddMultiple.txt";
        public int ShowReminderFileInMins = 60;
        public int DefaultShowReminderCountInSeconds = 0;
        public Dictionary<int, string> ReminderItems = new Dictionary<int, string>();
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

        public Dictionary<int, TaskModel> DictTasks = new Dictionary<int, TaskModel>();
        public Dictionary<string, int> whenToRunValidateStrings = new Dictionary<string, int>();
        private const int DefaultTimer = 15;

        private TaskModel()
        {

        }

        private TaskModel(string alias, string link, string whenToRun)
        {            
            Alias = alias;
            Link = link;
            WhenToRun = whenToRun;
            TimeToRun = ParseWhenToRunToTimeToRun(whenToRun);
        }

        private static readonly object lockObject = new object();
        private static TaskModel instance;

        public static TaskModel Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new TaskModel();
                            instance.whenToRunValidateStrings.Add("days", 1440);
                            instance.whenToRunValidateStrings.Add("day", 1440);
                            instance.whenToRunValidateStrings.Add("hours", 60);
                            instance.whenToRunValidateStrings.Add("mins", 1);
                            instance.whenToRunValidateStrings.Add("hour", 60);
                            instance.whenToRunValidateStrings.Add("min", 1);
                            instance.LoadFromDB();
                            instance.SetShowReminderFileInMins();
                        }
                    }                    
                }
                return instance;
            }
        }

        public void SetShowReminderFileInMins()
        {
            DefaultShowReminderCountInSeconds = TaskModel.Instance.ShowReminderFileInMins * 60;
        }        

        public string GetAliasName(string alias)
        {
            string aliasName = string.Empty;            
            TaskModel.Instance.WriteLine("Enter Alias Name / Optional");
            aliasName = Console.ReadLine();

            if (aliasName.Trim() == string.Empty)
            {
                aliasName = alias;
            }
            return aliasName;
        }

        public string GetLink()
        {
            TaskModel.Instance.WriteLine("Enter Link/Path");
            string link = Console.ReadLine();
            return link;
        }        

        public string GetWhenToRunUntilValidationSuccess()
        {
            string whenToRun = string.Empty;            
            TaskModel.Instance.WriteLine("Enter When To Run(Required)");
            whenToRun = Console.ReadLine();

            if (TaskModel.Instance.ValidateWhenToRun(whenToRun.Trim()) == string.Empty)
            {
                whenToRun = GetWhenToRunUntilValidationSuccess();                
            }
            return whenToRun;            
        }

        public int GetKeyUntilValidationSuccess(string keyParam)
        {            
            bool exitFound = false;
            if (TaskModel.Instance.ValidateKey(keyParam.Trim()) == -1)
            {
                TaskModel.Instance.WriteLine("Enter Key (Required) Enter Exit to come out");
                keyParam = Console.ReadLine();

                if (keyParam.ToLower().Trim() == "exit")
                {
                    exitFound = true;
                }
                else
                {
                    keyParam = GetKeyUntilValidationSuccess(keyParam).ToString();
                }
            }
            if (!exitFound)
            {
                return Convert.ToInt32(keyParam);
            }
            return -1;
        }

        public int GetMinsUntilValidationSuccess(string param)
        {            
            bool exitFound = false;
            if (TaskModel.Instance.ValidateMins(param) == -1)
            {
                TaskModel.Instance.WriteLine("Enter mins (Required) Enter Exit to come out");
                param = Console.ReadLine();
                if (param.ToLower().Trim() == "exit")
                {
                    exitFound = true;                    
                }
                else
                {
                    param = GetMinsUntilValidationSuccess(param).ToString();
                }
            }
            if (!exitFound)
            {
                return Convert.ToInt32(param);
            }
            return -1;
        }

        public int ValidateMins(string propertyValue)
        {            
            int mins;
            bool success = int.TryParse(propertyValue, out mins);
            if (success)
            {
                return mins;
            }
            else
            {
                TaskModel.Instance.WriteLine(ValidationMessageMinsNumber);                
            }
                
            return -1;
        }

        public string ValidateWhenToRun(string propertyValue)
        {
            string result = string.Empty;
           
            if (ParseWhenToRunToTimeToRun(propertyValue) != null)
            {
                return propertyValue;
            }
            else
            {

                TaskModel.Instance.WriteLine(ValidationMessageWhenToRun);
                result = string.Empty;
            }           

            return result;
        }

        public int ValidateKey(string propertyValue)
        {            
                
            int key;
            bool success = int.TryParse(propertyValue, out key);
                
            if (success)
                {
                    if (Instance.DictTasks.ContainsKey(key))
                    {
                        return key;
                    }
                    else
                    {
                        TaskModel.Instance.WriteLine(ValidationMessageKeyNotExists);                        
                    }
                }
                else
                {
                    TaskModel.Instance.WriteLine(ValidationMessageKeyNumber);                    
                }
            
            return -1;
        }

        private DateTime? ParseWhenToRunToTimeToRun(string whenToRun)
        {
            try
            {
                string[] splitPropertyValue = whenToRun.Split(' ');

                if (splitPropertyValue.Length == 2)
                {
                    if (instance.whenToRunValidateStrings.ContainsKey(splitPropertyValue.GetValue(1).ToString()))
                    {
                        return DateTime.Now.AddMinutes(Convert.ToInt32(splitPropertyValue.GetValue(0).ToString()) *
                            instance.whenToRunValidateStrings[splitPropertyValue.GetValue(1).ToString()]);                        
                    }
                }
                else if (whenToRun.Contains("/"))
                {
                    DateTime dateTime = Convert.ToDateTime(whenToRun);
                    if (DateTime.Now < dateTime)
                    {
                        return dateTime;
                    }
                    else
                    {
                        return DateTime.Now.AddMinutes(DefaultTimer);
                    }            
                }
            }
            catch
            {                
                
            }            
            return null;
        }

        public void StartProcess(string path)
        {
            try
            {
                if (path != string.Empty)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(ex.Message, "StartProcessMethod");
            }            
        }

        public bool Add(string alias,string link, string whenToRun)
        {
            int key = GetMaxKey(Instance.DictTasks);
            if(!Instance.DictTasks.ContainsKey(key))
            {                
                Instance.DictTasks.Add(key, new TaskModel(alias, link, whenToRun));
                WriteToDB();
                return true;
            }
            else
            {
                TaskModel.Instance.WriteLine(ValidationMessageAddKeyExists);
                return false;
            }
        }        

        public bool Update(int key, string alias, string link, string whenToRun)
        {                        
            if (Instance.DictTasks.ContainsKey(key))
            {               
                Instance.DictTasks[key] = new TaskModel(alias,link,whenToRun);
                WriteToDB();
                return true;
            }
            else
            {
                TaskModel.Instance.WriteLine(ValidationMessageKeyNotExists);
                return false;
            }
        }

        public void Delete(int key)
        {
            if(Instance.DictTasks.ContainsKey(key))
            {
                Instance.DictTasks.Remove(key);
                WriteToDB();
            }
            else
            {
                TaskModel.Instance.WriteLine(ValidationMessageKeyNotExists);                
            }
        }

        public void Display(GetRemindersType remindersType, string text)
        {            
            if(Instance.DictTasks.Count ==0)
            {
                TaskModel.Instance.WriteLine(NoItemsToShow);
            }
            else if(remindersType == GetRemindersType.All)
            {
                (string, int) result = GetItemsAsText(GetItemsType.All,string.Empty);
                TaskModel.Instance.WriteLine(string.Format("{0}{1}{2}", result.Item1, "\r\nTotal Items: ",
                    result.Item2));
            }
            else if (remindersType == GetRemindersType.Today)
            {
                (string, int) result = GetItemsAsText(GetItemsType.TodayAliasLink,string.Empty);
                TaskModel.Instance.WriteLine(string.Format("{0}{1}{2}", result.Item1, "\r\nTotal Items: ",
                    result.Item2));
            }
            else if (remindersType == GetRemindersType.Tomorrow)
            {
                (string, int) result = GetItemsAsText(GetItemsType.TomorrowAliasLink,string.Empty);
                TaskModel.Instance.WriteLine(string.Format("{0}{1}{2}", result.Item1, "\r\nTotal Items: ",
                    result.Item2));
            }
            else if (remindersType == GetRemindersType.Name)
            {
                (string, int) result = GetItemsAsText(GetItemsType.AllWithPassingText,text);
                TaskModel.Instance.WriteLine(string.Format("{0}{1}{2}", result.Item1, "\r\nTotal Items: ",
                    result.Item2));
            }
        }

        public void ShowHelpCommands()
        {
            TaskModel.Instance.WriteLine("Type commands followed by 'ENTER'\r\n" +
                "add {name}\r\n" +
                "quickadd {name}\r\n" +
                "update {key}\r\n" +
                "delete {key}\r\n" +
                "display {today/tomorrow/name/all}\r\n" +
                "setdefaultreminderstime {mins}\r\n" +
                "open {key}\r\n" +
                "deletecompleted\r\n" +
                "updateall\r\n" +
                "Press CTL+C to Terminate");            
        }

        public int GetMaxKey(Dictionary<int,TaskModel> dictTasks)
        {
            int maxKey = 0;
            foreach (int key in dictTasks.Keys)
            {
                if(key>maxKey)
                {
                    maxKey = key;
                }
            }
            return maxKey + 1;
        }

        public void WriteToDB()
        {
            string items = GetItemsAsText(GetItemsType.AliasLinkWhenToRun,string.Empty).Item1;
            File.WriteAllText(DBPath, items);
        }

        public (string,int) GetItemsAsText(GetItemsType ItemsType,string text, int dictKey=0)
        {
            string Items = string.Empty;
            int totalCount = 0;
            foreach (var item in Instance.DictTasks)
            {
                if(GetItemsType.All == ItemsType)
                {
                    totalCount = totalCount + 1;
                    Items += string.Format("{0}{1}{2}{3}", nameof(Key), DBFieldSeperator, item.Key, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(Alias), DBFieldSeperator, item.Value.Alias, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(Link), DBFieldSeperator, item.Value.Link, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(WhenToRun), DBFieldSeperator, item.Value.WhenToRun, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TimeToRun), DBFieldSeperator, item.Value.TimeToRun, "\r\n");                    
                    Items += "\r\n";
                }
                else if(GetItemsType.AllWithPassingText == ItemsType && (item.Value.Alias.ToLower().Contains(text.ToLower())|| item.Value.Link.ToLower().Contains(text.ToLower())))
                {
                    totalCount = totalCount + 1;
                    Items += string.Format("{0}{1}{2}{3}", nameof(Key), DBFieldSeperator, item.Key, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(Alias), DBFieldSeperator, item.Value.Alias, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(Link), DBFieldSeperator, item.Value.Link, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(WhenToRun), DBFieldSeperator, item.Value.WhenToRun, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TimeToRun), DBFieldSeperator, item.Value.TimeToRun, "\r\n");
                    Items += "\r\n";
                }
                else if (GetItemsType.AliasLinkWhenToRun == ItemsType)
                {
                    totalCount = totalCount + 1;
                    Items += string.Format("{0}{1}{2}{3}", nameof(Alias), DBFieldSeperator, item.Value.Alias, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(Link), DBFieldSeperator, item.Value.Link, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(WhenToRun), DBFieldSeperator, item.Value.WhenToRun, "\r\n");                    
                    Items += DBItemsSeperator;
                    Items += "\r\n";
                }
                else if (GetItemsType.AliasLinkWithOnlyPassingKey == ItemsType && dictKey==item.Key)
                {
                    totalCount = totalCount + 1;
                    Items += string.Format("{0}{1}", item.Value.Alias, "\r\n");
                    if (item.Value.Link != string.Empty)
                    {
                        Items += string.Format("{0}{1}", item.Value.Link, "\r\n");
                    }
                    Items += "\r\n";
                }
                else if (GetItemsType.TodayAliasLink == ItemsType && (
                    (item.Value.WhenToRun.Contains("min") || item.Value.WhenToRun.Contains("mins") ||
                    item.Value.WhenToRun.Contains("hour") || item.Value.WhenToRun.Contains("hours")) ||
                    Convert.ToDateTime(item.Value.TimeToRun.ToString().Contains("/") ? item.Value.TimeToRun.ToString() : DateTime.Now.ToString()).ToShortDateString() == DateTime.Now.ToShortDateString())
                    )
                {
                    totalCount = totalCount + 1;
                    Items += string.Format("{0}{1}", item.Value.Alias, "\r\n");
                    if(item.Value.Link!=string.Empty) {
                        Items += string.Format("{0}{1}", item.Value.Link, "\r\n");
                    }                    
                    Items += "\r\n";
                }
                else if (GetItemsType.TomorrowAliasLink == ItemsType && (
                    (item.Value.WhenToRun.Contains("min") || item.Value.WhenToRun.Contains("mins") ||
                    item.Value.WhenToRun.Contains("hour") || item.Value.WhenToRun.Contains("hours")) ||
                    Convert.ToDateTime(item.Value.TimeToRun.ToString().Contains("/")? item.Value.TimeToRun.ToString():DateTime.Now.ToString()).ToShortDateString() == DateTime.Now.AddDays(1).ToShortDateString())
                    )
                {
                    totalCount = totalCount + 1;
                    Items += string.Format("{0}{1}", item.Value.Alias, "\r\n");
                    if (item.Value.Link != string.Empty)
                    {
                        Items += string.Format("{0}{1}", item.Value.Link, "\r\n");
                    }
                    Items += "\r\n";
                }
            }
            return (Items,totalCount);
        }

        public void LoadFromDB()
        {
            try
            {                
                string Items = File.ReadAllText(DBPath);
                string[] SplitItems = Items.Split(DBItemsSeperator, StringSplitOptions.RemoveEmptyEntries);
                bool DBIssueFound = false;

                if (Items.Contains(DBItemsSeperator))
                {
                    foreach (string items in SplitItems)
                    {
                        string alias = string.Empty;
                        string link = string.Empty;
                        string whenToRun = string.Empty;
                        foreach (string item in items.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (item.StartsWith(nameof(Alias)))
                            {
                                alias = item.Replace(nameof(Alias) + DBFieldSeperator, string.Empty);
                            }
                            else if (item.StartsWith(nameof(Link)))
                            {
                                link = item.Replace(nameof(Link) + DBFieldSeperator, string.Empty);
                            }
                            else if (item.StartsWith(nameof(WhenToRun)))
                            {
                                whenToRun = item.Replace(nameof(WhenToRun) + DBFieldSeperator, string.Empty);
                            }
                            else
                            {
                                TaskModel.Instance.WriteLine(DBLoadingError + alias + link + whenToRun);
                                DBIssueFound = true; ;
                            }  
                        }
                        if (!DBIssueFound)
                        {
                            if (alias != string.Empty || link != string.Empty || whenToRun != string.Empty)
                            {
                                int key = GetMaxKey(Instance.DictTasks);
                                if (!Instance.DictTasks.ContainsKey(key))
                                {
                                    Instance.DictTasks.Add(key, new TaskModel(alias, link, whenToRun));
                                }
                            }
                        }
                    }
                    if (!DBIssueFound)
                    {
                        TaskModel.Instance.WriteLine(DBLoadedSuccessfully);
                    }
                    File.WriteAllText(DBPath, Items);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(ex.Message, "LoadFromDB");
            }            
        }

        public void WriteToErrorLog(string message, string fromMethod)
        {
            string fileText = File.ReadAllText(ErrorLogPath);
            fileText += string.Format("{0}{1}{2}{3}{4}{5}","\r\n" , "Message: ", message , "\r\n" ,"From Method: ", fromMethod);
            File.WriteAllText(ErrorLogPath, fileText);
        }

        public void WriteLine(string value)
        {
            Console.WriteLine("*****************************");
            Console.WriteLine(value);
            Console.WriteLine("*****************************");
        }
    }
}
