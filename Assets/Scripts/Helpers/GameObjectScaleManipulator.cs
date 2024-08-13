using UnityEngine;

public static class GameObjectScaleManipulator
{
    public static void Hide(GameObject obj)
    {
        obj.transform.localScale = new Vector3(0, 0, 0);
    }

    public static void Show(GameObject obj)
    {
        obj.transform.localScale = new Vector3(1, 1, 1);
    }

}
