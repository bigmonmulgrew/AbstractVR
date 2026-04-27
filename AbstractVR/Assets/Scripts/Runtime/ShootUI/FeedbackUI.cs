using System;
using System.Collections;
using System.IO;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class FeedbackUI : MonoBehaviour
{
    public static FeedbackUI Instance;
    const int RECORDING_LENGTH = 60;

    TMP_InputField textInput;
    TextMeshProUGUI placeholder;
    string placeholderText;

    AudioSource feedbackAudioSource;
    AudioClip recordedClip;

    bool isRecording;
    bool isPlaying;

    public event Action OnFeedbackSent;

    TouchScreenKeyboard keyboard;
    bool keyboardOpen;

    Coroutine autoStop;
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        textInput = GetComponentInChildren<TMP_InputField>();
        if (!textInput)
        {
            Debug.LogError("No Text input field found for feedback");
            return;
        }
        placeholder = textInput.placeholder as TextMeshProUGUI;
        if (placeholder != null) placeholderText = placeholder.text;

        feedbackAudioSource = GetComponentInChildren<AudioSource>();
        if (!feedbackAudioSource)
        {
            Debug.Log("No audio source input field found for feedback UI");
            return;
        }
        feedbackAudioSource.ignoreListenerPause = true;

    }
    public void SelectTextInput()
    {
        textInput.Select();

        textInput.ActivateInputField();


    }
    public void OpenKeyboard()
    {
        textInput.Select();
        textInput.ActivateInputField();

#if UNITY_ANDROID && !UNITY_EDITOR
    keyboard = TouchScreenKeyboard.Open(
        textInput.text,
        TouchScreenKeyboardType.Default,
        true,   // autocorrection
        true,   // multiline
        false,  // secure/password
        false,  // alert
        "Enter feedback..."
    );

    keyboardOpen = true;
#endif
    }
    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    if (!keyboardOpen || keyboard == null)
        return;

    textInput.text = keyboard.text;

    if (keyboard.status == TouchScreenKeyboard.Status.Done ||
        keyboard.status == TouchScreenKeyboard.Status.Canceled ||
        keyboard.status == TouchScreenKeyboard.Status.LostFocus)
    {
        keyboardOpen = false;
        keyboard = null;
        textInput.DeactivateInputField();
    }
#endif
    }
    void TryStartRecording()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            Debug.Log("Microphone permission requested");
            return;
        }
#endif

        StartRecording();
    }
    void StartRecording()
    {
        if (isPlaying) StopPlayingAudio();

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone device found");
            return;
        }

        AudioManager.QuietMode(true);
#if UNITY_ANDROID && !UNITY_EDITOR
        recordedClip = Microphone.Start(null, false, RECORDING_LENGTH, AudioSettings.outputSampleRate);
#else
        string deviceName = Microphone.devices[2];
        recordedClip = Microphone.Start(deviceName, false, RECORDING_LENGTH, AudioSettings.outputSampleRate);
#endif


        if (recordedClip == null)
        {
            Debug.LogWarning("Microphone failed to start");
            return;
        }

        if (placeholder) placeholder.text = "Recording...";

        isRecording = true;

        Debug.Log("Start Recording");
        

    }
    void StopRecording()
    {
        if (!isRecording) return;

        if (placeholder) placeholder.text = placeholderText;

#if UNITY_ANDROID && !UNITY_EDITOR
        int position = Microphone.GetPosition(null);
        Microphone.End(null);
#else
        string deviceName = Microphone.devices[2];
        int position = Microphone.GetPosition(deviceName);
        Microphone.End(deviceName);
#endif


        if (position <= 0)
        {
            Debug.LogWarning("Recording too short or no audio captured");
            recordedClip = null;
        }
        else
        {
            recordedClip = TrimClip(recordedClip, position);
        }

        AudioManager.QuietMode(false);
        isRecording = false;

        Debug.Log("Stop recording");
    }
    AudioClip TrimClip(AudioClip source, int endSamplePosition)
    {
        if (source == null || endSamplePosition <= 0) return source;

        int channels = source.channels;
        int frequency = source.frequency;

        float[] data = new float[endSamplePosition * channels];
        source.GetData(data, 0);

        AudioClip trimmed = AudioClip.Create(
            source.name + "_trimmed",
            endSamplePosition,
            channels,
            frequency,
            false
        );

        trimmed.SetData(data, 0);
        return trimmed;
    }
    void PlayAudio()
    {

        if (isRecording) StopRecording();

        if (recordedClip == null)
        {
            Debug.Log("No audio recorded yet");
            return;
        }

        feedbackAudioSource.clip = recordedClip;
        feedbackAudioSource.Play();

        Debug.Log("Playing audio");
        AudioManager.QuietMode(true);
        isPlaying = true;

        if (autoStop != null) StopCoroutine(autoStop);

        autoStop = StartCoroutine(AutoStopPlaying());

    }

    IEnumerator AutoStopPlaying()
    {
        yield return new WaitForSecondsRealtime(recordedClip != null ? recordedClip.length : 0);
        StopPlayingAudio();
    }
    void StopPlayingAudio()
    {
        if (feedbackAudioSource  != null)feedbackAudioSource.Stop();
        AudioManager.QuietMode(false);

        Debug.Log("Stopping audio");
        isPlaying = false;
    }
    #region UI Events
    public void HitRecord()
    {
        if (!isRecording) TryStartRecording();
        else StopRecording();
    }
    public void HitSend()
    {
        Debug.Log("Saving feedback");

        string folderPath = Path.Combine(Application.persistentDataPath, "Feedback");
        Directory.CreateDirectory(folderPath);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        string text = textInput.text;

        if (!string.IsNullOrWhiteSpace(text))
        {
            string textPath = Path.Combine(folderPath, $"feedback_{timestamp}.txt");
            File.WriteAllText(textPath, text);

            Debug.Log($"Feedback text saved to: {textPath}");
        }

        if (recordedClip != null)
        {
            string audioPath = Path.Combine(folderPath, $"feedback_{timestamp}.wav");
            Utils.Wav.Save(audioPath, recordedClip);
            
            Debug.Log($"Feedback audio saved to: {audioPath}");
        }

        Debug.Log($"Feedback folder: {folderPath}");
        OnFeedbackSent?.Invoke();
    }
    public void HitPlay()
    {
        if (isPlaying) StopPlayingAudio();
        else PlayAudio();
            
    }
    #endregion
    
}
