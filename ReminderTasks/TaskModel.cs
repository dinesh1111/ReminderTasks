using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReminderTasks
{
    public enum GetItemsType
    {
        All,
        AllWithPassingKey,
        AliasLinkWithOnlyPassingKey,
        AliasLinkWhenToRun
    }
    public sealed class TaskModel
    {
        private const string ValidationMessageWhenToRun = "WhenToRun should be like following 1 min, 2 mins, 1 hour, 2 hours, dateandtime";
        private const string ValidationMessageKeyNumber = "Key should be number";
        private const string ValidationMessageKeyNotExists = "Key not exists";
        private const string ValidationMessageAddKeyExists = "Key already exists";
        private const string NoItemsToShow = "No items to show";
        private const string AddMultipleItemsMessage = "Add items with line seperator in AddMultiple.txt file. Save & Close -> Enter";
        private const string ErrorFoundWhileAddingMultiple = "Error Found while adding multiple";
        private const string DBLoadingError = "DB Loading issue";
        private const string DBLoadedSuccessfully = "DB Loaded Successfully";
        private static string DBPath = @"DB.txt";
        private const string DBItemsSeperator = "**********";
        private const string DBItemsDisplaySeperator = "----------";
        private const string DBFieldSeperator = ":";
        private static string ErrorLogPath = @"ErrorLog.txt";
        private static string AddMultiplePath = @"AddMultiple.txt";

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
                            instance.whenToRunValidateStrings.Add("hours", 60);
                            instance.whenToRunValidateStrings.Add("mins", 1);
                            instance.whenToRunValidateStrings.Add("hour", 60);
                            instance.whenToRunValidateStrings.Add("min", 1);
                            instance.LoadFromDB();
                        }
                    }                    
                }
                return instance;
            }
        }

        public string GetAliasName()
        {
            string aliasName = string.Empty;
            Console.WriteLine("Enter Alias Name / Optional");
            aliasName = Console.ReadLine();

            if (aliasName.Trim() == string.Empty)
            {
                aliasName = "Default";
            }
            return aliasName;
        }

        public string GetAliasName(string alias)
        {
            string aliasName = string.Empty;
            Console.WriteLine("Enter Alias Name / Optional");
            aliasName = Console.ReadLine();

            if (aliasName.Trim() == string.Empty)
            {
                aliasName = alias;
            }
            return aliasName;
        }

        public string GetLink()
        {
            Console.WriteLine("Enter Link/Path");
            string link = Console.ReadLine();
            return link;
        }        

        public string GetWhenToRunUntilValidationSuccess()
        {
            string whenToRun = string.Empty;
            Console.WriteLine("Enter When To Run(Required)");
            whenToRun = Console.ReadLine();

            if (TaskModel.Instance.Validate(nameof(WhenToRun), whenToRun.Trim()) == string.Empty)
            {
                whenToRun = GetWhenToRunUntilValidationSuccess();
            }
            return whenToRun;
        }

        public string GetKeyUntilValidationSuccess()
        {
            Console.WriteLine("Enter Key (Required)");
            string key = Console.ReadLine();


            if (TaskModel.Instance.Validate(nameof(Key), key.Trim()) == string.Empty)
            {
                key = GetKeyUntilValidationSuccess();
            }
            return key;
        }

        public string Validate(string propertyName, string propertyValue)
        {
            string result = string.Empty;

            if(propertyName== nameof(WhenToRun))
            {
                if(ParseWhenToRunToTimeToRun(propertyValue) !=null)
                {
                    return propertyValue;
                }
                else
                {
                    
                    Console.WriteLine(ValidationMessageWhenToRun);
                    result = string.Empty;
                }
            }

            if (propertyName == nameof(Key))
            {
                int key;
                bool success = int.TryParse(propertyValue, out key);                
                if(success)
                {
                    if (Instance.DictTasks.ContainsKey(key))
                    {
                        return propertyValue;
                    }
                    else
                    {
                        Console.WriteLine(ValidationMessageKeyNotExists);
                        result = string.Empty;
                    }
                }
                else
                {
                    Console.WriteLine(ValidationMessageKeyNumber);
                    result = string.Empty;
                }
            }
            return result;
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
            if (path != string.Empty)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
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
                Console.WriteLine(ValidationMessageAddKeyExists);
                return false;
            }
        }

        public void AddMultiple()
        {
            bool ErrorFound=false;
            string ErrorItems = string.Empty;
            Console.WriteLine(AddMultipleItemsMessage);
            TaskModel.Instance.StartProcess(AddMultiplePath);
            string readFromFile = File.ReadAllText(AddMultiplePath);
            if(readFromFile.Trim()!=string.Empty)
            {                
                foreach (string item in readFromFile.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
                {                    
                    string alias = string.Empty;
                    string link = string.Empty;
                    string whenToRun = string.Empty;                    
                    alias = item;
                    Console.WriteLine("Adding Item " + alias);
                    link = GetLink();
                    whenToRun = GetWhenToRunUntilValidationSuccess();

                    if (!Add(alias, link, whenToRun))
                    {                        
                        ErrorFound = true;
                        ErrorItems += alias+"\r\n";
                    }                    
                }
                if(!ErrorFound)
                {
                    File.WriteAllText(AddMultiplePath, string.Empty);
                    Console.WriteLine("Added");
                }
                else
                {
                    Console.WriteLine(ErrorFoundWhileAddingMultiple);
                    Console.WriteLine(ErrorItems);
                }
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
                Console.WriteLine(ValidationMessageKeyNotExists);
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
                Console.WriteLine(ValidationMessageKeyNotExists);                
            }
        }

        public void Display()
        {
            if(Instance.DictTasks.Count ==0)
            {
                Console.WriteLine(NoItemsToShow);
            }
            else
            {
                (string, int) result = GetItemsAsText(GetItemsType.All);
                Console.WriteLine(string.Format("{0}{1}{2}", result.Item1, "\r\nTotal Items: ",
                    result.Item2));
            }
        }

        public void ShowHelpCommands()
        {
            Console.WriteLine("Type commands followed by 'ENTER'");
            Console.WriteLine("add");
            Console.WriteLine("addmultiple");
            Console.WriteLine("update");
            Console.WriteLine("delete/remove");
            Console.WriteLine("display");
            Console.WriteLine("Press CTL+C to Terminate");            
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
            string items = GetItemsAsText(GetItemsType.AliasLinkWhenToRun).Item1;
            File.WriteAllText(DBPath, items);
        }

        public (string,int) GetItemsAsText(GetItemsType ItemsType,int dictKey=0)
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
                    Items += DBItemsDisplaySeperator;                    
                    Items += "\r\n";
                }
                else if(GetItemsType.AllWithPassingKey == ItemsType && dictKey == item.Key)
                {
                    Items += string.Format("{0}{1}{2}{3}", nameof(Key), DBFieldSeperator, item.Key, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(Alias), DBFieldSeperator, item.Value.Alias, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(Link), DBFieldSeperator, item.Value.Link, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(WhenToRun), DBFieldSeperator, item.Value.WhenToRun, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(TimeToRun), DBFieldSeperator, item.Value.TimeToRun, "\r\n");
                    Items += DBItemsDisplaySeperator;
                    Items += "\r\n";
                }
                else if (GetItemsType.AliasLinkWhenToRun == ItemsType)
                {
                    Items += string.Format("{0}{1}{2}{3}", nameof(Alias), DBFieldSeperator, item.Value.Alias, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(Link), DBFieldSeperator, item.Value.Link, "\r\n");
                    Items += string.Format("{0}{1}{2}{3}", nameof(WhenToRun), DBFieldSeperator, item.Value.WhenToRun, "\r\n");                    
                    Items += DBItemsSeperator;
                    Items += "\r\n";
                }
                else if (GetItemsType.AliasLinkWithOnlyPassingKey == ItemsType && dictKey==item.Key)
                {
                    Items += string.Format("{0}{1}", item.Value.Alias, "\r\n");
                    Items += string.Format("{0}{1}", item.Value.Link, "\r\n");
                    Items += DBItemsDisplaySeperator;
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
                                Console.WriteLine(DBLoadingError);
                                DBIssueFound = true;
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
                        Console.WriteLine(DBLoadedSuccessfully);
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
    }
}
