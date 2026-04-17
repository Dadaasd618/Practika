using System.Windows;
using System.Windows.Controls;

namespace PractikaPM11
{
    public partial class GeneralDepartmentWindow : Window
    {
        private Employee _currentEmployee;
        private List<ExtendedRequestInfo> _allRequests;

        public GeneralDepartmentWindow(Employee employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            txtWelcome.Text = $"Добро пожаловать, {employee.FullName}";
            LoadFilters();
            LoadDepartments();
            LoadRequests();
        }

        private void LoadFilters()
        {
            var types = new List<string> { "Все", "Личная", "Групповая" };
            cmbFilterType.ItemsSource = types;
            cmbFilterType.SelectedIndex = 0;

            var statuses = new List<string> { "Все", "На проверке", "Одобрена", "Отклонена" };
            cmbFilterStatus.ItemsSource = statuses;
            cmbFilterStatus.SelectedIndex = 0;
        }

        private void LoadDepartments()
        {
            var departments = DatabaseHelper.GetDepartments();
            var listWithAll = new List<Department>();
            listWithAll.Add(new Department { DepartmentId = 0, Name = "Все" });
            listWithAll.AddRange(departments);

            cmbFilterDepartment.ItemsSource = listWithAll;
            cmbFilterDepartment.SelectedIndex = 0;
        }

        private void LoadRequests()
        {
            _allRequests = DatabaseHelper.GetAllExtendedRequests();
            ApplyFilter(null, null);
        }

        private void ApplyFilter(object sender, SelectionChangedEventArgs e)
        {
            if (_allRequests == null) return;

            string typeFilter = cmbFilterType.SelectedItem as string;

            int deptFilter = 0;
            if (cmbFilterDepartment.SelectedItem != null)
            {
                deptFilter = (cmbFilterDepartment.SelectedItem as Department).DepartmentId;
            }

            string statusFilter = cmbFilterStatus.SelectedItem as string;

            var filtered = _allRequests.Where(r =>
                (typeFilter == "Все" || r.Type == (typeFilter == "Личная" ? "individual" : "group")) &&
                (deptFilter == 0 || r.DepartmentId == deptFilter) &&
                (statusFilter == "Все" || r.Status == GetStatusValue(statusFilter))
            ).ToList();

            dgRequests.ItemsSource = filtered;
        }

        private string GetStatusValue(string statusText)
        {
            switch (statusText)
            {
                case "На проверке": return "pending";
                case "Одобрена": return "approved";
                case "Отклонена": return "rejected";
                default: return "";
            }
        }

        private void DgRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRequests.SelectedItem is ExtendedRequestInfo selected)
            {
                var detailWindow = new RequestDetailWindow(selected, _currentEmployee);
                detailWindow.ShowDialog();
                LoadRequests();
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}