# NWinLog
simple logger for windows

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

### Setup
build and copy NWinLog.dll

### initialize
```
WinLogger.Initialize();
```

### use
textlog
```
WinLogger.WriteLine("log mesage");
WinLogger.WriteLine(LOG_LEVEL.TRACE, "log mesage");
```
eventlog
```
WinLogger.WriteEvent("event message");
WinLogger.WriteEvent(LOG_LEVEL.TRACE, "event mesage");
```
