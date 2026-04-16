using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace PractikaPM11
{
    public partial class MyRequestsWindow : Window
    {
        private int _currentUserId;

        public MyRequestsWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadRequests();
        }

        private void LoadRequests()
        {
            var requests = DatabaseHelper.GetUserRequests(_currentUserId);
            dgRequests.ItemsSource = requests;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Конвертер для цвета статуса
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string ?? "";

            if (status.Contains("✅"))
                return System.Windows.Media.Brushes.Green;
            else if (status.Contains("❌"))
                return System.Windows.Media.Brushes.Red;
            else
                return System.Windows.Media.Brushes.Orange;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}