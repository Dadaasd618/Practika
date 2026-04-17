using System.Windows;

namespace PractikaPM11
{
    public partial class RequestDetailWindow : Window
    {
        private ExtendedRequestInfo _request;
        private Employee _currentEmployee;
        private bool _isInBlacklist = false;

        public RequestDetailWindow(ExtendedRequestInfo request, Employee employee)
        {
            InitializeComponent();
            _request = request;
            _currentEmployee = employee;

            LoadData();
            CheckBlacklist();
        }

        private void LoadData()
        {
            txtUserEmail.Text = $"Email: {_request.UserEmail}";
            txtVisitorName.Text = $"Посетитель: {_request.VisitorFullName}";
            txtPassport.Text = $"Паспорт: {_request.VisitorPassport}";

            txtType.Text = $"Тип: {(_request.Type == "individual" ? "Личная" : "Групповая")}";
            txtDates.Text = $"Даты: {_request.StartDate} - {_request.EndDate}";
            txtDepartment.Text = $"Подразделение: {_request.DepartmentName}";
            txtPurpose.Text = $"Цель: {_request.PurposeName}";
            txtComment.Text = $"Примечание: {_request.Comment}";

            dpVisitDate.SelectedDate = _request.StartDate.ToDateTime(TimeOnly.MinValue);
        }

        private void CheckBlacklist()
        {
            bool inBlacklist = DatabaseHelper.CheckInBlacklist(_request.VisitorPassport);

            if (inBlacklist)
            {
                _isInBlacklist = true;
                txtBlacklistResult.Text = "⚠ ВНИМАНИЕ: Посетитель находится в ЧЁРНОМ СПИСКЕ!\n" +
                                          "Заявка автоматически отклонена. Изменение статуса невозможно.";
                borderBlacklist.Visibility = Visibility.Visible;
                borderBlacklist.BorderBrush = System.Windows.Media.Brushes.Red;
                borderBlacklist.Background = System.Windows.Media.Brushes.LightPink;
                txtBlacklistResult.Foreground = System.Windows.Media.Brushes.Red;
                borderManagement.Visibility = Visibility.Collapsed;

                DatabaseHelper.UpdateRequestStatus(_request.RequestId, "rejected",
                    "Заявка отклонена автоматически: посетитель в чёрном списке");
            }
            else
            {
                txtBlacklistResult.Text = "✅ Посетитель не найден в чёрном списке";
                borderBlacklist.Visibility = Visibility.Visible;
                borderBlacklist.BorderBrush = System.Windows.Media.Brushes.Green;
                borderBlacklist.Background = System.Windows.Media.Brushes.LightGreen;
                txtBlacklistResult.Foreground = System.Windows.Media.Brushes.Green;
                borderManagement.Visibility = Visibility.Visible;
                panelRejectionReason.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (dpVisitDate.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату посещения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtVisitTime.Text))
            {
                MessageBox.Show("Укажите время посещения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = DatabaseHelper.UpdateRequestStatus(_request.RequestId, "approved", null);

            if (success)
            {
                MessageBox.Show($"Заявка одобрена!\nДата посещения: {dpVisitDate.SelectedDate:dd.MM.yyyy}\nВремя: {txtVisitTime.Text}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при одобрении заявки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            string reason = txtRejectionReason.Text;
            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Укажите причину отклонения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = DatabaseHelper.UpdateRequestStatus(_request.RequestId, "rejected", reason);

            if (success)
            {
                MessageBox.Show("Заявка отклонена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при отклонении заявки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}