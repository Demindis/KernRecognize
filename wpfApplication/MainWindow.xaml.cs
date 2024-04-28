using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace wpfApplication
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public static int X;
        public static int Y;
        public static string PREResult;
        public static string ImagePath;
        public static int ImageWidth;
        public static int ImageHeight;
        public Kern[] kernArray;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new Content();
        }

        /// <summary>
        /// Interaction logic for MainWindow.xamlR
        /// </summary>
        // Создадим класс для реализации интерфейса ICommand
        // Чтобы использовать его в XAML'е
        public class Command : ICommand
        {
            public Command(System.Action action)
            {
                this.action = action;
            }

            System.Action action;

            EventHandler canExecuteChanged;

            event EventHandler ICommand.CanExecuteChanged
            {
                add { canExecuteChanged += value; }
                remove { canExecuteChanged -= value; }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                action();
            }
        }

        // В классе Content реализуем интерфейс INotifyPropertyChanged
        // Чтобы послать нотификацию о том, что свойство Image поменялось
        public class Content : INotifyPropertyChanged
        {
            public Content()
            {
                // Инициализация команды
                openFileDialogCommand = new Command(ExecuteOpenFileDialog);
                // Инициализация OpenFileDialog
                openFileDialog = new OpenFileDialog()
                {
                    Multiselect = true,
                    Filter =
                        "Image files (*.BMP, *.JPG, *.GIF, *.TIF, *.PNG, *.ICO, *.EMF, *.WMF)|*.bmp;*.jpg;*.gif; *.tif; *.png; *.ico; *.emf; *.wmf"
                };
            }

            readonly OpenFileDialog openFileDialog;

            // Наша картинка
            public ImageSource Image { get; private set; }

            readonly ICommand openFileDialogCommand;

            public ICommand OpenFileDialogCommand
            {
                get { return openFileDialogCommand; }
            }

            // Действие при нажатии на кнопку "Open File Dialog"
            void ExecuteOpenFileDialog()
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    
                    string ImagePath1 = openFileDialog.FileName;
                    string ImagePathNew = "C:\\Users\\Admin\\img.jpg";
                    Mat img = Cv2.ImRead(ImagePath1);
                    Cv2.ImWrite(ImagePathNew, img);

                    using (var stream = new FileStream(ImagePathNew, FileMode.Open))
                    {
                        BitmapFrame imageFrame = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                        ImageWidth = imageFrame.PixelWidth;
                        ImageHeight = imageFrame.PixelHeight;

                        ImagePath = ImagePathNew;
                        Image = imageFrame;
                        RaisePropertyChanged("Image");
                    }
                }
            }

            void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            // Реализация интерфейса INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            
        }


        //обработчик события MouseWheel
        private void Image_photoKerna_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleX = ImageScaleTransform.ScaleX;
            double scaleY = ImageScaleTransform.ScaleY;

            System.Windows.Point position = e.GetPosition(Image_photoKerna);

            // Фактор изменения масштаба
            double zoomFactor = 0.1; 

            // Уменьшаем масштаб при прокрутке колеса вниз и увеличиваем при прокрутке вверх
            if (e.Delta > 0)
            {
                scaleX += zoomFactor;
                scaleY += zoomFactor;
            }
            else
            {
                scaleX -= zoomFactor;
                scaleY -= zoomFactor;
            }

            // Ограничиваем минимальный и максимальный масштаб
            double minScale = 1; 
            double maxScale = 5.0;
            scaleX = Math.Min(Math.Max(scaleX, minScale), maxScale);
            scaleY = Math.Min(Math.Max(scaleY, minScale), maxScale);

            // Определяем смещение для сохранения позиции курсора
            double offsetX = (position.X - Image_photoKerna.ActualWidth / 2) * (scaleX - ImageScaleTransform.ScaleX);
            double offsetY = (position.Y - Image_photoKerna.ActualHeight / 2) * (scaleY - ImageScaleTransform.ScaleY);

            // Применяем новый масштаб и смещение
            ImageScaleTransform.ScaleX = scaleX;
            ImageScaleTransform.ScaleY = scaleY;
            ImageTranslateTransform.X -= offsetX;
            ImageTranslateTransform.Y -= offsetY;
        }



        private void PhotoFolder_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Controls.Image image && e != null)
                {

                    System.Windows.Point pointsPosition = e.GetPosition(image);

                    double currentWidth = image.ActualWidth;
                    double currentHeight = image.ActualHeight;

                    X = (int)(pointsPosition.X / currentWidth * ImageWidth);
                    Y = (int)(pointsPosition.Y / currentHeight * ImageHeight);
                    string csv_Path = PREResult;

                    kernArray = KernParser.ParseKernsFromCsv(csv_Path);
                    int element = SearchId.BoxInResult(X, Y, kernArray);

                    if (element != -1)
                    {
                        RedImage dialog = new RedImage();
                        if (dialog.ShowDialog() == true)
                        {
                            // Получение введенного пользователем значения
                            string newValue = dialog.InputText;

                            // Преобразование нового значения в int

                            // Проверяем, что элемент с индексом element существует в массиве kernArray и не выходит за его границы
                            if (element >= 0 && element < kernArray.Length)
                            {
                                // Обновляем значение элемента с найденным индексом на новое значение
                                kernArray[element].Number = Convert.ToString(newValue);
                                Mat drawnImage = Drawing.Draw(ImagePath, kernArray);
                                BitmapSource bitmapSource = drawnImage.ToBitmapSource();
                                Image_photoKerna.Source = bitmapSource;
                            }
                            else
                            {
                                MessageBox.Show("Ошибка: элемент с таким индексом не найден.");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ошибка: элемент с таким значением не найден.");
                    }
                }

            }
            catch

            {
                MessageBox.Show("Для редактирования значений выполните распознавание");
            }
        }



        private async void Button_detect_Click(object sender, RoutedEventArgs e) // асинхронный метод
        {
            Grid_main.Effect = new BlurEffect { Radius = 15 };
            LoadingGif.Visibility = Visibility.Visible;

            string pythonScriptPath = @"C:\Users\Admin\source\repos\DenisDemin1\KernRecognize\wpfApplication\1.py"; // Укажите путь к вашему скрипту Python здесь
            // Путь к изображению
            //string imagePath = MyGlobals.ImagePath; // Укажите путь к вашему изображению здесь
            if (ImagePath != null)
            {
                // Создание процесса для выполнения Python скрипта
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "pythonw"; // Имя исполняемого файла Python
                psi.Arguments =
                    $"{pythonScriptPath} \"{ImagePath}\""; // Аргументы командной строки (путь к скрипту и изображению)
                psi.UseShellExecute =
                    false; // Установка UseShellExecute в false, чтобы получить доступ к стандартному выводу процесса
                psi.RedirectStandardOutput =
                    true; // Перенаправление стандартного вывода для получения результата работы скрипта

                try
                {
                    // Создание процесса и запуск скрипта Python
                    using (Process process = Process.Start(psi))
                    {
                        await Task.Run(() => process.WaitForExit()); //асинхроный запуск
                        // Получение вывода Python скрипта
                        using (StreamReader reader = process.StandardOutput)
                        {
                            PREResult = reader.ReadToEnd();

                        }
                    }

                    LoadingGif.Visibility = Visibility.Hidden;
                    Grid_main.Effect = null; ;

                    string csv_path = PREResult;
                    kernArray = KernParser.ParseKernsFromCsv(csv_path);
                    Mat drawnImage = Drawing.Draw(ImagePath, kernArray);
                    BitmapSource bitmapSource = drawnImage.ToBitmapSource();
                    Image_photoKerna.Source = bitmapSource;


                    MessageBox.Show($"Успешно распознано");
                }
                catch (Exception ex)
                {
                    LoadingGif.Visibility = Visibility.Hidden;
                    Grid_main.Effect = null;
                    MessageBox.Show($"Ошибка распознавания: {ex.Message}");

                }
            }
            else
            {
                LoadingGif.Visibility = Visibility.Hidden;
                Grid_main.Effect = null;
                MessageBox.Show($"Ошибка, выберете фото", "Ошибка распозонавания", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
        }
        
        private void Button_addToExcelFile_Click(object sender, RoutedEventArgs e)
        {


            if (PREResult != null)
            {
                // Показываем диалоговое окно "Сохранить как"
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;


                if (saveFileDialog.ShowDialog() == true)
                {
                    // Сохранение значения в выбранный файл CSV
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                        {
                            foreach (Kern item in kernArray)
                            {
                                sw.WriteLine(item.Number);
                            } // Запись значения в файл
                        }

                        MessageBox.Show("Значение успешно сохранено в файл CSV.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении значения в файл CSV: {ex.Message}");
                        return;
                    }

                    // Открытие файла CSV в Excel
                    try
                    {
                        Process.Start("excel.exe", saveFileDialog.FileName); // Запуск Excel и открытие файла CSV
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файла CSV в Excel: {ex.Message}");
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show($"Ошибка, файл пуст, выполните распознование", "Ошибка сохранения", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
        }

    }
}


