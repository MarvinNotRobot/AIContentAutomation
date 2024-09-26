using System.Threading.Tasks;

namespace ShopifyAIFunction
{
    public interface IProcessHandler<T1, T2>
    {
        Task<T2> ProcessRequest(T1 input);
    }
}
