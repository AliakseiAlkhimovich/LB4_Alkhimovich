using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LB4_Alkhimovich
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            await CalculateAsync();
        }

        private CancellationTokenSource _cancellationTokenSource;

        private async Task CalculateAsync()
        {
            LB.Items.Clear();
            if (!int.TryParse(DataN.Text, out int n))
            {
                MessageBox.Show("Введите корректное значение для n.");
                return;
            }
            Disp.IsEnabled = false;
            Back.IsEnabled = false;
            var step = Math.Round((double)n / 100);

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Run(() =>
                {
                    double a = 0;
                    double b = 2 * Math.PI;
                    double h = (b - a) / n;
                    double integral = 0;

                    for (int i = 1; i <= n; i++)
                    {
                        if (_cancellationTokenSource.Token.IsCancellationRequested) break;

                        double x = a + h * i;
                        integral += Math.Sin(x) * h;

                        Dispatcher.Invoke(() =>
                        {
                            LB.Items.Add($"a= {a}, b= {b}, N= {i}, Integral=  {integral}");
                            if (i % step == 0) LB.ScrollIntoView(LB.Items[LB.Items.Count - 1]);
                            PecentLb.Content = Math.Round((double)i / n * 100) + "%";
                            pBar.Value = (double)i / n * 100;
                        });
                        Thread.Sleep(1);
                    }
                }, _cancellationTokenSource.Token);
            }
            finally
            {
                Disp.IsEnabled = true;
                Back.IsEnabled = true;
            }
        }
 /*-----------------------------------------------------------------------------------------------------------------------------*/
        private void StopCalculation(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            StopBackgroundWorker(bgWorker);
            return;
        }
/*-----------------------------------------------------------------------------------------------------------------------------*/
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {           
                BackgroundWorker bgWorker = sender as BackgroundWorker;
            if (bgWorker.IsBusy && !bgWorker.CancellationPending)
            {
                Dispatcher.Invoke(() =>
                {
                    Disp.IsEnabled = false;
                    Back.IsEnabled = false;
                    LB.Items.Clear();
                });

                int n = (int)e.Argument;
                double a = 0;
                double b = 2 * Math.PI;
                double h = (b - a) / n;
                double integral = 0;
                var step = Math.Round((double)n / 100);

                for (int i = 1; i <= n; i++)
                {
                    if (bgWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    Thread.Sleep(1);
                    bgWorker.ReportProgress(i * 100 / n, i);


                    double x = a + h * i;
                    integral += Math.Sin(x) * h;
                    Dispatcher.Invoke(() =>
                    {
                        LB.Items.Add($"a= {a}, b= {b}, N= {i}, Integral= {integral}");
                        if (i % step == 0) LB.ScrollIntoView(LB.Items[LB.Items.Count - 1]);
                    });
                }

                e.Result = integral;
                Dispatcher.Invoke(() =>
                {
                    Disp.IsEnabled = true;
                    Back.IsEnabled = true;
                });
            }
        }
             
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pBar.Value = e.ProgressPercentage;
            PecentLb.Content = $"{e.ProgressPercentage}%";
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                pBar.Value = 0;
                PecentLb.Content = "";
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Ошибка: " + e.Error.Message);
            }           
        }

        private  BackgroundWorker bgWorker = null;
   
        private void StartBackgroundWorker(int n)
        {
            if (bgWorker == null || !bgWorker.IsBusy)
            {
                bgWorker = new BackgroundWorker();
                bgWorker.WorkerSupportsCancellation = true;
                bgWorker.DoWork += Worker_DoWork;
                bgWorker.ProgressChanged += Worker_ProgressChanged;
                bgWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                bgWorker.WorkerReportsProgress = true;
                bgWorker.RunWorkerAsync(n);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            
            if (int.TryParse(DataN.Text, out int n))
            {   
                    StartBackgroundWorker(n);          
            }
            else
            {
                MessageBox.Show("Введите корректное значение 'n'.");
            }
        }

        private void StopBackgroundWorker(BackgroundWorker bgWorker)
        {
            if (bgWorker != null && bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
                Dispatcher.Invoke(() =>
                {
                    Disp.IsEnabled = true;
                    Back.IsEnabled = true;
                });
            }
        }
    }
}