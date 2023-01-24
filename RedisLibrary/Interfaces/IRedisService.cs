using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisLibrary.Interfaces
{
    public interface IRedisService
    {
        void PublishMessage<T>(RedisDataTransfer<T> dataTransfer);
        void PublishMessage<T>(string channel, RedisNestDataTransfer<T> dataTransfer);
        void GetMessage<T>(string channelName, Action<object> action);
        T GetData<T>(string key);
        void InsertData<T>(string key, T data);
        void Expire(string key, int seconds);
        void Delete(string key);
        bool Exists(string key);
    }
}
