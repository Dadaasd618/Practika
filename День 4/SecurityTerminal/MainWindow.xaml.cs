using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SecurityTerminal
{
    public partial class MainWindow : Window
    {
        private Employee _currentEmployee;
        private List<ApprovedRequest> _allRequests;
        private List<Department> _departments;

        public MainWindow(Employee employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            txtWelcome.Text = $"Добро пожаловать, {employee.FullName}";
            LoadDepartments();
            LoadRequests();
        }

        private void LoadDepartments()
        {
            _departments = DatabaseHelper.GetDepartments();
            var listWithAll = new List<Department>();
            listWithAll.Add(new Department { DepartmentId = 0, Name = "Все" });
            listWithAll.AddRange(_departments);
            cmbFilterDepartment.ItemsSource = listWithAll;
            cmbFilterDepartment.DisplayMemberPath = "Name";
            cmbFilterDepartment.SelectedValuePath = "DepartmentId";
            cmbFilterDepartment.SelectedIndex = 0;
        }

        private void LoadRequests()
        {
            _allRequests = DatabaseHelper.GetApprovedRequests();
            ApplyFilter(null, null);
        }

        private void ApplyFilter(object sender, SelectionChangedEventArgs e)
        {
            if (_allRequests == null) return;

            string typeFilter = (cmbFilterType.SelectedItem as ComboBoxItem)?.Content.ToString();

            int deptFilter = 0;
            if (cmbFilterDepartment.SelectedItem != null)
            {
                deptFilter = (cmbFilterDepartment.SelectedItem as Department).DepartmentId;
            }

            DateOnly? dateFilter = dpFilterDate.SelectedDate != null ? DateOnly.FromDateTime(dpFilterDate.SelectedDate.Value) : (DateOnly?)null;

            var filtered = _allRequests.Where(r =>
                (typeFilter == "Все" || r.Type == (typeFilter == "Личная" ? "individual" : "group")) &&
                (deptFilter == 0 || r.DepartmentId == deptFilter) &&
                (dateFilter == null || r.StartDate == dateFilter)
            ).ToList();

            dgRequests.ItemsSource = filtered;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtSearch.Text;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadRequests();
            }
            else
            {
                var searched = DatabaseHelper.SearchApprovedRequests(searchText);
                dgRequests.ItemsSource = searched;
            }
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbFilterType.SelectedIndex = 0;
            cmbFilterDepartment.SelectedIndex = 0;
            dpFilterDate.SelectedDate = null;
            txtSearch.Text = "";
            LoadRequests();
        }

        private void DgRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRequests.SelectedItem is ApprovedRequest selected)
            {
                var passWindow = new PassWindow(selected, _currentEmployee);
                passWindow.ShowDialog();
                LoadRequests();
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}