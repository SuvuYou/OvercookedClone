using UnityEngine;

public class LobbyCharacterVisual : MonoBehaviour
{
    private const string SHADER_COLOR_STRING = "_Color";
    private const string SHADER_ALPHA_STRING = "_Alpha";

    [SerializeField] private MeshRenderer _bodyMesh;
    [SerializeField] private MeshRenderer _headMesh;

    private Material _customMaterial;

    private void Awake()
    {
        _initCustomMaterial();
    }

    public void AssignColor(Color newColor)
    {
        _customMaterial.SetColor(SHADER_COLOR_STRING, newColor);
    }

    public void AssignColorAlpha(float newAlpha)
    {
        _customMaterial.SetFloat(SHADER_ALPHA_STRING, newAlpha);
    }

    private void _initCustomMaterial()
    {
        _customMaterial = new (_bodyMesh.material);
        _bodyMesh.material = _customMaterial;
        _headMesh.material = _customMaterial;
    }
}
