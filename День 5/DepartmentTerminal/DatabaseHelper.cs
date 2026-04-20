using Npgsql;
using System;
using System.Collections.Generic;

namespace DepartmentTerminal
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Host=localhost;Port=5432;Database=Practice_KhranitelPRO;Username=postgres;Password=postgres";

        // Получить сотрудника по коду (только сотрудники подразделений)
        public static Employee GetDepartmentEmployeeByCode(int employeeCode)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT e.employee_id, e.full_name, e.department_id, e.section, d.name as department_name
                    FROM employees e
                    LEFT JOIN departments d ON e.department_id = d.department_id
                    WHERE e.employee_id = @code AND e.department_id IS NOT NULL";
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
                                Section = reader.IsDBNull(3) ? null : reader.GetString(3),
                                DepartmentName = reader.IsDBNull(4) ? null : reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Получить одобренные заявки для подразделения
        public static List<ApprovedRequestForDepartment> GetApprovedRequestsForDepartment(int departmentId)
        {
            var list = new List<ApprovedRequestForDepartment>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Запрос для личных заявок
                string sqlIndividual = @"
            SELECT 
                r.request_id, r.type, r.start_date, r.end_date,
                d.name as department_name, d.department_id,
                vp.name as purpose_name,
                COALESCE(v.last_name || ' ' || v.first_name, 'Посетитель') as visitor_name,
                COALESCE(v.passport_series || ' ' || v.passport_number, '') as visitor_passport,
                COALESCE(v.phone, '') as phone,
                p.entry_time, p.exit_time,
                COALESCE(v.visitor_id, 0) as visitor_id
            FROM requests r
            LEFT JOIN departments d ON r.department_id = d.department_id
            LEFT JOIN visit_purposes vp ON r.purpose_id = vp.purpose_id
            LEFT JOIN individual_requests ir ON r.request_id = ir.request_id
            LEFT JOIN visitors v ON ir.visitor_id = v.visitor_id
            LEFT JOIN passes p ON r.request_id = p.request_id
            WHERE r.status = 'approved' AND r.department_id = @deptId AND r.type = 'individual'
            ORDER BY r.start_date ASC";

                using (var cmd = new NpgsqlCommand(sqlIndividual, conn))
                {
                    cmd.Parameters.AddWithValue("@deptId", departmentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ApprovedRequestForDepartment
                            {
                                RequestId = reader.GetInt32(0),
                                Type = reader.GetString(1),
                                StartDate = DateOnly.FromDateTime(reader.GetDateTime(2)),
                                EndDate = DateOnly.FromDateTime(reader.GetDateTime(3)),
                                DepartmentName = reader.GetString(4),
                                DepartmentId = reader.GetInt32(5),
                                PurposeName = reader.GetString(6),
                                VisitorFullName = reader.GetString(7),
                                VisitorPassport = reader.GetString(8),
                                VisitorPhone = reader.GetString(9),
                                EntryTime = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                                ExitTime = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                                VisitorId = reader.GetInt32(12)
                            });
                        }
                    }
                }

                // Запрос для групповых заявок (с правильным приведением типов)
                string sqlGroup = @"
            SELECT 
                r.request_id, r.type, r.start_date, r.end_date,
                d.name as department_name, d.department_id,
                vp.name as purpose_name,
                'Групповая заявка' as visitor_name,
                '' as visitor_passport,
                '' as phone,
                p.entry_time::timestamp as entry_time,
                p.exit_time::timestamp as exit_time,
                0 as visitor_id
            FROM requests r
            LEFT JOIN departments d ON r.department_id = d.department_id
            LEFT JOIN visit_purposes vp ON r.purpose_id = vp.purpose_id
            LEFT JOIN passes p ON r.request_id = p.request_id
            WHERE r.status = 'approved' AND r.department_id = @deptId AND r.type = 'group'
            ORDER BY r.start_date ASC";

                using (var cmd = new NpgsqlCommand(sqlGroup, conn))
                {
                    cmd.Parameters.AddWithValue("@deptId", departmentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime? entryTime = null;
                            DateTime? exitTime = null;

                            // Безопасное чтение времени
                            try
                            {
                                if (!reader.IsDBNull(8))
                                    entryTime = reader.GetDateTime(8);
                            }
                            catch { entryTime = null; }

                            try
                            {
                                if (!reader.IsDBNull(9))
                                    exitTime = reader.GetDateTime(9);
                            }
                            catch { exitTime = null; }

                            list.Add(new ApprovedRequestForDepartment
                            {
                                RequestId = reader.GetInt32(0),
                                Type = reader.GetString(1),
                                StartDate = DateOnly.FromDateTime(reader.GetDateTime(2)),
                                EndDate = DateOnly.FromDateTime(reader.GetDateTime(3)),
                                DepartmentName = reader.GetString(4),
                                DepartmentId = reader.GetInt32(5),
                                PurposeName = reader.GetString(6),
                                VisitorFullName = reader.GetString(7),
                                VisitorPassport = "",
                                VisitorPhone = "",
                                EntryTime = entryTime,
                                ExitTime = exitTime,
                                VisitorId = 0
                            });
                        }
                    }
                }
            }
            return list;
        }

        // Зафиксировать вход посетителя
        public static bool RecordEntryTime(int requestId, int visitorId, DateTime entryTime)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string checkSql = "SELECT pass_id, entry_time FROM passes WHERE request_id = @requestId";
                int? passId = null;
                DateTime? existingEntry = null;
                using (var cmd = new NpgsqlCommand(checkSql, conn))
                {
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            passId = reader.GetInt32(0);
                            existingEntry = reader.IsDBNull(1) ? null : reader.GetDateTime(1);
                        }
                    }
                }

                if (passId == null || existingEntry != null)
                {
                    return false;
                }

                string sql = "UPDATE passes SET entry_time = @entryTime WHERE request_id = @requestId";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@entryTime", entryTime);
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Зафиксировать выход посетителя
        public static bool RecordExitTime(int requestId, DateTime exitTime)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE passes SET exit_time = @exitTime WHERE request_id = @requestId";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@exitTime", exitTime);
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Добавить в чёрный список (без ON CONFLICT)
        public static bool AddToBlacklist(string passportNumber, string fullName, string reason)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Сначала проверяем, есть ли уже запись
                string checkSql = "SELECT COUNT(*) FROM blacklist WHERE passport_number = @passport";
                using (var checkCmd = new NpgsqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@passport", passportNumber);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        // Обновляем существующую запись
                        string updateSql = "UPDATE blacklist SET reason = @reason, added_at = NOW() WHERE passport_number = @passport";
                        using (var updateCmd = new NpgsqlCommand(updateSql, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@reason", reason);
                            updateCmd.Parameters.AddWithValue("@passport", passportNumber);
                            return updateCmd.ExecuteNonQuery() > 0;
                        }
                    }
                    else
                    {
                        // Добавляем новую запись
                        string insertSql = @"
                    INSERT INTO blacklist (passport_number, full_name, reason, added_at) 
                    VALUES (@passport, @fullName, @reason, NOW())";
                        using (var insertCmd = new NpgsqlCommand(insertSql, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@passport", passportNumber);
                            insertCmd.Parameters.AddWithValue("@fullName", fullName);
                            insertCmd.Parameters.AddWithValue("@reason", reason);
                            return insertCmd.ExecuteNonQuery() > 0;
                        }
                    }
                }
            }
        }

        // Проверка в чёрном списке
        public static bool IsInBlacklist(string passportNumber)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM blacklist WHERE passport_number = @passport";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@passport", passportNumber);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    }
}