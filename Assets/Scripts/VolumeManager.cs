using System;

public class VolumeManager
{
    public float Volume {get; private set;}
    public event Action<float> OnVolumeChange;

    public void SetVolume(float volume)
    {
        if (volume > 1) volume = 1;
        if (volume < 0) volume = 0;

        Volume = volume;

        OnVolumeChange?.Invoke(volume);
    }
}
