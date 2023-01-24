using RedisLibrary.Interfaces;

namespace RedisLibrary.DTOs
{
    public class RedisDataTransfer<T>: IRedisDataTransfer<T>
    {
        public string ChannelName { get; set; }
        public T Data { get; set; }
    }
}