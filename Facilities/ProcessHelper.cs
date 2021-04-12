using System.Diagnostics;

namespace WebPageRefresherC19.Facilities
{
    public static class ProcessHelper
    {
        public static void Open(string app, string args)
        {
            using (Process myProcess = new Process())
            {
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = app;
                myProcess.StartInfo.Arguments = args;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
            }
        }

        public static void OpenVsCode(string filePath)
        {
            Open("code", filePath);
        }
    }
}
