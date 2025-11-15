namespace EuroNoteExplorer.Shared.Caching
{
	public interface ICache
	{
		/// <summary>
		/// Retrieves a cached item by key.
		/// Returns default(T) if not found.
		/// </summary>
		T? Get<T>(string key);

		/// <summary>
		/// Adds or updates a cached item.
		/// Optional expiration can be provided.
		/// </summary>
		void Set<T>(string key, T value, TimeSpan? expiration = null);

		/// <summary>
		/// Removes an item from the cache.
		/// </summary>
		void Remove(string key);

		/// <summary>
		/// Checks whether a key exists in the cache.
		/// </summary>
		bool Contains(string key);

		/// <summary>
		/// Clears the entire cache.
		/// </summary>
		void Clear();
	}

}
