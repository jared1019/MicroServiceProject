using Newtonsoft.Json;
using StackExchange.Redis;

namespace MicroService.Core.RedisExtend
{
    /// <summary>
    /// Redis操作方法基础类
    /// </summary>
    public class RedisHelper
    {
        /// <summary>
        /// Redis操作对象
        /// </summary>
        protected readonly IDatabase redis = null;


        /// <summary>
        /// 初始化Redis操作方法基础类
        /// </summary>
        /// <param name="dbNum"></param>
        public RedisHelper(string connstr, int dbNum = 0)
        {
            redis = ConnectionMultiplexer.Connect(connstr).GetDatabase(dbNum);
        }
        #region 常规
        /// <summary>
        /// 获取Redis事务对象
        /// </summary>
        /// <returns></returns>
        public ITransaction CreateTransaction() => redis.CreateTransaction();

        /// <summary>
        /// 获取Redis服务和常用操作对象
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase() => redis;


        /// <summary>
        /// 执行Redis事务
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public bool RedisTransaction(Action<ITransaction> act)
        {
            var tran = redis.CreateTransaction();
            act.Invoke(tran);
            bool committed = tran.Execute();
            return committed;
        }
        /// <summary>
        /// Redis锁
        /// </summary>
        /// <param name="act"></param>
        /// <param name="ts">锁住时间</param>
        public void RedisLockTake(Action act, TimeSpan ts)
        {
            RedisValue token = Environment.MachineName;
            string lockKey = "lock_LockTake";
            if (redis.LockTake(lockKey, token, ts))
            {
                try
                {
                    act();
                }
                finally
                {
                    redis.LockRelease(lockKey, token);
                }
            }
        }


        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">要删除的key</param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete(string key)
        {

            return redis.KeyDelete(key);
        }

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">要删除的key集合</param>
        /// <returns>成功删除的个数</returns>
        public long KeyDelete(params string[] keys)
        {
            RedisKey[] newKeys = keys.Select(o => (RedisKey)(o)).ToArray();
            return redis.KeyDelete(newKeys);
        }

        /// <summary>
        /// 清空当前DataBase中所有Key
        /// </summary>
        public void KeyFulsh()
        {
            //直接执行清除命令
            redis.Execute("FLUSHDB");
        }

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        /// <param name="key">要判断的key</param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {

            return redis.KeyExists(key);
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        public bool KeyRename(string key, string newKey)
        {

            return redis.KeyRename(key, newKey);
        }

        /// <summary>
        /// 设置Key的过期时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?))
        {

            return redis.KeyExpire(key, expiry);
        }
        /// <summary>
        /// 将对象转换成string字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string ConvertJson<T>(T value)
        {
            string result = value is string ? value.ToString() :
                JsonConvert.SerializeObject(value, Formatting.None);
            return result;
        }
        /// <summary>
        /// 将值反系列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected T ConvertObj<T>(RedisValue value)
        {
            return value.IsNullOrEmpty ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// 将值反系列化成对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        protected List<T> ConvetList<T>(RedisValue[] values)
        {
            List<T> result = new List<T>();
            foreach (var item in values)
            {
                var model = ConvertObj<T>(item);
                result.Add(model);
            }
            return result;
        }
        /// <summary>
        /// 将string类型的Key转换成 <see cref="RedisKey"/> 型的Key
        /// </summary>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        protected RedisKey[] ConvertRedisKeys(List<string> redisKeys) => redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();

        /// <summary>
        /// 将string类型的Key转换成 <see cref="RedisKey"/> 型的Key
        /// </summary>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        protected RedisKey[] ConvertRedisKeys(params string[] redisKeys) => redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();

        /// <summary>
        /// 将string类型的Key转换成 <see cref="RedisKey"/> 型的Key
        /// </summary>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        protected RedisKey[] ConvertRedisKeysAddSysCustomKey(params string[] redisKeys) => redisKeys.Select(redisKey => (RedisKey)(redisKey)).ToArray();
        /// <summary>
        /// 将值集合转换成RedisValue集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="redisValues"></param>
        /// <returns></returns>
        protected RedisValue[] ConvertRedisValue<T>(params T[] redisValues) => redisValues.Select(o => (RedisValue)ConvertJson<T>(o)).ToArray();
        #endregion

        #region string

        /// <summary>
        /// 添加单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool StringSet(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {

            return redis.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 添加多个key/value
        /// </summary>
        /// <param name="valueList">key/value集合</param>
        /// <returns></returns>
        public bool StringSet(Dictionary<string, string> valueList)
        {
            var newkeyValues = valueList.Select(p => new KeyValuePair<RedisKey, RedisValue>(p.Key, p.Value)).ToArray();
            return redis.StringSet(newkeyValues);
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">保存的Key名称</param>
        /// <param name="value">对象实体</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool StringSet<T>(string key, T value, TimeSpan? expiry = default(TimeSpan?))
        {

            string jsonValue = ConvertJson(value);
            return redis.StringSet(key, jsonValue, expiry);
        }

        /// <summary>
        /// 在原有key的value值之后追加value
        /// </summary>
        /// <param name="key">追加的Key名称</param>
        /// <param name="value">追加的值</param>
        /// <returns></returns>
        public long StringAppend(string key, string value)
        {

            return redis.StringAppend(key, value);
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">要读取的Key名称</param>
        /// <returns></returns>
        public string StringGet(string key)
        {

            return redis.StringGet(key);
        }

        /// <summary>
        /// 获取多个key的value值
        /// </summary>
        /// <param name="keys">要获取值的Key集合</param>
        /// <returns></returns>
        public List<string> StringGet(params string[] keys)
        {
            var newKeys = ConvertRedisKeysAddSysCustomKey(keys);
            var values = redis.StringGet(newKeys);
            return values.Select(o => o.ToString()).ToList();
        }


        /// <summary>
        /// 获取单个key的value值
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="key">要获取值的Key集合</param>
        /// <returns></returns>
        public T StringGet<T>(string key)
        {

            var values = redis.StringGet(key);
            return ConvertObj<T>(values);
        }

        /// <summary>
        /// 获取多个key的value值
        /// </summary>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <param name="keys">要获取值的Key集合</param>
        /// <returns></returns>
        public List<T> StringGet<T>(params string[] keys)
        {
            var newKeys = ConvertRedisKeysAddSysCustomKey(keys);
            var values = redis.StringGet(newKeys);
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 获取旧值赋上新值
        /// </summary>
        /// <param name="key">Key名称</param>
        /// <param name="value">新值</param>
        /// <returns></returns>
        public string StringGetSet(string key, string value)
        {

            return redis.StringGetSet(key, value);
        }

        /// <summary>
        /// 获取旧值赋上新值
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">Key名称</param>
        /// <param name="value">新值</param>
        /// <returns></returns>
        public T StringGetSet<T>(string key, T value)
        {

            string jsonValue = ConvertJson(value);
            var oValue = redis.StringGetSet(key, jsonValue);
            return ConvertObj<T>(oValue);
        }


        /// <summary>
        /// 获取值的长度
        /// </summary>
        /// <param name="key">Key名称</param>
        /// <returns></returns>
        public long StringGetLength(string key)
        {

            return redis.StringLength(key);
        }

        /// <summary>
        /// 数字增长val，返回自增后的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double StringIncrement(string key, double val = 1)
        {
            return redis.StringIncrement(key, val);
        }

        /// <summary>
        /// 数字减少val，返回自减少的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double StringDecrement(string key, double val = 1)
        {

            return redis.StringDecrement(key, val);
        }
        #endregion

        #region hash

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashExists(string key, string dataKey)
        {

            return redis.HashExists(key, dataKey);
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSet<T>(string key, string dataKey, T t)
        {

            string json = ConvertJson(t);
            return redis.HashSet(key, dataKey, json);
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashDelete(string key, string dataKey)
        {

            return redis.HashDelete(key, dataKey);
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public long HashDelete(string key, params string[] dataKeys)
        {

            var newValues = dataKeys.Select(o => (RedisValue)o).ToArray();
            return redis.HashDelete(key, newValues);
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public T HashGet<T>(string key, string dataKey)
        {

            string value = redis.HashGet(key, dataKey);
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 数字增长val，返回自增后的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double HashIncrement(string key, string dataKey, double val = 1)
        {

            return redis.HashIncrement(key, dataKey, val);
        }

        /// <summary>
        /// 数字减少val，返回自减少的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double HashDecrement(string key, string dataKey, double val = 1)
        {

            return redis.HashDecrement(key, dataKey, val);
        }

        /// <summary>
        /// 获取hashkey所有key名称
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] HashKeys(string key)
        {

            RedisValue[] values = redis.HashKeys(key);
            return values.Select(o => o.ToString()).ToArray();
        }

        /// <summary>
        /// 获取hashkey所有key与值，必须保证Key内的所有数据类型一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string, T> HashGetAll<T>(string key)
        {

            var query = redis.HashGetAll(key);
            Dictionary<string, T> dic = new Dictionary<string, T>();
            foreach (var item in query)
            {
                dic.Add(item.Name, ConvertObj<T>(item.Value));
            }
            return dic;
        }


        #endregion

        #region List

        /// <summary>
        /// 从左侧向list中添加一个值，返回集合总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long ListLeftPush<T>(string key, T value)
        {

            string jValue = ConvertJson(value);
            return redis.ListLeftPush(key, jValue);
        }

        /// <summary>
        /// 从左侧向list中添加多个值，返回集合总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long ListLeftPush<T>(string key, List<T> value)
        {

            RedisValue[] valueList = ConvertRedisValue(value.ToArray());
            return redis.ListLeftPush(key, valueList);
        }

        /// <summary>
        /// 从右侧向list中添加一个值，返回集合总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long ListRightPush<T>(string key, T value)
        {
            string jValue = ConvertJson(value);
            return redis.ListRightPush(key, jValue);
        }

        /// <summary>
        /// 从右侧向list中添加多个值，返回集合总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long ListRightPush<T>(string key, List<T> value)
        {
            RedisValue[] valueList = ConvertRedisValue(value.ToArray());
            return redis.ListRightPush(key, valueList);
        }

        /// <summary>
        /// 从左侧向list中取出一个值并从list中删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListLeftPop<T>(string key)
        {
            var rValue = redis.ListLeftPop(key);
            return ConvertObj<T>(rValue);
        }

        /// <summary>
        /// 从右侧向list中取出一个值并从list中删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListRightPop<T>(string key)
        {

            var rValue = redis.ListRightPop(key);
            return ConvertObj<T>(rValue);
        }

        /// <summary>
        /// 从key的List中右侧取出一个值，并从左侧添加到destination集合中，且返回该数据对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">要取出数据的List名称</param>
        /// <param name="destination">要添加到的List名称</param>
        /// <returns></returns>
        public T ListRightPopLeftPush<T>(string key, string destination)
        {
            var rValue = redis.ListRightPopLeftPush(key, destination);
            return ConvertObj<T>(rValue);
        }

        /// <summary>
        /// 在key的List指定值pivot之后插入value，返回集合总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="pivot">索引值</param>
        /// <param name="value">要插入的值</param>
        /// <returns></returns>
        public long ListInsertAfter<T>(string key, T pivot, T value)
        {

            string pValue = ConvertJson(pivot);
            string jValue = ConvertJson(value);
            return redis.ListInsertAfter(key, pValue, jValue);
        }

        /// <summary>
        /// 在key的List指定值pivot之前插入value，返回集合总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="pivot">索引值</param>
        /// <param name="value">要插入的值</param>
        /// <returns></returns>
        public long ListInsertBefore<T>(string key, T pivot, T value)
        {

            string pValue = ConvertJson(pivot);
            string jValue = ConvertJson(value);
            return redis.ListInsertBefore(key, pValue, jValue);
        }

        /// <summary>
        /// 从key的list中取出所有数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> ListRange<T>(string key)
        {

            var rValue = redis.ListRange(key);
            return ConvetList<T>(rValue);
        }

        /// <summary>
        /// 从key的List获取指定索引的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public T ListGetByIndex<T>(string key, long index)
        {

            var rValue = redis.ListGetByIndex(key, index);
            return ConvertObj<T>(rValue);
        }

        /// <summary>
        /// 获取key的list中数据个数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ListLength(string key)
        {

            return redis.ListLength(key);
        }

        /// <summary>
        /// 从key的List中移除指定的值，返回删除个数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long ListRemove<T>(string key, T value)
        {

            string jValue = ConvertJson(value);
            return redis.ListRemove(key, jValue);
        }

        #endregion

        #region Set 
        /// <summary>
        /// 在Key集合中添加一个value值
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">Key名称</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public bool SetAdd<T>(string key, T value)
        {

            string jValue = ConvertJson(value);
            return redis.SetAdd(key, jValue);
        }
        /// <summary>
        /// 在Key集合中添加多个value值
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">Key名称</param>
        /// <param name="value">值列表</param>
        /// <returns></returns>
        public long SetAdd<T>(string key, List<T> value)
        {

            RedisValue[] valueList = ConvertRedisValue(value.ToArray());
            return redis.SetAdd(key, valueList);
        }

        /// <summary>
        /// 获取key集合值的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SetLength(string key)
        {

            return redis.SetLength(key);
        }

        /// <summary>
        /// 判断Key集合中是否包含指定的值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key"></param>
        /// <param name="value">要判断是值</param>
        /// <returns></returns>
        public bool SetContains<T>(string key, T value)
        {

            string jValue = ConvertJson(value);
            return redis.SetContains(key, jValue);
        }

        /// <summary>
        /// 随机获取key集合中的一个值
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T SetRandomMember<T>(string key)
        {

            var rValue = redis.SetRandomMember(key);
            return ConvertObj<T>(rValue);
        }

        /// <summary>
        /// 获取key所有值的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SetMembers<T>(string key)
        {

            var rValue = redis.SetMembers(key);
            return ConvetList<T>(rValue);
        }

        /// <summary>
        /// 删除key集合中指定的value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long SetRemove<T>(string key, params T[] value)
        {

            RedisValue[] valueList = ConvertRedisValue(value);
            return redis.SetRemove(key, valueList);
        }

        /// <summary>
        /// 随机删除key集合中的一个值，并返回该值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T SetPop<T>(string key)
        {

            var rValue = redis.SetPop(key);
            return ConvertObj<T>(rValue);
        }


        /// <summary>
        /// 获取几个集合的并集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        public List<T> SetCombineUnion<T>(params string[] keys)
        {
            return _SetCombine<T>(SetOperation.Union, keys);
        }
        /// <summary>
        /// 获取几个集合的交集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        public List<T> SetCombineIntersect<T>(params string[] keys)
        {
            return _SetCombine<T>(SetOperation.Intersect, keys);
        }
        /// <summary>
        /// 获取几个集合的差集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        public List<T> SetCombineDifference<T>(params string[] keys)
        {
            return _SetCombine<T>(SetOperation.Difference, keys);
        }

        /// <summary>
        /// 获取几个集合的并集,并保存到一个新Key中
        /// </summary>
        /// <param name="destination">保存的新Key名称</param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        public long SetCombineUnionAndStore(string destination, params string[] keys)
        {
            return _SetCombineAndStore(SetOperation.Union, destination, keys);
        }
        /// <summary>
        /// 获取几个集合的交集,并保存到一个新Key中
        /// </summary>
        /// <param name="destination">保存的新Key名称</param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        public long SetCombineIntersectAndStore(string destination, params string[] keys)
        {
            return _SetCombineAndStore(SetOperation.Intersect, destination, keys);
        }
        /// <summary>
        /// 获取几个集合的差集,并保存到一个新Key中
        /// </summary>
        /// <param name="destination">保存的新Key名称</param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        public long SetCombineDifferenceAndStore(string destination, params string[] keys)
        {
            return _SetCombineAndStore(SetOperation.Difference, destination, keys);
        }

        #region 内部辅助方法
        /// <summary>
        /// 获取几个集合的交叉并集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation">Union：并集  Intersect：交集  Difference：差集  详见 <see cref="SetOperation"/></param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        private List<T> _SetCombine<T>(SetOperation operation, params string[] keys)
        {
            RedisKey[] keyList = ConvertRedisKeysAddSysCustomKey(keys);
            var rValue = redis.SetCombine(operation, keyList);
            return ConvetList<T>(rValue);
        }

        /// <summary>
        /// 获取几个集合的交叉并集合,并保存到一个新Key中
        /// </summary>
        /// <param name="operation">Union：并集  Intersect：交集  Difference：差集  详见 <see cref="SetOperation"/></param>
        /// <param name="destination">保存的新Key名称</param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        private long _SetCombineAndStore(SetOperation operation, string destination, params string[] keys)
        {

            RedisKey[] keyList = ConvertRedisKeysAddSysCustomKey(keys);
            return redis.SetCombineAndStore(operation, destination, keyList);
        }
        /// <summary>
        /// 获取几个集合的交叉并集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation">Union：并集  Intersect：交集  Difference：差集  详见 <see cref="SetOperation"/></param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        private async Task<List<T>> _SetCombineAsync<T>(SetOperation operation, params string[] keys)
        {
            RedisKey[] keyList = ConvertRedisKeysAddSysCustomKey(keys);
            var rValue = await redis.SetCombineAsync(operation, keyList);
            return ConvetList<T>(rValue);
        }
        /// <summary>
        /// 获取几个集合的交叉并集合,并保存到一个新Key中
        /// </summary>
        /// <param name="operation">Union：并集  Intersect：交集  Difference：差集  详见 <see cref="SetOperation"/></param>
        /// <param name="destination">保存的新Key名称</param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        private async Task<long> _SetCombineAndStoreAsync(SetOperation operation, string destination, params string[] keys)
        {

            RedisKey[] keyList = ConvertRedisKeysAddSysCustomKey(keys);
            return await redis.SetCombineAndStoreAsync(operation, destination, keyList);
        }

        #endregion
        #endregion

        #region Zset

        /// <summary>
        /// 添加一个值到Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score">排序分数，为空将获取集合中最大score加1</param>
        /// <returns></returns>
        public bool SortedSetAdd<T>(string key, T value, double? score = null)
        {

            double scoreNum = score ?? _GetScore(key);
            return redis.SortedSetAdd(key, ConvertJson<T>(value), scoreNum);
        }

        /// <summary>
        /// 添加一个集合到Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score">排序分数，为空将获取集合中最大score加1</param>
        /// <returns></returns>
        public long SortedSetAdd<T>(string key, List<T> value, double? score = null)
        {

            double scoreNum = score ?? _GetScore(key);
            SortedSetEntry[] rValue = value.Select(o => new SortedSetEntry(ConvertJson<T>(o), scoreNum++)).ToArray();
            return redis.SortedSetAdd(key, rValue);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SortedSetLength(string key)
        {

            return redis.SortedSetLength(key);
        }

        /// <summary>
        /// 获取指定起始值到结束值的集合数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="startValue">起始值</param>
        /// <param name="endValue">结束值</param>
        /// <returns></returns>
        public long SortedSetLengthByValue<T>(string key, T startValue, T endValue)
        {

            var sValue = ConvertJson<T>(startValue);
            var eValue = ConvertJson<T>(endValue);
            return redis.SortedSetLengthByValue(key, sValue, eValue);
        }

        /// <summary>
        /// 获取指定Key的排序Score值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double? SortedSetScore<T>(string key, T value)
        {

            var rValue = ConvertJson<T>(value);
            return redis.SortedSetScore(key, rValue);
        }

        /// <summary>
        /// 获取指定Key中最小Score值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double SortedSetMinScore(string key)
        {

            double dValue = 0;
            var rValue = redis.SortedSetRangeByRankWithScores(key, 0, 0, Order.Ascending).FirstOrDefault();
            dValue = rValue != null ? rValue.Score : 0;
            return dValue;
        }

        /// <summary>
        /// 获取指定Key中最大Score值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double SortedSetMaxScore(string key)
        {

            double dValue = 0;
            var rValue = redis.SortedSetRangeByRankWithScores(key, 0, 0, Order.Descending).FirstOrDefault();
            dValue = rValue != null ? rValue.Score : 0;
            return dValue;
        }

        /// <summary>
        /// 删除Key中指定的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public long SortedSetRemove<T>(string key, params T[] value)
        {

            var rValue = ConvertRedisValue<T>(value);
            return redis.SortedSetRemove(key, rValue);
        }

        /// <summary>
        /// 删除指定起始值到结束值的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="startValue">起始值</param>
        /// <param name="endValue">结束值</param>
        /// <returns></returns>
        public long SortedSetRemoveRangeByValue<T>(string key, T startValue, T endValue)
        {

            var sValue = ConvertJson<T>(startValue);
            var eValue = ConvertJson<T>(endValue);
            return redis.SortedSetRemoveRangeByValue(key, sValue, eValue);
        }

        /// <summary>
        /// 删除 从 start 开始的 stop 条数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public long SortedSetRemoveRangeByRank(string key, long start, long stop)
        {

            return redis.SortedSetRemoveRangeByRank(key, start, stop);
        }

        /// <summary>
        /// 根据排序分数Score，删除从 start 开始的 stop 条数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public long SortedSetRemoveRangeByScore(string key, double start, double stop)
        {
            return redis.SortedSetRemoveRangeByScore(key, start, stop);
        }

        /// <summary>
        /// 获取从 start 开始的 stop 条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start">起始数</param>
        /// <param name="stop">-1表示到结束，0为1条</param>
        /// <param name="desc">是否按降序排列</param>
        /// <returns></returns>
        public List<T> SortedSetRangeByRank<T>(string key, long start = 0, long stop = -1, bool desc = false)
        {

            Order orderBy = desc ? Order.Descending : Order.Ascending;
            var rValue = redis.SortedSetRangeByRank(key, start, stop, orderBy);
            return ConvetList<T>(rValue);
        }

        /// <summary>
        /// 获取从 start 开始的 stop 条数据包含Score，返回数据格式：Key=值，Value = Score
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start">起始数</param>
        /// <param name="stop">-1表示到结束，0为1条</param>
        /// <param name="desc">是否按降序排列</param>
        /// <returns></returns>
        public Dictionary<T, double> SortedSetRangeByRankWithScores<T>(string key, long start = 0, long stop = -1, bool desc = false)
        {

            Order orderBy = desc ? Order.Descending : Order.Ascending;
            var rValue = redis.SortedSetRangeByRankWithScores(key, start, stop, orderBy);
            Dictionary<T, double> dicList = new Dictionary<T, double>();
            foreach (var item in rValue)
            {
                dicList.Add(ConvertObj<T>(item.Element), item.Score);
            }
            return dicList;
        }

        /// <summary>
        ///  根据Score排序 获取从 start 开始的 stop 条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start">起始数</param>
        /// <param name="stop">-1表示到结束，0为1条</param>
        /// <param name="desc">是否按降序排列</param>
        /// <returns></returns>
        public List<T> SortedSetRangeByScore<T>(string key, double start = 0, double stop = -1, bool desc = false)
        {

            Order orderBy = desc ? Order.Descending : Order.Ascending;
            var rValue = redis.SortedSetRangeByScore(key, start, stop, Exclude.None, orderBy);
            return ConvetList<T>(rValue);
        }

        /// <summary>
        /// 根据Score排序  获取从 start 开始的 stop 条数据包含Score，返回数据格式：Key=值，Value = Score
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start">起始数</param>
        /// <param name="stop">-1表示到结束，0为1条</param>
        /// <param name="desc">是否按降序排列</param>
        /// <returns></returns>
        public Dictionary<T, double> SortedSetRangeByScoreWithScores<T>(string key, double start = 0, double stop = -1, bool desc = false)
        {

            Order orderBy = desc ? Order.Descending : Order.Ascending;
            var rValue = redis.SortedSetRangeByScoreWithScores(key, start, stop, Exclude.None, orderBy);
            Dictionary<T, double> dicList = new Dictionary<T, double>();
            foreach (var item in rValue)
            {
                dicList.Add(ConvertObj<T>(item.Element), item.Score);
            }
            return dicList;
        }

        /// <summary>
        /// 获取指定起始值到结束值的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="startValue">起始值</param>
        /// <param name="endValue">结束值</param>
        /// <returns></returns>
        public List<T> SortedSetRangeByValue<T>(string key, T startValue, T endValue)
        {

            var sValue = ConvertJson<T>(startValue);
            var eValue = ConvertJson<T>(endValue);
            var rValue = redis.SortedSetRangeByValue(key, sValue, eValue);
            return ConvetList<T>(rValue);
        }

        /// <summary>
        /// 获取几个集合的并集,并保存到一个新Key中
        /// </summary>
        /// <param name="destination">保存的新Key名称</param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        public long SortedSetCombineUnionAndStore(string destination, params string[] keys)
        {
            return _SortedSetCombineAndStore(SetOperation.Union, destination, keys);
        }

        /// <summary>
        /// 获取几个集合的交集,并保存到一个新Key中
        /// </summary>
        /// <param name="destination">保存的新Key名称</param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        public long SortedSetCombineIntersectAndStore(string destination, params string[] keys)
        {
            return _SortedSetCombineAndStore(SetOperation.Intersect, destination, keys);
        }


        //交集似乎并不支持
        ///// <summary>
        ///// 获取几个集合的差集,并保存到一个新Key中
        ///// </summary>
        ///// <param name="destination">保存的新Key名称</param>
        ///// <param name="keys">要操作的Key集合</param>
        ///// <returns></returns>
        //public long SortedSetCombineDifferenceAndStore(string destination, params string[] keys)
        //{
        //    return _SortedSetCombineAndStore(SetOperation.Difference, destination, keys);
        //}



        /// <summary>
        /// 修改指定Key和值的Scores在原值上减去scores，并返回最终Scores
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="scores"></param>
        /// <returns></returns>
        public double SortedSetDecrement<T>(string key, T value, double scores)
        {

            var rValue = ConvertJson<T>(value);
            return redis.SortedSetDecrement(key, rValue, scores);
        }

        /// <summary>
        /// 修改指定Key和值的Scores在原值上增加scores，并返回最终Scores
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="scores"></param>
        /// <returns></returns>
        public double SortedSetIncrement<T>(string key, T value, double scores)
        {

            var rValue = ConvertJson<T>(value);
            return redis.SortedSetIncrement(key, rValue, scores);
        }

        #region 内部辅助方法
        /// <summary>
        /// 获取指定Key中最大Score值,
        /// </summary>
        /// <param name="key">key名称，注意要先添加上Key前缀</param>
        /// <returns></returns>
        private double _GetScore(string key)
        {
            double dValue = 0;
            var rValue = redis.SortedSetRangeByRankWithScores(key, 0, 0, Order.Descending).FirstOrDefault();
            dValue = rValue != null ? rValue.Score : 0;
            return dValue + 1;
        }

        /// <summary>
        /// 获取几个集合的交叉并集合,并保存到一个新Key中
        /// </summary>
        /// <param name="operation">Union：并集  Intersect：交集  Difference：差集  详见 <see cref="SetOperation"/></param>
        /// <param name="destination">保存的新Key名称</param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        private long _SortedSetCombineAndStore(SetOperation operation, string destination, params string[] keys)
        {
            #region 查看源码，似乎并不支持Difference
            //RedisCommand command;
            //if (operation != SetOperation.Union)
            //{
            //    if (operation != SetOperation.Intersect)
            //    {
            //        throw new ArgumentOutOfRangeException("operation");
            //    }
            //    command = RedisCommand.ZINTERSTORE;
            //}
            //else
            //{
            //    command = RedisCommand.ZUNIONSTORE;
            //}
            #endregion


            RedisKey[] keyList = ConvertRedisKeysAddSysCustomKey(keys);
            var rValue = redis.SortedSetCombineAndStore(operation, destination, keyList);
            return rValue;

        }

        /// <summary>
        /// 获取几个集合的交叉并集合,并保存到一个新Key中
        /// </summary>
        /// <param name="operation">Union：并集  Intersect：交集  Difference：差集  详见 <see cref="SetOperation"/></param>
        /// <param name="destination">保存的新Key名称</param>
        /// <param name="keys">要操作的Key集合</param>
        /// <returns></returns>
        private async Task<long> _SortedSetCombineAndStoreAsync(SetOperation operation, string destination, params string[] keys)
        {

            RedisKey[] keyList = ConvertRedisKeysAddSysCustomKey(keys);
            var rValue = await redis.SortedSetCombineAndStoreAsync(operation, destination, keyList);
            return rValue;
        }

        #endregion

        #endregion
    }
}

