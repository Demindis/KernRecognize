using System.Windows;

namespace wpfApplication
{
    /// <summary>
    /// Логика взаимодействия для RedImage.xaml
    /// </summary>
    public partial class RedImage : Window
    {
        public string InputText { get; private set; }

        public RedImage()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = textBoxRed.Text;
            DialogResult = true;
            Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
