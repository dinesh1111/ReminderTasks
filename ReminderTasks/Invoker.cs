

namespace ReminderTasks
{    
    public class Invoker
    {
        public ICommand GetCommand(string action)
        {
            ICommand cmd = null;
            Dictionary<string, string> CommandsExcludeParam = new Dictionary<string, string>()
            {
                {"deletecompleted","deletecompleted" },
                {"updateall","updateall" }
            };
            string[] splitActions = action?.Split(' ',StringSplitOptions.RemoveEmptyEntries);
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
            else if(splitActions.Length == 1 && CommandsExcludeParam.ContainsKey(splitActions[0]))
            {
                command = splitActions[0];                
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
                case "quickadd":
                    cmd = new AddQickCommand(parameter);
                    break;
                case "deletecompleted":
                    cmd = new DeleteCompleteCommand();
                    break;
                case "updateall":
                    cmd = new UpdateAllCommand();
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
