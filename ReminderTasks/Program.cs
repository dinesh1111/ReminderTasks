using System.ComponentModel;

namespace ReminderTasks
{
    internal class Program
    {
        readonly static CancellationTokenSource _cancelTokenSrc = new CancellationTokenSource();
        static Invoker InvokerObject = new Invoker();        
        private static string ShowTodoPath = @"Todo.txt";        
        private const string OperationCanceled = "Operation Canceled";
        private const string MainMethod = "Main Method";
        private const string DoWorkMethod = "Do Work Method";        
        static void Main(string[] args)
        {
            //CTL + C is the built-in cancellation for console apps;
            Console.CancelKeyPress += Console_CancelKeyPress;
            CancellationToken cancelToken = _cancelTokenSrc.Token;

            try
            {                
                Task.Run(()=>DoWork(),cancelToken);
                Task.Run(() => ListenForInput(), cancelToken);
                cancelToken.WaitHandle.WaitOne();
                cancelToken.ThrowIfCancellationRequested();
            }
            catch(OperationCanceledException ex)
            {
                TaskModel.Instance.WriteLine(OperationCanceled);                
                TaskModel.Instance.WriteToErrorLog(ex.Message, MainMethod);
            }
            catch (Exception ex)
            {
                TaskModel.Instance.WriteToErrorLog(ex.Message, MainMethod);
            }            
        }

        static void ListenForInput()
        {
            while(true)
            {
                string userInput = Console.ReadLine();
                ICommand command = InvokerObject.GetCommand(userInput);
                command.Execute();
            }
        }

        static void DoWork()
        {
            while(true)
            {
                Thread.Sleep(1000);
                foreach (var item in TaskModel.Instance.DictTasks)
                {
                    if (DateTime.Now >= item.Value.TimeToRun)
                    {
                        TaskModel.Instance.Update(item.Key, item.Value.Alias, item.Value.Link, item.Value.WhenToRun);
                        if (!TaskModel.Instance.ReminderItems.ContainsKey(item.Key))
                        {
                            TaskModel.Instance.ReminderItems.Add(item.Key, TaskModel.Instance.GetItemsAsText(GetItemsType.AliasLinkWithOnlyPassingKey,string.Empty, item.Key).Item1);
                        }
                        TaskModel.Instance.StartProcess(item.Value.Link);
                    }
                }
                TaskModel.Instance.DefaultShowReminderCountInSeconds--;
                if(TaskModel.Instance.DefaultShowReminderCountInSeconds <=0)
                {
                    string result = string.Empty;
                    foreach (var item in TaskModel.Instance.ReminderItems)
                    {
                        result += item.Value + "\r\n";
                    }
                    if (result.Trim() != string.Empty)
                    {
                        File.WriteAllText(ShowTodoPath, result);
                        TaskModel.Instance.StartProcess(ShowTodoPath);
                    }
                    TaskModel.Instance.DefaultShowReminderCountInSeconds = TaskModel.Instance.ShowReminderFileInMins * 60;
                    TaskModel.Instance.ReminderItems.Clear();
                }
            }
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            TaskModel.Instance.WriteLine("Cancelling....");            
            _cancelTokenSrc.Cancel();
        }
    }
}