using UnityEngine;

public class LoadingSceneCallbackManager : MonoBehaviour
{
    private bool _isFirstUpdate = true;

    private void Update ()
    {
        if (_isFirstUpdate)
        {
            _isFirstUpdate = true;
            SceneLoader.LoadCallback();
        }
    }
}
