using System.Collections.Concurrent;

namespace EuroNoteExplorer.Shared.Caching
{
	public class InMemoryCache : ICache
	{
		private class CacheItem
		{
			public object? Value { get; set; }
			public DateTime? Expiration { get; set; }
		}

		private readonly ConcurrentDictionary<string, CacheItem> _cache
			= new ConcurrentDictionary<string, CacheItem>();

		public T? Get<T>(string key)
		{
			if (!_cache.TryGetValue(key, out var entry))
				return default;

			// Check expiration
			if (entry.Expiration.HasValue && entry.Expiration.Value < DateTime.UtcNow)
			{
				_cache.TryRemove(key, out _);
				return default;
			}

			return entry.Value is T t ? t : default;
		}

		public void Set<T>(string key, T value, TimeSpan? expiration = null)
		{
			var expiresAt = expiration.HasValue
				? DateTime.UtcNow.Add(expiration.Value)
				: (DateTime?)null;

			var item = new CacheItem
			{
				Value = value,
				Expiration = expiresAt
			};

			_cache[key] = item;
		}

		public void Remove(string key)
		{
			_cache.TryRemove(key, out _);
		}

		public bool Contains(string key)
		{
			if (!_cache.TryGetValue(key, out var entry))
				return false;

			if (entry.Expiration.HasValue && entry.Expiration.Value < DateTime.UtcNow)
			{
				_cache.TryRemove(key, out _);
				return false;
			}

			return true;
		}

		public void Clear()
		{
			_cache.Clear();
		}
	}
}
