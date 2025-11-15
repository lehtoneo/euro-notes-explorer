namespace EuroNoteExplorer.Shared.Caching
{
	public static class CacheExtensions
	{
		/// <summary>
		/// Retrieves a value from cache or executes the async factory method to produce and store it.
		/// </summary>
		public static async Task<T> GetOrAddAsync<T>(
			this ICache cache,
			string key,
			Func<Task<T>> factory,
			TimeSpan? expiration = null)
		{
			if (cache.Contains(key))
			{
				var cached = cache.Get<T>(key);
				if (cached is not null)
					return cached;
			}

			var value = await factory();
			cache.Set(key, value, expiration);
			return value;
		}
	}

}
