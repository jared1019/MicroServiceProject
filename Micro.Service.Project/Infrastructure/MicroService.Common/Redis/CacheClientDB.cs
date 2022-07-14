using Microsoft.Extensions.Options;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;
using System.Collections;
using System.Diagnostics;

namespace MicroService.Common
{
    public class CacheClientDB : IDisposable
    {
        private readonly RedisOptions _redisOptions;
        public CacheClientDB(IOptionsMonitor<RedisOptions> redisOptions)
        {
            _redisOptions = redisOptions.CurrentValue;
            client = new RedisClient(_redisOptions.Host, _redisOptions.Prot, _redisOptions.Password, _redisOptions.DB);
        }
        // 管道模式 三种模式
        public IRedisClient GetClient()
        {
            return client;
        }
        private IRedisClient client;

        public void Dispose()
        {

            TryCatchException(delegate
            {
                client.Dispose();
            }, string.Empty);
        }
        // 为了以后全链路做准备
        private void TryCatchException(Action action, string key)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {


            }
        }

        private T TryCatch<T>(Func<T> action, string key)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Exception ex = null;
            bool isError = false;
            T result;
            try
            {
                result = action();
            }
            catch (Exception exinfo)
            {
                isError = true;
                throw exinfo;
                ex = exinfo;
            }
            finally
            {

                sw.Stop();

            }
            return result;
        }

        private void TryCatch(Action action, string key)
        {

            Stopwatch sw = Stopwatch.StartNew();
            bool isError = false;
            Exception ex = null;
            try
            {
                action();
            }
            catch (Exception exinfo)
            {
                isError = true;
                throw exinfo;
            }
            finally
            {
                sw.Stop();

            }
        }
        public bool Add<T>(string key, T value)
        {

            return TryCatch<bool>(() => client.Add<T>(key, value), key);
        }
        /// <summary>
        ///  简单模式  事务模式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiresAt"></param>
        /// <returns></returns>
        public bool Add<T>(string key, T value, DateTime expiresAt)
        {

            return TryCatch<bool>(() => client.Add<T>(key, value, expiresAt), key);
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            return TryCatch<bool>(() => client.Add<T>(key, value, expiresIn), key);
        }

        public long Decrement(string key, uint amount)
        {
            return TryCatch<long>(() => client.Decrement(key, amount), key);
        }

        public void FlushAll()
        {
            TryCatch(delegate
            {
                client.FlushAll();
            }, string.Empty);
        }

        public T Get<T>(string key)
        {
            return TryCatch<T>(() => client.Get<T>(key), key);
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            return TryCatch<IDictionary<string, T>>(() => client.GetAll<T>(keys), keys.FirstOrDefault());
        }

        public long Increment(string key, uint amount)
        {
            return TryCatch<long>(() => client.Increment(key, amount), key);
        }

        public bool Remove(string key)
        {
            return TryCatch(() => client.Remove(key), key);
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            TryCatch(delegate
            {
                client.RemoveAll(keys);
            }, keys.FirstOrDefault());
        }

        public bool Replace<T>(string key, T value)
        {
            return TryCatch<bool>(() => client.Replace<T>(key, value), key);
        }

        public bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            return TryCatch<bool>(() => client.Replace<T>(key, value, expiresAt), key);
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            return TryCatch<bool>(() => client.Replace<T>(key, value, expiresIn), key);
        }

        public bool Set<T>(string key, T value)
        {
            return TryCatch<bool>(() => client.Set<T>(key, value), key);
        }

        public bool Set<T>(string key, T value, DateTime expiresAt)
        {
            return TryCatch<bool>(() => client.Set<T>(key, value, expiresAt), key);
        }

        public bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            return TryCatch<bool>(() => client.Set<T>(key, value, expiresIn), key);
        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            TryCatch(delegate
            {
                client.SetAll<T>(values);
            }, values.Keys.FirstOrDefault());
        }


        public void Delete<T>(T entity) where T : class, new()
        {
            TryCatch(delegate
            {
                client.Delete<T>(entity);
            }, string.Empty);
        }

        public void DeleteAll<TEntity>() where TEntity : class, new()
        {
            TryCatch(delegate
            {
                client.DeleteAll<TEntity>();
            }, string.Empty);
        }

        public void DeleteById<T>(object id) where T : class, new()
        {
            TryCatch(delegate
            {
                client.DeleteById<T>(id);
            }, string.Empty);
        }

        public void DeleteByIds<T>(ICollection ids) where T : class, new()
        {
            TryCatch(delegate
            {
                client.DeleteById<T>(ids);
            }, string.Empty);
        }

        public T GetById<T>(object id) where T : class, new()
        {
            return TryCatch<T>(() => client.GetById<T>(id), string.Empty);
        }

        public IList<T> GetByIds<T>(ICollection ids) where T : class, new()
        {
            return TryCatch<IList<T>>(() => client.GetByIds<T>(ids), string.Empty);
        }

        public T Store<T>(T entity) where T : class, new()
        {
            return TryCatch<T>(() => client.Store<T>(entity), string.Empty);
        }

        public void StoreAll<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, new()
        {
            TryCatch(delegate
            {
                client.StoreAll<TEntity>(entities);
            }, string.Empty);
        }

        public void AddItemToList(string listId, string value)
        {
            TryCatch(delegate
            {
                client.AddItemToList(listId, value);
            }, listId);
        }

        public void AddItemToSet(string setId, string item)
        {
            TryCatch(delegate
            {
                client.AddItemToSet(setId, item);
            }, setId);
        }

        public bool AddItemToSortedSet(string setId, string value)
        {
            return TryCatch<bool>(() => client.AddItemToSortedSet(setId, value), setId);
        }

        public bool AddItemToSortedSet(string setId, string value, double score)
        {
            return TryCatch<bool>(() => client.AddItemToSortedSet(setId, value, score), setId);
        }

        public void AddRangeToList(string listId, List<string> values)
        {
            TryCatch(delegate
            {
                client.AddRangeToList(listId, values);
            }, listId);
        }

        public void AddRangeToSet(string setId, List<string> items)
        {
            TryCatch(delegate
            {
                client.AddRangeToSet(setId, items);
            }, setId);
        }

        public bool AddRangeToSortedSet(string setId, List<string> values, double score)
        {
            return TryCatch<bool>(() => client.AddRangeToSortedSet(setId, values, score), setId);
        }

        public bool AddRangeToSortedSet(string setId, List<string> values, long score)
        {
            return TryCatch<bool>(() => client.AddRangeToSortedSet(setId, values, score), setId);
        }

        public long AppendToValue(string key, string value)
        {
            return TryCatch<long>(() => client.AppendToValue(key, value), key);
        }

        public string BlockingDequeueItemFromList(string listId, TimeSpan? timeOut)
        {
            return TryCatch<string>(() => client.BlockingDequeueItemFromList(listId, timeOut), listId);
        }

        public KeyValuePair<string, string> BlockingDequeueItemFromLists(string[] listIds, TimeSpan? timeOut)
        {
            return this.TryCatch<KeyValuePair<string, string>>(delegate
            {
                ItemRef item = client.BlockingDequeueItemFromLists(listIds, timeOut);
                return new KeyValuePair<string, string>(item.Id, item.Item);
            }, listIds[0]);
        }

        public string BlockingPopAndPushItemBetweenLists(string fromListId, string toListId, TimeSpan? timeOut)
        {
            return TryCatch<string>(() => client.BlockingPopAndPushItemBetweenLists(fromListId, toListId, timeOut), fromListId);
        }

        public string BlockingPopItemFromList(string listId, TimeSpan? timeOut)
        {
            return TryCatch<string>(() => client.BlockingPopItemFromList(listId, timeOut), listId);
        }

        public KeyValuePair<string, string> BlockingPopItemFromLists(string[] listIds, TimeSpan? timeOut)
        {
            return this.TryCatch<KeyValuePair<string, string>>(delegate
            {
                ItemRef item = client.BlockingPopItemFromLists(listIds, timeOut);
                return new KeyValuePair<string, string>(item.Id, item.Item);
            }, listIds[0]);
        }

        public string BlockingRemoveStartFromList(string listId, TimeSpan? timeOut)
        {
            return TryCatch<string>(() => client.BlockingRemoveStartFromList(listId, timeOut), listId);
        }

        public KeyValuePair<string, string> BlockingRemoveStartFromLists(string[] listIds, TimeSpan? timeOut)
        {
            return this.TryCatch<KeyValuePair<string, string>>(delegate
            {
                ItemRef item = client.BlockingRemoveStartFromLists(listIds, timeOut);
                return new KeyValuePair<string, string>(item.Id, item.Item);
            }, listIds[0]);
        }

        public bool ContainsKey(string key)
        {
            return TryCatch<bool>(() => client.ContainsKey(key), key);
        }

        public long DecrementValue(string key)
        {
            return TryCatch<long>(() => client.DecrementValue(key), key);
        }

        public long DecrementValueBy(string key, int count)
        {
            return TryCatch<long>(() => client.DecrementValueBy(key, count), key);
        }

        public string DequeueItemFromList(string listId)
        {
            return TryCatch<string>(() => client.DequeueItemFromList(listId), listId);
        }

        public void EnqueueItemOnList(string listId, string value)
        {
            TryCatch(delegate
            {
                client.EnqueueItemOnList(listId, value);
            }, listId);
        }

        public bool ExpireEntryAt(string key, DateTime expireAt)
        {
            return TryCatch<bool>(() => client.ExpireEntryAt(key, expireAt), key);
        }

        public bool ExpireEntryIn(string key, TimeSpan expireIn)
        {
            return TryCatch<bool>(() => client.ExpireEntryIn(key, expireIn), key);
        }

        public Dictionary<string, string> GetAllEntriesFromHash(string hashId)
        {
            return TryCatch<Dictionary<string, string>>(() => client.GetAllEntriesFromHash(hashId), hashId);
        }

        public List<string> GetAllItemsFromList(string listId)
        {
            return TryCatch<List<string>>(() => client.GetAllItemsFromList(listId), listId);
        }

        public HashSet<string> GetAllItemsFromSet(string setId)
        {
            return TryCatch<HashSet<string>>(() => client.GetAllItemsFromSet(setId), setId);
        }

        public List<string> GetAllItemsFromSortedSet(string setId)
        {
            return TryCatch<List<string>>(() => client.GetAllItemsFromSortedSet(setId), setId);
        }

        public List<string> GetAllItemsFromSortedSetDesc(string setId)
        {
            return TryCatch<List<string>>(() => client.GetAllItemsFromSortedSetDesc(setId), setId);
        }

        public List<string> GetAllKeys()
        {
            return TryCatch<List<string>>(() => client.GetAllKeys(), string.Empty);
        }

        public IDictionary<string, double> GetAllWithScoresFromSortedSet(string setId)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetAllWithScoresFromSortedSet(setId), setId);
        }

        public string GetAndSetEntry(string key, string value)
        {
            return TryCatch<string>(() => client.GetAndSetValue(key, value), key);
        }

        public HashSet<string> GetDifferencesFromSet(string fromSetId, params string[] withSetIds)
        {
            return TryCatch<HashSet<string>>(() => client.GetDifferencesFromSet(fromSetId, withSetIds), fromSetId);
        }

        public T GetFromHash<T>(object id)
        {
            return TryCatch<T>(() => client.GetFromHash<T>(id), string.Empty);
        }

        public long GetHashCount(string hashId)
        {
            return TryCatch<long>(() => client.GetHashCount(hashId), hashId);
        }

        public List<string> GetHashKeys(string hashId)
        {
            return TryCatch<List<string>>(() => client.GetHashKeys(hashId), hashId);
        }

        public List<string> GetHashValues(string hashId)
        {
            return TryCatch<List<string>>(() => client.GetHashValues(hashId), hashId);
        }

        public HashSet<string> GetIntersectFromSets(params string[] setIds)
        {
            return TryCatch<HashSet<string>>(() => client.GetIntersectFromSets(setIds), setIds[0]);
        }

        public string GetItemFromList(string listId, int listIndex)
        {
            return TryCatch<string>(() => client.GetItemFromList(listId, listIndex), listId);
        }

        public long GetItemIndexInSortedSet(string setId, string value)
        {
            return TryCatch<long>(() => client.GetItemIndexInSortedSet(setId, value), setId);
        }

        public long GetItemIndexInSortedSetDesc(string setId, string value)
        {
            return TryCatch<long>(() => client.GetItemIndexInSortedSetDesc(setId, value), setId);
        }

        public double GetItemScoreInSortedSet(string setId, string value)
        {
            return TryCatch<double>(() => client.GetItemScoreInSortedSet(setId, value), setId);
        }

        public long GetListCount(string listId)
        {
            return TryCatch<long>(() => client.GetListCount(listId), listId);
        }

        public string GetRandomItemFromSet(string setId)
        {
            return TryCatch<string>(() => client.GetRandomItemFromSet(setId), setId);
        }

        public List<string> GetRangeFromList(string listId, int startingFrom, int endingAt)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromList(listId, startingFrom, endingAt), listId);
        }

        public List<string> GetRangeFromSortedList(string listId, int startingFrom, int endingAt)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedList(listId, startingFrom, endingAt), listId);
        }

        public List<string> GetRangeFromSortedSet(string setId, int fromRank, int toRank)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSet(setId, fromRank, toRank), setId);
        }

        public List<string> GetRangeFromSortedSetByHighestScore(string setId, double fromScore, double toScore)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByHighestScore(setId, fromScore, toScore), setId);
        }

        public List<string> GetRangeFromSortedSetByHighestScore(string setId, long fromScore, long toScore)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByHighestScore(setId, fromScore, toScore), setId);
        }

        public List<string> GetRangeFromSortedSetByHighestScore(string setId, string fromStringScore, string toStringScore)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByHighestScore(setId, fromStringScore, toStringScore), setId);
        }

        public List<string> GetRangeFromSortedSetByHighestScore(string setId, double fromScore, double toScore, int? skip, int? take)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByHighestScore(setId, fromScore, toScore, skip, take), setId);
        }

        public List<string> GetRangeFromSortedSetByHighestScore(string setId, long fromScore, long toScore, int? skip, int? take)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByHighestScore(setId, fromScore, toScore, skip, take), setId);
        }

        public List<string> GetRangeFromSortedSetByHighestScore(string setId, string fromStringScore, string toStringScore, int? skip, int? take)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByHighestScore(setId, fromStringScore, toStringScore, skip, take), setId);
        }

        public List<string> GetRangeFromSortedSetByLowestScore(string setId, double fromScore, double toScore)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByLowestScore(setId, fromScore, toScore), setId);
        }

        public List<string> GetRangeFromSortedSetByLowestScore(string setId, long fromScore, long toScore)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByLowestScore(setId, fromScore, toScore), setId);
        }

        public List<string> GetRangeFromSortedSetByLowestScore(string setId, string fromStringScore, string toStringScore)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByLowestScore(setId, fromStringScore, toStringScore), setId);
        }

        public List<string> GetRangeFromSortedSetByLowestScore(string setId, double fromScore, double toScore, int? skip, int? take)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByLowestScore(setId, fromScore, toScore, skip, take), setId);
        }

        public List<string> GetRangeFromSortedSetByLowestScore(string setId, long fromScore, long toScore, int? skip, int? take)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByLowestScore(setId, fromScore, toScore, skip, take), setId);
        }

        public List<string> GetRangeFromSortedSetByLowestScore(string setId, string fromStringScore, string toStringScore, int? skip, int? take)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetByLowestScore(setId, fromStringScore, toStringScore, skip, take), setId);
        }

        public List<string> GetRangeFromSortedSetDesc(string setId, int fromRank, int toRank)
        {
            return TryCatch<List<string>>(() => client.GetRangeFromSortedSetDesc(setId, fromRank, toRank), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSet(string setId, int fromRank, int toRank)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSet(setId, fromRank, toRank), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByHighestScore(string setId, double fromScore, double toScore)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByHighestScore(setId, fromScore, toScore), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByHighestScore(string setId, long fromScore, long toScore)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByHighestScore(setId, fromScore, toScore), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByHighestScore(string setId, string fromStringScore, string toStringScore)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByHighestScore(setId, fromStringScore, toStringScore), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByHighestScore(string setId, double fromScore, double toScore, int? skip, int? take)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByHighestScore(setId, fromScore, toScore, skip, take), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByHighestScore(string setId, long fromScore, long toScore, int? skip, int? take)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByHighestScore(setId, fromScore, toScore, skip, take), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByHighestScore(string setId, string fromStringScore, string toStringScore, int? skip, int? take)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByHighestScore(setId, fromStringScore, toStringScore, skip, take), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByLowestScore(string setId, double fromScore, double toScore)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByHighestScore(setId, fromScore, toScore), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByLowestScore(string setId, long fromScore, long toScore)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByLowestScore(setId, fromScore, toScore), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByLowestScore(string setId, string fromStringScore, string toStringScore)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByLowestScore(setId, fromStringScore, toStringScore), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByLowestScore(string setId, double fromScore, double toScore, int? skip, int? take)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByLowestScore(setId, fromScore, toScore, skip, take), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByLowestScore(string setId, long fromScore, long toScore, int? skip, int? take)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByLowestScore(setId, fromScore, toScore, skip, take), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetByLowestScore(string setId, string fromStringScore, string toStringScore, int? skip, int? take)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetByLowestScore(setId, fromStringScore, toStringScore, skip, take), setId);
        }

        public IDictionary<string, double> GetRangeWithScoresFromSortedSetDesc(string setId, int fromRank, int toRank)
        {
            return TryCatch<IDictionary<string, double>>(() => client.GetRangeWithScoresFromSortedSetDesc(setId, fromRank, toRank), setId);
        }

        public long GetSetCount(string setId)
        {
            return TryCatch<long>(() => client.GetSetCount(setId), setId);
        }

        public List<string> GetSortedEntryValues(string key, int startingFrom, int endingAt)
        {
            return TryCatch<List<string>>(() => client.GetSortedEntryValues(key, startingFrom, endingAt), key);
        }

        public long GetSortedSetCount(string setId)
        {
            return TryCatch<long>(() => client.GetSortedSetCount(setId), setId);
        }

        public long GetSortedSetCount(string setId, double fromScore, double toScore)
        {
            return TryCatch<long>(() => client.GetSortedSetCount(setId, fromScore, toScore), setId);
        }

        public long GetSortedSetCount(string setId, long fromScore, long toScore)
        {
            return TryCatch<long>(() => client.GetSortedSetCount(setId, fromScore, toScore), setId);
        }

        public long GetSortedSetCount(string setId, string fromStringScore, string toStringScore)
        {
            return TryCatch<long>(() => client.GetSortedSetCount(setId, fromStringScore, toStringScore), setId);
        }

        public string GetSubstring(string key, int fromIndex, int toIndex)
        {
            return this.TryCatch<string>(delegate
            {
                byte[] bytes = ((RedisClient)client).GetRange(key, fromIndex, toIndex);
                if (bytes != null)
                {
                    return StringExtensions.FromUtf8Bytes(bytes);
                }
                return null;
            }, key);
        }

        public TimeSpan GetTimeToLive(string key)
        {
            return TryCatch(delegate
            {
                TimeSpan? t = client.GetTimeToLive(key);
                if (!t.HasValue)
                {
                    return TimeSpan.Zero;
                }
                return t.Value;
            }, key);
        }

        public HashSet<string> GetUnionFromSets(params string[] setIds)
        {
            return TryCatch<HashSet<string>>(() => client.GetUnionFromSets(setIds), setIds[0]);
        }

        public string GetValue(string key)
        {
            return TryCatch<string>(() => client.GetValue(key), key);
        }

        public string GetValueFromHash(string hashId, string key)
        {
            return TryCatch<string>(() => client.GetValueFromHash(hashId, key), hashId);
        }

        public List<string> GetValues(List<string> keys)
        {
            return TryCatch<List<string>>(() => client.GetValues(keys), keys[0]);
        }

        public List<T> GetValues<T>(List<string> keys)
        {
            return TryCatch<List<T>>(() => client.GetValues<T>(keys), keys[0]);
        }

        public List<string> GetValuesFromHash(string hashId, params string[] keys)
        {
            return TryCatch<List<string>>(() => client.GetValuesFromHash(hashId, keys), hashId);
        }

        public Dictionary<string, T> GetValuesMap<T>(List<string> keys)
        {
            return TryCatch<Dictionary<string, T>>(() => client.GetValuesMap<T>(keys), keys[0]);
        }

        public Dictionary<string, string> GetValuesMap(List<string> keys)
        {
            return TryCatch<Dictionary<string, string>>(() => client.GetValuesMap(keys), keys[0]);
        }

        public bool HashContainsEntry(string hashId, string key)
        {
            return TryCatch<bool>(() => client.HashContainsEntry(hashId, key), hashId);
        }

        public double IncrementItemInSortedSet(string setId, string value, double incrementBy)
        {
            return TryCatch<double>(() => client.IncrementItemInSortedSet(setId, value, incrementBy), setId);
        }

        public double IncrementItemInSortedSet(string setId, string value, long incrementBy)
        {
            return TryCatch<double>(() => client.IncrementItemInSortedSet(setId, value, incrementBy), setId);
        }

        public long IncrementValue(string key)
        {
            return TryCatch<long>(() => client.IncrementValue(key), key);
        }

        public long IncrementValueBy(string key, int count)
        {
            return TryCatch<long>(() => client.IncrementValueBy(key, count), key);
        }

        public long IncrementValueInHash(string hashId, string key, int incrementBy)
        {
            return TryCatch<long>(() => client.IncrementValueInHash(hashId, key, incrementBy), hashId);
        }

        public void MoveBetweenSets(string fromSetId, string toSetId, string item)
        {
            TryCatch(delegate
            {
                client.MoveBetweenSets(fromSetId, toSetId, item);
            }, fromSetId);
        }

        public string PopAndPushItemBetweenLists(string fromListId, string toListId)
        {
            return TryCatch<string>(() => client.PopAndPushItemBetweenLists(fromListId, toListId), fromListId);
        }

        public string PopItemFromList(string listId)
        {
            return TryCatch<string>(() => client.PopItemFromList(listId), listId);
        }

        public string PopItemFromSet(string setId)
        {
            return TryCatch<string>(() => client.PopItemFromSet(setId), setId);
        }

        public string PopItemWithHighestScoreFromSortedSet(string setId)
        {
            return TryCatch<string>(() => client.PopItemWithHighestScoreFromSortedSet(setId), setId);
        }

        public string PopItemWithLowestScoreFromSortedSet(string setId)
        {
            return TryCatch<string>(() => client.PopItemWithLowestScoreFromSortedSet(setId), setId);
        }

        public void PrependItemToList(string listId, string value)
        {
            TryCatch(delegate
            {
                client.PrependItemToList(listId, value);
            }, listId);
        }

        public void PrependRangeToList(string listId, List<string> values)
        {
            TryCatch(delegate
            {
                client.PrependRangeToList(listId, values);
            }, listId);
        }

        public long PublishMessage(string toChannel, string message)
        {
            return TryCatch<long>(() => client.PublishMessage(toChannel, message), string.Empty);
        }

        public void PushItemToList(string listId, string value)
        {
            TryCatch(delegate
            {
                client.PushItemToList(listId, value);
            }, listId);
        }

        public void RemoveAllFromList(string listId)
        {
            TryCatch(delegate
            {
                client.Remove(listId);
            }, listId);
        }

        public string RemoveEndFromList(string listId)
        {
            return TryCatch<string>(() => client.RemoveEndFromList(listId), listId);
        }

        public bool RemoveEntry(params string[] args)
        {
            return TryCatch<bool>(() => client.RemoveEntry(args), args[0]);
        }

        public bool RemoveEntryFromHash(string hashId, string key)
        {
            return TryCatch<bool>(() => client.RemoveEntryFromHash(hashId, key), hashId);
        }

        public long RemoveItemFromList(string listId, string value)
        {
            return TryCatch<long>(() => client.RemoveItemFromList(listId, value), listId);
        }

        public long RemoveItemFromList(string listId, string value, int noOfMatches)
        {
            return TryCatch<long>(() => client.RemoveItemFromList(listId, value, noOfMatches), listId);
        }

        public void RemoveItemFromSet(string setId, string item)
        {
            TryCatch(delegate
            {
                client.RemoveItemFromSet(setId, item);
            }, setId);
        }

        public bool RemoveItemFromSortedSet(string setId, string value)
        {
            return TryCatch<bool>(() => client.RemoveItemFromSortedSet(setId, value), setId);
        }
        /// <summary>
        ///  骚操作-- redis 连接池-- 如果出现高并发,客户端的连接数量会上限,为了节省资源,重复利用连接对象,通过线程池去获取连接
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IRedisClientsManager GetPoolClient(string host, int port, int db)
        {
            return new PooledRedisClientManager(db, host + ":" + port);
        }
        public long RemoveRangeFromSortedSet(string setId, int minRank, int maxRank)
        {
            return TryCatch<long>(() => client.RemoveRangeFromSortedSet(setId, minRank, maxRank), setId);
        }

        public long RemoveRangeFromSortedSetByScore(string setId, double fromScore, double toScore)
        {
            return TryCatch<long>(() => client.RemoveRangeFromSortedSetByScore(setId, fromScore, toScore), setId);
        }

        public long RemoveRangeFromSortedSetByScore(string setId, long fromScore, long toScore)
        {
            return TryCatch<long>(() => client.RemoveRangeFromSortedSetByScore(setId, fromScore, toScore), setId);
        }

        public string RemoveStartFromList(string listId)
        {
            return TryCatch<string>(() => client.RemoveStartFromList(listId), listId);
        }

        public void RenameKey(string fromName, string toName)
        {
            TryCatch(delegate
            {
                client.RenameKey(fromName, toName);
            }, string.Empty);
        }

        public List<string> SearchKeys(string pattern)
        {
            return TryCatch<List<string>>(() => client.SearchKeys(pattern), pattern);
        }

        public void SetAll(Dictionary<string, string> map)
        {
            TryCatch(delegate
            {
                client.SetAll(map);
            }, string.Empty);
        }

        public void SetAll(IEnumerable<string> keys, IEnumerable<string> values)
        {
            TryCatch(delegate
            {
                client.SetAll(keys, values);
            }, string.Empty);
        }

        public bool SetContainsItem(string setId, string item)
        {
            return TryCatch<bool>(() => client.SetContainsItem(setId, item), setId);
        }

        public void SetEntry(string key, string value)
        {
            TryCatch(delegate
            {
                client.SetValue(key, value);
            }, key);
        }

        public void SetEntry(string key, string value, TimeSpan expireIn)
        {
            TryCatch(delegate
            {
                client.SetValue(key, value, expireIn);
            }, key);
        }

        public bool SetEntryIfNotExists(string key, string value)
        {
            return TryCatch<bool>(() => client.SetValueIfNotExists(key, value), key);
        }

        public bool SetEntryInHash(string hashId, string key, string value)
        {
            return TryCatch<bool>(() => client.SetEntryInHash(hashId, key, value), hashId);
        }

        public bool SetEntryInHashIfNotExists(string hashId, string key, string value)
        {
            return TryCatch<bool>(() => client.SetEntryInHashIfNotExists(hashId, key, value), hashId);
        }

        public void SetItemInList(string listId, int listIndex, string value)
        {
            TryCatch(delegate
            {
                client.SetItemInList(listId, listIndex, value);
            }, listId);
        }

        public void SetRangeInHash(string hashId, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            TryCatch(delegate
            {
                client.SetRangeInHash(hashId, keyValuePairs);
            }, hashId);
        }

        public bool SortedSetContainsItem(string setId, string value)
        {
            return TryCatch<bool>(() => client.SortedSetContainsItem(setId, value), setId);
        }

        public void StoreAsHash<T>(T entity)
        {
            TryCatch(delegate
            {
                client.StoreAsHash<T>(entity);
            }, string.Empty);
        }


        public bool SetEntryInHash<T>(string hashId, string key, T value)
        {
            return this.TryCatch<bool>(() => client.SetEntryInHash(hashId, key, TextExtensions.SerializeToString<T>(value)), hashId);
        }

        public T GetValueFromHash<T>(string hashId, string key)
        {
            return this.TryCatch<T>(() => JsonSerializer.DeserializeFromString<T>(client.GetValueFromHash(hashId, key)), hashId);
        }

        public bool SetEntryInHashIfNotExists<T>(string hashId, string key, T value)
        {
            return this.TryCatch<bool>(() => client.SetEntryInHashIfNotExists(hashId, key, TextExtensions.SerializeToString<T>(value)), hashId);
        }

        public IDisposable AcquireLock(string key)
        {
            return TryCatch<IDisposable>(() => client.AcquireLock(key), key);
        }

        public IDisposable AcquireLock(string key, TimeSpan timeOut)
        {
            return TryCatch<IDisposable>(() => client.AcquireLock(key, timeOut), key);
        }


        public DateTime GetServerTime()
        {
            return TryCatch<DateTime>(() => client.GetServerTime(), string.Empty);
        }


    }
}
