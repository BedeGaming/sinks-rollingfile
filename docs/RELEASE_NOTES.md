### 2.0.7 (Released 2017/01/26)
* Fixed bug where files weren't rolling over at midnight
* Fixed bug where hourly sink couldn't log if the file was locked
* ITextFormatter support added for both sinks

### 2.0.5 (Released 2016/11/02)
* Unsealing HourlyRollingFileSink to allow for extension

### 2.0.3 (Released 2016/07/13)
* Fixing package not containing dlls when built in TC

### 2.0.2 (Released 2016/07/12)
* Fixed System.Text.RegularExpressions references for earlier framework versions

### 2.0.1 (Released 2016/07/12)
* Fixed System.IO references for earlier framework versions

### 2.0.0 (Released 2016/07/08)
* Migration to work with .Net Standard 1.6

### 1.3.0 (Released 2015/09/28)
* Hourly rolling log files

### 1.2.0 (Released 2015/08/28)
* if sink can't get write access to file it now chooses next sequence number

### 1.1.0 (Released 2015/06/23)
* adds configurator for new sink
* removes filename configuration, only log folder is configurable now

### 1.0.1 (Released 2015/06/16)
* initial release
