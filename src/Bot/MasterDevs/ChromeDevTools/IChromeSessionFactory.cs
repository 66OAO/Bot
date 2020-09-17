namespace MasterDevs.ChromeDevTools
{
    public interface IChromeSessionFactory
    {
        ChromeSession Create(string endpointUrl,string title);
    }
}