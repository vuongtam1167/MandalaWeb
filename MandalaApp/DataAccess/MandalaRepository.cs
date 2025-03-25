using MandalaApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using System.Text;

namespace MandalaApp.DataAccess
{
    public class MandalaRepository
    {
        private readonly string _connectionString;

        public MandalaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<MandalaDetail> GetMandalaDetails()
        {
            var details = new List<MandalaDetail>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Updated SELECT to include MandalaLv and MandalaID
                string sql = "SELECT ID, MandalaLv, MandalaID, Target, Deadline, Status, Action, Result, Person FROM MandalaDetail";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detail = new MandalaDetail
                            {
                                ID = reader.GetInt64(reader.GetOrdinal("ID")),
                                MandalaLv = reader["MandalaLv"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MandalaLv"]),
                                MandalaID = reader["MandalaID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["MandalaID"]),
                                Target = reader["Target"] == DBNull.Value ? null : reader["Target"].ToString(),
                                Deadline = reader["Deadline"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Deadline"]),
                                Status = reader["Status"] == DBNull.Value ? false : Convert.ToBoolean(reader["Status"]),
                                Action = reader["Action"] == DBNull.Value ? null : reader["Action"].ToString(),
                                Result = reader["Result"] == DBNull.Value ? null : reader["Result"].ToString(),
                                Person = reader["Person"] == DBNull.Value ? null : reader["Person"].ToString()
                            };

                            details.Add(detail);
                        }
                    }
                }
            }

            return details;
        }


        public List<Mandala> GetMandalas()
        {
            var madalas = new List<Mandala>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Mandala";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var madala = new Mandala
                            {
                                ID = reader.GetInt64(reader.GetOrdinal("ID")),
                                Name = reader["Name"] == DBNull.Value ? null : reader["Name"].ToString(),
                                CreatedDate = reader["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedDate"]),
                                CreatedUserID = reader.GetInt64(reader.GetOrdinal("CreatedUserID")),
                                ModifiedDate = reader["ModifiedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ModifiedDate"]),
                                ModifiedUserID = reader.GetInt64(reader.GetOrdinal("ModifiedUserID")),
                                Status = reader["Status"] == DBNull.Value ? false : Convert.ToBoolean(reader["Status"])
                            };

                            madalas.Add(madala);
                        }
                    }
                }
            }

            return madalas;
        }

        public List<MandalaHome> GetMandalaHomes(long currentUserId)
        {
            var madalahomes = new List<MandalaHome>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
SELECT M.Name AS MandalaName, 
       M.ModifiedDate AS MandalaModifiedDate, 
       U.Name AS UserName, 
       M.ID AS MandalaID
FROM Mandala M
INNER JOIN [User] U ON M.ModifiedUserID = U.ID
WHERE M.Status = 1
  AND (M.ModifiedUserID = @CurrentUserID
       OR M.ID IN (
           SELECT MS.MandalaID
           FROM MandalaShare MS
           WHERE MS.SharedUserID = @CurrentUserID
             AND M.Status = 1
       ))
";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CurrentUserID", currentUserId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var madalahome = new MandalaHome
                            {
                                NameMandala = reader["MandalaName"] == DBNull.Value ? null : reader["MandalaName"].ToString(),
                                ModifiedDate = reader["MandalaModifiedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["MandalaModifiedDate"]),
                                NameUser = reader["UserName"] == DBNull.Value ? null : reader["UserName"].ToString(),
                                MandalaID = reader.GetInt64(reader.GetOrdinal("MandalaID"))
                            };

                            madalahomes.Add(madalahome);
                        }
                    }
                }
            }

            return madalahomes;
        }

        public List<string> GetMandalaTargetsByIdMandala(long mandalaId)
        {
            var mandalaTargets = new Dictionary<int, string>();

            // Sắp xếp dữ liệu theo sơ đồ (9x9)
            var orderedPositions = new List<int>
    {
        55, 52, 56, 31, 28, 32, 63, 60, 64,
        51, 7, 53, 27, 4, 29, 59, 8, 61,
        54, 50, 57, 30, 26, 33, 62, 58, 65,
        23, 20, 24, 7, 4, 8, 39, 36, 40,
        19, 3, 21, 3, 1, 5, 35, 5, 37,
        22, 18, 25, 6, 2, 9, 38, 34, 41,
        47, 44, 48, 15, 12, 16, 71, 68, 72,
        43, 6, 45, 11, 2, 13, 67, 9, 69,
        46, 42, 49, 14, 10, 17, 70, 66, 73
    };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT MandalaLv, Target FROM Mandala_Target WHERE MandalaID = @mandalaId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@mandalaId", mandalaId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int level = reader.GetInt32(0); // MandalaLv
                            string target = reader.IsDBNull(1) ? "" : reader.GetString(1); // Target

                            mandalaTargets[level] = target;
                        }
                    }
                }
            }

            // Tạo danh sách kết quả theo sơ đồ, nếu không có dữ liệu thì để trống
            var result = orderedPositions.Select(pos => mandalaTargets.ContainsKey(pos) ? mandalaTargets[pos] : "").ToList();

            return result;
        }

        public List<string> GetMandalaTargets3x3(long mandalaId)
        {
            var mandalaTargets3x3 = new Dictionary<int, string>();

            // Thứ tự vị trí của ô 3x3
            var orderedPositions3x3 = new List<int> { 7, 4, 8, 3, 1, 5, 6, 2, 9 };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT MandalaLv, Target FROM Mandala_Target WHERE MandalaID = @mandalaId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@mandalaId", mandalaId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int level = reader.GetInt32(0);
                            string target = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            mandalaTargets3x3[level] = target;
                        }
                    }
                }
            }

            // Sắp xếp lại theo sơ đồ 3x3
            return orderedPositions3x3.Select(pos => mandalaTargets3x3.ContainsKey(pos) ? mandalaTargets3x3[pos] : "").ToList();
        }

        public int GetMandalaClassById(long mandalaId)
        {
            int mandalaClass = 1;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT Class FROM Mandala WHERE ID = @mandalaId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@mandalaId", mandalaId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            mandalaClass = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        }
                    }
                }
            }

            return mandalaClass;
        }
        public void SaveMandalaTargets(long mandalaId, List<string> data, DateTime modifiedDate, long modifiedUserId)
        {
            List<int> orderedPositions;
            if (data.Count == 9)
            {
                // Sơ đồ 3x3: thứ tự lưu tương ứng với ô hiển thị
                orderedPositions = new List<int> { 7, 4, 8, 3, 1, 5, 6, 2, 9 };
            }
            else if (data.Count == 81)
            {
                // Sơ đồ 9x9: thứ tự lưu tương ứng với ô hiển thị
                orderedPositions = new List<int>
        {
            55, 52, 56, 31, 28, 32, 63, 60, 64,
            51, 7, 53, 27, 4, 29, 59, 8, 61,
            54, 50, 57, 30, 26, 33, 62, 58, 65,
            23, 20, 24, 7, 4, 8, 39, 36, 40,
            19, 3, 21, 3, 1, 5, 35, 5, 37,
            22, 18, 25, 6, 2, 9, 38, 34, 41,
            47, 44, 48, 15, 12, 16, 71, 68, 72,
            43, 6, 45, 11, 2, 13, 67, 9, 69,
            46, 42, 49, 14, 10, 17, 70, 66, 73
        };
            }
            else
            {
                throw new ArgumentException("Số lượng ô của grid không hợp lệ!");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Lưu hoặc cập nhật từng target
                for (int i = 0; i < data.Count; i++)
                {
                    int mandalaLv = orderedPositions[i];
                    string targetValue = data[i];

                    string sql = @"
IF EXISTS (SELECT 1 FROM Mandala_Target WHERE MandalaLv = @MandalaLv AND MandalaID = @MandalaID)
    UPDATE Mandala_Target 
    SET Target = @Target 
    WHERE MandalaLv = @MandalaLv AND MandalaID = @MandalaID
ELSE
    INSERT INTO Mandala_Target (MandalaLv, MandalaID, Target) 
    VALUES (@MandalaLv, @MandalaID, @Target)";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@MandalaLv", mandalaLv);
                        command.Parameters.AddWithValue("@MandalaID", mandalaId);
                        command.Parameters.AddWithValue("@Target", string.IsNullOrEmpty(targetValue) ? (object)DBNull.Value : targetValue);
                        command.ExecuteNonQuery();
                    }
                }

                // Sau khi lưu target, cập nhật ModifiedDate và ModifiedUserID của Mandala
                string updateMandalaSql = "UPDATE Mandala SET ModifiedDate = @ModifiedDate, ModifiedUserID = @ModifiedUserID WHERE ID = @MandalaID";
                using (SqlCommand command = new SqlCommand(updateMandalaSql, connection))
                {
                    command.Parameters.AddWithValue("@ModifiedDate", modifiedDate);
                    command.Parameters.AddWithValue("@ModifiedUserID", modifiedUserId);
                    command.Parameters.AddWithValue("@MandalaID", mandalaId);
                    command.ExecuteNonQuery();
                }
            }
        }


        public String GetMandalaNameById(long Id)
        {
            var result = "";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT Name\r\nFROM Mandala\r\nWHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = reader["Name"] == DBNull.Value ? null : reader["Name"].ToString();
                        }
                    }
                }
            }

            return result;
        }

        public void UpdateMandalaName(long mandalaId, string name, DateTime modifiedDate, long modifiedUserId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "UPDATE Mandala SET Name = @Name, ModifiedDate = @ModifiedDate, ModifiedUserID = @ModifiedUserID WHERE ID = @MandalaId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@ModifiedDate", modifiedDate);
                    command.Parameters.AddWithValue("@ModifiedUserID", modifiedUserId);
                    command.Parameters.AddWithValue("@MandalaId", mandalaId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<MandalaDetail> GetMandalaDetailsByMandalaId(long mandalaId)
        {
            var details = new List<MandalaDetail>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Truy vấn dữ liệu với điều kiện lọc dựa trên MandalaLv
                string sql = @"
                SELECT 
                    d.ID, 
                    d.MandalaLv,
                    d.MandalaID,
                    t.Target, 
                    d.Deadline, 
                    d.Status, 
                    d.Action, 
                    d.Result, 
                    d.Person 
                FROM dbo.Mandala_Detail d
                INNER JOIN dbo.Mandala_Target t 
                    ON d.MandalaID = t.MandalaID AND d.MandalaLv = t.MandalaLv
                WHERE d.MandalaID = @mandalaId
                  AND NOT (
                        (d.MandalaLv = 1 AND EXISTS (SELECT 1 FROM dbo.Mandala_Target tt WHERE tt.Target IS NOT NULL AND tt.MandalaID = d.MandalaID AND tt.MandalaLv BETWEEN 2 AND 9))
                    OR  (d.MandalaLv = 2 AND EXISTS (SELECT 1 FROM dbo.Mandala_Target tt WHERE tt.Target IS NOT NULL AND tt.MandalaID = d.MandalaID AND tt.MandalaLv BETWEEN 10 AND 17))
                    OR  (d.MandalaLv = 3 AND EXISTS (SELECT 1 FROM dbo.Mandala_Target tt WHERE tt.Target IS NOT NULL AND tt.MandalaID = d.MandalaID AND tt.MandalaLv BETWEEN 18 AND 25))
                    OR  (d.MandalaLv = 4 AND EXISTS (SELECT 1 FROM dbo.Mandala_Target tt WHERE tt.Target IS NOT NULL AND tt.MandalaID = d.MandalaID AND tt.MandalaLv BETWEEN 26 AND 33))
                    OR  (d.MandalaLv = 5 AND EXISTS (SELECT 1 FROM dbo.Mandala_Target tt WHERE tt.Target IS NOT NULL AND tt.MandalaID = d.MandalaID AND tt.MandalaLv BETWEEN 34 AND 41))
                    OR  (d.MandalaLv = 6 AND EXISTS (SELECT 1 FROM dbo.Mandala_Target tt WHERE tt.Target IS NOT NULL AND tt.MandalaID = d.MandalaID AND tt.MandalaLv BETWEEN 42 AND 49))
                    OR  (d.MandalaLv = 7 AND EXISTS (SELECT 1 FROM dbo.Mandala_Target tt WHERE tt.Target IS NOT NULL AND tt.MandalaID = d.MandalaID AND tt.MandalaLv BETWEEN 50 AND 57))
                    OR  (d.MandalaLv = 8 AND EXISTS (SELECT 1 FROM dbo.Mandala_Target tt WHERE tt.Target IS NOT NULL AND tt.MandalaID = d.MandalaID AND tt.MandalaLv BETWEEN 58 AND 65))
                    OR  (d.MandalaLv = 9 AND EXISTS (SELECT 1 FROM dbo.Mandala_Target tt WHERE tt.Target IS NOT NULL AND tt.MandalaID = d.MandalaID AND tt.MandalaLv BETWEEN 66 AND 73))
                  )";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@mandalaId", mandalaId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detail = new MandalaDetail
                            {
                                ID = reader.GetInt64(reader.GetOrdinal("ID")),
                                MandalaLv = reader["MandalaLv"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MandalaLv"]),
                                MandalaID = reader["MandalaID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["MandalaID"]),
                                Target = reader["Target"] == DBNull.Value ? null : reader["Target"].ToString(),
                                Deadline = reader["Deadline"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Deadline"]),
                                Status = reader["Status"] == DBNull.Value ? false : Convert.ToBoolean(reader["Status"]),
                                Action = reader["Action"] == DBNull.Value ? null : reader["Action"].ToString(),
                                Result = reader["Result"] == DBNull.Value ? null : reader["Result"].ToString(),
                                Person = reader["Person"] == DBNull.Value ? null : reader["Person"].ToString()
                            };
                            details.Add(detail);
                        }
                    }
                }
            }
            return details;
        }


        public List<SelectListItem> GetTargetOptions(long mandalaId)
        {
            // List to hold all distinct (Target, MandalaLv) records
            var records = new List<(int MandalaLv, string Target)>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Get DISTINCT Target and MandalaLv, ordered by MandalaLv
                string sql = @"
            SELECT DISTINCT Target, MandalaLv
            FROM dbo.Mandala_Target
            WHERE MandalaID = @mandalaId
            ORDER BY MandalaLv;
        ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@mandalaId", mandalaId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string target = reader["Target"] == DBNull.Value ? "" : reader["Target"].ToString();
                            int lv = reader["MandalaLv"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MandalaLv"]);

                            if (!string.IsNullOrEmpty(target))
                            {
                                records.Add((lv, target));
                            }
                        }
                    }
                }
            }

            // Apply the filtering rules:
            // Nếu có một trong level 2-9 bỏ level 1
            if (records.Any(r => r.MandalaLv >= 2 && r.MandalaLv <= 9))
            {
                records.RemoveAll(r => r.MandalaLv == 1);
            }
            // Nếu có một trong level 10-17 bỏ level 2
            if (records.Any(r => r.MandalaLv >= 10 && r.MandalaLv <= 17))
            {
                records.RemoveAll(r => r.MandalaLv == 2);
            }
            // Nếu có một trong level 18-25 bỏ level 3
            if (records.Any(r => r.MandalaLv >= 18 && r.MandalaLv <= 25))
            {
                records.RemoveAll(r => r.MandalaLv == 3);
            }
            // Nếu có một trong level 26-33 bỏ level 4
            if (records.Any(r => r.MandalaLv >= 26 && r.MandalaLv <= 33))
            {
                records.RemoveAll(r => r.MandalaLv == 4);
            }
            // Nếu có một trong level 34-41 bỏ level 5
            if (records.Any(r => r.MandalaLv >= 34 && r.MandalaLv <= 41))
            {
                records.RemoveAll(r => r.MandalaLv == 5);
            }
            // Nếu có một trong level 42-49 bỏ level 6
            if (records.Any(r => r.MandalaLv >= 42 && r.MandalaLv <= 49))
            {
                records.RemoveAll(r => r.MandalaLv == 6);
            }
            // Nếu có một trong level 50-57 bỏ level 7
            if (records.Any(r => r.MandalaLv >= 50 && r.MandalaLv <= 57))
            {
                records.RemoveAll(r => r.MandalaLv == 7);
            }
            // Nếu có một trong level 58-65 bỏ level 8
            if (records.Any(r => r.MandalaLv >= 58 && r.MandalaLv <= 65))
            {
                records.RemoveAll(r => r.MandalaLv == 8);
            }
            // Nếu có một trong level 66-73 bỏ level 9
            if (records.Any(r => r.MandalaLv >= 66 && r.MandalaLv <= 73))
            {
                records.RemoveAll(r => r.MandalaLv == 9);
            }

            // Optional: order the remaining records by MandalaLv
            var filteredRecords = records.OrderBy(r => r.MandalaLv).ToList();

            // Build the list of SelectListItem from filtered records.
            var options = new List<SelectListItem>();
            foreach (var record in filteredRecords)
            {
                options.Add(new SelectListItem
                {
                    Value = record.Target,
                    Text = record.Target
                });
            }
            return options;
        }

        public void SaveMandalaDetails(long mandalaId, List<MandalaDetail> details, DateTime modifiedDate, long modifiedUserId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                foreach (var detail in details)
                {
                    string sql = @"
IF EXISTS (SELECT 1 FROM Mandala_Detail WHERE ID = @ID)
    UPDATE Mandala_Detail
    SET MandalaLv = @MandalaLv,
        MandalaID = @MandalaID,
        Deadline = @Deadline,
        Status = @Status,
        Action = @Action,
        Result = @Result,
        Person = @Person
    WHERE ID = @ID
ELSE
    INSERT INTO Mandala_Detail (MandalaID, MandalaLv, Deadline, Status, Action, Result, Person)
    VALUES (@MandalaID, @MandalaLv, @Deadline, @Status, @Action, @Result, @Person)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", detail.ID);
                        command.Parameters.AddWithValue("@MandalaID", mandalaId);
                        command.Parameters.AddWithValue("@MandalaLv", GetMandalaLvByTarget(mandalaId, detail.Target));
                        command.Parameters.AddWithValue("@Deadline", detail.Deadline == DateTime.MinValue ? (object)DBNull.Value : detail.Deadline);
                        command.Parameters.AddWithValue("@Status", detail.Status);
                        command.Parameters.AddWithValue("@Action", string.IsNullOrEmpty(detail.Action) ? (object)DBNull.Value : detail.Action);
                        command.Parameters.AddWithValue("@Result", string.IsNullOrEmpty(detail.Result) ? (object)DBNull.Value : detail.Result);
                        command.Parameters.AddWithValue("@Person", string.IsNullOrEmpty(detail.Person) ? (object)DBNull.Value : detail.Person);

                        command.ExecuteNonQuery();
                    }
                }

                // Sau khi cập nhật chi tiết, cập nhật bảng Mandala
                string updateMandalaSql = "UPDATE Mandala SET ModifiedDate = @ModifiedDate, ModifiedUserID = @ModifiedUserID WHERE ID = @MandalaID";
                using (SqlCommand command = new SqlCommand(updateMandalaSql, connection))
                {
                    command.Parameters.AddWithValue("@ModifiedDate", modifiedDate);
                    command.Parameters.AddWithValue("@ModifiedUserID", modifiedUserId);
                    command.Parameters.AddWithValue("@MandalaID", mandalaId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetMandalaLvByTarget(long mandalaId, string target)
        {
            int mandalaLv = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
            SELECT MandalaLv
            FROM dbo.Mandala_Target
            WHERE MandalaID = @mandalaId AND Target = @target;
        ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@mandalaId", mandalaId);
                    command.Parameters.AddWithValue("@target", target);

                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        mandalaLv = Convert.ToInt32(result);
                    }
                }
            }
            return mandalaLv;
        }

        public void DeleteMandalaDetails(List<long> deletedIds)
        {
            if (deletedIds == null || deletedIds.Count == 0)
                return;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                foreach (var id in deletedIds)
                {
                    string sql = "DELETE FROM Mandala_Detail WHERE ID = @ID";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public User AuthenticateUser(string username, string password)
        {
            User user = null;
            // Mã hóa mật khẩu đầu vào bằng MD5
            string hashedPassword = ComputeMD5Hash(password);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Giả sử bảng [User] lưu mật khẩu ở dạng MD5 hash
                string sql = "SELECT ID, UserName, Name FROM [User] WHERE UserName = @username AND Password = @password";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                ID = reader.GetInt64(reader.GetOrdinal("ID")),
                                UserName = reader["Username"].ToString(),
                                Name = reader["Name"].ToString()
                            };
                        }
                    }
                }
            }
            return user;
        }

        private string ComputeMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                // Chuyển đổi mảng byte thành chuỗi hex
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        public string ComputeMD5HashPass(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return string.Concat(hashBytes.Select(b => b.ToString("x2")));
            }
        }

        public void InsertUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "INSERT INTO [User] (UserName, Password, Name, Email, CreatedDate, Status) " +
                             "VALUES (@UserName, @Password, @Name, @Email, @CreatedDate, @Status)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserName", user.UserName);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@CreatedDate", user.CreatedDate);
                    command.Parameters.AddWithValue("@Status", user.Status);
                    command.ExecuteNonQuery();
                }
            }
        }

        public long InsertMandala(Mandala mandala)
        {
            long newId = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
            INSERT INTO Mandala (Name, Class, CreatedDate, CreatedUserID, ModifiedDate, ModifiedUserID, Status)
            VALUES (@Name, @Class, @CreatedDate, @CreatedUserID, @ModifiedDate, @ModifiedUserID, @Status);
            SELECT CAST(SCOPE_IDENTITY() as bigint);";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", mandala.Name);
                    command.Parameters.AddWithValue("@Class", mandala.Class);
                    command.Parameters.AddWithValue("@CreatedDate", mandala.CreatedDate.HasValue ? (object)mandala.CreatedDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedUserID", mandala.CreatedUserID);
                    command.Parameters.AddWithValue("@ModifiedDate", mandala.ModifiedDate.HasValue ? (object)mandala.ModifiedDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@ModifiedUserID", mandala.ModifiedUserID);
                    command.Parameters.AddWithValue("@Status", mandala.Status);
                    newId = Convert.ToInt64(command.ExecuteScalar());
                }
            }
            return newId;
        }

        public void InsertMandalaTarget(MandalaTarget target)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
            INSERT INTO Mandala_Target (MandalaLv, MandalaID, Target)
            VALUES (@MandalaLv, @MandalaID, @Target)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MandalaLv", target.MandalaLv);
                    command.Parameters.AddWithValue("@MandalaID", target.MandalaID);
                    command.Parameters.AddWithValue("@Target", string.IsNullOrEmpty(target.Target) ? (object)DBNull.Value : target.Target);
                    command.ExecuteNonQuery();
                }
            }
        }


        public bool IsMandalaAccessible(long currentUserId, long mandalaId)
        {
            bool accessible = false;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
            SELECT SUM(cnt)
            FROM (
                SELECT COUNT(*) AS cnt
                FROM Mandala 
                WHERE ID = @MandalaID AND CreatedUserID = @CurrentUserID
                UNION ALL
                SELECT COUNT(*) AS cnt
                FROM MandalaShare 
                WHERE MandalaID = @MandalaID AND SharedUserID = @CurrentUserID
            ) AS T";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MandalaID", mandalaId);
                    command.Parameters.AddWithValue("@CurrentUserID", currentUserId);
                    object result = command.ExecuteScalar();
                    int count = (result == DBNull.Value || result == null) ? 0 : Convert.ToInt32(result);
                    accessible = (count > 0);
                }
            }
            return accessible;
        }


        public User GetUserById(long id)
        {
            User user = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM [User] WHERE ID = @Id";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                ID = reader.GetInt64(reader.GetOrdinal("ID")),
                                UserName = reader["UserName"] == DBNull.Value ? null : reader["UserName"].ToString(),
                                Password = reader["Password"] == DBNull.Value ? null : reader["Password"].ToString(),
                                Name = reader["Name"] == DBNull.Value ? null : reader["Name"].ToString(),
                                Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                                Avatar = reader["Avatar"] == DBNull.Value ? null : reader["Avatar"].ToString(),
                                CreatedDate = reader["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["CreatedDate"]),
                                Status = reader["Status"] == DBNull.Value ? false : Convert.ToBoolean(reader["Status"])
                            };
                        }
                    }
                }
            }
            return user;
        }


        public void UpdateUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
                    UPDATE [User]
                    SET Name = @Name,
                        Email = @Email,
                        Avatar = @Avatar,
                        Status = @Status
                    WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(user.Name) ? (object)DBNull.Value : user.Name);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(user.Email) ? (object)DBNull.Value : user.Email);
                    command.Parameters.AddWithValue("@Avatar", string.IsNullOrEmpty(user.Avatar) ? (object)DBNull.Value : user.Avatar);
                    command.Parameters.AddWithValue("@Status", user.Status);
                    command.Parameters.AddWithValue("@ID", user.ID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateUserPassword(User user)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"
                    UPDATE [User]
                    SET Name = @Name,
                        Email = @Email,
                        Avatar = @Avatar,
                        Status = @Status,
                        Password = @Password
                    WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(user.Name) ? (object)DBNull.Value : user.Name);
                    command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(user.Email) ? (object)DBNull.Value : user.Email);
                    command.Parameters.AddWithValue("@Avatar", string.IsNullOrEmpty(user.Avatar) ? (object)DBNull.Value : user.Avatar);
                    command.Parameters.AddWithValue("@Status", user.Status);
                    // Mã hóa mật khẩu bằng MD5 trước khi lưu
                    command.Parameters.AddWithValue("@Password", string.IsNullOrEmpty(user.Password) ? (object)DBNull.Value : ComputeMD5Hash(user.Password));
                    command.Parameters.AddWithValue("@ID", user.ID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void ShareMandala(long mandalaId, long sharedUserId, string permission, long sharedBy)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Chèn bản ghi vào bảng MandalaShare, bao gồm thông tin người chia sẻ (SharedBy)
                string sql = @"
            INSERT INTO MandalaShare (MandalaID, SharedUserID, Permission, SharedDate, SharedBy)
            VALUES (@MandalaID, @SharedUserID, @Permission, GETDATE(), @SharedBy)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MandalaID", mandalaId);
                    command.Parameters.AddWithValue("@SharedUserID", sharedUserId);
                    command.Parameters.AddWithValue("@Permission", permission);
                    command.Parameters.AddWithValue("@SharedBy", sharedBy);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteMandala(long mandalaId, long currentUserId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Xóa mềm: cập nhật Status = 0, đồng thời cập nhật ModifiedDate và ModifiedUserID
                string sql = @"
            UPDATE Mandala
            SET Status = 0,
                ModifiedDate = GETDATE(),
                ModifiedUserID = @CurrentUserID
            WHERE ID = @MandalaID";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MandalaID", mandalaId);
                    command.Parameters.AddWithValue("@CurrentUserID", currentUserId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<UserSearchResult> SearchUsers(string query, long currentUserId)
        {
            var results = new List<UserSearchResult>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Truy vấn tìm kiếm theo Name, chỉ lấy những người có Status = 1 và không lấy user hiện đang đăng nhập
                string sql = @"
            SELECT ID, Name, Avatar 
            FROM [User]
            WHERE Name LIKE @query
              AND Status = 1
              AND ID <> @currentUserId
            ORDER BY Name ASC";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@query", "%" + query + "%");
                    command.Parameters.AddWithValue("@currentUserId", currentUserId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new UserSearchResult
                            {
                                id = reader.GetInt64(reader.GetOrdinal("ID")),
                                name = reader["Name"] == DBNull.Value ? "" : reader["Name"].ToString(),
                                avatar = reader["Avatar"] == DBNull.Value ? "default-avatar.png" : reader["Avatar"].ToString()
                            });
                        }
                    }
                }
            }

            return results;
        }

        public string GetAvatarPathByUserId(long userId)
        {
            string avatarPath = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT Avatar FROM [User] WHERE ID = @UserId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        avatarPath = result.ToString();
                    }
                }
            }
            return avatarPath;
        }

        public bool IsUserExists(string username)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM [User] WHERE UserName = @UserName";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", username);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public void UpdateMandalaClass(long mandalaId, int newClass, DateTime modifiedDate, long modifiedUserId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "UPDATE Mandala SET Class = @newClass, ModifiedDate = @modifiedDate, ModifiedUserID = @modifiedUserId WHERE ID = @mandalaId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@newClass", newClass);
                    command.Parameters.AddWithValue("@modifiedDate", modifiedDate);
                    command.Parameters.AddWithValue("@modifiedUserId", modifiedUserId);
                    command.Parameters.AddWithValue("@mandalaId", mandalaId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
