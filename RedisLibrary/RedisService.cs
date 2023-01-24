using RedisLibrary.Interfaces;
using Serilog;
using ServiceStack.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack;
using Newtonsoft.Json;
#nullable enable
namespace RedisLibrary
{
    public class RedisDataTransfer<T>
    {
        public string ChannelName { get; set; }
        public T Data { get; set; }
    }

    public class RedisNestDataTransfer<T>
    {
        public T MatchId { get; set; }
    }
    public class RedisService : IRedisService
    {

        private readonly ILogger _logger = Log.ForContext<RedisService>();
        private readonly string Host;
        private readonly int Port;
        private readonly string? Password;
        public RedisService(string host, int port, string? password = null)
        {
            Port = port;
            Host = host;
            Password = password;
        }


        public void PublishMessage<T>(RedisDataTransfer<T> dataTransfer)
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                using (var redisPublisher = new RedisClient(Host, Port, Password))
                {
                    redisPublisher.PublishMessage(dataTransfer.ChannelName, JsonConvert.SerializeObject(dataTransfer.Data));
                    _logger.Information("Send '{0}' to '{1}'", JsonConvert.SerializeObject(dataTransfer.Data), dataTransfer.ChannelName);
                }
            });
        }
        public void PublishMessage<T>(string channelName, RedisNestDataTransfer<T> dataTransfer)
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                using (var redisPublisher = new RedisClient(Host, Port, Password))
                {
                    redisPublisher.PublishMessage(channelName, JsonConvert.SerializeObject(dataTransfer));
                    _logger.Information("Send '{0}' to '{1}'", JsonConvert.SerializeObject(dataTransfer), channelName);
                }
            });
        }
        public void GetMessage<T>(string channelName, Action<object> action)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(x =>
                {
                    using (var redisConsumer = new RedisClient(Host, Port, Password))
                    {
                        using (var subscription = redisConsumer.CreateSubscription())
                        {
                            subscription.OnSubscribe = channel =>
                            {
                                _logger.Information("Subscribed to '{0}'", channel);
                            };
                            subscription.OnUnSubscribe = channel =>
                            {
                                _logger.Information("UnSubscribed from '{0}'", channel);
                            };
                            subscription.OnMessage = (channel, msg) =>
                            {
                                _logger.Information("OnMessage from '{0}'", channel);
                                Task.Factory.StartNew(action, JsonConvert.DeserializeObject<T>(msg));

                            };

                            subscription.SubscribeToChannels(channelName);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                _logger.Error($"Error : {e.Message}");
                throw e;
            }
        }

        public T GetData<T>(string key)
        {
            using (var redisPublisher = new RedisClient(Host, Port, Password))
            {
                return redisPublisher.Get<T>(key);
            }
        }

        public void InsertData<T>(string key, T data)
        {
            using (var redisClient = new RedisClient(Host, Port, Password))
            {
                redisClient.Set<T>(key, data);
            }
        }

        public void Expire(string key, int seconds)
        {
            using (var redisClient = new RedisClient(Host, Port, Password))
            {
                redisClient.ExpireEntryIn(key, TimeSpan.FromSeconds(seconds));
            }
        }

        public void Delete(string key)
        {
            using (var redisCient = new RedisClient(Host, Port, Password))
            {
                redisCient.Remove(key);
            }
        }

        public bool Exists(string key)
        {
            using var redisClient = new RedisClient(Host, Port, Password);
            return redisClient.Exists(key) == 1;
        }

        public void Incr(string key, uint amount)
        {
            using var redisClient = new RedisClient(Host, Port, Password);
            redisClient.Increment(key, amount);
        }

        public void Decr(string key, uint amount)
        {
            using var redisClient = new RedisClient(Host, Port, Password);
            redisClient.Decrement(key, amount);
        }
    }
}
