using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NWinLog
{
    public enum LOG_LEVEL {
        ERROR,
        WARNING,
        INFORMATION,
        TRACE
    };
    public static class WinLogger
    {
        static string _WarningLogFile = null;
        static string _ErrorLogFile = null;
        static string _InfoLogFile = null;
        static string _TraceLogFile = null;

        private static object GetRegVal(string namae) {
            object val = null;
            try {
                Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NWinLog\System");
                if(null == regkey) {
                    regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NWinLog\System");
                }

                if(null == regkey)
                    return null;
                val = regkey.GetValue(namae);
                regkey.Close();
            } catch(Exception exp) {
                WinLogger.WriteLine(exp);
            }
            return val;
        }
        private static void SetRegVal(string namae,object val) {
            try {
                Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NWinLog\System",true);
                if(null == regkey) {
                    regkey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SOFTWARE\NWinLog\System");
                }
                regkey.SetValue(namae,val);
                regkey.Close();
            } catch(Exception exp) {
                WinLogger.WriteLine(exp);
            }
        }
        private static string GetLogDir() {
            string AppLogPath = String.Format(@"{0}\{1}\{2}\log",
                Config.AppSettingsPath,
                Config.CompanyName,
                Config.ProductName
                );

            object obj = GetRegVal("logdir");
            if(null == obj)
                return AppLogPath;
            if(obj.ToString() == "")
                return AppLogPath;
            return obj.ToString();
        }
        private static string GetLogFile() {
            return GetLogDir() + "/NWinLog.log";
        }
        public static bool EnableTraceLog {
            set {
                int nval = value == true ? 1 : 0;
                SetRegVal("trace",(object)nval);
                WinLogger.TraceLogFile = value == true ? GetLogFile() : "";
            }
            get {
                object val = GetRegVal("trace");
                if(null == val)
                    return false;
                return (int)val == 1 ? true : false;
            }
        }
        public static bool EnableErrorLog {
            set {
                int nval = value == true ? 1 : 0;
                SetRegVal("error",(object)nval);
                WinLogger.ErrorLogFile = value == true ? GetLogFile() : "";
            }
            get {
                object val = GetRegVal("error");
                if(null == val)
                    return false;
                return (int)val == 1 ? true : false;
            }
        }
        public static bool EnableInformationLog {
            set {
                int nval = value == true ? 1 : 0;
                SetRegVal("info",(object)nval);
                WinLogger.InformationLogFile = value == true ? GetLogFile() : "";
            }
            get {
                object val = GetRegVal("info");
                if(null == val)
                    return false;
                return (int)val == 1 ? true : false;
            }
        }
        public static bool EnableWarningLog {
            set {
                int nval = value == true ? 1 : 0;
                SetRegVal("warning",(object)nval);
                WinLogger.WarningLogFile = value == true ? GetLogFile() : "";
            }
            get {
                object val = GetRegVal("warning");
                if(null == val)
                    return false;
                return (int)val == 1 ? true : false;
            }
        }
        public static void Initialize() {
            WinLogger.TraceLogFile = GetLogFile();
            WinLogger.ErrorLogFile = GetLogFile();
            WinLogger.InformationLogFile = GetLogFile();
            WinLogger.WarningLogFile = GetLogFile();

            bool IsEnable = EnableTraceLog;
            IsEnable = EnableErrorLog;
            IsEnable = EnableInformationLog;
            IsEnable = EnableWarningLog;

            object obj = GetRegVal("logdir");
            if(null == obj) {
                SetRegVal("logdir",GetLogDir());
            } else {
                if(obj.ToString() == "") {
                    SetRegVal("logdir",GetLogDir());
                }
            }
        }
        [DllImport("Logger.dll",CharSet = CharSet.Auto)]
        static extern void SetLogFile(ushort logLevel,string LogFile);
        [DllImport("Logger.dll",CharSet = CharSet.Auto)]
        static extern void WriteLine(ushort logKind,string message);
        static System.Threading.Mutex _mutex = new System.Threading.Mutex(false,"NWinLogMTX");
        public static void WriteLine(LOG_LEVEL lvl, string format,params object[] args) {
            try {
                _mutex.WaitOne();
                string logmsg = string.Format(format,args);
                WriteLine(lvl,logmsg);
            } catch(Exception exp) {
                WinLogger.WriteLine(exp);
            } finally {
                _mutex.ReleaseMutex();
            }
        }
        public static void WriteLine(LOG_LEVEL lvl,string logmsg) {
            try {
                _mutex.WaitOne();
                if(lvl == LOG_LEVEL.TRACE) {
#if DEBUG
                    System.Diagnostics.Trace.WriteLine(logmsg);
#endif
                }
                switch(lvl) {
                    case LOG_LEVEL.ERROR:
                    if(!EnableErrorLog)
                        return;
                    //WinLogger.ErrorLogFile = GetLogFile();
                    break;
                    case LOG_LEVEL.INFORMATION:
                    if(!EnableInformationLog)
                        return;
                    //WinLogger.InformationLogFile = GetLogFile();
                    break;
                    case LOG_LEVEL.TRACE:
                    if(!EnableTraceLog)
                        return;
                    //WinLogger.TraceLogFile = GetLogFile();
                    break;
                    case LOG_LEVEL.WARNING:
                    if(!EnableWarningLog)
                        return;
                    //WinLogger.WarningLogFile = GetLogFile();
                    break;
                }
                string logFile = "";
                switch(lvl) {
                    case LOG_LEVEL.ERROR:
                    logFile = _ErrorLogFile;
                    break;
                    case LOG_LEVEL.INFORMATION:
                    logFile = _InfoLogFile;
                    break;
                    case LOG_LEVEL.TRACE:
                    logFile = _TraceLogFile;
                    break;
                    case LOG_LEVEL.WARNING:
                    logFile = _WarningLogFile;
                    break;
                }
                if(!System.IO.Path.IsPathRooted(logFile))
                    return;
                System.IO.FileInfo chkf = new System.IO.FileInfo(logFile);
                if(!chkf.Directory.Exists) {
                    chkf.Directory.Create();
                }
                System.Diagnostics.Process ps = System.Diagnostics.Process.GetCurrentProcess();
                string wmsg = "[" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString() + "]" + " " +
                    "[" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + "]" + " " +
                    "[" + lvl.ToString() + "]" + " [" +
                    logmsg + "]";
                WriteLine((ushort)lvl,wmsg);
                object obj = GetRegVal("lotate");
                if(null == obj)
                    return;
                if((int)obj != 1)
                    return;
                long size = 1;
                obj = GetRegVal("logsize");
                if(null != obj) {
                    size = (int)obj;
                }
                System.IO.FileInfo fi = new System.IO.FileInfo(logFile);
                if(fi.Exists) {
                    if(fi.Length > (long)(size * 1024L * 1024L)) {
                        String status = String.Format("{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}",
                            DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,DateTime.Now.Hour,DateTime.Now.Minute,DateTime.Now.Second);
                        string fileNameBody = System.IO.Path.GetFileNameWithoutExtension(logFile);
                        System.IO.File.Copy(logFile,fi.DirectoryName + "/" + fileNameBody + "_" + status + ".log");
                        System.IO.File.WriteAllText(logFile,"");
                    }
                }
                int days = 14;
                obj = GetRegVal("rotation");
                if(null != obj) {
                    days = (int)obj;
                }
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(fi.DirectoryName);
                System.IO.FileInfo[] fis = di.GetFiles();
                foreach(System.IO.FileInfo fii in fis) {
                    string tmps = System.IO.Path.GetFileNameWithoutExtension(fii.Name);
                    string[] seps = tmps.Split('_');
                    if(seps.Length < 2)
                        continue;
                    String strtm = seps[1];
                    DateTime fdt;
                    if(DateTime.TryParseExact(strtm,"yyyyMMddHHmmss",System.Globalization.DateTimeFormatInfo.InvariantInfo,System.Globalization.DateTimeStyles.None,out fdt)) {
                        if(DateTime.Now - fdt > TimeSpan.FromDays(days)) {
                            fii.Delete();
                        }
                    }
                }
            } catch(Exception exp) {
                WinLogger.WriteLine(exp);
            } finally {
                _mutex.ReleaseMutex();
            }
        }
        public static void WriteLine(string logmsg) {
            try {
                _mutex.WaitOne();
                WriteLine(LOG_LEVEL.ERROR,logmsg);
            } catch(Exception exp) {
                WinLogger.WriteLine(exp);
            } finally {
                _mutex.ReleaseMutex();
            }
        }
        public static void WriteLine(Exception exp,LOG_LEVEL lvl = LOG_LEVEL.TRACE) {
            try {
                _mutex.WaitOne();
                string logmsg = exp.ToString() + "\n" + exp.Message + "\n" + exp.Source + "\n" + exp.StackTrace;

                if(lvl == LOG_LEVEL.TRACE) {
                    System.Diagnostics.Trace.WriteLine(logmsg);
                }

                WriteLine(lvl,logmsg);
            } catch(Exception) {
                ///
            } finally {
                _mutex.ReleaseMutex();
            }
        }
        public static void WriteLine(Exception exp) {
            WriteLine(exp,LOG_LEVEL.TRACE);
        }
        [DllImport("Logger.dll",CharSet = CharSet.Auto)]
        static extern void WriteEvent(ushort lvl,string message,string sourceName);
        public static void WriteEvent(LOG_LEVEL lvl,string format,params object[] args) {
            string logmsg = string.Format(format,args);
            WriteEvent(lvl,logmsg);
        }
        public static void WriteEvent(string logmsg,LOG_LEVEL lvl = LOG_LEVEL.TRACE) {
            if(lvl == LOG_LEVEL.TRACE)
                System.Diagnostics.Trace.WriteLine(logmsg);

            switch(lvl) {
                case LOG_LEVEL.ERROR: {
                    object obj = GetRegVal("errorevt");
                    if(null == obj)
                        return;
                    if(((int)obj) != 1)
                        return;
                }
                break;
                case LOG_LEVEL.INFORMATION: {
                    object obj = GetRegVal("infoevt");
                    if(null == obj)
                        return;
                    if(((int)obj) != 1)
                        return;
                }
                break;
                case LOG_LEVEL.TRACE: {
                    object obj = GetRegVal("traceevt");
                    if(null == obj)
                        return;
                    if(((int)obj) != 1)
                        return;
                }
                break;
                case LOG_LEVEL.WARNING: {
                    object obj = GetRegVal("warningevt");
                    if(null == obj)
                        return;
                    if(((int)obj) != 1)
                        return;
                }
                break;
            }
            WriteEvent((ushort)lvl,logmsg,"NWinLog");
        }
        public static void WriteEvent(string logmsg) {
            WriteEvent(LOG_LEVEL.ERROR,logmsg);
        }
        public static void WriteEvent(Exception exp,LOG_LEVEL lvl = LOG_LEVEL.TRACE) {
            string logmsg = exp.Message + "\n" + exp.Source + "\n" + exp.StackTrace;
            WriteEvent(lvl,logmsg);
        }
        public static void WriteEvent(Exception exp) {
            WriteEvent(exp,LOG_LEVEL.TRACE);
        }
        [DllImport("Logger.dll",CharSet = CharSet.Auto)]
        public static extern void MoveLog();
        public static string TraceLogFile {
            set {
                _TraceLogFile = value;
                SetLogFile((ushort)LOG_LEVEL.TRACE,value);
            }
            get {
                return _TraceLogFile;
            }
        }
        public static string WarningLogFile {
            set {
                _WarningLogFile = value;
                SetLogFile((ushort)LOG_LEVEL.WARNING,value);
            }
            get {
                return _WarningLogFile;
            }
        }
        public static string ErrorLogFile {
            set {
                _ErrorLogFile = value;
                SetLogFile((ushort)LOG_LEVEL.ERROR,value);
            }
            get {
                return _ErrorLogFile;
            }
        }
        public static string InformationLogFile {
            set {
                _InfoLogFile = value;
                SetLogFile((ushort)LOG_LEVEL.INFORMATION,value);
            }
            get {
                return _InfoLogFile;
            }
        }
    }
}
