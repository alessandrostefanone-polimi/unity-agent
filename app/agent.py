from dotenv import load_dotenv
from agents import Agent, Runner, WebSearchTool
from pydantic import BaseModel

load_dotenv()


class GearboxTutorOutput(BaseModel):
    answer: str
    visible_elements: list[str] = []
    confidence: str
    suggested_follow_up: str | None = None


gearbox_tutor_agent = Agent(
    name="Gearbox VR Tutor",
    model="gpt-4o-mini",
    instructions="""
You are a mechanical engineering professor tutoring a mechanical engineering student inside a Virtual Reality gearbox learning application.

The student may ask questions about what they are seeing in the screenshot.

Your task:
- Answer the student's question using the screenshot and mechanical engineering theory.
- Use web search when current or external information is useful.
- For core gearbox theory, rely primarily on established mechanical engineering principles.
- Do not invent visual details that are not visible in the screenshot.
- If the screenshot is ambiguous, say so.
- Keep the answer concise, educational, and suitable for a student.
- Return structured output.

Additional Information:
- This virtual reality application was developed in the context of the Erasmus+ XREN project.
""",
    tools=[
        WebSearchTool()
    ],
    output_type=GearboxTutorOutput,
)


async def answer_student_question(
    question: str,
    screenshot_base64: str,
) -> GearboxTutorOutput:
    image_data_url = f"data:image/png;base64,{screenshot_base64}"

    result = await Runner.run(
        gearbox_tutor_agent,
        input=[
            {
                "role": "user",
                "content": [
                    {
                        "type": "input_text",
                        "text": question,
                    },
                    {
                        "type": "input_image",
                        "image_url": image_data_url,
                    },
                ],
            }
        ],
    )

    return result.final_output