using System.ComponentModel;

namespace ReminderTasks
{
    internal class Program:TaskViewModelBase
    {
        readonly static CancellationTokenSource _cancelTokenSrc = new CancellationTokenSource();
        static Invoker InvokerObject = new Invoker();
        static TaskViewModelBase taskVMBase = new TaskViewModelBase();

        
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
                TaskViewModel.Instance.WriteLine(OperationCanceled);                
                TaskViewModel.Instance.WriteToErrorLog(ex.Message, MainMethod);
            }
            catch (Exception ex)
            {
                TaskViewModel.Instance.WriteToErrorLog(ex.Message, MainMethod);
            }            
        }

        static void ListenForInput()
        {
            while(true)
            {
                if (!TaskViewModel.Instance.ApplicationError)
                {
                    string userInput = Console.ReadLine();
                    ICommand command = InvokerObject.GetCommand(userInput);
                    command.Execute();
                }
                else
                {
                    TaskViewModel.Instance.WriteLine("Application Error Found. Check error log");
                }
            }
        }

        static void DoWork()
        {
            while(true)
            {
                Thread.Sleep(1000);
                if (taskVMBase.NotInPause())
                {
                    taskVMBase.FillReminders();
                    TaskViewModel.Instance.DefaultShowReminderCountInSeconds--;
                    if (TaskViewModel.Instance.DefaultShowReminderCountInSeconds <= 0)
                    {
                        taskVMBase.ShowReminders();
                        TaskViewModel.Instance.DefaultShowReminderCountInSeconds = TaskViewModel.Instance.ShowReminderFileInMins * 60;
                        taskVMBase.ClearReminders();
                    }
                }
                else
                {
                    Console.WriteLine("Paused in seconds " + TaskViewModel.Instance.PauseCountInSeconds);                    
                }
            }            
        }

        public static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            TaskViewModel.Instance.WriteLine("Cancelling....");            
            _cancelTokenSrc.Cancel();
        }
    }
}