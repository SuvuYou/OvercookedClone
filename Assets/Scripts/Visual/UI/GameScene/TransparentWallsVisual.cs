using System.Collections;
using UnityEngine;

public class TransparentWallsVisual : MonoBehaviour
{
    private const string MATERIAL_ALPHA_VARIABLE_NAME = "_Alpha"; 
    private float TRANPARANT_WALL_MATERIAL_MAX_ALPHA = 1f;
    private float TRANPARANT_WALL_MATERIAL_MIN_ALPHA = 0.4f;

    private float _currentAlpha = 0f;

    [SerializeField] KitchenArea _kitchenArea;
    [SerializeField] MeshRenderer[] _transparentWalls;
    [SerializeField] Material _wallsTransparentMaterial;
    [SerializeField] Material _wallsVisibleMaterial;

    private void Awake()
    {
        _currentAlpha = TRANPARANT_WALL_MATERIAL_MAX_ALPHA;

        _kitchenArea.OnKitchenEnter += _setWallsTransparent;
        _kitchenArea.OnKitchenExit += _setWallsVisible;
    }

    private void OnDestroy()
    {
        _kitchenArea.OnKitchenEnter -= _setWallsTransparent;
        _kitchenArea.OnKitchenExit -= _setWallsVisible;
    }

    private void _switchMaterials(Material newMaterial)
    {
        foreach (MeshRenderer mesh in _transparentWalls)
        {
            mesh.material = newMaterial;
        }
    } 

    private IEnumerator _transparentAnimation()
    {
        while(_currentAlpha > TRANPARANT_WALL_MATERIAL_MIN_ALPHA)
        {
            _currentAlpha -= 0.05f;
            _wallsTransparentMaterial.SetFloat(MATERIAL_ALPHA_VARIABLE_NAME, _currentAlpha);
            yield return new WaitForEndOfFrame();
        }     
    }

    private void _setWallsVisible()
    {
        _switchMaterials(_wallsVisibleMaterial);
        _wallsTransparentMaterial.SetFloat(MATERIAL_ALPHA_VARIABLE_NAME, TRANPARANT_WALL_MATERIAL_MAX_ALPHA);
        _currentAlpha = TRANPARANT_WALL_MATERIAL_MAX_ALPHA;
    }

    private void _setWallsTransparent()
    {
        _switchMaterials(_wallsTransparentMaterial);
        StartCoroutine(_transparentAnimation());
    }
}
