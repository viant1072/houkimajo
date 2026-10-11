using UnityEngine;

namespace Houkimajo.Systems
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Quản lý state tổng của game (menu / đang chơi / pause / thua).
    /// Singleton đơn giản, sống xuyên suốt các scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            bool shouldFreeze = newState == GameState.Paused || newState == GameState.GameOver;
            Time.timeScale = shouldFreeze ? 0f : 1f;
        }
    }
}
