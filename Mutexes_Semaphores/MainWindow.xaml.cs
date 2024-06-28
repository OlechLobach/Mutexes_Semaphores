using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MultiTaskApp
{
    public partial class MainWindow : Window
    {
        private Mutex mutex = new Mutex();
        private Semaphore semaphore = new Semaphore(3, 3);

        public MainWindow()
        {
            InitializeComponent();
        }

        // Task 1: Display numbers from 0 to 20 and then from 10 to 0
        private async void StartTask1_Click(object sender, RoutedEventArgs e)
        {
            OutputTask1TextBox.Clear();
            await Task.Run(() => DisplayNumbers());
            await Task.Run(() => DisplayReverseNumbers());
        }

        private void DisplayNumbers()
        {
            mutex.WaitOne();
            for (int i = 0; i <= 20; i++)
            {
                Dispatcher.Invoke(() => OutputTask1TextBox.AppendText(i + " "));
                Thread.Sleep(100);
            }
            mutex.ReleaseMutex();
        }

        private void DisplayReverseNumbers()
        {
            mutex.WaitOne();
            for (int i = 10; i >= 0; i--)
            {
                Dispatcher.Invoke(() => OutputTask1TextBox.AppendText(i + " "));
                Thread.Sleep(100);
            }
            mutex.ReleaseMutex();
        }

        // Task 2: Modify array and find max value
        private async void StartTask2_Click(object sender, RoutedEventArgs e)
        {
            OutputTask2TextBox.Clear();
            int[] array = Enumerable.Range(1, 10).ToArray();
            await Task.Run(() => ModifyArray(array));
            int max = await Task.Run(() => FindMaxValue(array));
            Dispatcher.Invoke(() => OutputTask2TextBox.Text = $"Modified Array: {string.Join(", ", array)}\nMax Value: {max}");
        }

        private void ModifyArray(int[] array)
        {
            mutex.WaitOne();
            Random random = new Random();
            for (int i = 0; i < array.Length; i++)
            {
                array[i] += random.Next(1, 10);
                Thread.Sleep(100);
            }
            mutex.ReleaseMutex();
        }

        private int FindMaxValue(int[] array)
        {
            mutex.WaitOne();
            int max = array.Max();
            mutex.ReleaseMutex();
            return max;
        }

        // Task 3: Modify array and find max value (display in Main)
        private async void StartTask3_Click(object sender, RoutedEventArgs e)
        {
            OutputTask3TextBox.Clear();
            int[] array = Enumerable.Range(1, 10).ToArray();
            await Task.Run(() => ModifyArray(array));
            int max = FindMaxValue(array);
            OutputTask3TextBox.Text = $"Modified Array: {string.Join(", ", array)}\nMax Value: {max}";
        }

        // Task 4: Single Instance Application
        private void StartTask4_Click(object sender, RoutedEventArgs e)
        {
            bool createdNew;
            using (Mutex singleInstanceMutex = new Mutex(true, "SingleInstanceApp", out createdNew))
            {
                if (!createdNew)
                {
                    OutputTask4TextBox.Text = "Application is already running.";
                    Application.Current.Shutdown();
                }
                else
                {
                    OutputTask4TextBox.Text = "Application is running.";
                }
            }
        }

        // Task 5: Display random numbers with limited concurrent threads
        private async void StartTask5_Click(object sender, RoutedEventArgs e)
        {
            OutputTask5TextBox.Clear();
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                int taskId = i;
                tasks.Add(Task.Run(() => DisplayRandomNumbers(taskId)));
            }
            await Task.WhenAll(tasks);
        }

        private void DisplayRandomNumbers(int taskId)
        {
            semaphore.WaitOne();
            Random random = new Random();
            Dispatcher.Invoke(() => OutputTask5TextBox.AppendText($"Task {taskId}: "));
            for (int i = 0; i < 5; i++)
            {
                int number = random.Next(1, 100);
                Dispatcher.Invoke(() => OutputTask5TextBox.AppendText(number + " "));
                Thread.Sleep(100);
            }
            Dispatcher.Invoke(() => OutputTask5TextBox.AppendText(Environment.NewLine));
            semaphore.Release();
        }
    }
}