﻿using Newtonsoft.Json;
using Serilog;
using System.Text;

namespace MicroService.Common
{
    public class LogLock
    {
        static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        static int WritedCount = 0;
        static int FailedCount = 0;
        static string _contentRoot = string.Empty;

        public LogLock(string contentPath)
        {
            _contentRoot = contentPath;
        }

        public static void OutSql2Log(string prefix, string[] dataParas, bool IsHeader = true, bool isWrt = false)
        {

            if (Appsettings.app(new string[] { "AppSettings", "LogToDb", "Enabled" }).ObjToBool())
            {
                OutSql2LogToDB(prefix, dataParas, IsHeader);
            }
            else
            {
                OutSql2LogToFile(prefix, dataParas, IsHeader, isWrt);
            }
        }

        public static void OutSql2LogToFile(string prefix, string[] dataParas, bool IsHeader = true, bool isWrt = false)
        {
            try
            {
                //设置读写锁为写入模式独占资源，其他写入请求需要等待本次写入结束之后才能继续写入
                //注意：长时间持有读线程锁或写线程锁会使其他线程发生饥饿 (starve)。 为了得到最好的性能，需要考虑重新构造应用程序以将写访问的持续时间减少到最小。
                //      从性能方面考虑，请求进入写入模式应该紧跟文件操作之前，在此处进入写入模式仅是为了降低代码复杂度
                //      因进入与退出写入模式应在同一个try finally语句块内，所以在请求进入写入模式之前不能触发异常，否则释放次数大于请求次数将会触发异常
                LogWriteLock.EnterWriteLock();

                var folderPath = Path.Combine(_contentRoot, "Log");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                //string logFilePath = Path.Combine(path, $@"{filename}.log");
                var logFilePath = FileHelper.GetAvailableFileWithPrefixOrderSize(folderPath, prefix);

                var now = DateTime.Now;
                string logContent = string.Join("\r\n", dataParas);
                if (IsHeader)
                {
                    logContent =
                       "--------------------------------\r\n" +
                       DateTime.Now + "|\r\n" +
                       string.Join("\r\n", dataParas) + "\r\n"
                       ;
                }

                //if (logContent.IsNotEmptyOrNull() && logContent.Length > 500)
                //{
                //    logContent = logContent.Substring(0, 500) + "\r\n";
                //}
                if (isWrt)
                {
                    File.WriteAllText(logFilePath, logContent);

                }
                else
                {
                    File.AppendAllText(logFilePath, logContent);
                }
                WritedCount++;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                FailedCount++;
            }
            finally
            {
                //退出写入模式，释放资源占用
                //注意：一次请求对应一次释放
                //      若释放次数大于请求次数将会触发异常[写入锁定未经保持即被释放]
                //      若请求处理完成后未释放将会触发异常[此模式不下允许以递归方式获取写入锁定]
                LogWriteLock.ExitWriteLock();
            }
        }
        public static void OutSql2LogToDB(string prefix, string[] dataParas, bool IsHeader = true)
        {

            string logContent = string.Join("\r\n", dataParas);
            if (IsHeader)
            {
                logContent =
                   "--------------------------------\r\n" +
                   DateTime.Now + "|\r\n" +
                   string.Join("\r\n", dataParas) + "\r\n"
                   ;
            }
            switch (prefix)
            {
                case "AOPLog":
                    Log.Logger.Information(logContent);
                    break;
                case "AOPLogEx":
                    Log.Logger.Error(logContent);
                    break;
                case "RequestIpInfoLog":
                    Log.Logger.Debug(logContent);
                    break;
                case "RecordAccessLogs":
                    Log.Logger.Debug(logContent);
                    break;
                case "SqlLog":
                    Log.Logger.Information(logContent);
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="encode">编码</param>
        /// <param name="readType">读取类型(0:精准,1:前缀模糊)</param>
        /// <returns></returns>
        public static string ReadLog(string folderPath, string fileName, Encoding encode, ReadType readType = ReadType.Accurate, int takeOnlyTop = -1)
        {
            string s = "";
            try
            {
                LogWriteLock.EnterReadLock();

                // 根据文件名读取当前文件内容
                if (readType == ReadType.Accurate)
                {
                    var filePath = Path.Combine(folderPath, fileName);
                    if (!File.Exists(filePath))
                    {
                        s = null;
                    }
                    else
                    {
                        StreamReader f2 = new StreamReader(filePath, encode);
                        s = f2.ReadToEnd();
                        f2.Close();
                        f2.Dispose();
                    }
                }

                // 根据前缀读取所有文件内容
                if (readType == ReadType.Prefix)
                {
                    var allFiles = new DirectoryInfo(folderPath);
                    var selectFiles = allFiles.GetFiles().Where(fi => fi.Name.ToLower().Contains(fileName.ToLower())).ToList();

                    selectFiles = takeOnlyTop > 0 ? selectFiles.OrderByDescending(d => d.Name).Take(takeOnlyTop).ToList() : selectFiles;

                    foreach (var item in selectFiles)
                    {
                        if (File.Exists(item.FullName))
                        {
                            StreamReader f2 = new StreamReader(item.FullName, encode);
                            s += f2.ReadToEnd();
                            f2.Close();
                            f2.Dispose();
                        }
                    }
                }

                // 根据前缀读取 最新文件 时间倒叙
                if (readType == ReadType.PrefixLatest)
                {
                    var allFiles = new DirectoryInfo(folderPath);
                    var selectLastestFile = allFiles.GetFiles().Where(fi => fi.Name.ToLower().Contains(fileName.ToLower())).OrderByDescending(d => d.Name).FirstOrDefault();

                    if (selectLastestFile != null && File.Exists(selectLastestFile.FullName))
                    {
                        StreamReader f2 = new StreamReader(selectLastestFile.FullName, encode);
                        s = f2.ReadToEnd();
                        f2.Close();
                        f2.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                FailedCount++;
            }
            finally
            {
                LogWriteLock.ExitReadLock();
            }
            return s;
        }

        private static List<RequestInfo> GetRequestInfo(ReadType readType)
        {
            List<RequestInfo> requestInfos = new();
            var accessLogs = ReadLog(Path.Combine(_contentRoot, "Log"), "RequestIpInfoLog_", Encoding.UTF8, readType).ObjToString();
            try
            {
                return JsonConvert.DeserializeObject<List<RequestInfo>>("[" + accessLogs + "]");
            }
            catch (Exception)
            {
                var accLogArr = accessLogs.Split("\r\n");
                foreach (var item in accLogArr)
                {
                    if (item.ObjToString() != "")
                    {
                        try
                        {
                            var accItem = JsonConvert.DeserializeObject<RequestInfo>(item.TrimEnd(','));
                            requestInfos.Add(accItem);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

            }

            return requestInfos;
        }


        public static List<LogInfo> GetLogData()
        {
            List<LogInfo> aopLogs = new List<LogInfo>();
            List<LogInfo> excLogs = new List<LogInfo>();
            List<LogInfo> sqlLogs = new List<LogInfo>();
            List<LogInfo> reqresLogs = new List<LogInfo>();

            try
            {
                var aoplogContent = ReadLog(Path.Combine(_contentRoot, "Log"), "AOPLog_", Encoding.UTF8, ReadType.Prefix);

                if (!string.IsNullOrEmpty(aoplogContent))
                {
                    aopLogs = aoplogContent.Split("--------------------------------")
                 .Where(d => !string.IsNullOrEmpty(d) && d != "\n" && d != "\r\n")
                 .Select(d => new LogInfo
                 {
                     Datetime = d.Split("|")[0].ObjToDate(),
                     Content = d.Split("|")[1]?.Replace("\r\n", "<br>"),
                     LogColor = "AOP",
                 }).ToList();
                }
            }
            catch (Exception) { }

            try
            {
                var exclogContent = ReadLog(Path.Combine(_contentRoot, "Log"), $"GlobalExceptionLogs_{DateTime.Now.ToString("yyyMMdd")}.log", Encoding.UTF8);

                if (!string.IsNullOrEmpty(exclogContent))
                {
                    excLogs = exclogContent.Split("--------------------------------")
                                 .Where(d => !string.IsNullOrEmpty(d) && d != "\n" && d != "\r\n")
                                 .Select(d => new LogInfo
                                 {
                                     Datetime = d.Split("|")[0].Split(',')[0].ObjToDate(),
                                     Content = d.Split("|")[1]?.Replace("\r\n", "<br>"),
                                     LogColor = "EXC",
                                     Import = 9,
                                 }).ToList();
                }
            }
            catch (Exception) { }


            try
            {
                var sqllogContent = ReadLog(Path.Combine(_contentRoot, "Log"), "SqlLog_", Encoding.UTF8, ReadType.PrefixLatest);

                if (!string.IsNullOrEmpty(sqllogContent))
                {
                    sqlLogs = sqllogContent.Split("--------------------------------")
                                  .Where(d => !string.IsNullOrEmpty(d) && d != "\n" && d != "\r\n")
                                  .Select(d => new LogInfo
                                  {
                                      Datetime = d.Split("|")[0].ObjToDate(),
                                      Content = d.Split("|")[1]?.Replace("\r\n", "<br>"),
                                      LogColor = "SQL",
                                  }).ToList();
                }
            }
            catch (Exception) { }

            //try
            //{
            //    reqresLogs = ReadLog(Path.Combine(_contentRoot, "Log", "RequestResponseLog.log"), Encoding.UTF8)?
            //          .Split("--------------------------------")
            //          .Where(d => !string.IsNullOrEmpty(d) && d != "\n" && d != "\r\n")
            //          .Select(d => new LogInfo
            //          {
            //              Datetime = d.Split("|")[0].ObjToDate(),
            //              Content = d.Split("|")[1]?.Replace("\r\n", "<br>"),
            //              LogColor = "ReqRes",
            //          }).ToList();
            //}
            //catch (Exception)
            //{
            //}

            try
            {
                var Logs = GetRequestInfo(ReadType.PrefixLatest);

                Logs = Logs.Where(d => d.Datetime.ObjToDate() >= DateTime.Today).ToList();

                reqresLogs = Logs.Select(d => new LogInfo
                {
                    Datetime = d.Datetime.ObjToDate(),
                    Content = $"IP:{d.Ip}<br>{d.Url}",
                    LogColor = "ReqRes",
                }).ToList();
            }
            catch (Exception)
            {
            }

            if (excLogs != null)
            {
                aopLogs.AddRange(excLogs);
            }
            if (sqlLogs != null)
            {
                aopLogs.AddRange(sqlLogs);
            }
            if (reqresLogs != null)
            {
                aopLogs.AddRange(reqresLogs);
            }
            aopLogs = aopLogs.OrderByDescending(d => d.Import).ThenByDescending(d => d.Datetime).Take(100).ToList();

            return aopLogs;
        }


        public static RequestApiWeekView RequestApiinfoByWeek()
        {
            List<RequestInfo> Logs = new List<RequestInfo>();
            List<ApiWeek> apiWeeks = new List<ApiWeek>();
            string apiWeeksJson = string.Empty;
            List<string> columns = new List<string>();
            columns.Add("日期");


            try
            {
                Logs = GetRequestInfo(ReadType.Prefix);

                apiWeeks = (from n in Logs
                            group n by new { n.Week, n.Url } into g
                            select new ApiWeek
                            {
                                week = g.Key.Week,
                                url = g.Key.Url,
                                count = g.Count(),
                            }).ToList();

                //apiWeeks = apiWeeks.OrderByDescending(d => d.count).Take(8).ToList();

            }
            catch (Exception)
            {
            }

            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");

            var weeks = apiWeeks.GroupBy(x => new { x.week }).Select(s => s.First()).ToList();
            foreach (var week in weeks)
            {
                var apiweeksCurrentWeek = apiWeeks.Where(d => d.week == week.week).OrderByDescending(d => d.count).Take(5).ToList();
                jsonBuilder.Append("{");

                jsonBuilder.Append("\"");
                jsonBuilder.Append("日期");
                jsonBuilder.Append("\":\"");
                jsonBuilder.Append(week.week);
                jsonBuilder.Append("\",");

                foreach (var item in apiweeksCurrentWeek)
                {
                    columns.Add(item.url);
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(item.url);
                    jsonBuilder.Append("\":\"");
                    jsonBuilder.Append(item.count);
                    jsonBuilder.Append("\",");
                }
                if (apiweeksCurrentWeek.Count > 0)
                {
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                }
                jsonBuilder.Append("},");
            }

            if (weeks.Count > 0)
            {
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            }
            jsonBuilder.Append("]");

            //columns.AddRange(apiWeeks.OrderByDescending(d => d.count).Take(8).Select(d => d.url).ToList());
            columns = columns.Distinct().ToList();

            return new RequestApiWeekView()
            {
                columns = columns,
                rows = jsonBuilder.ToString(),
            };
        }

        public static AccessApiDateView AccessApiByDate()
        {
            List<RequestInfo> Logs = new List<RequestInfo>();
            List<ApiDate> apiDates = new List<ApiDate>();
            try
            {
                Logs = GetRequestInfo(ReadType.Prefix);

                apiDates = (from n in Logs
                            group n by new { n.Date } into g
                            select new ApiDate
                            {
                                date = g.Key.Date,
                                count = g.Count(),
                            }).ToList();

                apiDates = apiDates.OrderByDescending(d => d.date).Take(7).ToList();

            }
            catch (Exception)
            {
            }

            return new AccessApiDateView()
            {
                columns = new string[] { "date", "count" },
                rows = apiDates.OrderBy(d => d.date).ToList(),
            };
        }

        public static AccessApiDateView AccessApiByHour()
        {
            List<RequestInfo> Logs = new List<RequestInfo>();
            List<ApiDate> apiDates = new List<ApiDate>();
            try
            {
                Logs = GetRequestInfo(ReadType.Prefix);

                apiDates = (from n in Logs
                            where n.Datetime.ObjToDate() >= DateTime.Today
                            group n by new { hour = n.Datetime.ObjToDate().Hour } into g
                            select new ApiDate
                            {
                                date = g.Key.hour.ToString("00"),
                                count = g.Count(),
                            }).ToList();

                apiDates = apiDates.OrderBy(d => d.date).Take(24).ToList();

            }
            catch (Exception)
            {
            }

            return new AccessApiDateView()
            {
                columns = new string[] { "date", "count" },
                rows = apiDates,
            };
        }
    }

    public enum ReadType
    {
        /// <summary>
        /// 精确查找一个
        /// </summary>
        Accurate,
        /// <summary>
        /// 指定前缀，模糊查找全部
        /// </summary>
        Prefix,
        /// <summary>
        /// 指定前缀，最新一个文件
        /// </summary>
        PrefixLatest
    }
}
