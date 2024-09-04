using UnityEngine;

public class GridTile : MonoBehaviour
{
    private const string MATERIAL_EMISSION_COLOR_STRING = "_EmissionColor";

    [SerializeField] private bool _isForbiden;
    [SerializeField] private Transform _placeholder;
    [SerializeField] private GameObject _placeholderIndicator;
    
    [SerializeField] private Color _availablePlaceholderColor;
    [SerializeField] private Color _unavailablePlaceholderColor;

    private Material _currentIndicatorMaterial;

    public Vector2Int Coordinats { get; private set; }

    private void Start()
    {
        var mesh = _placeholderIndicator.GetComponent<MeshRenderer>();
        _currentIndicatorMaterial = new (mesh.material);
        mesh.material = _currentIndicatorMaterial;

        _updateIndicatorColor();
        SetIsIndicatorVisible(isVisible: false);
    }

    public void InitCoordinats(Vector2Int coords) => Coordinats = coords;

    public void SetIsIndicatorVisible(bool isVisible) => _placeholderIndicator.SetActive(isVisible);
    
    public Vector3 GetPlacePosition() => _placeholder.position;

    public bool IsAvailable() => TileMapGrid.Instance.IsTileAvailable(Coordinats) && !_isForbiden;

    private void _updateIndicatorColor() => _setMaterialColor(IsAvailable() ? _availablePlaceholderColor : _unavailablePlaceholderColor);

    private void _setMaterialColor(Color color) => _currentIndicatorMaterial.SetColor(MATERIAL_EMISSION_COLOR_STRING, color); 
}    

