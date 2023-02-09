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
                Console.WriteLine("Started");
                Console.ReadLine();
                Task.Run(()=>DoWork(),cancelToken);
                Task.Run(() => ListenForInput(), cancelToken);
                cancelToken.WaitHandle.WaitOne();
                cancelToken.ThrowIfCancellationRequested();
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine(OperationCanceled);
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
            string result = string.Empty;
            while(true)
            {
                Thread.Sleep(1000);
                try
                {
                    foreach (var item in TaskModel.Instance.DictTasks)
                    {                        
                        if(DateTime.Now >= item.Value.TimeToRun)
                        {
                            TaskModel.Instance.Update(item.Key,item.Value.Alias,item.Value.Link, item.Value.WhenToRun);
                            result += TaskModel.Instance.GetItemsAsText(GetItemsType.AliasLinkWithOnlyPassingKey,item.Key).Item1;
                            TaskModel.Instance.StartProcess(item.Value.Link);
                        }
                    }
                    if (result != string.Empty)
                    {
                        File.WriteAllText(ShowTodoPath,result);
                        TaskModel.Instance.StartProcess(ShowTodoPath);
                        result = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    TaskModel.Instance.WriteToErrorLog(ex.Message, DoWorkMethod);
                    File.WriteAllText(ShowTodoPath, result);
                    TaskModel.Instance.StartProcess(ShowTodoPath);
                    result = string.Empty;
                }
            }
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine("Cancelling....");
            _cancelTokenSrc.Cancel();
        }
    }
}