using System;
using System.Collections;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class VoiceTutorClient : MonoBehaviour
{
    [SerializeField] private string apiUrl = "http://localhost:8000/ask-audio";
    [SerializeField] private string ttsUrl = "http://localhost:8000/tts";
    [SerializeField] private string ttsVoice = "alloy";
    [SerializeField] private int maxRecordingSeconds = 10;
    [SerializeField] private int sampleRate = 16000;

    private AudioClip recording;
    private string microphoneDevice;
    private bool isRecording = false;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            Debug.Log("Using microphone: " + microphoneDevice);
        }
        else
        {
            Debug.LogError("No microphone found.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartRecording();
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            StopRecordingAndAsk();
        }
    }

    public void StartRecording()
    {
        if (isRecording || string.IsNullOrEmpty(microphoneDevice))
            return;

        recording = Microphone.Start(
            microphoneDevice,
            false,
            maxRecordingSeconds,
            sampleRate
        );

        isRecording = true;
        Debug.Log("Recording started.");
    }

    public void StopRecordingAndAsk()
    {
        if (!isRecording)
            return;

        int position = Microphone.GetPosition(microphoneDevice);
        Microphone.End(microphoneDevice);
        isRecording = false;

        Debug.Log("Recording stopped.");

        byte[] wavBytes = WavUtility.FromAudioClip(recording, position);

        StartCoroutine(CaptureAndSend(wavBytes));
    }

    private IEnumerator CaptureAndSend(byte[] wavBytes)
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(
            Screen.width,
            Screen.height,
            TextureFormat.RGB24,
            false
        );

        screenshot.ReadPixels(
            new Rect(0, 0, Screen.width, Screen.height),
            0,
            0
        );

        screenshot.Apply();

        byte[] pngBytes = screenshot.EncodeToPNG();
        string base64Screenshot = Convert.ToBase64String(pngBytes);

        Destroy(screenshot);

        WWWForm form = new WWWForm();
        form.AddBinaryData("audio", wavBytes, "student_question.wav", "audio/wav");
        form.AddField("screenshot_base64", base64Screenshot);
        form.AddField("student_id", "student_001");
        form.AddField("session_id", "session_001");

        UnityWebRequest request = UnityWebRequest.Post(apiUrl, form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Tutor API error: " + request.error);
            Debug.LogError(request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Tutor response: " + request.downloadHandler.text);

            AskResponse response =
                JsonUtility.FromJson<AskResponse>(request.downloadHandler.text);

            Debug.Log("AI answer: " + response.answer);

            StartCoroutine(SpeakAnswer(response.answer));

            // Display response.answer in your VR UI and reproduce audio.
        }
    }

    private IEnumerator SpeakAnswer(string answerText)
    {
        TTSRequest ttsRequest = new TTSRequest
        {
            text = answerText,
            voice = ttsVoice
        };

        string json = JsonUtility.ToJson(ttsRequest);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(ttsUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerAudioClip(ttsUrl, AudioType.MPEG);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("TTS API error: " + request.error);
            Debug.LogError(request.downloadHandler.text);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = clip;
        audioSource.Play();
    }
}

[Serializable]
public class AskResponse
{
    public string answer;
    public string[] visible_elements;
    public string confidence;
    public string suggested_follow_up;
}

[Serializable]
public class TTSRequest
{
    public string text;
    public string voice;
}