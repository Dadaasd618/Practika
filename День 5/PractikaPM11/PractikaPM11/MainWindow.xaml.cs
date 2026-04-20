using System.Windows;

namespace PractikaPM11
{
    public partial class MainWindow : Window
    {
        private int _currentUserId;

        // Конструктор по умолчанию (нужен для XAML)
        public MainWindow()
        {
            InitializeComponent();
            _currentUserId = -1;
            txtWelcome.Text = "Добро пожаловать!";
        }

        // Основной конструктор с userId
        public MainWindow(int userId) : this()
        {
            _currentUserId = userId;
            txtWelcome.Text = $"Добро пожаловать!\nВы вошли как пользователь ID: {userId}";
        }

        private void BtnNewRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserId > 0)
            {
                var requestWindow = new RequestWindow(_currentUserId);
                requestWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Ошибка авторизации!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMyRequests_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUserId > 0)
            {
                var myRequestsWindow = new MyRequestsWindow(_currentUserId);
                myRequestsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Ошибка авторизации!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}