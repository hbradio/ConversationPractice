using System.Diagnostics;

class Tts
{
    public static void SpeakText(string text, int speed=175)
    {
      var process = Process.Start("say", $"-v paulina -r {speed} {text}");
      process.WaitForExit();
    }
}