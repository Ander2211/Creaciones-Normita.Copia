namespace Client.Services
{
    public class ToastService
    {
        // Evento que notifica al componente Toast que debe mostrarse
        public event Action<string, string, int>? OnShow;

        // Método para disparar un toast
        public Task ShowToast(string message, string type = "info", int duration = 3000)
        {
            OnShow?.Invoke(message, type, duration);
            return Task.CompletedTask;
        }
    }
}
