using TMPro;
using UnityEngine;
using UnityEngine.Android;
public class FeedbackUI : MonoBehaviour
{
    const int RECORDING_LENGTH = 60;

    TMP_InputField textInput;
    TextMeshProUGUI placeholder;
    string placeholderText;

    AudioSource feedbackAudioSource;
    AudioClip recordedClip;

    bool isRecording;
    bool isPlaying;
    private void Awake()
    {
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

    }
    public void SelectTextInput()
    {
        textInput.Select();

        textInput.ActivateInputField();
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

        recordedClip = Microphone.Start(null, false, RECORDING_LENGTH, AudioSettings.outputSampleRate);

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
        

        

        int position = Microphone.GetPosition(null);
        Microphone.End(null);

        if (position <= 0)
        {
            Debug.LogWarning("Recording too short or no audio captured");
            recordedClip = null;
        }
        else
        {
            recordedClip = TrimClip(recordedClip, position);
        }

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

        isPlaying = true;
    }
    void StopPlayingAudio()
    {
        if (feedbackAudioSource  != null)feedbackAudioSource.Stop();
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

        string text = textInput.text;

        // Next step: save text/audio locally here.
    }

    public void HitPlay()
    {
        if (isPlaying) StopPlayingAudio();
        else PlayAudio();
            
    }
    #endregion
    
}
