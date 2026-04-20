using System;
using System.Windows;

namespace DepartmentTerminal
{
    public partial class RequestDetailWindow : Window
    {
        private ApprovedRequestForDepartment _request;
        private Employee _currentEmployee;

        public RequestDetailWindow(ApprovedRequestForDepartment request, Employee employee)
        {
            InitializeComponent();
            _request = request;
            _currentEmployee = employee;
            LoadData();
            CheckBlacklistStatus();
        }

        private void LoadData()
        {
            txtVisitorName.Text = $"ФИО: {_request.VisitorFullName}";
            txtPassport.Text = $"Паспорт: {_request.VisitorPassport}";
            txtPhone.Text = $"Телефон: {_request.VisitorPhone}";

            txtType.Text = $"Тип: {(_request.Type == "individual" ? "Личная" : "Групповая")}";
            txtDates.Text = $"Даты: {_request.StartDate} - {_request.EndDate}";
            txtPurpose.Text = $"Цель: {_request.PurposeName}";
            txtDepartment.Text = $"Подразделение: {_request.DepartmentName}";

            if (_request.EntryTime != null)
            {
                txtEntryTime.Text = _request.EntryTime.Value.ToString("HH:mm");
                txtStatus.Text = $"✅ Вход зафиксирован в {_request.EntryTime:HH:mm:ss}";
            }

            if (_request.ExitTime != null)
            {
                txtExitTime.Text = _request.ExitTime.Value.ToString("HH:mm");
                txtStatus.Text += $"\n🏁 Выход зафиксирован в {_request.ExitTime:HH:mm:ss}";
            }
        }

        private void CheckBlacklistStatus()
        {
            bool inBlacklist = DatabaseHelper.IsInBlacklist(_request.VisitorPassport);
            if (inBlacklist)
            {
                txtBlacklistInfo.Text = "⚠ Посетитель находится в ЧЁРНОМ СПИСКЕ!";
                txtBlacklistInfo.Foreground = System.Windows.Media.Brushes.Red;
                btnAddToBlacklist.IsEnabled = false;
            }
            else
            {
                txtBlacklistInfo.Text = "Посетитель не в чёрном списке";
                txtBlacklistInfo.Foreground = System.Windows.Media.Brushes.Green;
                btnAddToBlacklist.IsEnabled = true;
            }
        }

        private void BtnRecordEntry_Click(object sender, RoutedEventArgs e)
        {
            if (!TimeSpan.TryParse(txtEntryTime.Text, out TimeSpan time))
            {
                MessageBox.Show("Введите корректное время в формате HH:MM!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Исправление: используем DateOnly + TimeSpan
            DateTime entryTime = _request.StartDate.ToDateTime(TimeOnly.MinValue).Add(time);

            bool success = DatabaseHelper.RecordEntryTime(_request.RequestId, _request.VisitorId, entryTime);

            if (success)
            {
                MessageBox.Show($"Вход зафиксирован в {entryTime:HH:mm:ss}!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                _request.EntryTime = entryTime;
                txtStatus.Text = $"✅ Вход зафиксирован в {entryTime:HH:mm:ss}";
                txtEntryTime.Text = entryTime.ToString("HH:mm");
            }
            else
            {
                MessageBox.Show("Не удалось зафиксировать вход. Возможно, доступ ещё не разрешён охраной!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRecordExit_Click(object sender, RoutedEventArgs e)
        {
            if (_request.EntryTime == null)
            {
                MessageBox.Show("Сначала зафиксируйте вход посетителя!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(txtExitTime.Text, out TimeSpan time))
            {
                MessageBox.Show("Введите корректное время в формате HH:MM!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Исправление: используем DateOnly + TimeSpan
            DateTime exitTime = _request.StartDate.ToDateTime(TimeOnly.MinValue).Add(time);

            bool success = DatabaseHelper.RecordExitTime(_request.RequestId, exitTime);

            if (success)
            {
                MessageBox.Show($"Выход зафиксирован в {exitTime:HH:mm:ss}!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                _request.ExitTime = exitTime;
                txtStatus.Text = $"✅ Вход: {_request.EntryTime:HH:mm:ss}\n🏁 Выход: {exitTime:HH:mm:ss}";
                txtExitTime.Text = exitTime.ToString("HH:mm");
            }
            else
            {
                MessageBox.Show("Ошибка при фиксации выхода!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddToBlacklist_Click(object sender, RoutedEventArgs e)
        {
            var blacklistWindow = new AddToBlacklistWindow(_request.VisitorFullName, _request.VisitorPassport);
            if (blacklistWindow.ShowDialog() == true)
            {
                CheckBlacklistStatus();
                MessageBox.Show("Посетитель добавлен в чёрный список!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}