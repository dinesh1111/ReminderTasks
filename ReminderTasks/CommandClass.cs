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
        public void Execute()
        {
            string aliasName = string.Empty;
            string whenToRun = string.Empty;
            string link = string.Empty;

            aliasName = TaskModel.Instance.GetAliasName();
            link = TaskModel.Instance.GetLink();
            whenToRun = TaskModel.Instance.GetWhenToRunUntilValidationSuccess();

            if (TaskModel.Instance.Add(aliasName, link, whenToRun))
            {
                Console.WriteLine("Added");
            }
        }
    }

    public class AddMultipleCommand : ICommand
    {
        public void Execute()
        {
            TaskModel.Instance.AddMultiple();
        }
    }

    public class UpdateCommand : ICommand
    {
        public void Execute()
        {            
            string key = string.Empty;
            string alias = string.Empty;
            string link = string.Empty;
            string whenToRun = string.Empty;            

            key = TaskModel.Instance.GetKeyUntilValidationSuccess();
            int keyValue = Convert.ToInt32(key);
            Console.WriteLine(TaskModel.Instance.GetItemsAsText(GetItemsType.AllWithPassingKey, keyValue).Item1);
            alias = TaskModel.Instance.GetAliasName(TaskModel.Instance.DictTasks[keyValue].Alias);
            link = TaskModel.Instance.GetLink();
            whenToRun = TaskModel.Instance.GetWhenToRunUntilValidationSuccess();

            if (TaskModel.Instance.Update(keyValue, alias,link,whenToRun))
            {
                  Console.WriteLine("Updated");
            }
        }
    }

    public class DeleteCommand : ICommand
    {
        public void Execute()
        {
            string key = string.Empty;            

            key = TaskModel.Instance.GetKeyUntilValidationSuccess();
            TaskModel.Instance.Delete(Convert.ToInt32(key));
            Console.WriteLine("Deleted");             
        }
    }
    public class DisplayCommand : ICommand
    {
        public void Execute()
        {
            TaskModel.Instance.Display();
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
