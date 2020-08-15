namespace BrickPi.WebCore
{
    using System.Diagnostics;
    using System.IO;

    public class RunPython
    {
        public string Run(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python";
            start.Arguments = string.Format("\"{0}\" \"{1}\"", cmd, args);
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                    string result = !string.IsNullOrEmpty(stderr) ? stderr : reader.ReadToEnd();
                    return result;
                }
            }
        }
    }
}
