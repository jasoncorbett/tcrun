# Introduction #

If you've run a lot of tests, you want more than a commandline summary don't you?  You probably want to show some
fancy graph that makes your manager happy, and somewhat proves you did some work.  As with most test runners tcrunij
has the ability to generate a html report.  It's very pretty, comes with on page filters, and links to your diagnostic
information (like logs and screenshots).  There are some tradeoffs with having such a fancy page, but hey it is fancy.

# Details #

To generate a html report, you use the command line option `--html-report`.  You'll get the default page name and title,
but if you want to customize the title of the report just add your title as an option to `--html-report`.  For instance
if I wanted to name my report _My TPS Report_, then I would use the command line `--html-report "My TPS Report"`.

There are 2 parameters of information included on the screen, the **Test Plan** name and the **Environment**.  The test plan
is set to the value of the `-p` argument, if one was included.  The environment is set to the value of `-e` or  `--environment`
if one was given.

# Output #
The output html, and json (explained later in this document) is placed in the results directory for the test run.  If you
are using linux or mac, it is probably in your `results/last` directory.

A word about viewing this report, you need to do it over a http connection, in other words you need to store the html report
on a web server.  The reason is the ajax call to get all the results is done using xmlhttprequest, which can only load
over http, not from the file system.

This may change in the future, and there is a [bug](http://code.google.com/p/tcrun/issues/detail?id=72) in on it already,
but even if we could get the results to load, it is unlikely the same workaround would work for the other files (logs
and images).

# Example Report #

You can view an example report from the [build server](http://build.tcrun.org).  The latest artifacts include the latest
[integration test report](http://build.tcrun.org/job/tcrunij/lastSuccessfulBuild/artifact/tcrunij-integration/integration-runtime/tcrunij/results/last/index.html).