namespace Sample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var ipmi = new IPMITools.Ipmi(@"C:\Program Files (x86)\Dell\SysMgt\bmc\ipmitool.exe", "host ip", "user", "password");


            Console.WriteLine(ipmi.PowerStatus());

            ipmi.LoadSensorOutput();

            Console.WriteLine(ipmi.Raw);


        }
    }
}