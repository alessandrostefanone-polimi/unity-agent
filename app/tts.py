from openai import OpenAI

client = OpenAI()

def synthesize_speech_to_file(
    text: str,
    output_path: str,
    voice: str = "alloy",
):
    with client.audio.speech.with_streaming_response.create(
        model="gpt-4o-mini-tts",
        voice=voice,
        input=text,
        response_format="mp3",
    ) as response:
        response.stream_to_file(output_path)