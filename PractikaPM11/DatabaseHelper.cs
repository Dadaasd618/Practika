using Npgsql;
using System.Security.Cryptography;
using System.Text;

namespace PractikaPM11
{
    public static class DatabaseHelper
    {
        // Строка подключения прямо здесь (просто и понятно)
        private static string connectionString = "Host=localhost;Port=5432;Database=Practice_KhranitelPRO;Username=postgres;Password=postgres";

        // MD5 хеширование
        public static string GetMD5Hash(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        // ========== АВТОРИЗАЦИЯ (3 способа) ==========

        // Способ 1: Прямой SQL
        public static int LoginSQL(string email, string password)
        {
            string hash = GetMD5Hash(password);
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT user_id FROM users WHERE email = @email AND password_hash = @hash";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@hash", hash);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        // Способ 2: Хранимая процедура
        // Способ 2: Хранимая процедура (исправленный)
        public static int LoginProcedure(string email, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT login_user(@email, @password)", conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", password);  // ← сырой пароль, НЕ хешируем!
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        // Способ 3: ORM (упрощённый, без Entity Framework)
        public static User LoginORM(string email, string password)
        {
            string hash = GetMD5Hash(password);
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT user_id, email, role FROM users WHERE email = @email AND password_hash = @hash";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@hash", hash);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserId = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                Role = reader.GetString(2)
                            };
                        }
                    }
                }
            }
            return null;
        }

        // ========== РЕГИСТРАЦИЯ (3 способа) ==========

        public static bool RegisterSQL(string email, string password)
        {
            string hash = GetMD5Hash(password);
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO users (email, password_hash, role, registered_at) VALUES (@email, @hash, 'user', NOW())";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@hash", hash);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Способ 2: Хранимая процедура (исправленный)
        public static bool RegisterProcedure(string email, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT register_user(@email, @password)", conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", password);  // ← сырой пароль, НЕ хешируем!
                    var result = cmd.ExecuteScalar();
                    return result != null && (bool)result;
                }
            }
        }

        public static bool RegisterORM(string email, string password)
        {
            string hash = GetMD5Hash(password);
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO users (email, password_hash, role, registered_at) VALUES (@email, @hash, 'user', NOW())";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@hash", hash);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ========== ЗАЯВКИ ==========

        // Получить подразделения
        public static List<Department> GetDepartments()
        {
            var list = new List<Department>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT department_id, name FROM departments";
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

        // Получить сотрудников по подразделению
        public static List<Employee> GetEmployeesByDepartment(int departmentId)
        {
            var list = new List<Employee>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT employee_id, full_name FROM employees WHERE department_id = @deptId";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@deptId", departmentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Employee
                            {
                                EmployeeId = reader.GetInt32(0),
                                FullName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            return list;
        }

        // Получить цели посещения
        public static List<VisitPurpose> GetPurposes()
        {
            var list = new List<VisitPurpose>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT purpose_id, name FROM visit_purposes";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new VisitPurpose
                        {
                            PurposeId = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return list;
        }

        // Создать заявку
        public static int CreateRequest(int userId, string type, DateOnly startDate, DateOnly endDate,
            int purposeId, int departmentId, int employeeId, string comment)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO requests (user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, comment, created_at) 
                               VALUES (@userId, @type, @startDate, @endDate, @purposeId, @deptId, @empId, 'pending', @comment, NOW())
                               RETURNING request_id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@purposeId", purposeId);
                    cmd.Parameters.AddWithValue("@deptId", departmentId);
                    cmd.Parameters.AddWithValue("@empId", employeeId);
                    cmd.Parameters.AddWithValue("@comment", comment);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // Создать посетителя и связать с заявкой (для личной заявки)
        public static bool CreateVisitorAndIndividualRequest(int requestId, Visitor visitor)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Вставка посетителя
                        string sqlVisitor = @"INSERT INTO visitors (last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, passport_scan_path) 
                                              VALUES (@lastName, @firstName, @middleName, @phone, @email, @org, @birthDate, @passportSeries, @passportNumber, @scanPath)
                                              RETURNING visitor_id";
                        int visitorId;
                        using (var cmd = new NpgsqlCommand(sqlVisitor, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@lastName", visitor.LastName);
                            cmd.Parameters.AddWithValue("@firstName", visitor.FirstName);
                            cmd.Parameters.AddWithValue("@middleName", (object?)visitor.MiddleName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@phone", (object?)visitor.Phone ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@email", visitor.Email);
                            cmd.Parameters.AddWithValue("@org", (object?)visitor.Organization ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@birthDate", visitor.BirthDate);
                            cmd.Parameters.AddWithValue("@passportSeries", visitor.PassportSeries);
                            cmd.Parameters.AddWithValue("@passportNumber", visitor.PassportNumber);
                            cmd.Parameters.AddWithValue("@scanPath", visitor.PassportScanPath);
                            visitorId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Связь с заявкой
                        string sqlInd = "INSERT INTO individual_requests (request_id, visitor_id) VALUES (@reqId, @visId)";
                        using (var cmd = new NpgsqlCommand(sqlInd, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@reqId", requestId);
                            cmd.Parameters.AddWithValue("@visId", visitorId);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }

        // Получить заявки пользователя
        public static List<RequestInfo> GetUserRequests(int userId)
        {
            var list = new List<RequestInfo>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT r.request_id, r.type, r.start_date, r.end_date, r.status, r.rejection_reason, 
                                      d.name as department_name, vp.name as purpose_name
                               FROM requests r
                               LEFT JOIN departments d ON r.department_id = d.department_id
                               LEFT JOIN visit_purposes vp ON r.purpose_id = vp.purpose_id
                               WHERE r.user_id = @userId
                               ORDER BY r.created_at DESC";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new RequestInfo
                            {
                                RequestId = reader.GetInt32(0),
                                Type = reader.GetString(1),
                                StartDate = DateOnly.FromDateTime(reader.GetDateTime(2)),
                                EndDate = DateOnly.FromDateTime(reader.GetDateTime(3)),
                                Status = reader.GetString(4),
                                RejectionReason = reader.IsDBNull(5) ? null : reader.GetString(5),
                                DepartmentName = reader.GetString(6),
                                PurposeName = reader.GetString(7)
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}