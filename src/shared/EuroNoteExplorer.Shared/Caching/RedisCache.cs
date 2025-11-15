using System.Text.Json;
using StackExchange.Redis;

namespace EuroNoteExplorer.Shared.Caching
{
	public class RedisCache : ICache
	{
		private readonly IDatabase _db;
		private readonly IServer _server;

		public RedisCache(RedisOptions opts)
		{
			var muxer = ConnectionMultiplexer.Connect(opts.ConnectionString);

			_db = muxer.GetDatabase();
			_server = muxer.GetServer(muxer.GetEndPoints()[0]);
		}

		public T? Get<T>(string key)
		{
			var value = _db.StringGet(key);
			if (value.IsNullOrEmpty)
				return default;

			return JsonSerializer.Deserialize<T>(value!);
		}

		public void Set<T>(string key, T value, TimeSpan? expiration = null)
		{
			var json = JsonSerializer.Serialize(value);
			_db.StringSet(key, json, expiration);
		}

		public void Remove(string key)
		{
			_db.KeyDelete(key);
		}

		public bool Contains(string key)
		{
			return _db.KeyExists(key);
		}

		public void Clear()
		{
			// WARNING: FlushDatabase deletes everything for this Redis DB
			_server.FlushDatabase();
		}
	}
}
