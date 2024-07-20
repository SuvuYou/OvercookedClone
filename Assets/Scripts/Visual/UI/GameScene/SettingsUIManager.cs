using UnityEngine;
using UnityEngine.UI;

public class SettingsUIManager : MonoBehaviour
{
    [SerializeField] private PauseUIManager _pauseUIManager;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private Button _backButton;
    [SerializeField] private SliderWithTitleUI _musicVolumeSlider;
    [SerializeField] private SliderWithTitleUI _SFXVolumeSlider;

    private void Start()
    {
        _backButton.onClick.AddListener(() => 
        {
            _settingsPanel.SetActive(false);
            _pauseUIManager.DisplayPauseUI();
        });
        _musicVolumeSlider.SyncValue(MusicManager.Volume.Volume);
        _SFXVolumeSlider.SyncValue(SoundManager.Volume.Volume);

        _musicVolumeSlider.OnValueChange += _updateMusicVolume;
        _SFXVolumeSlider.OnValueChange += _updateSFXVolume;

        GameManager.Instance.OnLocalPlayerPause += _handleVisibility;
    }

    private void OnDestroy()
    {
        _backButton.onClick.RemoveAllListeners();
        _musicVolumeSlider.OnValueChange -= _updateMusicVolume;
        _SFXVolumeSlider.OnValueChange -= _updateSFXVolume;

        GameManager.Instance.OnLocalPlayerPause -= _handleVisibility;
    }

    private void _updateMusicVolume (float newValue)
    {
        MusicManager.Volume.SetVolume(newValue);
        PlayerPrefs.SetFloat(MusicManager.MUSIC_VOLUME_PLAYER_PREF, newValue);
        PlayerPrefs.Save();
    }

    private void _updateSFXVolume (float newValue)
    {
        SoundManager.Volume.SetVolume(newValue);
        PlayerPrefs.SetFloat(SoundManager.SFX_SOUND_VOLUME_PLAYER_PREF, newValue);
        PlayerPrefs.Save();
    }

    private void _handleVisibility(bool isPaused)
    {
        if (!isPaused)
        {
            _settingsPanel.SetActive(false);
        }
    }

    public void DisplaySettingsUI()
    {
        _settingsPanel.SetActive(true);
        _musicVolumeSlider.SelectSlider();
    }
}
