using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReminderTasks
{
    public class TaskViewModel : TaskViewModelBase, ITaskViewModel
    {

        public int ShowReminderFileInMins = 60;
        public int DefaultShowReminderCountInSeconds = 0;
        public int PauseCountInSeconds = 0;
        public ConcurrentDictionary<string, string> ReminderItems = new ConcurrentDictionary<string, string>();
        

        public ConcurrentDictionary<string, TaskModel> DictTasks = new ConcurrentDictionary<string, TaskModel>();
        public Dictionary<string, int> whenToRunValidateStrings = new Dictionary<string, int>();
        public const int DefaultTimer = 15;

        public TaskViewModel()
        {

        }

        public static readonly object lockObject = new object();
        private static TaskViewModel instance;
        public static TaskViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            Console.WriteLine("Loading");
                            instance = new TaskViewModel();
                            instance.whenToRunValidateStrings.Add("days", 1440);
                            instance.whenToRunValidateStrings.Add("day", 1440);
                            instance.whenToRunValidateStrings.Add("hours", 60);
                            instance.whenToRunValidateStrings.Add("mins", 1);
                            instance.whenToRunValidateStrings.Add("hour", 60);
                            instance.whenToRunValidateStrings.Add("min", 1);
                            instance.LoadFromDB();
                            instance.SetShowReminderFileInMins();
                            Console.WriteLine("Ready");
                        }
                    }
                }
                return instance;
            }
        }

        public void SetShowReminderFileInMins()
        {
            DefaultShowReminderCountInSeconds = ShowReminderFileInMins * 60;
        }

        public string GetAliasName(string alias)
        {
            string aliasName = string.Empty;            
            TaskViewModel.Instance.WriteLine("Enter Alias Name / Optional");
            aliasName = Console.ReadLine();

            if (aliasName.Trim() == string.Empty)
            {
                aliasName = alias;
            }
            return aliasName;
        }

        public string GetLink()
        {
            TaskViewModel.Instance.WriteLine("Enter Link/Path");
            string link = Console.ReadLine();
            return link;
        }        

        public string GetWhenToRunUntilValidationSuccess()
        {
            string whenToRun = string.Empty;            
            TaskViewModel.Instance.WriteLine("Enter When To Run(Required)");
            whenToRun = Console.ReadLine();

            if (TaskViewModel.Instance.ValidateWhenToRun(whenToRun.Trim()) == string.Empty)
            {
                whenToRun = GetWhenToRunUntilValidationSuccess();                
            }
            return whenToRun;            
        }

        public string GetKeyUntilValidationSuccess(string keyParam)
        {            
            bool exitFound = false;
            if (TaskViewModel.Instance.ValidateKey(keyParam.Trim()) == string.Empty)
            {
                TaskViewModel.Instance.WriteLine("Enter Key (Required) Enter Exit to come out");
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
                return keyParam;
            }
            return string.Empty;
        }

        public int GetMinsUntilValidationSuccess(string param)
        {            
            bool exitFound = false;
            if (TaskViewModel.Instance.ValidateMins(param) == -1)
            {
                TaskViewModel.Instance.WriteLine("Enter mins (Required) Enter Exit to come out");
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
                TaskViewModel.Instance.WriteLine(ValidationMessageMinsNumber);                
            }
                
            return -1;
        }

        public string ValidateWhenToRun(string propertyValue)
        {
            string result = string.Empty;
           
            if (ValidateTimeToRun(propertyValue) != null)
            {
                return propertyValue;
            }
            else
            {

                TaskViewModel.Instance.WriteLine(ValidationMessageWhenToRun);
                result = string.Empty;
            }           

            return result;
        }

        public string ValidateKey(string propertyValue)
        {                            
            if (Instance.DictTasks.ContainsKey(propertyValue))
            {
                return propertyValue;
            }
            else
            {
                TaskViewModel.Instance.WriteLine(ValidationMessageKeyNotExists);                        
            }
            return string.Empty;
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
                TaskViewModel.Instance.WriteToErrorLog(ex.Message, "StartProcessMethod");
                ApplicationError = true;
            }            
        }

        public bool Add(string alias,string link, string whenToRun)
        {
            try
            {
                string key = GetMaxKey(Instance.DictTasks);
                if (!Instance.DictTasks.ContainsKey(key))
                {
                    TaskModel taskModel = new TaskModel(alias, link, whenToRun);
                    ParseAndFillTimeToRun(taskModel);
                    Instance.DictTasks.TryAdd(key, taskModel);

                    AddToItemsDB(key, alias, link, whenToRun);
                    return true;
                }
                else
                {
                    TaskViewModel.Instance.WriteToErrorLog(ValidationMessageAddKeyExists, "Add");
                    ApplicationError = true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                TaskViewModel.Instance.WriteToErrorLog(ex.Message, "Add");
                ApplicationError = true;                                
            }
            return false;
            
        }

        public void UpdateTimeToRun(TaskModel taskModel)
        {
            try
            {
                ParseAndFillTimeToRun(taskModel);
            }
            catch (Exception ex)
            {
                TaskViewModel.Instance.WriteToErrorLog(ex.Message, "UpdateTimeToRun");
                ApplicationError = true;
            }            
        }

        public void Delete(string key)
        {
            if(Instance.DictTasks.ContainsKey(key))
            {
                TaskModel value;
                Instance.DictTasks.TryRemove(key,out value);
                DeleteItemDB(key);
            }
            else
            {
                TaskViewModel.Instance.WriteToErrorLog(ValidationMessageAddKeyExists, "Delete");
                ApplicationError = true;
            }
        }

        public void Display(GetRemindersType remindersType, string text)
        {            
            if(Instance.DictTasks.Count ==0)
            {
                TaskViewModel.Instance.WriteLine(NoItemsToShow);
            }
            else if(remindersType == GetRemindersType.All)
            {
                (string, int) result = GetItemsAsText(GetItemsType.All,string.Empty);
                TaskViewModel.Instance.WriteLine(string.Format("{0}{1}{2}", result.Item1, "\r\nTotal Items: ",
                    result.Item2));
            }
            else if (remindersType == GetRemindersType.Today)
            {
                (string, int) result = GetItemsAsText(GetItemsType.TodayAliasLink,string.Empty);
                TaskViewModel.Instance.WriteLine(string.Format("{0}{1}{2}", result.Item1, "\r\nTotal Items: ",
                    result.Item2));
            }
            else if (remindersType == GetRemindersType.Tomorrow)
            {
                (string, int) result = GetItemsAsText(GetItemsType.TomorrowAliasLink,string.Empty);
                TaskViewModel.Instance.WriteLine(string.Format("{0}{1}{2}", result.Item1, "\r\nTotal Items: ",
                    result.Item2));
            }
            else if (remindersType == GetRemindersType.Name)
            {
                (string, int) result = GetItemsAsText(GetItemsType.AllWithPassingText, text);
                TaskViewModel.Instance.WriteLine(string.Format("{0}{1}{2}", result.Item1, "\r\nTotal Items: ",
                    result.Item2));
            }
        }

        public void ShowHelpCommands()
        {
            TaskViewModel.Instance.WriteLine(HelpCommands);            
        }

        public string GetMaxKey(ConcurrentDictionary<string,TaskModel> dictTasks)
        {
            int maxKey = 0;
            foreach (string key in dictTasks.Keys)
            {
                if(Convert.ToInt32(key) > maxKey)
                {
                    maxKey = Convert.ToInt32(key);
                }
            }
            return (maxKey + 1).ToString();
        }

        public void AddToItemsDB(string key, string alias, string link, string whenToRun)
        {
            string writeResult = string.Empty;
            writeResult = string.Format("{0}{1}{2}{3}", nameof(TaskModel.Alias), DBFieldSeperator, alias, "\r\n");
            writeResult += string.Format("{0}{1}{2}{3}", nameof(TaskModel.Link), DBFieldSeperator, link, "\r\n");
            writeResult += string.Format("{0}{1}{2}{3}", nameof(TaskModel.WhenToRun), DBFieldSeperator, whenToRun, "\r\n");
            File.WriteAllText(Path.Combine(DBFolderPath,key.ToString()+".txt"), writeResult);
        }

        public void DeleteItemDB(string key)
        {            
            File.Delete(Path.Combine(DBFolderPath, key.ToString()+".txt"));
        }

        public (string,int) GetItemsAsText(GetItemsType ItemsType,string text, string dictKey="")
        {
            string Items = string.Empty;
            int totalCount = 0;
            foreach (var item in Instance.DictTasks)
            {
                if(GetItemsType.All == ItemsType)
                {
                    totalCount = totalCount + 1;
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.Key), DBFieldSeperator, item.Key, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.Alias), DBFieldSeperator, item.Value.Alias, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.Link), DBFieldSeperator, item.Value.Link, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.WhenToRun), DBFieldSeperator, item.Value.WhenToRun, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.TimeToRun), DBFieldSeperator, item.Value.TimeToRun, "\r\n");                    
                    Items += "\r\n";
                }
                else if(GetItemsType.AllWithPassingText == ItemsType && (item.Value.Alias.ToLower().Contains(text.ToLower())|| item.Value.Link.ToLower().Contains(text.ToLower())))
                {
                    totalCount = totalCount + 1;
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.Key), DBFieldSeperator, item.Key, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.Alias), DBFieldSeperator, item.Value.Alias, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.Link), DBFieldSeperator, item.Value.Link, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.WhenToRun), DBFieldSeperator, item.Value.WhenToRun, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TaskModel.TimeToRun), DBFieldSeperator, item.Value.TimeToRun, "\r\n");
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
                if(!Directory.Exists(DBFolderPath))
                {
                    Directory.CreateDirectory(DBFolderPath);
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(DBFolderPath);

                foreach (FileInfo file in directoryInfo.GetFiles("*.txt"))
                {
                    string Items = File.ReadAllText(file.FullName);
                    bool DBIssueFound = false;
                    
                    string alias = string.Empty;
                    string link = string.Empty;
                    string whenToRun = string.Empty;

                    foreach (string item in Items.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (item.StartsWith(nameof(TaskModel.Alias)))
                        {
                            alias = item.Replace(nameof(TaskModel.Alias) + DBFieldSeperator, string.Empty);
                        }
                        else if (item.StartsWith(nameof(TaskModel.Link)))
                        {
                            link = item.Replace(nameof(TaskModel.Link) +  DBFieldSeperator, string.Empty);
                        }
                        else if (item.StartsWith(nameof(TaskModel.WhenToRun)))
                        {
                            whenToRun = item.Replace(nameof(TaskModel.WhenToRun) + DBFieldSeperator, string.Empty);
                        }
                        else
                        {
                            TaskViewModel.Instance.WriteLine(DBLoadingIssueFound + alias + link + whenToRun);
                            DBIssueFound = true;
                            break;
                        }
                         
                    }
                    
                    if (!DBIssueFound)
                    {
                        if (alias != string.Empty || link != string.Empty || whenToRun != string.Empty)
                        {                            
                            if (!Instance.DictTasks.ContainsKey(file.Name))
                            {
                                TaskModel taskModel = new TaskModel(alias, link, whenToRun);
                                ParseAndFillTimeToRun(taskModel);
                                Instance.DictTasks.TryAdd(file.Name.Replace(".txt",string.Empty), taskModel);
                            }
                        }                        
                    }
                    else
                    {
                        ApplicationError = true;
                        break;
                    }
                }                
            }
            catch (Exception ex)
            {
                TaskViewModel.Instance.WriteToErrorLog(ex.Message, "StartProcessMethod");
                ApplicationError = true;
            }            
        }

        public void WriteToErrorLog(string message, string fromMethod)
        {
            if(File.Exists(ErrorLogPath))
            {
                File.Create(ErrorLogPath);
            }
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
       
        public void ParseAndFillTimeToRun(TaskModel taskModel)
        {
            string[] splitPropertyValue = taskModel?.WhenToRun.Split(' ');

            if (splitPropertyValue.Length == 2)
            {
                if (instance.whenToRunValidateStrings.ContainsKey(splitPropertyValue.GetValue(1).ToString()))
                {
                    taskModel.FillTimeToRun(DateTime.Now.AddMinutes(Convert.ToInt32(splitPropertyValue.GetValue(0).ToString()) *
                        instance.whenToRunValidateStrings[splitPropertyValue.GetValue(1).ToString()]));
                }
            }
            else if (taskModel.WhenToRun.Contains("/"))
            {
                DateTime dateTime = Convert.ToDateTime(taskModel.WhenToRun);
                if (DateTime.Now < dateTime)
                {
                    taskModel.FillTimeToRun(dateTime);
                }
                else
                {
                    taskModel.FillTimeToRun(DateTime.Now.AddMinutes(DefaultTimer));
                }
            }

        }      

        public bool ValidateTimeToRun(string whenToRun)
        {
            string[] splitPropertyValue = whenToRun.Split(' ');

            if (splitPropertyValue.Length == 2)
            {
                if (instance.whenToRunValidateStrings.ContainsKey(splitPropertyValue.GetValue(1).ToString()))
                {
                    return true;
                }
            }
            else if (whenToRun.Contains("/"))
            {
                DateTime dateTime = Convert.ToDateTime(whenToRun);
                if (DateTime.Now < dateTime)
                {
                    return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public void ApplyPause(string value)
        {
            try
            {
                int mins = Convert.ToInt32(value);
                TaskViewModel.Instance.PauseCountInSeconds = mins * 60;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Enter number in mins");
            }            
        }
    }
}
