import tempfile
from fastapi import FastAPI, File, Form, UploadFile, HTTPException, BackgroundTasks
from fastapi.responses import FileResponse
from dotenv import load_dotenv
from pydantic import BaseModel

from app.agent import answer_student_question
from app.models import AskResponse, TTSRequest
from app.stt import transcribe_audio_file
from app.tts import synthesize_speech_to_file
import os

load_dotenv()

app = FastAPI(title="VR Gearbox Tutor API")

api_key = os.getenv("OPENAI_API_KEY")

if not api_key:
    raise RuntimeError("OPENAI_API_KEY is not set")

def cleanup_file(path: str):
    if os.path.exists(path):
        os.remove(path)

@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/ask-audio", response_model=AskResponse)
async def ask_audio(
    audio: UploadFile = File(...),
    screenshot_base64: str = Form(...),
    student_id: str | None = Form(default=None),
    session_id: str | None = Form(default=None),
):
    temp_audio_path = None

    try:
        suffix = ".wav"

        if audio.filename:
            lower_name = audio.filename.lower()
            if lower_name.endswith(".webm"):
                suffix = ".webm"
            elif lower_name.endswith(".mp3"):
                suffix = ".mp3"
            elif lower_name.endswith(".m4a"):
                suffix = ".m4a"
            elif lower_name.endswith(".wav"):
                suffix = ".wav"

        content = await audio.read()

        with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as temp_audio:
            temp_audio.write(content)
            temp_audio_path = temp_audio.name

        question = transcribe_audio_file(temp_audio_path)

        result = await answer_student_question(
            question=question,
            screenshot_base64=screenshot_base64,
        )

        return AskResponse(
            transcript=question,
            answer=result.answer,
            visible_elements=result.visible_elements,
            confidence=result.confidence,
            suggested_follow_up=result.suggested_follow_up,
        )

    except Exception as exc:
        raise HTTPException(status_code=500, detail=str(exc))

    finally:
        if temp_audio_path and os.path.exists(temp_audio_path):
            os.remove(temp_audio_path)
        
@app.post("/tts")
async def tts(request: TTSRequest, background_tasks: BackgroundTasks):
    try:
        with tempfile.NamedTemporaryFile(delete=False, suffix=".mp3") as temp_audio:
            temp_audio_path = temp_audio.name

        synthesize_speech_to_file(
            text=request.text,
            output_path=temp_audio_path,
            voice=request.voice,
        )

        background_tasks.add_task(cleanup_file, temp_audio_path)

        return FileResponse(
            temp_audio_path,
            media_type="audio/mpeg",
            filename="tutor_answer.mp3",
        )

    except Exception as exc:
        raise HTTPException(status_code=500, detail=str(exc))