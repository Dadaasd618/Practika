using Npgsql;
using System.Security.Cryptography;
using System.Text;
using PractikaPM11.Data;

namespace PractikaPM11
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Host=localhost;Port=5432;Database=Practice_KhranitelPRO;Username=postgres;Password=postgres";

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
        public static int LoginProcedure(string email, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT login_user(@email, @password)", conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", password);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
        }

        // Способ 3: ORM (Entity Framework)
        public static User LoginORM(string email, string password)
        {
            string hash = GetMD5Hash(password);
            using (var context = new AppDbContext())
            {
                return context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);
            }
        }

        // ========== РЕГИСТРАЦИЯ (3 способа) ==========

        // Способ 1: Прямой SQL
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

        // Способ 2: Хранимая процедура
        public static bool RegisterProcedure(string email, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT register_user(@email, @password)", conn))
                {
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@password", password);
                    var result = cmd.ExecuteScalar();
                    return result != null && (bool)result;
                }
            }
        }

        // Способ 3: ORM (Entity Framework)
        public static bool RegisterORM(string email, string password)
        {
            string hash = GetMD5Hash(password);
            using (var context = new AppDbContext())
            {
                var user = new User
                {
                    Email = email,
                    PasswordHash = hash,
                    Role = "user",
                    RegisteredAt = DateTime.Now
                };
                context.Users.Add(user);
                return context.SaveChanges() > 0;
            }
        }

        // ========== ОСТАЛЬНЫЕ МЕТОДЫ (без изменений) ==========

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
                        list.Add(new Department { DepartmentId = reader.GetInt32(0), Name = reader.GetString(1) });
                    }
                }
            }
            return list;
        }

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
                            list.Add(new Employee { EmployeeId = reader.GetInt32(0), FullName = reader.GetString(1) });
                        }
                    }
                }
            }
            return list;
        }

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
                        list.Add(new VisitPurpose { PurposeId = reader.GetInt32(0), Name = reader.GetString(1) });
                    }
                }
            }
            return list;
        }

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

        public static bool CreateVisitorAndIndividualRequest(int requestId, Visitor visitor)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
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
        // Добавь этот метод в конец класса DatabaseHelper

        // Создание групповой заявки (SQL способ)
        public static bool CreateGroupRequest(int userId, string type, DateOnly startDate, DateOnly endDate,
            int purposeId, int departmentId, int employeeId, string comment, List<GroupMember> members)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Создаём заявку
                        string sqlRequest = @"INSERT INTO requests (user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, comment, created_at) 
                                      VALUES (@userId, @type, @startDate, @endDate, @purposeId, @deptId, @empId, 'pending', @comment, NOW())
                                      RETURNING request_id";
                        int requestId;
                        using (var cmd = new NpgsqlCommand(sqlRequest, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@type", type);
                            cmd.Parameters.AddWithValue("@startDate", startDate);
                            cmd.Parameters.AddWithValue("@endDate", endDate);
                            cmd.Parameters.AddWithValue("@purposeId", purposeId);
                            cmd.Parameters.AddWithValue("@deptId", departmentId);
                            cmd.Parameters.AddWithValue("@empId", employeeId);
                            cmd.Parameters.AddWithValue("@comment", comment);
                            requestId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 2. Создаём групповую заявку
                        string sqlGroup = "INSERT INTO group_requests (request_id) VALUES (@requestId) RETURNING group_id";
                        int groupId;
                        using (var cmd = new NpgsqlCommand(sqlGroup, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@requestId", requestId);
                            groupId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 3. Добавляем каждого участника
                        int row = 1;
                        foreach (var member in members)
                        {
                            // Вставляем посетителя
                            string sqlVisitor = @"INSERT INTO visitors (last_name, first_name, middle_name, phone, email, birth_date, passport_series, passport_number, passport_scan_path) 
                                          VALUES (@lastName, @firstName, @middleName, @phone, @email, @birthDate, @passportSeries, @passportNumber, '')
                                          RETURNING visitor_id";
                            int visitorId;
                            using (var cmd = new NpgsqlCommand(sqlVisitor, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@lastName", member.LastName);
                                cmd.Parameters.AddWithValue("@firstName", member.FirstName);
                                cmd.Parameters.AddWithValue("@middleName", (object?)member.MiddleName ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@phone", (object?)member.Phone ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@email", member.Email);
                                cmd.Parameters.AddWithValue("@birthDate", member.BirthDate);
                                cmd.Parameters.AddWithValue("@passportSeries", member.PassportSeries);
                                cmd.Parameters.AddWithValue("@passportNumber", member.PassportNumber);
                                visitorId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            // Связываем с группой
                            string sqlMember = "INSERT INTO group_members (group_id, visitor_id, row_number) VALUES (@groupId, @visitorId, @rowNum)";
                            using (var cmd = new NpgsqlCommand(sqlMember, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@groupId", groupId);
                                cmd.Parameters.AddWithValue("@visitorId", visitorId);
                                cmd.Parameters.AddWithValue("@rowNum", row);
                                cmd.ExecuteNonQuery();
                            }
                            row++;
                        }

                        tran.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }
        // ORM создание индивидуальной заявки
        public static bool CreateIndividualRequestORM(int userId, string type, DateOnly startDate, DateOnly endDate,
            int purposeId, int departmentId, int employeeId, string comment, Visitor visitor)
        {
            using (var context = new AppDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var request = new Request
                        {
                            UserId = userId,
                            Type = type,
                            StartDate = startDate,
                            EndDate = endDate,
                            PurposeId = purposeId,
                            DepartmentId = departmentId,
                            EmployeeId = employeeId,
                            Status = "pending",
                            Comment = comment,
                            CreatedAt = DateTime.Now
                        };
                        context.Requests.Add(request);
                        context.SaveChanges();

                        context.Visitors.Add(visitor);
                        context.SaveChanges();

                        var individualRequest = new IndividualRequest
                        {
                            RequestId = request.RequestId,
                            VisitorId = visitor.VisitorId
                        };
                        context.IndividualRequests.Add(individualRequest);
                        context.SaveChanges();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        // ORM создание групповой заявки
        public static bool CreateGroupRequestORM(int userId, string type, DateOnly startDate, DateOnly endDate,
            int purposeId, int departmentId, int employeeId, string comment, List<GroupMember> members)
        {
            using (var context = new AppDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var request = new Request
                        {
                            UserId = userId,
                            Type = type,
                            StartDate = startDate,
                            EndDate = endDate,
                            PurposeId = purposeId,
                            DepartmentId = departmentId,
                            EmployeeId = employeeId,
                            Status = "pending",
                            Comment = comment,
                            CreatedAt = DateTime.Now
                        };
                        context.Requests.Add(request);
                        context.SaveChanges();

                        var groupRequest = new GroupRequest { RequestId = request.RequestId };
                        context.GroupRequests.Add(groupRequest);
                        context.SaveChanges();

                        int row = 1;
                        foreach (var member in members)
                        {
                            var visitor = new Visitor
                            {
                                LastName = member.LastName,
                                FirstName = member.FirstName,
                                MiddleName = member.MiddleName,
                                Phone = member.Phone,
                                Email = member.Email,
                                BirthDate = member.BirthDate,
                                PassportSeries = member.PassportSeries,
                                PassportNumber = member.PassportNumber,
                                PassportScanPath = ""
                            };
                            context.Visitors.Add(visitor);
                            context.SaveChanges();

                            var groupMember = new GroupMemberEntity
                            {
                                GroupId = groupRequest.GroupId,
                                VisitorId = visitor.VisitorId,
                                RowNumber = row
                            };
                            context.GroupMembers.Add(groupMember);
                            row++;
                        }
                        context.SaveChanges();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
        // Получить сотрудника по коду
        public static Employee GetEmployeeByCode(int employeeCode)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT employee_id, full_name, department_id, section FROM employees WHERE employee_id = @code";
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

        // Получить все заявки с расширенной информацией
        public static List<ExtendedRequestInfo> GetAllExtendedRequests()
        {
            var list = new List<ExtendedRequestInfo>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
            SELECT 
                r.request_id, r.type, r.start_date, r.end_date, r.status, r.rejection_reason,
                d.name as department_name, d.department_id,
                vp.name as purpose_name,
                u.email as user_email,
                r.comment, r.created_at,
                COALESCE(v.last_name || ' ' || v.first_name, 'Групповая заявка') as visitor_name,
                v.passport_series || ' ' || v.passport_number as visitor_passport
            FROM requests r
            LEFT JOIN departments d ON r.department_id = d.department_id
            LEFT JOIN visit_purposes vp ON r.purpose_id = vp.purpose_id
            LEFT JOIN users u ON r.user_id = u.user_id
            LEFT JOIN individual_requests ir ON r.request_id = ir.request_id
            LEFT JOIN visitors v ON ir.visitor_id = v.visitor_id
            ORDER BY r.created_at DESC";

                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ExtendedRequestInfo
                        {
                            RequestId = reader.GetInt32(0),
                            Type = reader.GetString(1),
                            StartDate = DateOnly.FromDateTime(reader.GetDateTime(2)),
                            EndDate = DateOnly.FromDateTime(reader.GetDateTime(3)),
                            Status = reader.GetString(4),
                            RejectionReason = reader.IsDBNull(5) ? null : reader.GetString(5),
                            DepartmentName = reader.GetString(6),
                            DepartmentId = reader.GetInt32(7),
                            PurposeName = reader.GetString(8),
                            UserEmail = reader.GetString(9),
                            Comment = reader.GetString(10),
                            CreatedAt = reader.GetDateTime(11),
                            VisitorFullName = reader.GetString(12),
                            VisitorPassport = reader.IsDBNull(13) ? "" : reader.GetString(13)
                        });
                    }
                }
            }
            return list;
        }

        // Проверка наличия в чёрном списке
        public static bool CheckInBlacklist(string passportNumber)
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

        // Обновление статуса заявки
        public static bool UpdateRequestStatus(int requestId, string status, string rejectionReason)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"UPDATE requests 
                       SET status = @status, rejection_reason = @reason 
                       WHERE request_id = @requestId";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@reason", rejectionReason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@requestId", requestId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}