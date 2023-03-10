using Microsoft.Data.Sqlite;

namespace TicTacToe_API
{
    public static class Database
    {
        public static SqliteConnection connection = new SqliteConnection("Data Source = tic-tac-toe.db");
    }
}

