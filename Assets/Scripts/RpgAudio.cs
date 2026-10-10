using System.Collections;
using UnityEngine;

public sealed class RpgAudio : MonoBehaviour
{
    private const int SampleRate = 22050;
    private const float TwoPi = Mathf.PI * 2f;
    private const float MusicVolume = 0.32f;
    private const float MusicCrossfadeDuration = 0.85f;

    private static readonly int[] OverworldMelody =
    {
        72, 76, 79, 84, 79, 76, 74, 79, 77, 74, 72, 67, 69, 72, 76, 81,
        79, 76, 74, 71, 72, 76, 79, 84, 81, 79, 76, 72, 74, 71, 67, 71
    };

    private static readonly int[] OverworldBass = { 48, 53, 55, 50, 48, 53, 55, 43 };

    private static readonly int[] BattleMelody =
    {
        76, 76, 79, 83, 84, 83, 79, 76, 74, 74, 77, 81, 83, 81, 77, 74,
        72, 76, 79, 84, 86, 84, 79, 76, 83, 81, 79, 77, 76, 74, 72, 74
    };

    private static readonly int[] BattleBass = { 40, 45, 47, 43, 40, 48, 43, 38 };

    private static RpgAudio instance;

    private AudioSource musicSource;
    private AudioSource alternateMusicSource;
    private AudioSource effectSource;
    private Coroutine musicTransition;
    private AudioClip overworldMusic;
    private AudioClip battleMusic;
    private AudioClip footstep;
    private AudioClip interact;
    private AudioClip attack;
    private AudioClip hit;
    private AudioClip magic;
    private AudioClip healing;
    private AudioClip item;
    private AudioClip encounter;
    private AudioClip flee;
    private AudioClip victory;
    private AudioClip defeat;
    private int currentTrack = -1;

    public static RpgAudio Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RpgAudio>();
                if (instance == null)
                {
                    instance = new GameObject("Echoes of Aether Audio").AddComponent<RpgAudio>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        if (listeners.Length == 0)
        {
            gameObject.AddComponent<AudioListener>();
        }
        else
        {
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].enabled = i == 0;
            }
        }

        musicSource = CreateMusicSource();
        alternateMusicSource = CreateMusicSource();

        effectSource = gameObject.AddComponent<AudioSource>();
        effectSource.playOnAwake = false;
        effectSource.spatialBlend = 0f;
        effectSource.volume = 0.8f;

        overworldMusic = CreateMusic("Greenwick Overworld", OverworldMelody, OverworldBass, 126);
        battleMusic = CreateMusic("Road Battle", BattleMelody, BattleBass, 148);
        footstep = CreateSweep("Soft Footstep", 150f, 85f, 0.07f, true);
        interact = CreateSweep("Interact Chime", 620f, 920f, 0.12f, false);
        attack = CreateSweep("Sword Sweep", 490f, 115f, 0.15f, true);
        hit = CreateSweep("Impact", 170f, 55f, 0.12f, true);
        magic = CreateSweep("Magic Cast", 430f, 1320f, 0.34f, false);
        healing = CreateSweep("Healing Chime", 580f, 1080f, 0.25f, false);
        item = CreateSweep("Item Chime", 760f, 1120f, 0.18f, false);
        encounter = CreateSweep("Encounter Stinger", 180f, 920f, 0.42f, true);
        flee = CreateSweep("Escape Chime", 730f, 360f, 0.24f, false);
        victory = CreateSweep("Victory Stinger", 510f, 1040f, 0.5f, false);
        defeat = CreateSweep("Defeat Stinger", 280f, 70f, 0.52f, false);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        DestroyClip(overworldMusic);
        DestroyClip(battleMusic);
        DestroyClip(footstep);
        DestroyClip(interact);
        DestroyClip(attack);
        DestroyClip(hit);
        DestroyClip(magic);
        DestroyClip(healing);
        DestroyClip(item);
        DestroyClip(encounter);
        DestroyClip(flee);
        DestroyClip(victory);
        DestroyClip(defeat);
    }

    public void PlayOverworldMusic()
    {
        PlayMusic(overworldMusic, 0);
    }

    public void PlayBattleMusic()
    {
        PlayMusic(battleMusic, 1);
    }

    public void PlayFootstep()
    {
        PlayEffect(footstep, 0.2f);
    }

    public void PlayInteract()
    {
        PlayEffect(interact, 0.55f);
    }

    public void PlayAttack()
    {
        PlayEffect(attack, 0.55f);
    }

    public void PlayHit()
    {
        PlayEffect(hit, 0.55f);
    }

    public void PlayMagic()
    {
        PlayEffect(magic, 0.6f);
    }

    public void PlayHealing()
    {
        PlayEffect(healing, 0.55f);
    }

    public void PlayItem()
    {
        PlayEffect(item, 0.55f);
    }

    public void PlayEncounter()
    {
        PlayEffect(encounter, 0.75f);
    }

    public void PlayFlee()
    {
        PlayEffect(flee, 0.65f);
    }

    public void PlayVictory()
    {
        PlayEffect(victory, 0.7f);
    }

    public void PlayDefeat()
    {
        PlayEffect(defeat, 0.7f);
    }

    private void PlayMusic(AudioClip clip, int track)
    {
        if (musicSource == null || alternateMusicSource == null || clip == null)
        {
            return;
        }

        AudioSource incoming = FindPlayingMusicSource(clip);
        AudioSource outgoing;
        if (incoming != null)
        {
            outgoing = incoming == musicSource ? alternateMusicSource : musicSource;
        }
        else if (!musicSource.isPlaying)
        {
            incoming = musicSource;
            outgoing = alternateMusicSource;
        }
        else if (!alternateMusicSource.isPlaying)
        {
            incoming = alternateMusicSource;
            outgoing = musicSource;
        }
        else if (musicSource.volume <= alternateMusicSource.volume)
        {
            incoming = musicSource;
            outgoing = alternateMusicSource;
        }
        else
        {
            incoming = alternateMusicSource;
            outgoing = musicSource;
        }

        if (currentTrack == track && incoming.isPlaying && incoming.clip == clip)
        {
            return;
        }

        if (musicTransition != null)
        {
            StopCoroutine(musicTransition);
            musicTransition = null;
        }

        if (incoming.clip != clip || !incoming.isPlaying)
        {
            incoming.Stop();
            incoming.clip = clip;
            incoming.loop = true;
            incoming.volume = 0f;
            incoming.Play();
        }

        currentTrack = track;
        musicTransition = StartCoroutine(CrossfadeMusic(outgoing, incoming));
    }

    private IEnumerator CrossfadeMusic(AudioSource outgoing, AudioSource incoming)
    {
        float outgoingStartVolume = outgoing.isPlaying ? outgoing.volume : 0f;
        float incomingStartVolume = incoming.volume;
        float elapsed = 0f;

        while (elapsed < MusicCrossfadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / MusicCrossfadeDuration));
            incoming.volume = Mathf.Lerp(incomingStartVolume, MusicVolume, progress);
            outgoing.volume = Mathf.Lerp(outgoingStartVolume, 0f, progress);
            yield return null;
        }

        incoming.volume = MusicVolume;
        outgoing.volume = 0f;
        if (outgoing.isPlaying)
        {
            outgoing.Stop();
        }

        musicTransition = null;
    }

    private AudioSource FindPlayingMusicSource(AudioClip clip)
    {
        if (musicSource.isPlaying && musicSource.clip == clip)
        {
            return musicSource;
        }

        if (alternateMusicSource.isPlaying && alternateMusicSource.clip == clip)
        {
            return alternateMusicSource;
        }

        return null;
    }

    private AudioSource CreateMusicSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
        return source;
    }

    private void PlayEffect(AudioClip clip, float volume)
    {
        if (effectSource != null && clip != null)
        {
            effectSource.PlayOneShot(clip, volume);
        }
    }

    private static AudioClip CreateMusic(string clipName, int[] melody, int[] bassNotes, int tempo)
    {
        float beatDuration = 60f / tempo;
        int samplesPerBeat = Mathf.RoundToInt(beatDuration * SampleRate);
        int sampleCount = samplesPerBeat * melody.Length;
        float[] samples = new float[sampleCount];
        float[] melodyFrequencies = new float[melody.Length];
        float[] bassFrequencies = new float[bassNotes.Length];
        for (int i = 0; i < melody.Length; i++)
        {
            melodyFrequencies[i] = MidiToFrequency(melody[i]);
        }
        for (int i = 0; i < bassNotes.Length; i++)
        {
            bassFrequencies[i] = MidiToFrequency(bassNotes[i]);
        }

        float melodyPhase = 0f;
        float bassPhase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            int beat = i / samplesPerBeat;
            float beatTime = (i % samplesPerBeat) / (float)SampleRate;
            float melodyEnvelope = Mathf.Min(
                Mathf.Clamp01(beatTime / 0.015f),
                Mathf.Clamp01((beatDuration - beatTime) / 0.04f));

            int bassBeatLength = samplesPerBeat * 4;
            int bassNoteIndex = (i / bassBeatLength) % bassNotes.Length;
            float bassTime = (i % bassBeatLength) / (float)SampleRate;
            float bassDuration = beatDuration * 4f;
            float bassEnvelope = Mathf.Min(
                Mathf.Clamp01(bassTime / 0.025f),
                Mathf.Clamp01((bassDuration - bassTime) / 0.05f));

            float melodyFrequency = melodyFrequencies[beat];
            float bassFrequency = bassFrequencies[bassNoteIndex];
            melodyPhase = (melodyPhase + TwoPi * melodyFrequency / SampleRate) % TwoPi;
            bassPhase = (bassPhase + TwoPi * bassFrequency / SampleRate) % TwoPi;

            float pulseWave = Mathf.Sin(melodyPhase) >= 0f ? 1f : -1f;
            float bassWave = Mathf.Sin(bassPhase);
            samples[i] = pulseWave * melodyEnvelope * 0.13f + bassWave * bassEnvelope * 0.17f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateSweep(string clipName, float startFrequency, float endFrequency,
        float duration, bool pulseWave)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
        float[] samples = new float[sampleCount];
        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float progress = i / (float)sampleCount;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
            phase = (phase + TwoPi * frequency / SampleRate) % TwoPi;
            float wave = pulseWave
                ? (Mathf.Sin(phase) >= 0f ? 1f : -1f)
                : Mathf.Sin(phase);
            float attackEnvelope = Mathf.Clamp01(progress * 24f);
            float releaseEnvelope = Mathf.Pow(1f - progress, 1.7f);
            samples[i] = wave * attackEnvelope * releaseEnvelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static float MidiToFrequency(int note)
    {
        return 440f * Mathf.Pow(2f, (note - 69) / 12f);
    }

    private static void DestroyClip(AudioClip clip)
    {
        if (clip != null)
        {
            Destroy(clip);
        }
    }
}
