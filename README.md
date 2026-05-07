# A Very Basic Unity Agent

This project implements a voice-enabled AI tutor for a Virtual Reality gearbox learning application built in Unity.

Users can speak naturally while interacting with a scene in VR. The Unity application records the user's voice, captures the current screen, and sends both to a Python backend. The backend transcribes the user's speech, sends the question and screenshot to an OpenAI-powered agent, and returns an educational answer. The answer is then converted to speech and played back inside Unity.

## Overview

The system is composed of two main parts:

```text
Unity VR Application
    ├── Records student voice
    ├── Captures current VR view as screenshot
    ├── Sends audio + screenshot to backend
    ├── Receives AI tutor answer
    └── Plays answer aloud using TTS

Python FastAPI Backend
    ├── Receives Unity request
    ├── Performs speech-to-text
    ├── Sends transcript + screenshot to OpenAI Agent
    ├── Returns structured answer
    └── Provides text-to-speech endpoint
```

## Features

* Voice-based student interaction
* Screenshot-based visual context from the VR application
* AI tutor powered by the OpenAI Agents SDK
* Speech-to-text transcription
* Text-to-speech response playback in Unity
* FastAPI backend
* Unity C# client integration
* Designed for mechanical engineering education
* Initial use case: gearbox learning in Virtual Reality

## Educational Context

The AI agent is designed to act as a mechanical engineering tutor for students learning gearbox principles in VR.

The tutor can help explain concepts such as:

* Gear ratio
* Torque and speed trade-off
* Input and output shafts
* Driver and driven gears
* Direction of rotation
* Gear teeth interaction
* Mechanical transmission principles
* Gearbox function and purpose

The VR application was developed in the context of the Erasmus+ XREN project.

## Repository Structure

```text
.
├── backend/
│   ├── app/
│   │   ├── main.py
│   │   ├── agent.py
│   │   ├── stt.py
│   │   ├── tts.py
│   │   └── models.py
│   ├── .env
│   └── requirements.txt
│
├── unity/
│   └── Scripts/
│       ├── VoiceTutorClient.cs
│       └── WavUtility.cs
│
└── README.md
```

The exact structure may vary depending on how the Unity project and backend are organized.

## Requirements

### Backend

* Python 3.10 or later
* OpenAI API key
* FastAPI
* Uvicorn
* OpenAI Python SDK
* OpenAI Agents SDK
* python-dotenv
* python-multipart

### Unity

* Unity 2022 or later recommended
* XR project setup, if using VR
* Input System package, if using Unity's new Input System
* Microphone access
* An `AudioSource` component for playing tutor responses
* An `AudioListener` in the scene, usually on the main XR camera

## Backend Setup

### 1. Create and activate a virtual environment

From the backend directory:

```bash
python -m venv venv
```

On Windows PowerShell:

```powershell
.\venv\Scripts\Activate.ps1
```

On macOS or Linux:

```bash
source venv/bin/activate
```

### 2. Install dependencies

```bash
pip install fastapi uvicorn openai openai-agents python-dotenv python-multipart
```

Alternatively, if a `requirements.txt` file is provided:

```bash
pip install -r requirements.txt
```

### 3. Create a `.env` file

Create a `.env` file in the backend root:

```env
OPENAI_API_KEY=your_openai_api_key_here
```

Do not commit this file to GitHub.

Add it to `.gitignore`:

```gitignore
.env
venv/
__pycache__/
```

### 4. Run the backend

```bash
uvicorn app.main:app --reload --port 8000
```

If the Unity app is running on another device, such as a standalone VR headset, expose the backend on the local network:

```bash
uvicorn app.main:app --host 0.0.0.0 --port 8000
```

## Backend API

### Health Check

```http
GET /health
```

Example response:

```json
{
  "status": "ok"
}
```

### Ask with Audio and Screenshot

```http
POST /ask-audio
```

This endpoint receives:

* Student audio file
* Screenshot from Unity as Base64
* Optional student ID
* Optional session ID

It performs:

1. Speech-to-text transcription
2. AI tutor response generation
3. Structured JSON response

Example response:

```json
{
  "transcript": "What is the function of this gearbox?",
  "answer": "A gearbox transfers rotational motion and changes the relationship between speed and torque...",
  "visible_elements": ["gear", "shaft"],
  "confidence": "medium",
  "suggested_follow_up": "Can you identify which gear is connected to the input shaft?"
}
```

### Text-to-Speech

```http
POST /tts
```

Request body:

```json
{
  "text": "A gearbox transfers rotational motion and changes speed and torque.",
  "voice": "alloy"
}
```

The endpoint returns an audio file, currently configured as MP3.

## Testing TTS Locally

Using PowerShell:

```powershell
Invoke-WebRequest `
  -Uri "http://localhost:8000/tts" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"text":"A gearbox transfers rotational motion and changes speed and torque.","voice":"alloy"}' `
  -OutFile "tutor_answer.mp3"
```

Play the result:

```powershell
Start-Process .\tutor_answer.mp3
```

## Unity Setup

### 1. Add the Unity scripts

Add the following scripts to your Unity project:

```text
Assets/Scripts/VoiceTutorClient.cs
Assets/Scripts/WavUtility.cs
```

### 2. Create a tutor GameObject

In your Unity scene:

1. Create an empty GameObject.
2. Name it `Voice Tutor Client`.
3. Add the `VoiceTutorClient` script.
4. Add an `AudioSource` component.

Recommended `AudioSource` settings for initial testing:

```text
Play On Awake: unchecked
Spatial Blend: 0
Volume: 1
Mute: unchecked
```

For VR spatial audio, `Spatial Blend` can later be set to `1`.

### 3. Add an Audio Listener

Make sure there is exactly one active `AudioListener` in the scene.

Usually this should be attached to:

```text
XR Origin → Camera Offset → Main Camera
```

or the main camera used by the player.

The tutor GameObject should have:

```text
VoiceTutorClient
AudioSource
```

The camera should have:

```text
Camera
AudioListener
```

### 4. Configure backend URLs

In the `VoiceTutorClient` Inspector, configure:

```text
API URL: http://localhost:8000/ask-audio
TTS URL: http://localhost:8000/tts
TTS Voice: alloy
```

If running on a standalone VR headset, replace `localhost` with the IP address of the computer running the backend:

```text
http://192.168.x.x:8000/ask-audio
http://192.168.x.x:8000/tts
```

In that case, the backend must be started with:

```bash
uvicorn app.main:app --host 0.0.0.0 --port 8000
```

### 5. Configure input

The prototype uses push-to-talk.

If your Unity project uses the old input system, you may test with a keyboard key.

If your project uses the new Unity Input System, create an `InputAction` for push-to-talk and bind it to a keyboard key or XR controller button.

Example bindings:

```text
Keyboard: V
XR Controller: Trigger
XR Controller: Grip
```

The student interaction is:

```text
Hold button → speak
Release button → send request to AI tutor
```

## End-to-End Flow

```text
1. Student holds push-to-talk button.
2. Unity starts microphone recording.
3. Student asks a question.
4. Student releases the button.
5. Unity stops recording.
6. Unity converts the recording to WAV.
7. Unity captures the current screen.
8. Unity sends audio + screenshot to /ask-audio.
9. Backend transcribes the audio.
10. Backend sends transcript + screenshot to the OpenAI Agent.
11. Backend returns an answer.
12. Unity sends the answer text to /tts.
13. Backend returns MP3 audio.
14. Unity plays the answer using AudioSource.
```

## Troubleshooting

### Unity says the script must derive from MonoBehaviour

Make sure the class name matches the file name.

For example:

```text
VoiceTutorClient.cs
```

must contain:

```csharp
public class VoiceTutorClient : MonoBehaviour
```

Also ensure there are no compile errors in the Unity Console.

### Unity Input error

If you see:

```text
You are trying to read Input using the UnityEngine.Input class,
but you have switched active Input handling to Input System package
```

Either:

1. Set Unity to use both input systems:

```text
Edit → Project Settings → Player → Active Input Handling → Both
```

or

2. Update the script to use Unity's new Input System.

### Backend returns Permission Denied for temp WAV files

On Windows, temporary files must be closed before being reopened.

Use:

```python
tempfile.NamedTemporaryFile(delete=False, suffix=".wav")
```

Then delete the file manually after processing.

### Agents SDK error: Unknown tool type dict

Do not pass raw dictionaries as tools.

Use SDK tool objects, for example:

```python
from agents import WebSearchTool

tools=[
    WebSearchTool()
]
```

not:

```python
tools=[
    {"type": "web_search"}
]
```

### FMOD error when loading TTS audio

If Unity cannot load the returned audio file, make sure the backend returns MP3 and Unity expects MPEG:

Backend:

```python
response_format="mp3"
```

Unity:

```csharp
new DownloadHandlerAudioClip(ttsUrl, AudioType.MPEG)
```

### No audio listener in scene

Add an `AudioListener` to the main player camera or XR camera.

There should be exactly one active `AudioListener` in the scene.

### Unity Editor works, but headset build does not

Do not use `localhost` on a standalone headset.

Use the local IP address of the backend computer:

```text
http://192.168.x.x:8000
```

Also run the backend with:

```bash
uvicorn app.main:app --host 0.0.0.0 --port 8000
```

Check that the firewall allows incoming connections on port `8000`.

## Security Notes

* Never expose the OpenAI API key in Unity.
* Keep the API key only in the Python backend.
* Do not commit `.env` files to GitHub.
* If deploying publicly, add authentication to the backend.
* Consider rate limiting to avoid unexpected API costs.
* Be careful when storing student audio, screenshots, transcripts, or identifiers.
* For educational deployments, follow applicable privacy and data protection regulations.