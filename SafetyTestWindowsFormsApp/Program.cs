/*
namespace SafetyTestWindowsFormsApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new AdsConnectionForm());
            //Application.Run(new SafetyTestMainForm("10.100.30.103.1.1", 851));
        }
    }
}
*/
namespace SafetyTestWindowsFormsApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Show the AdsConnectionForm first
            using (var adsConnectionForm = new AdsConnectionForm())
            {
                if (adsConnectionForm.ShowDialog() == DialogResult.OK)
                {
                    // Retrieve connection details (e.g., AMS Net ID and Port) from AdsConnectionForm
                    string amsNetId = adsConnectionForm.AmsNetId; // You need to implement this property in AdsConnectionForm
                    int port = adsConnectionForm.Port; // You need to implement this property in AdsConnectionForm

                    // Show SafetyTestMainForm
                    Application.Run(new SafetyTestMainForm(amsNetId, port));
                }
            }
        }
    }
}
