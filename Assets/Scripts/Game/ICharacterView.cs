using CheersGame.Data;

namespace CheersGame.Game
{
    public interface ICharacterView
    {
        void Show(NPCData data);
        void Hide();
    }
}
