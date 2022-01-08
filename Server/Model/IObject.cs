using System.Threading.Tasks;

namespace Server.Model
{
    public interface IObject
    {
        Task<bool> UpdateAsync();

        ObjectState ToObjectState();
    }
}