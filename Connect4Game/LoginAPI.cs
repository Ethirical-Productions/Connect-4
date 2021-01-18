using System;
using MySql.Data.MySqlClient;
using SimpleCrypto;

namespace Connect4Game
{
    internal static class LoginAPI
    {
        private const int HashIterations = 50000;

        public static string GeneratePasswordHash(string pass)
        {
            ICryptoService PBKDF2 = new PBKDF2();
            PBKDF2.HashIterations = HashIterations;

            string salt = PBKDF2.HashIterations + "." + pass; //PBKDF2.GenerateSalt();
            return PBKDF2.Compute(pass, salt);
        }

        public static string DoesTableExist(string sqlConnS, string sqlTableComm)
        {
            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand doesTableExist = new MySqlCommand(sqlTableComm, sqlConn);

            try {
                sqlConn.Open();
            } catch {
                return "/0";
            }

            try {
                MySqlDataReader reader = doesTableExist.ExecuteReader();
                if (reader.HasRows) {
                    sqlConn.Close();
                    return "1";
                } else {
                    sqlConn.Close();
                    return "0";
                }
            } catch {
                sqlConn.Close();
                return "/0";
            }
        }

        public static bool CreateTable(string sqlConnS, string sqlTableCreate)
        {
            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand createTable = new MySqlCommand(sqlTableCreate, sqlConn);

            try {
                sqlConn.Open();
            } catch {
                return false;
            }

            try {
                createTable.ExecuteNonQuery();
                sqlConn.Close();
                return true;
            } catch {
                sqlConn.Close();
                return false;
            }
        }

        public static string Login(string passHash, string username, string sqlConnS, string sqlLoginComm, string userParamString, string pwParamString, string userIDstring) {
            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand searchLogin = new MySqlCommand(sqlLoginComm, sqlConn);
            searchLogin.Parameters.Add(userParamString, MySqlDbType.VarChar).Value = username;
            searchLogin.Parameters.Add(pwParamString, MySqlDbType.VarChar).Value = passHash;
            string sqlPass = "";

            try {
                sqlConn.Open();
            } catch {
                return "/0";
            }

            try {
                MySqlDataReader reader = searchLogin.ExecuteReader();
                while (reader.Read()) {
                    sqlPass = reader[userIDstring].ToString();
                }
                if (sqlPass == "") {
                    sqlConn.Close();
                    return "/9";
                }
            } catch {
                sqlConn.Close();
                return "/0";
            }
            sqlConn.Close();
            return sqlPass;
        }

        public static string Signup(string passHash, string username, string sqlConnS, string sqlSignupCommand, string userParamString, string pwParamString, string dateParamString)
        {
            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand signupComm = new MySqlCommand(sqlSignupCommand, sqlConn);
            signupComm.Parameters.Add(userParamString, MySqlDbType.VarChar).Value = username;
            signupComm.Parameters.Add(pwParamString, MySqlDbType.VarChar).Value = passHash;
            signupComm.Parameters.Add(dateParamString, MySqlDbType.DateTime).Value = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

            try {
                sqlConn.Open();
            } catch {
                return "/0";
            }

            try {
                signupComm.ExecuteNonQuery();
                sqlConn.Close();
                return "1";
            } catch {
                sqlConn.Close();
                return "/9";
            }
        }

        public static string AddIdToAnotherTable(int userId, string sqlConnS, string sqlInsertCommand, string idParamString)
        {
            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand addUserID = new MySqlCommand(sqlInsertCommand, sqlConn);
            addUserID.Parameters.Add(idParamString, MySqlDbType.Int32).Value = userId;

            try {
                sqlConn.Open();
            } catch {
                return "/0";
            }

            try {
                addUserID.ExecuteNonQuery();
                sqlConn.Close();
                return "1";
            } catch {
                sqlConn.Close();
                return "/9";
            }

        }

        public static string DoesUsernameExist(string username, string sqlConnS, string sqlUserCheckComm, string userParamString, string usernameTableString)
        {
            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand checkUsername = new MySqlCommand(sqlUserCheckComm, sqlConn);
            checkUsername.Parameters.Add(userParamString, MySqlDbType.VarChar).Value = username;

            try {
                sqlConn.Open();
            } catch {
                return "/0";
            }

            try {
                string sqlUser = "";
                MySqlDataReader reader = checkUsername.ExecuteReader();
                while (reader.Read()) {
                    sqlUser = reader[usernameTableString].ToString();
                    if (sqlUser != username) {
                        sqlConn.Close();
                        return "0";
                    }
                }
                if (sqlUser == "") {
                    sqlConn.Close();
                    return "0";
                }
            } catch {
                sqlConn.Close();
                return "/0";
            }
            sqlConn.Close();
            return "1";
        }

        public static string IsPasswordCorrect(string pwHash, string sqlConnS, string sqlPassCheckComm, string passParamString, string passTableString)
        {
            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand checkUsername = new MySqlCommand(sqlPassCheckComm, sqlConn);
            checkUsername.Parameters.Add(passParamString, MySqlDbType.VarChar).Value = pwHash;

            try {
                sqlConn.Open();
            } catch {
                return "/0";
            }

            try {
                string sqlPass = "";
                MySqlDataReader reader = checkUsername.ExecuteReader();
                while (reader.Read()) {
                    sqlPass = reader[passTableString].ToString();
                    if (sqlPass != pwHash) {
                        sqlConn.Close();
                        return "/3";
                    }
                }
                if (sqlPass?.Length == 0) {
                    sqlConn.Close();
                    return "/3";
                }
            } catch {
                sqlConn.Close();
                return "/0";
            }
            sqlConn.Close();
            return "1";
        }

        public static string GetUsernameFromUserID(string userID, string sqlConnS, string sqlGetUserComm, string uIDParam, string idTableString)
        {
            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand checkUsername = new MySqlCommand(sqlGetUserComm, sqlConn);
            checkUsername.Parameters.Add(uIDParam, MySqlDbType.VarChar).Value = userID;

            try {
                sqlConn.Open();
            } catch {
                return "/0";
            }

            string username = "";

            try {
                MySqlDataReader reader = checkUsername.ExecuteReader();
                while (reader.Read()) {
                    username = reader[idTableString].ToString();
                }
                if (username?.Length == 0) {
                    sqlConn.Close();
                    return "/1";
                }
            } catch {
                sqlConn.Close();
                return "/0";
            }
            sqlConn.Close();
            return username;
        }

        public static string UpdatePlayerStats(string[] playerStats, string[] updatedValues, string sqlConnS, string sqlUpdateStatsComm, string uID, string oWin, string oLose, string oGamesCount, string oTimesPlayedRed, string oTimesPlayedYellow, string uIdString)
        {
            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand signupComm = new MySqlCommand(sqlUpdateStatsComm, sqlConn);
            signupComm.Parameters.Add(uIdString, MySqlDbType.Int32).Value = uID;
            signupComm.Parameters.Add(oWin, MySqlDbType.Int32).Value = LocalUpdateStats(playerStats[0], updatedValues[0]);
            signupComm.Parameters.Add(oLose, MySqlDbType.Int32).Value = LocalUpdateStats(playerStats[1], updatedValues[1]);
            signupComm.Parameters.Add(oGamesCount, MySqlDbType.Int32).Value = LocalUpdateStats(playerStats[2], updatedValues[2]);
            signupComm.Parameters.Add(oTimesPlayedRed, MySqlDbType.Int32).Value = LocalUpdateStats(playerStats[3], updatedValues[3]);
            signupComm.Parameters.Add(oTimesPlayedYellow, MySqlDbType.Int32).Value = LocalUpdateStats(playerStats[4], updatedValues[4]);

            try {
                sqlConn.Open();
            } catch {
                return "/0";
            }

            try {
                signupComm.ExecuteNonQuery();
                sqlConn.Close();
                return "1";
            } catch {
                sqlConn.Close();
                return "/9";
            }
        }

        private static int LocalUpdateStats(string originalStat, string updateVal)
        {
            int original = int.Parse(originalStat);
            return int.Parse(updateVal) > 0 ? (++original) : int.Parse(originalStat);
        }

        public static string[] GetPlayerStats(string userID, string sqlConnS, string sqlGetUserComm, string uIDParam, string[] rowStrings)
        {
            string[] stats = new string[5];

            MySqlConnection sqlConn = new MySqlConnection(sqlConnS);
            MySqlCommand checkUsername = new MySqlCommand(sqlGetUserComm, sqlConn);
            checkUsername.Parameters.Add(uIDParam, MySqlDbType.VarChar).Value = userID;

            try {
                sqlConn.Open();
            } catch {
                stats[0] = "/0";
                return stats;
            }

            try {
                MySqlDataReader reader = checkUsername.ExecuteReader();
                while (reader.Read()) {
                    for (int i = 0; i < 5; i++) {
                        stats[i] = reader[rowStrings[i]].ToString();
                    }
                }
            } catch {
                sqlConn.Close();
                stats[0] = "/0";
                return stats;
            }
            sqlConn.Close();
            return stats;
        }
    }
}
