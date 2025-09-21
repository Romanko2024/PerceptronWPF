using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace PerceptronWPF
{
    public partial class MainWindow : Window
    {
        private List<TrainingSample> samples = new List<TrainingSample>(); //ліст навчальних прикладів
        private Perceptron perceptron = new Perceptron();
        private DispatcherTimer timer = new DispatcherTimer(); //таймер для покрок. режиму

        //індекс поточного прикладу для покрокового
        private int currentIndex = 0;

        // для графіка помилок історія помилок
        private List<int> errorHistory = new List<int>();

        public MainWindow()
        {
            InitializeComponent();
            dgSamples.ItemsSource = samples;
            timer.Tick += Timer_Tick;
            UpdateUI();
        }

        private void BtnLoadExample_Click(object sender, RoutedEventArgs e)
        {
            //завантаження приклада
            samples.Clear();
            samples.Add(new TrainingSample { X1 = -1, X2 = -1, Label = -1 });
            samples.Add(new TrainingSample { X1 = -1, X2 = 1, Label = -1 });
            samples.Add(new TrainingSample { X1 = 1, X2 = -1, Label = -1 });
            samples.Add(new TrainingSample { X1 = 1, X2 = 1, Label = 1 });
            dgSamples.Items.Refresh();
            DrawDataAndBoundary();
        }

        //очищення
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            samples.Clear();
            dgSamples.Items.Refresh();
            errorHistory.Clear();
            DrawDataAndBoundary();
            DrawErrorPlot();
        }

        //скидання ваг
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            perceptron.Reset(true);
            errorHistory.Clear();
            currentIndex = 0;
            UpdateUI();
            DrawDataAndBoundary();
            DrawErrorPlot();
        }

        private void BtnStep_Click(object sender, RoutedEventArgs e)
        {
            //один навчальний крок
            if (samples.Count == 0) return;
            if (!double.TryParse(tbLearningRate.Text, out double lr)) lr = 0.1;
            perceptron.LearningRate = lr;
            //навч на поточному прикладі
            var s = samples[currentIndex % samples.Count];
            bool updated = perceptron.TrainStep(s);
            currentIndex++;

            //кільк помилок після цього кроку
            int totalErrors = samples.Sum(x => perceptron.Predict(x.X1, x.X2) == x.Label ? 0 : 1);
            errorHistory.Add(totalErrors);

            tbInfo.Text = $"Крок: {currentIndex}, Оновлено: {updated}, Помилок: {totalErrors}";
            UpdateUI();
            DrawDataAndBoundary();
            DrawErrorPlot();
        }
        //запуск покрокового з тайцмером
        private void BtnStartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tbInterval.Text, out int ms)) ms = 500;
            timer.Interval = TimeSpan.FromMilliseconds(ms);
            timer.Start();
            tbInfo.Text = "Таймер запущено";
        }
        //зупинка таймера
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            tbInfo.Text = "Зупинено";
        }
        //автомат навч епохами
        private async void BtnAuto_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tbEpochs.Text, out int epochs)) epochs = 100;
            if (!double.TryParse(tbLearningRate.Text, out double lr)) lr = 0.1;
            perceptron.LearningRate = lr;

            // запуск епох. показувати лише *кінцевий* результат
            for (int ep = 0; ep < epochs; ep++)
            {
                int errors = perceptron.TrainEpoch(samples);
                errorHistory.Add(errors);
                if (errors == 0) break; // помилок нема – зупинка
            }

            tbInfo.Text = $"Автономне навчання завершено. Епох: {errorHistory.Count}, Остання помилка: {errorHistory.LastOrDefault()}";
            UpdateUI();
            DrawDataAndBoundary();
            DrawErrorPlot();
        }
        //тест
        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(tbTestX1.Text, out double x1)) x1 = 0;
            if (!double.TryParse(tbTestX2.Text, out double x2)) x2 = 0;
            int y = perceptron.Predict(x1, x2);
            tbTestResult.Text = $"Результат: {y}";
        }

        private void Timer_Tick(object sender, EventArgs e) //один тік один крок
        {
            BtnStep_Click(null, null);
        }

        private void UpdateUI() //вивід багів та зсуву (баяс)
        {
            tbWeights.Text = $"W1: {perceptron.W1:F3}  W2: {perceptron.W2:F3}  Bias: {perceptron.Bias:F3}";
        }

        private void DrawDataAndBoundary() //малювання точок і лінії
        {
            var cv = canvasData;
            cv.Children.Clear();
            double w = cv.ActualWidth; if (w == 0) w = cv.Width = 600;
            double h = cv.ActualHeight; if (h == 0) h = cv.Height = 400;

            double R = 2.5; //діапазон
            Func<double, double> mapX = (x) => (x + R) / (2 * R) * w; //з коорд на пікселі x
            Func<double, double> mapY = (y) => (1 - (y + R) / (2 * R)) * h; // y

            //осі
            var xaxis = new Line { X1 = 0, X2 = w, Y1 = mapY(0), Y2 = mapY(0), Stroke = Brushes.LightGray };
            var yaxis = new Line { X1 = mapX(0), X2 = mapX(0), Y1 = 0, Y2 = h, Stroke = Brushes.LightGray };
            cv.Children.Add(xaxis); cv.Children.Add(yaxis);

            // малювання точок
            foreach (var s in samples)
            {
                Ellipse e = new Ellipse { Width = 10, Height = 10, Stroke = Brushes.Black, StrokeThickness = 1 };
                if (s.Label >= 1) e.Fill = Brushes.LightBlue; else e.Fill = Brushes.LightCoral;
                Canvas.SetLeft(e, mapX(s.X1) - 5);
                Canvas.SetTop(e, mapY(s.X2) - 5);
                cv.Children.Add(e);
            }

            // малювання лінії W1*x + W2*y + Bias = 0 => y = -(W1/W2)x - Bias/W2 (if W2 != 0)
            if (Math.Abs(perceptron.W2) > 1e-6)
            {
                double xA = -R, xB = R;
                double yA = -(perceptron.W1 / perceptron.W2) * xA - perceptron.Bias / perceptron.W2;
                double yB = -(perceptron.W1 / perceptron.W2) * xB - perceptron.Bias / perceptron.W2;
                var line = new Line { X1 = mapX(xA), Y1 = mapY(yA), X2 = mapX(xB), Y2 = mapY(yB), Stroke = Brushes.Green, StrokeThickness = 2 };
                cv.Children.Add(line);
            }
        }

        private void DrawErrorPlot() //графік помилок
        {
            var cv = canvasError;
            cv.Children.Clear();
            double w = cv.ActualWidth; if (w == 0) w = cv.Width = 600;
            double h = cv.ActualHeight; if (h == 0) h = cv.Height = 200;

            //осі
            var xaxis = new Line { X1 = 0, X2 = w, Y1 = h - 20, Y2 = h - 20, Stroke = Brushes.LightGray };
            cv.Children.Add(xaxis);

            if (errorHistory.Count < 2) return;
            int maxErr = Math.Max(1, errorHistory.Max());
            //ламана по точка кільк помилок
            for (int i = 0; i < errorHistory.Count - 1; i++)
            {
                double x1 = (double)i / (errorHistory.Count - 1) * (w - 40) + 20;
                double x2 = (double)(i + 1) / (errorHistory.Count - 1) * (w - 40) + 20;
                double y1 = (1 - (double)errorHistory[i] / maxErr) * (h - 40) + 10;
                double y2 = (1 - (double)errorHistory[i + 1] / maxErr) * (h - 40) + 10;
                var l = new Line { X1 = x1, X2 = x2, Y1 = y1, Y2 = y2, Stroke = Brushes.DarkBlue, StrokeThickness = 2 };
                cv.Children.Add(l);
            }
        }
    }
}