using Cysharp.Threading.Tasks;

namespace TCG_Card_System.Scripts
{
    public interface IAttackDisplay
    {
        public UniTask ShowDamageTaken(int damage);
    }
}