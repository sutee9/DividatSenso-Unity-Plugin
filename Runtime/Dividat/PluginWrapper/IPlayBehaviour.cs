using Dividat;
using SimpleJSON;

namespace Dividat {
    public interface IPlayBehaviour
    {
        void OnHello(Settings settings, JSONNode memory);
        void OnPing();
        void OnSuspend();
        void OnResume();
    }
}
