namespace Models
{
    public class Leaderboard
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }

        public Leaderboard(string playerName, int score)
        {
            PlayerName = playerName;
            Score = score;
        }
    }
}
