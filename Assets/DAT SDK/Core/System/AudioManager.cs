using UnityEngine;
using DAT.Core.DesignPatterns;

namespace DAT.Managers
{
    [AddComponentMenu("DAT SDK/Managers/AudioManager")]
    [DisallowMultipleComponent]
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("SFX Channels")]
        [SerializeField] private AudioSource[] sfxChannels = new AudioSource[8];

        [Header("Music Sources")]
        [SerializeField] private AudioSource musicA;
        [SerializeField] private AudioSource musicB;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        public bool muteMaster;
        public bool muteMusic;
        public bool muteSfx;
        [Tooltip("Lưu volume settings vào PlayerPrefs.")]
        public bool isSaveToPlayerPref = true;

        const string PP_MASTER = "DAT_Audio_Master";
        const string PP_MUSIC = "DAT_Audio_Music";
        const string PP_SFX = "DAT_Audio_Sfx";
        const string PP_MUTE_MASTER = "DAT_Audio_Mute_Master";
        const string PP_MUTE_MUSIC = "DAT_Audio_Mute_Music";
        const string PP_MUTE_SFX = "DAT_Audio_Mute_Sfx";

        protected override void Awake()
        {
            base.Awake();

            if (isSaveToPlayerPref)
            {
                LoadVolumes();
            }

            ApplyAllVolumes();
        }

        void LoadVolumes()
        {
            if (PlayerPrefs.HasKey(PP_MASTER)) masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PP_MASTER));
            if (PlayerPrefs.HasKey(PP_MUSIC)) musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PP_MUSIC));
            if (PlayerPrefs.HasKey(PP_SFX)) sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PP_SFX));
            muteMaster = PlayerPrefs.GetInt(PP_MUTE_MASTER, 0) == 1;
            muteMusic = PlayerPrefs.GetInt(PP_MUTE_MUSIC, 0) == 1;
            muteSfx = PlayerPrefs.GetInt(PP_MUTE_SFX, 0) == 1;
            PlayerPrefs.Save();
        }

        void ApplyAllVolumes()
        {
            float master = muteMaster ? 0f : masterVolume;
            if (musicA != null) musicA.volume = (muteMusic ? 0f : musicVolume) * master;
            if (musicB != null) musicB.volume = (muteMusic ? 0f : musicVolume) * master;
            
            if (sfxChannels != null)
            {
                for (int i = 0; i < sfxChannels.Length; i++)
                {
                    if (sfxChannels[i] != null)
                    {
                        sfxChannels[i].volume = (muteSfx ? 0f : sfxVolume) * master;
                    }
                }
            }
        }

        #region Music API
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            
            AudioSource current = null;
            AudioSource next = null;
            
            if (musicA != null && musicA.isPlaying)
            {
                current = musicA;
                next = musicB;
            }
            else if (musicB != null && musicB.isPlaying)
            {
                current = musicB;
                next = musicA;
            }
            else
            {
                next = musicA != null ? musicA : musicB;
            }
            
            if (current != null) current.Stop();
            if (next == null) return;
            
            next.clip = clip;
            next.loop = loop;
            next.Play();
            ApplyAllVolumes();
        }

        public void StopMusic()
        {
            if (musicA != null) musicA.Stop();
            if (musicB != null) musicB.Stop();
        }

        public void PauseMusic()
        {
            if (musicA != null && musicA.isPlaying) musicA.Pause();
            if (musicB != null && musicB.isPlaying) musicB.Pause();
        }

        public void ResumeMusic()
        {
            if (musicA != null && musicA.clip != null && !musicA.isPlaying) musicA.UnPause();
            if (musicB != null && musicB.clip != null && !musicB.isPlaying) musicB.UnPause();
        }
        #endregion

        #region SFX API
        public AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            if (clip == null || sfxChannels == null || sfxChannels.Length == 0) return null;
            
            int idx = FindFreeSfxChannel();
            if (idx < 0 || sfxChannels[idx] == null) return null;

            var src = sfxChannels[idx];
            src.clip = clip;
            src.loop = loop;
            src.pitch = Mathf.Clamp(pitch, -3f, 3f);

            float master = muteMaster ? 0f : masterVolume;
            float group = (muteSfx ? 0f : sfxVolume) * master;
            src.volume = group * Mathf.Clamp01(volume);

            src.Play();
            return src;
        }

        public void StopSFX(AudioSource src)
        {
            if (src == null) return;
            src.Stop();
        }

        public void StopAllSFX()
        {
            if (sfxChannels == null) return;
            for (int i = 0; i < sfxChannels.Length; i++)
            {
                if (sfxChannels[i] != null) sfxChannels[i].Stop();
            }
        }

        int FindFreeSfxChannel()
        {
            if (sfxChannels == null) return -1;
            
            for (int i = 0; i < sfxChannels.Length; i++)
            {
                if (sfxChannels[i] == null) continue;
                if (!sfxChannels[i].isPlaying)
                {
                    return i;
                }
            }
            return 0;
        }
        #endregion

        #region Volume API
        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            if (isSaveToPlayerPref) PlayerPrefs.SetFloat(PP_MASTER, masterVolume);
            ApplyAllVolumes();
        }
        
        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            if (isSaveToPlayerPref) PlayerPrefs.SetFloat(PP_MUSIC, musicVolume);
            ApplyAllVolumes();
        }
        
        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            if (isSaveToPlayerPref) PlayerPrefs.SetFloat(PP_SFX, sfxVolume);
            ApplyAllVolumes();
        }
        
        public void SetMuteMaster(bool mute)
        {
            muteMaster = mute;
            if (isSaveToPlayerPref) PlayerPrefs.SetInt(PP_MUTE_MASTER, mute ? 1 : 0);
            ApplyAllVolumes();
        }
        
        public void SetMuteMusic(bool mute)
        {
            muteMusic = mute;
            if (isSaveToPlayerPref) PlayerPrefs.SetInt(PP_MUTE_MUSIC, mute ? 1 : 0);
            ApplyAllVolumes();
        }
        
        public void SetMuteSfx(bool mute)
        {
            muteSfx = mute;
            if (isSaveToPlayerPref) PlayerPrefs.SetInt(PP_MUTE_SFX, mute ? 1 : 0);
            ApplyAllVolumes();
        }
        #endregion
    }
}
