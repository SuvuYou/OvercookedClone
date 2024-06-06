using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum Scene {
    MainMenu,
    Loading,
    Game
}

public static class SceneLoader
{
    private static Scene TargetScene;

    private static readonly Dictionary<Scene, string> SceneTypeToSceneName = new() {
        [Scene.MainMenu] = "MainMenuScene",
        [Scene.Loading] = "LoadingScene",
        [Scene.Game] = "GameScene"
    };

    public static void LoadScene(Scene scene)
    {
        SceneLoader.TargetScene = scene;

        SceneManager.LoadScene(SceneTypeToSceneName[Scene.Loading]);
    }

    public static void LoadCallback()
    {
        SceneManager.LoadScene(SceneTypeToSceneName[SceneLoader.TargetScene]);
    }
}
