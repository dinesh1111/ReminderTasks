﻿using System.ComponentModel;

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
        static Dictionary<int,string> ReminderItems = new Dictionary<int,string>();
        static int DefaultShowReminderCountInSeconds = 15 * 60;
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
            while(true)
            {
                Thread.Sleep(1000);
                foreach (var item in TaskModel.Instance.DictTasks)
                {
                    if (DateTime.Now >= item.Value.TimeToRun)
                    {
                        TaskModel.Instance.Update(item.Key, item.Value.Alias, item.Value.Link, item.Value.WhenToRun);
                        if (!ReminderItems.ContainsKey(item.Key))
                        {
                            ReminderItems.Add(item.Key, TaskModel.Instance.GetItemsAsText(GetItemsType.AliasLinkWithOnlyPassingKey, item.Key).Item1);
                        }
                        TaskModel.Instance.StartProcess(item.Value.Link);
                    }
                }
                DefaultShowReminderCountInSeconds--;
                if(DefaultShowReminderCountInSeconds<=0)
                {
                    string result = string.Empty;
                    foreach (var item in ReminderItems)
                    {
                        result += item.Value + "\r\n";
                    }
                    File.WriteAllText(ShowTodoPath, result);
                    TaskModel.Instance.StartProcess(ShowTodoPath);
                    DefaultShowReminderCountInSeconds = 15 * 60;
                    ReminderItems.Clear();
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