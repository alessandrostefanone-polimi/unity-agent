from pydantic import BaseModel, Field

class AskRequest(BaseModel):
    student_id: str | None = None
    session_id: str | None = None
    question: str
    screenshot_base64: str = Field(
        description="Base64-encoded PNG or JPEG screenshot from Unity."
    )

class AskResponse(BaseModel):
    transcript: str | None = None
    answer: str
    visible_elements: list[str] = []
    confidence: str
    suggested_follow_up: str | None = None

class TTSRequest(BaseModel):
    text: str
    voice: str = "alloy"