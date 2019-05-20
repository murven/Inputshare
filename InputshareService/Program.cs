using System.ServiceProcess;

namespace InputshareService
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var service = new IsService())
            {
                ServiceBase.Run(service);
            }
        }
    }

   
}
