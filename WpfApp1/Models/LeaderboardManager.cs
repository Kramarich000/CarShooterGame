using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Models
{
    public class LeaderboardManager : INotifyPropertyChanged
    {
        private const string FilePath = "leaderboard.json";

        private List<Leaderboard> entries;

        public List<Leaderboard> Entries
        {
            get => entries;
            private set
            {
                entries = value;
                OnPropertyChanged(nameof(Entries));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public LeaderboardManager()
        {
            LoadLeaderboard();
        }

        public void AddEntry(Leaderboard entry)
        {
            Entries.Add(entry);
            Entries.Sort((x, y) => y.Score.CompareTo(x.Score));
            SaveLeaderboard();
        }

        public void ClearLeaderboard()
        {
            Entries.Clear();
            SaveLeaderboard();
            OnPropertyChanged(nameof(Entries));
        }

        private void LoadLeaderboard()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                Entries = JsonConvert.DeserializeObject<List<Leaderboard>>(json) ?? new List<Leaderboard>();
            }
            else
            {
                Entries = new List<Leaderboard>();
            }
        }

        private void SaveLeaderboard()
        {
            var json = JsonConvert.SerializeObject(Entries, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
