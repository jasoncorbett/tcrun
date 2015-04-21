# Introduction #

Logging is a very important aspect of automated tests. sometimes it is the only way to
determine why a test failed, or why a test passed.  Pre-configured logging is one of the
key features of tcrunij, making your life easier.

There are 2 default logs that are in use at any one time, the "runtime" log file, and
the "testcase" log file.  Logging configuration is held in the **conf** directory and is
called `logging-config.xml`.

# Details #

TCRunIJ uses [SLF4J](http://slf4j.org) as it's logging api of choice, and [logback](http://logback.qos.ch/)
as the chosen implementation.  SLF4J could be redirected to a different logging
implementation, but you may loose out on some of the structuring of the output.

## Which log files where ##

All logs go to the "testrun.log" file by default.  On startup a "test run id" is
determined by either creating one by timestamp or by using the value of the TESTRUNID
system environment variable (if it exists).  Under the results directory a directory
is created by the logging system for the test run id, and underneath that all logs
go to the testrun.log.

In addition, before a test case runs, a test case directory is determined, where the
test case log will be stored.  If the test case never logs, the directory is not created.
The directory for the test case and it's logs is determined like this (with the [.md](.md) and
everything inbetween replaced by the value of the described data):

```
results/[test run id]/[number of the order in which the test was run]-[test case id]/
```

The default name of the log file for each test is **test.log**.  **Logging statements which
have logger names that start with "test." will all go to the test case log file.**