using Dividat;

namespace Dividat {
    public interface IPlayBehaviour
    {
        void OnHello(Settings settings);
        void OnPing();
        void OnSuspend();
        void OnResume();
    }
}