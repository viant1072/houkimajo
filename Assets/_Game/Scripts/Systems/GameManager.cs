using UnityEngine;

namespace Houkimajo.Systems
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Shop,
        Paused,
        GameOver,
        Victory,
        Dialogue
    }

    /// <summary>
    /// Quản lý state tổng của game (menu / đang chơi / shop / pause / thua / thắng màn).
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
            bool shouldFreeze = newState == GameState.Paused || newState == GameState.GameOver
                || newState == GameState.Shop || newState == GameState.Victory || newState == GameState.Dialogue;
            Time.timeScale = shouldFreeze ? 0f : 1f;
        }
    }
}
