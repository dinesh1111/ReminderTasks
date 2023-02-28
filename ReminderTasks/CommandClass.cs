using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ReminderTasks
{
    public interface ICommand
    {
        void Execute();
    }
    public class AddCommand : ICommand
    {
        public string parameter = string.Empty;
        public AddCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {
            string aliasName = string.Empty;
            string whenToRun = string.Empty;
            string link = string.Empty;

            aliasName = parameter;
            link = TaskViewModel.Instance.GetLink();
            whenToRun = TaskViewModel.Instance.GetWhenToRunUntilValidationSuccess();

            if (TaskViewModel.Instance.Add(aliasName, link, whenToRun))
            {
                TaskViewModel.Instance.WriteLine("Added");
            }
        }
    }
    public class AddQickCommand : ICommand
    {
        public string parameter = string.Empty;
        public AddQickCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {
            string aliasName = string.Empty;
            string whenToRun = string.Empty;
            string link = string.Empty;

            aliasName = parameter;            
            whenToRun = "15 min";

            if (TaskViewModel.Instance.Add(aliasName, link, whenToRun))
            {
                TaskViewModel.Instance.WriteLine("Added as 15 min reminder.");
            }
        }
    }

    public class UpdateCommand : ICommand
    {
        public string parameter = string.Empty;
        public UpdateCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {
            string key = TaskViewModel.Instance.GetKeyUntilValidationSuccess(parameter);
            
            if (key != string.Empty)
            {
                string alias = TaskViewModel.Instance.DictTasks[key].Alias;
                string link = TaskViewModel.Instance.DictTasks[key].Link;
                string whenToRun = TaskViewModel.Instance.DictTasks[key].WhenToRun;
                Console.WriteLine("What you would like to update(alias/link/whentorun)?");
                string input=Console.ReadLine();
                if(input.ToLower().Trim() == "alias")
                {
                    alias = TaskViewModel.Instance.GetAliasName(TaskViewModel.Instance.DictTasks[key].Alias);
                }
                else if(input.ToLower().Trim() == "link")
                {
                    link = TaskViewModel.Instance.GetLink();
                }
                else if(input.ToLower().Trim() == "whentorun")
                {
                    whenToRun = TaskViewModel.Instance.GetWhenToRunUntilValidationSuccess();
                }
                else
                {
                    Console.WriteLine("No selection");
                }                                
                
                TaskViewModel.Instance.Add(alias, link, whenToRun);
                TaskViewModel.Instance.Delete(key);                
                TaskViewModel.Instance.WriteLine("Updated " + alias);
            }
            else
            {
                TaskViewModel.Instance.WriteLine("Key is required");
            }
        }
    }

    public class DeleteCompleteCommand : ICommand
    {        
        public DeleteCompleteCommand()
        {
            
        }
        public void Execute()
        {
            foreach (KeyValuePair<string,TaskModel> item in TaskViewModel.Instance.DictTasks)
            {
                TaskViewModel.Instance.WriteLine("Do you want to delete " + item.Value.Alias + "\r\n" + (item.Value.Link!=""?item.Value.Link + "\r\n":string.Empty) + "Type yes(y) Enter, Enter key(No), Exit to come out");
                string? input = Console.ReadLine();
                if (input?.ToLower() == "yes" || input?.ToLower() == "y")
                {
                    TaskViewModel.Instance.Delete(item.Key);
                    TaskViewModel.Instance.WriteLine("Deleted " + item.Value.Alias);
                }
                else if(input?.ToLower() == "exit")
                {
                    break;
                }
            }
            Console.WriteLine("Mark Completed");
        }            
    }    

    public class PauseCommand : ICommand
    {
        public string parameter = string.Empty;
        public PauseCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {
            TaskViewModel.Instance.Pause(parameter);        
        }
    }

    public class OpenCommand : ICommand
    {
        public string parameter = string.Empty;
        public OpenCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {
            string key = TaskViewModel.Instance.GetKeyUntilValidationSuccess(parameter);
            if(key!= string.Empty)
            {
                TaskViewModel.Instance.StartProcess(TaskViewModel.Instance.DictTasks[key].Link);
            }
            else
            {
                TaskViewModel.Instance.WriteLine("Exited");
            }
        }
    }

    public class DeleteCommand : ICommand
    {
        public string parameter = string.Empty;
        public DeleteCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {
            string key = TaskViewModel.Instance.GetKeyUntilValidationSuccess(parameter);
            if (key != string.Empty)
            {
                TaskViewModel.Instance.WriteLine("Deleted " + TaskViewModel.Instance.DictTasks[key].Alias);
                TaskViewModel.Instance.Delete(key);                
            }
            else
            {
                TaskViewModel.Instance.WriteLine("Exited");
            }
        }
    }
    public class DisplayCommand : ICommand
    {
        public string parameter = string.Empty;
        public DisplayCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {            
            if(parameter == "all")
            {
                TaskViewModel.Instance.Display(GetRemindersType.All,string.Empty);
            }
            else if (parameter.ToLower() == "today")
            {
                TaskViewModel.Instance.Display(GetRemindersType.Today,string.Empty);
            }
            else if (parameter.ToLower() == "tomorrow")
            {
                TaskViewModel.Instance.Display(GetRemindersType.Tomorrow,string.Empty);
            }
            else
            {                
                TaskViewModel.Instance.Display(GetRemindersType.Name,parameter);
            }

        }
    }
    public class ShowHelpCommand : ICommand
    {
        public void Execute()
        {
            TaskViewModel.Instance.ShowHelpCommands();
        }
    }
}
