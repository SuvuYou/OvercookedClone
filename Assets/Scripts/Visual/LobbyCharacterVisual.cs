using UnityEngine;

public class LobbyCharacterVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer _bodyMesh;
    [SerializeField] private MeshRenderer _headMesh;

    private Material _customMaterial;

    private void Awake()
    {
        _initCustomMaterial();
    }

    public void AssignColor(Color newColor)
    {
        _customMaterial.color = newColor;
    }

    private void _initCustomMaterial()
    {
        _customMaterial = new (_bodyMesh.material);
        _bodyMesh.material = _customMaterial;
        _headMesh.material = _customMaterial;
    }
}
