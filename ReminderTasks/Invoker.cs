

namespace ReminderTasks
{    
    public class Invoker
    {
        public ICommand GetCommand(string action)
        {
            ICommand cmd = null;
            switch (action)
            {
                case "add":
                    cmd = new AddCommand();
                    break;
                case "addmultiple":
                    cmd = new AddMultipleCommand();
                    break;
                case "update":
                    cmd = new UpdateCommand();
                    break;
                case "delete":
                case "remove":
                    cmd = new DeleteCommand();
                    break;
                case "display":
                case "list":
                    cmd = new DisplayCommand();
                    break;
                case "setdefaultreminderstime":
                    cmd = new SetDefaultRemindersTimeCommand();
                    break;
                default:
                    Console.WriteLine("Command is not correct");
                    cmd = new ShowHelpCommand();
                    break;

            }
            return cmd;            
        }
    }
}
