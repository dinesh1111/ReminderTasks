using System;
using System.Collections.Generic;
using System.Linq;
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
        private string parameter = string.Empty;
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
            link = TaskModel.Instance.GetLink();
            whenToRun = TaskModel.Instance.GetWhenToRunUntilValidationSuccess();

            if (TaskModel.Instance.Add(aliasName, link, whenToRun))
            {
                TaskModel.Instance.WriteLine("Added");
            }
        }
    }    

    public class UpdateCommand : ICommand
    {
        private string parameter = string.Empty;
        public UpdateCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {                        
            string alias = string.Empty;
            string link = string.Empty;
            string whenToRun = string.Empty;

            int key = TaskModel.Instance.GetKeyUntilValidationSuccess(parameter);
            if(key != -1)
            {
                alias = TaskModel.Instance.GetAliasName(TaskModel.Instance.DictTasks[key].Alias);
                link = TaskModel.Instance.GetLink();
                whenToRun = TaskModel.Instance.GetWhenToRunUntilValidationSuccess();

                if (TaskModel.Instance.Update(key, alias, link, whenToRun))
                    {
                    TaskModel.Instance.WriteLine("Updated " + alias);                    
                    }
            }
            else
            {
                TaskModel.Instance.WriteLine("Exited");
            }
        }
    }

    public class SetDefaultRemindersTimeCommand : ICommand
    {
        private string parameter = string.Empty;
        public SetDefaultRemindersTimeCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {
            int mins;
            mins = TaskModel.Instance.GetMinsUntilValidationSuccess(parameter);
            if (mins != -1)
            {
                int minValue = Convert.ToInt32(mins);
                TaskModel.Instance.ShowReminderFileInMins = Convert.ToInt32(minValue);
                TaskModel.Instance.SetShowReminderFileInMins();
                TaskModel.Instance.WriteLine("Updated");
            }
            else
            {
                TaskModel.Instance.WriteLine("Exited");
            }            
        }
    }

    public class OpenCommand : ICommand
    {
        private string parameter = string.Empty;
        public OpenCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {
            int key = TaskModel.Instance.GetKeyUntilValidationSuccess(parameter);
            if(key!= -1)
            {
                TaskModel.Instance.StartProcess(TaskModel.Instance.DictTasks[Convert.ToInt32(key)].Link);
            }
            else
            {
                TaskModel.Instance.WriteLine("Exited");
            }
        }
    }

    public class DeleteCommand : ICommand
    {
        private string parameter = string.Empty;
        public DeleteCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {
            int key = TaskModel.Instance.GetKeyUntilValidationSuccess(parameter);
            if (key != -1)
            {
                TaskModel.Instance.WriteLine("Deleted " + TaskModel.Instance.DictTasks[key].Alias);
                TaskModel.Instance.Delete(key);                
            }
            else
            {
                TaskModel.Instance.WriteLine("Exited");
            }
        }
    }
    public class DisplayCommand : ICommand
    {
        private string parameter = string.Empty;
        public DisplayCommand(string param)
        {
            parameter = param.Trim();
        }
        public void Execute()
        {            
            if(parameter == "all")
            {
                TaskModel.Instance.Display(GetRemindersType.All,string.Empty);
            }
            else if (parameter.ToLower() == "today")
            {
                TaskModel.Instance.Display(GetRemindersType.Today,string.Empty);
            }
            else if (parameter.ToLower() == "tomorrow")
            {
                TaskModel.Instance.Display(GetRemindersType.Tomorrow,string.Empty);
            }
            else
            {                
                TaskModel.Instance.Display(GetRemindersType.Name,parameter);
            }

        }
    }
    public class ShowHelpCommand : ICommand
    {
        public void Execute()
        {
            TaskModel.Instance.ShowHelpCommands();
        }
    }
}
