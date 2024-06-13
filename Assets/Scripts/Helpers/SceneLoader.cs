using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public enum Scene {
    MainMenu,
    Loading,
    Game,
    LobbyScene,
    PlayerSelectScene
}

public static class SceneLoader
{
    private static Scene TargetScene;

    private static readonly Dictionary<Scene, string> SceneTypeToSceneName = new() {
        [Scene.MainMenu] = "MainMenuScene",
        [Scene.Loading] = "LoadingScene",
        [Scene.Game] = "GameScene",
        [Scene.LobbyScene] = "LobbyScene",
        [Scene.PlayerSelectScene] = "PlayerSelectScene"
    };

    public static void LoadScene(Scene scene)
    {
        SceneLoader.TargetScene = scene;

        SceneManager.LoadScene(SceneTypeToSceneName[Scene.Loading]);
    }

    public static void LoadSceneOnNetwork(Scene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(SceneTypeToSceneName[scene], loadSceneMode: LoadSceneMode.Single);
    }

    public static void LoadCallback()
    {
        SceneManager.LoadScene(SceneTypeToSceneName[SceneLoader.TargetScene]);
    }
}
