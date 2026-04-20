using System.Windows;
using System.Media;

namespace SecurityTerminal
{
    public partial class PassWindow : Window
    {
        private ApprovedRequest _request;
        private Employee _currentEmployee;

        public PassWindow(ApprovedRequest request, Employee employee)
        {
            InitializeComponent();
            _request = request;
            _currentEmployee = employee;
            LoadData();
        }

        private void LoadData()
        {
            txtVisitorName.Text = $"Посетитель: {_request.VisitorFullName}";
            txtPassport.Text = $"Паспорт: {_request.VisitorPassport}";
            txtPhone.Text = $"Телефон: {_request.VisitorPhone}";
            txtDepartment.Text = $"Подразделение: {_request.DepartmentName}";
            txtPurpose.Text = $"Цель: {_request.PurposeName}";
            txtDate.Text = $"Дата посещения: {_request.StartDate} - {_request.EndDate}";
            txtStatus.Text = $"Статус: {_request.StatusText}";

            // Если уже прошёл, показываем время входа/выхода
            if (_request.EntryTime != null)
            {
                txtMessage.Text = $"Вход: {_request.EntryTime:HH:mm:ss}";
                if (_request.ExitTime != null)
                    txtMessage.Text += $"\nВыход: {_request.ExitTime:HH:mm:ss}";
            }
        }

        private void BtnGrantAccess_Click(object sender, RoutedEventArgs e)
        {
            // Системный звук
            SystemSounds.Beep.Play();

            // Отправка сообщения на сервер об открытии турникета (эмуляция)
            MessageBox.Show("🚪 ТУРНИКЕТ ОТКРЫТ\nРазрешение на проход получено!",
                "Доступ разрешён", MessageBoxButton.OK, MessageBoxImage.Information);

            // Фиксируем время начала посещения в БД
            bool success = DatabaseHelper.GrantAccess(_request.RequestId, _currentEmployee.EmployeeId);

            if (success)
            {
                txtMessage.Text = $"✅ Доступ разрешён в {DateTime.Now:HH:mm:ss}";
                txtStatus.Text = "Статус: 🟢 На территории";
            }
            else
            {
                MessageBox.Show("Ошибка при фиксации доступа!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRecordExit_Click(object sender, RoutedEventArgs e)
        {
            bool success = DatabaseHelper.RecordExit(_request.RequestId);

            if (success)
            {
                txtMessage.Text = $"🏁 Выход зафиксирован в {DateTime.Now:HH:mm:ss}";
                txtStatus.Text = "Статус: 🔴 Покинул";

                MessageBox.Show("Выход посетителя зафиксирован!",
                    "Выход", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ошибка при фиксации выхода!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}