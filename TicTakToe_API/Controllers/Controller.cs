using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Text;

namespace TicTacToe_API.Controllers
{
    [Route("/api/[action]")]
    [ApiController]
    public class Controller : ControllerBase
    {
        //GET: /api/session/2
        [HttpGet("{sessionId}")]           //get field and status of session 
        public JsonResult session(int sessionId)
        {
            Database.connection.Open();

            string commandText = $"SELECT field, status FROM sessions WHERE id={sessionId}";
            SqliteCommand command = new SqliteCommand(commandText, Database.connection);
            SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                string? field = reader.GetValue(0).ToString();
                string? status = reader.GetValue(1).ToString();
                return new JsonResult(Ok(new { field = field, status = status }));
            }
            else
            {
                return new JsonResult(NotFound());
            }

        }

        //GET: /api/session/3/player2Id
        [HttpGet("{sessionId}/player2Id")]
        public JsonResult session(uint sessionId)
        {
            Database.connection.Open();

            string commandText = $"SELECT idPlayer2 FROM sessions WHERE id = {sessionId}";
            SqliteCommand command = new SqliteCommand(commandText, Database.connection);
            SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                string? player2Id = reader.GetValue(0).ToString();
                int? i_player2Id = (player2Id == "" || player2Id == null) ? null : Convert.ToInt32(player2Id);

                return new JsonResult(Ok(i_player2Id));
            }
            else
            {
                return new JsonResult(NotFound());
            }
        }

        //GET: /api/player/2/session
        [HttpGet("{playerId}/session")]   //get sessionId, playerNum (first/second) and otherPlayerId (null, if you start new game and second player don't connect)
        public JsonResult player(int playerId)
        {
            Database.connection.Open();

            string commandText = $"SELECT id, idPlayer2 FROM sessions WHERE idPlayer1 = {playerId} ORDER BY id DESC";
            SqliteCommand command = new SqliteCommand(commandText, Database.connection);
            SqliteDataReader reader = command.ExecuteReader();

            string? sessionId = null;
            string? otherPlayerId = null;

            if (reader.Read())
            {
                sessionId = reader.GetValue(0).ToString();
                otherPlayerId = reader.GetValue(1).ToString();
            }
            string? playerNum = "first";

            reader.Close();

            if (sessionId == null)
            {
                commandText = $"SELECT id, idPlayer1 FROM sessions WHERE idPlayer2 = {playerId} ORDER BY id DESC";
                command = new SqliteCommand(commandText, Database.connection);
                reader = command.ExecuteReader();

                if (reader.Read())
                {
                    sessionId = reader.GetValue(0).ToString();
                    otherPlayerId = reader.GetValue(1).ToString();
                }

                reader.Close();

                playerNum = "second";
            }

            if (sessionId == null)
            {
                return new JsonResult(NotFound());
            }

            int? i_sessionId = (sessionId == null || sessionId == "") ? null : Convert.ToInt32(sessionId);
            int? i_otherPlayerId = (otherPlayerId == null || otherPlayerId == "") ? null : Convert.ToInt32(otherPlayerId);

            return new JsonResult(Ok(new { sessionId = i_sessionId, playerNum = playerNum, otherPlayerId = i_otherPlayerId }));
        }

        //POST: /api/player/Stalwitfen
        [HttpPost("{nickname}")]        //create new player
        public JsonResult player(string nickname) {

            Database.connection.Open();
            string commandText = $"INSERT INTO players (nickname) VALUES (\"{nickname}\")";
            SqliteCommand command = new SqliteCommand(commandText, Database.connection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            {
                return new JsonResult(BadRequest());
            }

            commandText = $"SELECT id FROM players WHERE nickname = \"{nickname}\"";
            command = new SqliteCommand(commandText, Database.connection);
            SqliteDataReader reader = command.ExecuteReader();

            int playerId = 0;

            if (reader.Read())
            {
                playerId = Convert.ToInt32(reader.GetValue(0));
            }
            reader.Close();

            return new JsonResult(Ok(new { playerId = playerId }));
        }

        //POST: /api/player/2/session
        [HttpPost("{playerId}/session")]  //create new game (session)
        public JsonResult player(uint playerId)
        {
            Database.connection.Open();

            int? sessionId = null;
            string? player2Id = null;

            string commandText = $"SELECT id, idPlayer2 FROM sessions ORDER BY id DESC LIMIT 1";
            SqliteCommand command = new SqliteCommand(commandText, Database.connection);
            SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                sessionId = Convert.ToInt32(reader.GetValue(0));        //last session
                player2Id = reader.GetValue(1).ToString();
            }
            reader.Close();

            if (player2Id == null || player2Id == "")
            {
                commandText = $"UPDATE sessions SET idPlayer2 = {playerId} WHERE id = {sessionId}";
                command = new SqliteCommand(commandText, Database.connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch
                {
                    return new JsonResult(BadRequest());
                }

                return new JsonResult(Ok(new { sessionId = sessionId, status = "stepPlayer1", playerNum = "second" }));
            }
            else
            {
                commandText = $"INSERT INTO sessions (idPlayer1) VALUES ({playerId})";
                command = new SqliteCommand(commandText, Database.connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch
                {
                    return new JsonResult(BadRequest());
                }
                sessionId++;

                return new JsonResult(Created($"/api/session/{sessionId}", new { field = ".........", status = "stepPlayer1", playerNum = "first" }));
            }

        }

        //PUT: /api/step
        [HttpPut]
        public JsonResult step([FromBody] Step step)
        {
            Database.connection.Open();

            char playerType;

            if (step.PlayerNum == "first")
            {
                playerType = 'X';
            }
            else if (step.PlayerNum == "second")
            {
                playerType = '0';
            }
            else
            {
                return new JsonResult(Conflict(new { errorMessage = "Invalid playerNum"}));
            }

            if (step.CellId < 0 || step.CellId > 8)
            {
                return new JsonResult(Conflict(new { errorMessage = "Invalid cellId" }));
            }

            string commandText = $"SELECT field, status FROM sessions WHERE id = {step.SessionId}";
            SqliteCommand command = new SqliteCommand(commandText, Database.connection);
            SqliteDataReader reader = command.ExecuteReader();

            string field;
            string status;

            try
            {
                reader.Read();
                field = reader.GetValue(0).ToString();
                status = reader.GetValue(1).ToString();
            }
            catch
            {
                return new JsonResult(Conflict(new { errorMessage = "Invalid sessionId" }));
            }
            reader.Close();

            string newStatus = "";

            if (status == "winPlayer1" || status == "winPlayer2" || status == "nobody")
            {
                return new JsonResult(Conflict(new { errorMessage = "The session is already over", sessionStatus = status }));
            }

            else if ((step.PlayerNum == "first" && status == "stepPlayer1") ||
                (step.PlayerNum == "second" && status == "stepPlayer2"))
            {
                if (field[step.CellId] == '.')
                {
                    StringBuilder tmp = new StringBuilder(field);
                    tmp[step.CellId] = playerType;      //X or 0
                    field = tmp.ToString();
                }
                else
                {
                    return new JsonResult(Conflict(new { errorMessage = "Invalid cellId" }));
                }

                for (int i = 0; i < 3; i++)
                {
                    int t = i * 3;
                    if (field[t] != '.' && field[t] == field[t + 1] && field[t] == field[t + 2])    //horizontal
                    {
                        newStatus = "win";
                        break;
                    }
                    if (field[i] != '.' && field[i] == field[i + 3] && field[i] == field[i + 6])    //vertical
                    {
                        newStatus = "win";
                        break;
                    }
                    if (field[0] != '.' && field[0] == field[4] && field[0] == field[8])    //diagonal
                    {
                        newStatus = "win";
                        break;
                    }
                    if (field[2] != '.' && field[2] == field[4] && field[2] == field[6])    //diagonal
                    {
                        newStatus = "win";
                        break;
                    } 
                }

                if (newStatus == "win")
                {
                    newStatus = (playerType == 'X') ? "winPlayer1" : "winPlayer2";
                }

                else if(newStatus != "win")
                {
                    if (field.IndexOf('.') == -1)
                    {
                        newStatus = "nobody";
                    }
                    else if (step.PlayerNum == "first")
                    {
                        newStatus = "stepPlayer2";
                    }
                    else
                    {
                        newStatus = "stepPlayer1";
                    }
                }

            }
            else
            {
                return new JsonResult(Conflict(new {errorMessage = "Another player's move", sessionStatus = status }));
            }


            commandText = $"UPDATE sessions SET field = '{field}', status = '{newStatus}' WHERE id = {step.SessionId}";
            command = new SqliteCommand(commandText, Database.connection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            {
                return new JsonResult(Conflict(new {errorMessage = "UPDATE database error"}));
            }

            return new JsonResult(Ok(new { field = field, status = newStatus }));

        }
    }
}
