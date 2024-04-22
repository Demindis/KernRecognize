using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
                    string ImagePathNew = "C:\\Users\\Denis\\img.jpg";
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


            // Реализация интерфейса INotifyPropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Реализация изменения масштаба изображнения прокруткой  
        private double currentScale = 1.0; // Текущий масштаб изображения
        private double minScale = 0.5; // Минимальный масштаб
        private double maxScale = 2.0; // Максимальный масштаб
        private double scaleStep = 0.1; // Шаг изменения масштаба

        //обработчик события MouseWheel
        private void Image_photoKerna_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Получение текущего масштаба
            double currentScaleX = ImageScaleTransform.ScaleX;
            double currentScaleY = ImageScaleTransform.ScaleY;

            // Определение направления прокрутки
            int delta = e.Delta > 0 ? 1 : -1;

            // Рассчитываем новый масштаб
            double newScaleX = currentScaleX + delta * scaleStep;
            double newScaleY = currentScaleY + delta * scaleStep;

            // Проверяем, чтобы новый масштаб не выходил за пределы minScale и maxScale
            newScaleX = Math.Max(minScale, Math.Min(maxScale, newScaleX));
            newScaleY = Math.Max(minScale, Math.Min(maxScale, newScaleY));

            // Получаем размеры рамки
            double containerWidth = ImageContainer.ActualWidth;
            double containerHeight = ImageContainer.ActualHeight;

            // Получаем размеры изображения после масштабирования
            double newImageWidth = Image_photoKerna.ActualWidth * newScaleX;
            double newImageHeight = Image_photoKerna.ActualHeight * newScaleY;

            // Проверяем, чтобы новый размер изображения не выходил за пределы размеров рамки
            if (newImageWidth <= containerWidth && newImageHeight <= containerHeight)
            {
                // Применяем новый масштаб
                ImageScaleTransform.ScaleX = newScaleX;
                ImageScaleTransform.ScaleY = newScaleY;
            }
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
                        InputDialog dialog = new InputDialog();
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

            string pythonScriptPath = "1.py"; // Укажите путь к вашему скрипту Python здесь
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



