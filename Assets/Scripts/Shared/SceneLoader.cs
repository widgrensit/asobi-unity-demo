using UnityEngine.SceneManagement;

namespace AsobiDemo
{
    public static class SceneLoader
    {
        public static void LoadLogin() => SceneManager.LoadScene("Login");
        public static void LoadLobby() => SceneManager.LoadScene("Lobby");
        public static void LoadArena() => SceneManager.LoadScene("Arena");
        public static void LoadResults() => SceneManager.LoadScene("Results");
    }
}
