using Npgsql;
using System;
using System.Collections.Generic;

namespace SecurityTerminal
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Host=localhost;Port=5432;Database=Practice_KhranitelPRO;Username=postgres;Password=postgres";

        // Получить сотрудника по коду (только охрана)
        public static Employee GetSecurityEmployeeByCode(int employeeCode)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT employee_id, full_name, department_id, section FROM employees WHERE employee_id = @code AND section = 'Охрана'";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@code", employeeCode);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Employee
                            {
                                EmployeeId = reader.GetInt32(0),
                                FullName = reader.GetString(1),
                                DepartmentId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                Section = reader.IsDBNull(3) ? null : reader.GetString(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Получить одобренные заявки
        public static List<ApprovedRequest> GetApprovedRequests()
        {
            var list = new List<ApprovedRequest>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
            SELECT 
                r.request_id, r.type, r.start_date, r.end_date,
                d.name as department_name,
                d.department_id,
                vp.name as purpose_name,
                COALESCE(v.last_name || ' ' || v.first_name, 'Групповая заявка') as visitor_name,
                v.passport_series || ' ' || v.passport_number as visitor_passport,
                v.phone,
                p.entry_time, p.exit_time
            FROM requests r
            LEFT JOIN departments d ON r.department_id = d.department_id
            LEFT JOIN visit_purposes vp ON r.purpose_id = vp.purpose_id
            LEFT JOIN individual_requests ir ON r.request_id = ir.request_id
            LEFT JOIN visitors v ON ir.visitor_id = v.visitor_id
            LEFT JOIN passes p ON r.request_id = p.request_id
            WHERE r.status = 'approved'
            ORDER BY r.start_date ASC";

                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ApprovedRequest
                        {
                            RequestId = reader.GetInt32(0),
                            Type = reader.GetString(1),
                            StartDate = DateOnly.FromDateTime(reader.GetDateTime(2)),
                            EndDate = DateOnly.FromDateTime(reader.GetDateTime(3)),
                            DepartmentName = reader.GetString(4),
                            DepartmentId = reader.GetInt32(5),
                            PurposeName = reader.GetString(6),
                            VisitorFullName = reader.GetString(7),
                            VisitorPassport = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            VisitorPhone = reader.IsDBNull(9) ? "" : reader.GetString(9),
                            EntryTime = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                            ExitTime = reader.IsDBNull(11) ? null : reader.GetDateTime(11)
                        });
                    }
                }
            }
            return list;
        }

        // Поиск заявки по паспорту или ФИО
        public static List<ApprovedRequest> SearchApprovedRequests(string searchText)
        {
            var all = GetApprovedRequests();
            if (string.IsNullOrWhiteSpace(searchText))
                return all;

            searchText = searchText.ToLower();
            return all.Where(r =>
                r.VisitorFullName.ToLower().Contains(searchText) ||
                r.VisitorPassport.ToLower().Contains(searchText)
            ).ToList();
        }

        // Фильтрация одобренных заявок
        public static List<ApprovedRequest> FilterApprovedRequests(string type, int? departmentId, DateOnly? date)
        {
            var all = GetApprovedRequests();
            return all.Where(r =>
                (string.IsNullOrEmpty(type) || r.Type == type) &&
                (departmentId == null || r.DepartmentId == departmentId) &&
                (date == null || r.StartDate == date)
            ).ToList();
        }

        // Разрешить доступ (открыть турникет)
        public static bool GrantAccess(int requestId, int securityEmployeeId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Проверяем, есть ли уже пропуск
                string checkSql = "SELECT pass_id FROM passes WHERE request_id = @requestId";
                int? passId = null;
                using (var cmd = new NpgsqlCommand(checkSql, conn))
                {
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    var result = cmd.ExecuteScalar();
                    passId = result != null ? Convert.ToInt32(result) : (int?)null;
                }

                if (passId == null)
                {
                    // Создаём пропуск
                    string insertSql = @"
                        INSERT INTO passes (request_id, issued_by, entry_time) 
                        VALUES (@requestId, @issuedBy, NOW())
                        RETURNING pass_id";
                    using (var cmd = new NpgsqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@requestId", requestId);
                        cmd.Parameters.AddWithValue("@issuedBy", securityEmployeeId);
                        passId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                else
                {
                    // Обновляем время входа
                    string updateSql = "UPDATE passes SET entry_time = NOW() WHERE pass_id = @passId";
                    using (var cmd = new NpgsqlCommand(updateSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@passId", passId);
                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
        }

        // Зафиксировать выход
        public static bool RecordExit(int requestId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE passes SET exit_time = NOW() WHERE request_id = @requestId";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        // Получить список подразделений
        public static List<Department> GetDepartments()
        {
            var list = new List<Department>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT department_id, name FROM departments ORDER BY name";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Department
                        {
                            DepartmentId = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }
    }
}