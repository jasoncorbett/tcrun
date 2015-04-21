Logging in tcrun is handled by the [log4net](http://logging.apache.org/log4net/index.html) library.  What makes this a little different is that tcrun configures logging programmatically since logging needs to be handled in some special ways.

Here is what happens:

  1. first when tcrun starts up, it moves the results\last folder to results\[date](date.md) with the date being the creation date of the results\last folder
  1. next the results\last folder is recreated
  1. all logging goes to a log file called results\last\runtime.log
  1. every time a test case accesses TCLog (part of [AbstractTestCase](AbstractTestCase.md)) for the first time, tcrun creates an output folder for the test, and places a log file default.log in it.  All logging for the test case goes to default.log

The names of the folders tcrun creates for the test case are as follows:

```
  [number of when the test was run]-[test class name]-[result of test]
```

Actually after the test is finished, the log file for the test is closed, and then the folder is moved to the naming convention above.  While the test is running the folder is named without the result of the test.