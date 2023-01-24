namespace RedisLibrary.Interfaces
{
    public interface IRedisDataTransfer<T>
    {
        string ChannelName { get; set; }
        T Data { get; set; }
    }
}