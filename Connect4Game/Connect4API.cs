using System;
using System.Text;
using System.IO;

namespace Connect4Game
{
    static class Connect4API
    {
        private static string _saveLoc =  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static bool _setupComplete;

        public static bool HasPlayerWon(int[,] board, int lastPlayerMove)
        {
            bool winBool = false;

            for (int i = 0; i <= 6; i++) {
                for (int j = 0; j <= 6; j++) {
                    if (board[i, j] == lastPlayerMove) {
                        if (i + 1 <= 6) {
                            if (board[i + 1, j] == lastPlayerMove) {
                                if (i + 2 <= 6) {
                                    if (board[i + 2, j] == lastPlayerMove) {
                                        if (i + 3 <= 6) {
                                            if (board[i + 3, j] == lastPlayerMove) {
                                                winBool = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (board[i, j] == lastPlayerMove) {
                        if (i - 1 >= 0) {
                            if (board[i - 1, j] == lastPlayerMove) {
                                if (i - 2 >= 0) {
                                    if (board[i - 2, j] == lastPlayerMove) {
                                        if (i - 3 >= 0) {
                                            if (board[i - 3, j] == lastPlayerMove) {
                                                winBool = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (board[i, j] == lastPlayerMove) {
                        if (j - 1 >= 0) {
                            if (board[i, j - 1] == lastPlayerMove) {
                                if (j - 2 >= 0) {
                                    if (board[i, j - 2] == lastPlayerMove) {
                                        if (j - 3 >= 0) {
                                            if (board[i, j - 3] == lastPlayerMove) {
                                                winBool = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (board[i, j] == lastPlayerMove) {
                        if (j + 1 <= 6) {
                            if (board[i, j + 1] == lastPlayerMove) {
                                if (j + 2 <= 6) {
                                    if (board[i, j + 2] == lastPlayerMove) {
                                        if (j + 3 <= 6) {
                                            if (board[i, j + 3] == lastPlayerMove) {
                                                winBool = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (board[i, j] == lastPlayerMove) {
                        if (i + 1 <= 6 && j + 1 <= 6) {
                            if (board[i + 1, j + 1] == lastPlayerMove) {
                                if (i + 2 <= 6 && j + 2 <= 6) {
                                    if (board[i + 2, j + 2] == lastPlayerMove) {
                                        if (i + 3 <= 6 && j + 3 <= 6) {
                                            if (board[i + 3, j + 3] == lastPlayerMove) {
                                                winBool = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (board[i, j] == lastPlayerMove) {
                        if (i - 1 >= 0 && j + 1 <= 6) {
                            if (board[i - 1, j + 1] == lastPlayerMove) {
                                if (i - 2 >= 0 && j + 2 <= 6) {
                                    if (board[i - 2, j + 2] == lastPlayerMove) {
                                        if (i - 3 >= 0 && j + 3 <= 6) {
                                            if (board[i - 3, j + 3] == lastPlayerMove) {
                                                winBool = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (board[i, j] == lastPlayerMove) {
                        if (j - 1 >= 0 && i + 1 <= 6) {
                            if (board[i + 1, j - 1] == lastPlayerMove) {
                                if (j - 2 >= 0 && i + 2 <= 6) {
                                    if (board[i + 2, j - 2] == lastPlayerMove) {
                                        if (j - 3 >= 0 && i + 3 <= 6) {
                                            if (board[i + 3, j - 3] == lastPlayerMove) {
                                                winBool = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (board[i, j] == lastPlayerMove) {
                        if (i - 1 >= 0 && j - 1 >= 0) {
                            if (board[i - 1, j - 1] == lastPlayerMove) {
                                if (i - 2 >= 0 && j - 2 >= 0) {
                                    if (board[i - 2, j - 2] == lastPlayerMove) {
                                        if (i - 3 >= 0 && j - 3 >= 0) {
                                            if (board[i - 3, j - 3] == lastPlayerMove) {
                                                winBool = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return winBool;
        }

        public static bool IsDraw(int[,] board)
        {
            bool containsEmpty = false;

            for (int i = 0; i < 7; i++) {
                for (int j = 0; j < 7; j++) {
                    if (board[i, j] == -1) {
                        containsEmpty = true;
                    }
                }
            }

            return !containsEmpty;
        }

        public static void SetNewSaveLocation(string saveLoc)
        {
            _saveLoc = saveLoc;
        }

        public static void LoadSaveLoc(bool overrideBool = false)
        {
            //try {
                if (!_setupComplete) {
                    if (!overrideBool) {
                        if (!Directory.Exists(_saveLoc + "/Ethirical Productions")) {
                            Directory.CreateDirectory(_saveLoc + "/Ethirical Productions");
                            _saveLoc += "/Ethirical Productions";
                        } else { _saveLoc += "/Ethirical Productions"; }

                        if (!Directory.Exists(_saveLoc + "/Connect4")) {
                            Directory.CreateDirectory(_saveLoc + "/Connect4");
                            _saveLoc += "/Connect4";
                        } else { _saveLoc += "/Connect4"; }
                    } else {}

                    if (!File.Exists(_saveLoc + "/LMC4SF")) { File.Create(_saveLoc + "/LMC4SF"); }
                    if (!File.Exists(_saveLoc + "/SPC4SF")) { File.Create(_saveLoc + "/SPC4SF"); }
                    _setupComplete = true;
                }
            //} catch { return false; }
        }

        public static bool SaveBoard(int [,] board, int playerMove, string fileName, bool overrideBool = false)
        {
            File.WriteAllText(_saveLoc + fileName, string.Empty);
            using (Stream stream = new FileStream(_saveLoc + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8)) {
                    writer.WriteLine("A");
                    for (int i = 0; i < 7; i++) {
                        for (int j = 0; j < 7; j++) {
                            writer.Write(board[i, j].ToString() != "-1" ? board[i, j].ToString() : "2");
                        }
                        writer.WriteLine();
                    }
                    writer.Write(playerMove.ToString());
                }
            }
            return true;
        }

        public static bool SaveFileExists(string fileName)
        {
            if (File.Exists(_saveLoc + fileName)) {
                string[] lines = File.ReadAllLines(_saveLoc + fileName);
                if (lines[0] == "A") {
                    bool t = false;
                    foreach (string line in lines) {
                        if (line.Contains("1") || line.Contains("0")) {
                            t = true;
                        }
                    }
                    return t;
                }
            } else { return false; }
            return false;
        }

        public static void SetFileAsWon(int[,] board, int playerMove, string fileName)
        {
            File.WriteAllText(_saveLoc + fileName, string.Empty);
            using (Stream stream = new FileStream(_saveLoc + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8)) {
                    writer.WriteLine("B");
                    for (int i = 0; i < 7; i++) {
                        for (int j = 0; j < 7; j++) {
                            if (board[i, j].ToString() != "-1") {
                                writer.Write(board[i, j].ToString());
                            } else {
                                writer.Write("2");
                            }
                        }
                        writer.WriteLine();
                    }
                    writer.Write(playerMove.ToString());
                }
            }
        }

        public static Tuple<int[,], int> LoadBoard(string fileName)
        {
            int[,] board = new int[7,7];
            int playerMove = -1;

            string[] lines = File.ReadAllLines(_saveLoc + fileName);

            int linecount = 0;
            foreach (string line in lines) {
                if (line.Length != 1) {
                    for (int i = 0; i < 7; i++) {
                        if (line[i].ToString() == "2") {
                            board[linecount, i] = -1;
                        } else { board[linecount, i] = (int)char.GetNumericValue(line[i]); }
                    }
                    linecount++;
                } else if ((line.Contains("1") || line.Contains("0")) && line.Length == 1) {
                    playerMove = (int)char.GetNumericValue(line[0]);
                }
            }

            return new Tuple<int[,], int>(board, playerMove);
        }
    }
}
