using UnityEngine;
using Utils;

namespace Highscores
{
    public class HighscoresManager : MonoSingleton<HighscoresManager>
    {
        private string BaseUrl
        {
            get
            {
#if UNITY_EDITOR
                if (!GameManager.Instance.forceProdServer)
                {
                    return GameManager.Instance.testServerBaseUrl;
                }
#endif

                return GameManager.Instance.serverBaseUrl;
            }
        }

        public string PlayerName
        {
            get => PlayerPrefs.GetString("PlayerName", "Player #" + Random.Range(0, 999));
            set => PlayerPrefs.SetString("PlayerName", value);
        }

        public string PlayerMessage
        {
            get => PlayerPrefs.GetString("PlayerMessage", "I am the best");
            set => PlayerPrefs.SetString("PlayerMessage", value);
        }

        public int Highscore
        {
            get => PlayerPrefs.GetInt("Highscore", 0);
            set => PlayerPrefs.SetInt("Highscore", value);
        }

        private Coroutine _sendStartCoroutine;

        private string _playGuid;

        public void AddNewScore(int score)
        {
            if (score > Highscore)
            {
                Highscore = score;
                UploadScore(new ScoreEntry(PlayerName, PlayerMessage, Highscore, _playGuid));
            }
            else
            {
                LoadHighscores();
            }
        }

        [NaughtyAttributes.Button]
        public void LoadHighscores()
        {
            Debug.Log($"Loading highscores..");
            UIController.Instance.LoadingHighscores = true;
            WebUtils.SendGetRequest<ScoreEntries>(
                BaseUrl + "highscores?" + WebUtils.GetQueryArg("player", PlayerName),
                OnHighscoresLoaded);
        }

        private void OnHighscoresLoaded(ScoreEntries scoreEntries)
        {
            Debug.Log($"Highscores loaded");
            UIController.Instance.SetHighscores(scoreEntries);
        }

        private void UploadScore(ScoreEntry scoreEntry)
        {
            Debug.Log($"Uploading score..");
            UIController.Instance.LoadingHighscores = true;
            WebUtils.SendPostRequest<ScoreEntries>(BaseUrl + "add_score", scoreEntry,
                OnHighscoresLoaded);
        }

        public void SendStartGame()
        {
            WebUtils.StopRequestCoroutine(_sendStartCoroutine);
            _sendStartCoroutine = WebUtils.SendGetRequest<PlayGuid>(BaseUrl + "game_started",
                (playGuid) => { _playGuid = playGuid.guid; });
        }
    }
}
