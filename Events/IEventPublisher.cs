using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Events
{
  public interface IEventPublisher
  {
    Task PublishAsync<T>(T @event);
  }
}
