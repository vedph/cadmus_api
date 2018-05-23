using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    public interface ICommand
    {
        Task Run();
    }
}
