using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Events
{
  public interface IEventHandler<in T>
  {
    Task HandleAsync(T @event);
  }
}
