namespace IPMITools
{
    using System.Diagnostics;

    public class Ipmi
    {
        public Ipmi(string programPath, string host, string user, string password)
        {
            ProgramPath = programPath;
            Host = host;
            User = user;
            Password = password;
        }

        public string ProgramPath { get; set; }

        public string Host { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public int Percentage { get; private set; } = 20;

        public bool IsControllingFan { get; private set; } = false;

        public string? Raw { get; private set; }

        public int Temp { get; private set; }

        public int FanSpeed { get; private set; }

        public int Watts { get; private set; }

        public void LoadSensorOutput()
        {
            ProcessSdrList(Impitool("sdr list full"));
        }

        public void SetFan(int percentage)
        {
            if (IsControllingFan)
            {
                Percentage = percentage;
                byte perc = (byte)percentage;
                string percHex = "0x" + perc.ToString("X2");
                Impitool("raw 0X30 0x30 0x02 0xff " + percHex);
            }
        }

        public void ControlFan(bool enable)
        {
            Impitool("raw 0X30 0x30 0x01 " + (enable == false ? "0x01" : "0x00"));
            IsControllingFan = enable;
        }

        public void PowerOn()
        {
            Impitool("chassis power on");
        }

        public string PowerStatus()
        {
            return Impitool("chassis status");
        }

        private string Impitool(string arg)
        {
            Process p = new();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = ProgramPath;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = string.Format("-I lanplus -H {0} -U {1} -P {2} {3}", Host, User, Password, arg);
            p.Start();
            string output = p.StandardOutput.ReadToEnd();

            // string error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            return output;
        }

        private void ProcessSdrList(string data)
        {
            Raw = data;
            string[] lines = data.Split('\n');
            string? tempLine = lines.Where(x => x.StartsWith("Ambient Temp")).FirstOrDefault();
            string[] fansLines = lines.Where(x => x.StartsWith("FAN ")).ToArray();
            string? wattsLine = lines.Where(x => x.StartsWith("System Level")).FirstOrDefault();

            Temp = int.Parse(tempLine!.Split('|')[1].Replace("degrees C", string.Empty).Trim());
            FanSpeed = (int)fansLines.Select(x => int.Parse(x.Split('|')[1].Replace("RPM", string.Empty).Trim())).Average();
            Watts = int.Parse(wattsLine!.Split('|')[1].Replace("Watts", string.Empty).Trim());
        }
    }
}