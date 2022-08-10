namespace IPMITools
{
    using System.Diagnostics;
    public class Ipmi
    {
        private int _percentage = 20;
        private bool _fanControl = false;
        private string? _raw;
        private int _temp;
        private int _fanSpeed;
        private int _watts;
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
        public int Percentage { get { return _percentage; } }
        public bool IsControllingFan { get { return _fanControl; } }
        public string? Raw { get { return _raw; } }
        public int Temp { get { return _temp; } }
        public int FanSpeed { get { return _fanSpeed; } }
        public int Watts { get { return _watts; } }

        public void LoadSensorOutput()
        {
            ProcessSdrList(Impitool("sdr list full"));
        }

        public void SetFan(int percentage)
        {
            if (_fanControl)
            {
                _percentage = percentage;
                byte perc = (byte)percentage;
                string percHex = "0x" + perc.ToString("X2");
                Impitool("raw 0X30 0x30 0x02 0xff " + percHex);
            }
        }
        public void ControlFan(bool enable)
        {
            Impitool("raw 0X30 0x30 0x01 " + (enable == false ? "0x01" : "0x00"));
            _fanControl = enable;
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
            p.StartInfo.FileName = this.ProgramPath;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = string.Format("-I lanplus -H {0} -U {1} -P {2} {3}", this.Host, this.User, this.Password, arg);
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            //string error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            return output;
        }

        private void ProcessSdrList(string data)
        {
            _raw = data;
            string[] lines = data.Split('\n');
            string? tempLine = lines.Where(x => x.StartsWith("Ambient Temp")).FirstOrDefault();
            string[] fansLines = lines.Where(x => x.StartsWith("FAN ")).ToArray();
            string? wattsLine = lines.Where(x => x.StartsWith("System Level")).FirstOrDefault();

            _temp = int.Parse(tempLine!.Split('|')[1].Replace("degrees C", "").Trim());
            _fanSpeed = (int)fansLines.Select(x => int.Parse(x.Split('|')[1].Replace("RPM", "").Trim())).Average();
            _watts = int.Parse(wattsLine!.Split('|')[1].Replace("Watts", "").Trim());
        }
    }
}