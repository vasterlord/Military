﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Military
{
    public partial class MainWindow : Window
    {
        int militaries = 70;
        double time = 20;
        Ellipse EmptyUI = new Ellipse();
        Generator generator = new Generator();
        ObservableCollection<Target> TargetList = new ObservableCollection<Target>();
        ObservableCollection<Aviation> AviationList = new ObservableCollection<Aviation>();
        ObservableCollection<MineThower> MineThowerList = new ObservableCollection<MineThower>();
        ObservableCollection<Thread> AviationsThreads = new ObservableCollection<Thread>();
        ObservableCollection<Thread> Mine_ThrowersThreads = new ObservableCollection<Thread>();
        DispatcherTimer dispatcherTimerWork = new DispatcherTimer();
        DispatcherTimer dispatcherTimerGen = new DispatcherTimer();
        public Polyline TargetUI = new Polyline();
        public PointCollection pointsCollection = new PointCollection();
        Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            pointsCollection = new PointCollection();
            TargetUI = new Polyline();
            int X = 13; int Y = 5;
            pointsCollection.Add(new Point(X, Y));
            pointsCollection.Add(new Point(X + 8, Y - 10));
            pointsCollection.Add(new Point(X + 16, Y));
            pointsCollection.Add(new Point(X + 16, Y + 10));
            pointsCollection.Add(new Point(X, Y + 10));
            TargetUI.Points = pointsCollection;
            TargetUI.StrokeDashArray = new DoubleCollection() { 5, 1, 3, 1 };
            TargetUI.Fill = new SolidColorBrush(Colors.DeepSkyBlue);
            TargetUI.StrokeThickness = 1.5;
            TextBlock OptionText = new TextBlock();
            OptionText.FontSize = 16;
            OptionText.FontStyle = FontStyles.Italic;
            OptionText.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
            Canvas.SetLeft(OptionText, X + 30);
            Canvas.SetTop(OptionText, Y - 9);
            OptionText.Text = "Targets";
            OptionTarget.Children.Add(TargetUI);
            OptionTarget.Children.Add(OptionText);
            EmptyUI = new Ellipse();
            EmptyUI.Width = 18;
            EmptyUI.Height = 18;
            EmptyUI.StrokeThickness = 5;
            EmptyUI.Margin = new Thickness(X, Y + 20, 1, 1);
            EmptyUI.Fill = new SolidColorBrush(Colors.MintCream);
            TextBlock nextOption = new TextBlock();
            nextOption.Foreground = new SolidColorBrush(Colors.MintCream);
            Canvas.SetLeft(nextOption, X + 30);
            Canvas.SetTop(nextOption, Y + 20);
            nextOption.FontSize = 16;
            nextOption.FontStyle = FontStyles.Italic;
            nextOption.Text = "Empties";
            OptionTarget.Children.Add(EmptyUI);
            OptionTarget.Children.Add(nextOption);
        }

        private void button_Generate_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (MineThowerList != null && TargetList != null)
                {
                    int k = MineThowerList.Count;
                    int tt = root_Canvas.Children.Count;
                    int z = TargetList.Count;
                }
                Mine_ThrowersThreads.Clear();
                AviationsThreads.Clear();
                Thread.Sleep(100);
                root_Canvas.Children.Clear();
                militaries = Convert.ToInt32(militaries_Count.Text);
                if (militaries <= 0)
                {
                    throw new Exception("Count of militaries can't be less or equal 0");
                }
                Thread.Sleep(200);
                int targetCount = generator.GenereteTargets(ref TargetList, militaries);
                Thread.Sleep(300);
                int avaiationCount = generator.GenerateAviations(ref AviationList);
                int mineThowerCount = generator.GenerateMineThowers(ref MineThowerList);
                count_MineThowers.Content = "Count mine-thowers : " + Convert.ToInt32(mineThowerCount);
                count_Aviations.Content = "Count aviations: " + Convert.ToInt32(avaiationCount);
                count_Targets.Content = "Count targets : " + Convert.ToInt32(targetCount);
                foreach (var target in TargetList)
                {
                    initTarget(target);
                }
                button_Generate.IsEnabled = true; 
                button_Start.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, " Error", MessageBoxButton.OK, MessageBoxImage.Error);
                button_Generate.IsEnabled = true;
            }
        }

        private void button_Start_Click(object sender, RoutedEventArgs e)
        {
            button_Generate.IsEnabled = false;
            try
            {
                time = Convert.ToInt32(militaries_Time.Text);
                if (time <= 0)
                {
                    throw new ArgumentException("Time can't be less or equal then 0!");
                }
            }
            catch (ArgumentException argumentException)
            {
                MessageBox.Show(argumentException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            int countThreadsMine = Mine_ThrowersThreads.Count;
            int countThreadsAviations = AviationsThreads.Count;
            bool messageAvia, messageThower;
            messageThower = (countThreadsMine > countThreadsAviations) ? true : false;
            messageAvia = (countThreadsMine < countThreadsAviations) ? true : false;
            messageAvia = (countThreadsMine == countThreadsAviations) ? true : false;
            foreach (var item in MineThowerList)
            {
                Mine_ThrowersThreads.Add(new Thread(() => item.Shoot(ref TargetList, time, countThreadsMine, messageThower)));
                item.DrawingTarget += DrawEventTargets;
                item.Enabled += Item_Enabled;
            }
            for (int i = 0; i < Mine_ThrowersThreads.Count; i++)
            {
                Mine_ThrowersThreads[i].Name = i.ToString();
            }
            foreach (var item in AviationList)
            {
                AviationsThreads.Add(new Thread(() => item.Shoot(ref TargetList, time, countThreadsAviations, messageAvia)));
                item.DrawingAvia += DrawEventTargets;
                item.Enabled += Item_Enabled;
            }
            for (int i = 0; i < AviationsThreads.Count; i++)
            {
                AviationsThreads[i].Name = i.ToString();
            }
            StartMineThowers();
            StartAviations();
            lock(this)
            {
                DrawEventTargets(this);
            }
            button_Start.IsEnabled = false;
        }

        private void Item_Enabled(object sender)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                button_Generate.IsEnabled = true; 
                button_TargetStats.IsEnabled = true; 
                button_MineThowerStats.IsEnabled = true; 
                button_AviationStats.IsEnabled = true;
            });
        }


        private void DrawEventTargets(object sender)
        {
            for (int i = 0; i < TargetList.Count; i++)
            {
                DrawTarget(TargetList[i]);
            }
        } 

        public void StartMineThowers()
        {
            foreach (var item in Mine_ThrowersThreads)
            {
                item.Start();
                Thread.Sleep(50);
            }
        }

        public void StartAviations()
        {
            foreach (var item in AviationsThreads)
            { 
                item.Start();
                Thread.Sleep(50);
            }
        }       

        public void DrawTarget(Target target)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate  
            {
                if (target.GetType() == typeof(Target))
                {
                    pointsCollection = new PointCollection(); 
                    TargetUI = new Polyline();
                    pointsCollection.Add(new Point(target.X, target.Y));
                    pointsCollection.Add(new Point(target.X + 7, target.Y - 8));
                    pointsCollection.Add(new Point(target.X + 13, target.Y));
                    pointsCollection.Add(new Point(target.X + 13, target.Y + 8));
                    pointsCollection.Add(new Point(target.X, target.Y + 8));
                    TargetUI.Points = pointsCollection;
                    TargetUI.StrokeDashArray = new DoubleCollection() { 5, 1, 3, 1 };
                    TargetUI.Fill = generator.targertsColor(target);
                    TargetUI.StrokeThickness = 1.5;
                    int targetNumber = TargetList.IndexOf(target);
                    TextBlock targetNumberUI = new TextBlock();
                    targetNumberUI.Text = targetNumber.ToString();
                    targetNumberUI.FontSize = 7;
                    targetNumberUI.FontStyle = FontStyles.Italic;
                    targetNumberUI.Foreground = new SolidColorBrush(Colors.BlueViolet);
                    targetNumberUI.FontWeight = FontWeights.Bold;
                    Canvas.SetLeft(targetNumberUI, target.X);
                    Canvas.SetTop(targetNumberUI, target.Y - 2);
                    
                    root_Canvas.Children.Add(TargetUI);root_Canvas.Children.Add(targetNumberUI);
                    
                }
                else
                {
                    Color randomColor = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
                    EmptyUI = new Ellipse();
                    EmptyUI.Width = 10;
                    EmptyUI.Height = 10;
                    EmptyUI.StrokeThickness = 5;
                    EmptyUI.Margin = new Thickness(target.X - 5, target.Y - 5, 1, 1);
                    EmptyUI.Fill = new SolidColorBrush(randomColor);
                    root_Canvas.Children.Add(EmptyUI);
                }
            });
        }

        private void initTarget(Target target)
        {
            if (target.GetType() == typeof(Target))
            {
                pointsCollection = new PointCollection(); ;
                TargetUI = new Polyline();
                pointsCollection.Add(new Point(target.X, target.Y));
                pointsCollection.Add(new Point(target.X + 7, target.Y - 8));
                pointsCollection.Add(new Point(target.X + 13, target.Y));
                pointsCollection.Add(new Point(target.X + 13, target.Y + 8));
                pointsCollection.Add(new Point(target.X, target.Y + 8));
                TargetUI.Points = pointsCollection;
                TargetUI.StrokeDashArray = new DoubleCollection() { 5, 1, 3, 1 };
                TargetUI.Fill = generator.targertsColor(target);
                TargetUI.StrokeThickness = 2;

                int targetNumber = TargetList.IndexOf(target);
                TextBlock targetNumberUI = new TextBlock();
                targetNumberUI.Text = targetNumber.ToString();
                targetNumberUI.FontSize = 7;
                targetNumberUI.FontStyle = FontStyles.Italic;
                targetNumberUI.Foreground = new SolidColorBrush(Colors.Black);
                targetNumberUI.FontWeight = FontWeights.Bold;
                Canvas.SetLeft(targetNumberUI, target.X);
                Canvas.SetTop(targetNumberUI, target.Y - 2);
                root_Canvas.Children.Add(TargetUI);
                root_Canvas.Children.Add(targetNumberUI);
            }
            else
            {
                EmptyUI = new Ellipse();
                EmptyUI.Width = 10;
                EmptyUI.Height = 10;
                EmptyUI.StrokeThickness = 5;
                EmptyUI.Margin = new Thickness(target.X - 5, target.Y - 5, 1, 1);
                EmptyUI.Fill = new SolidColorBrush(Colors.MintCream);
                root_Canvas.Children.Add(EmptyUI);
            }
        }
    }
}
