using Dividat;

namespace Dividat {
    public interface IPlayBehaviour
    {
        void OnHello(Settings settings, string memory);
        void OnPing();
        void OnSuspend();
        void OnResume();
    }
}
