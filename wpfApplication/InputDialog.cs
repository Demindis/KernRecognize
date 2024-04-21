using System.Windows;
using System.Windows.Controls;

namespace wpfApplication
{
    public class InputDialog : Window
    {
        public string InputText { get; private set; }

        private TextBox textBox;
        private Button okButton;
        private Button cancelButton;

        public InputDialog()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Title = "Введите новое значение";
            Width = 300;
            Height = 150;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            textBox = new TextBox();
            textBox.Margin = new Thickness(10);
            textBox.HorizontalAlignment = HorizontalAlignment.Stretch;

            okButton = new Button();
            okButton.Content = "OK";
            okButton.Margin = new Thickness(10);
            okButton.HorizontalAlignment = HorizontalAlignment.Left;
            okButton.Click += OkButton_Click;

            cancelButton = new Button();
            cancelButton.Content = "Cancel";
            cancelButton.Margin = new Thickness(10);
            cancelButton.HorizontalAlignment = HorizontalAlignment.Right;
            cancelButton.Click += CancelButton_Click;

            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(okButton);
            stackPanel.Children.Add(cancelButton);

            Content = stackPanel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = textBox.Text;
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
