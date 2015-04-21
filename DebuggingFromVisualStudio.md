# Introduction #

When debugging your test case code, pausing the program and using all the debugging features of Visual Studio such as stepping can be very valuable.


# Details #

Note: You may need to add a sleep at the beginning of your test case if you want to catch your test before it begins.
```

// pause for 15 seconds
System.Threading.Thread.Sleep(15000);
```
  * Add a sleep if necessary to the beginning of your test
  * Start your test using tcrun.exe from the command line
  * In Visual Studio click the Debug menu | click Attach To Process | click tcrun.exe in the Available Processes list | click the Attach button

Visual Studio is now attached to the tcrun.exe process which means you are free to pause it and start stepping through your code.