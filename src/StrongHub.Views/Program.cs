namespace StrongHub.Views;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        MessageBox.Show("StrongHub is starting...", "StrongHub");
    }
}
