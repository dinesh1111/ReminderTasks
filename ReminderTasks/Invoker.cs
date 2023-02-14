

namespace ReminderTasks
{    
    public class Invoker
    {
        public ICommand GetCommand(string action)
        {
            ICommand cmd = null;
            string[] splitActions = action.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            string command = string.Empty;
            string parameter = string.Empty;
            if (splitActions.Length > 1)
            {
                command = splitActions[0];
                foreach (string item in splitActions)
                {
                    parameter += item + " ";
                }
                parameter = parameter.Replace(command, string.Empty).Trim();
            }
            
            switch (command)
            {
                case "add":
                    cmd = new AddCommand(parameter);
                    break;
                case "update":
                    cmd = new UpdateCommand(parameter);
                    break;
                case "delete":
                    cmd = new DeleteCommand(parameter);
                    break;
                case "display":                
                    cmd = new DisplayCommand(parameter);
                    break;
                case "setdefaultreminderstime":
                    cmd = new SetDefaultRemindersTimeCommand(parameter);
                    break;
                case "open":
                    cmd = new OpenCommand(parameter);
                    break;               
                default:
                    TaskModel.Instance.WriteLine("Command is not correct");                    
                    cmd = new ShowHelpCommand();
                    break;

            }
            return cmd;            
        }
    }
}
