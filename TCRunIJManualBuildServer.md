# Introduction #

You can integrate tcrunij to run as part of any build process.  Since it's a command line tool
this may seem obvious.  This page will help provide pointers to make the integration smoother
and you're tests a little more reliable.

# Which Browser #

If you are using the [tcapi-selenium](TCRunIJTCApiSeleniumManual.md) module to create browser based
tests, you may ask which browser should you use?  There is a HTMLUnit based headless browser,
however it can have issues with some sites and some javascript.

If your build machine runs on linux, and firefox is one of your supported browsers, you're in
luck.  There is an easy way to run firefox in a headless mode on linux.  You'll need the Xvfb
X11 server, and a utility called xvfb-run.  This will allow you to run one command in a "virtual"
X server (graphical environment on linux).  We use this on http://build.tcrun.org to build
tcrunij.

## ffwin ##

If your application only supports firefox on windows, you can easily make your firefox on linux
look like it's coming from windows.  Just use the browser type `ffwin` instead of `ff`.  This
will replace the default user agent string to look like firefox on windows.

# Test Results #

Most continuous integration build servers have support for parsing JUnit XML output.  TCRunIJ
is capable of producing JUnit XML output for this purpose.  Just use the `--junit-report`
command line option, and then a TEST-results.xml file will be included in your test output
directory.  It will also contain all the configurations from the test run.

Also consider including the [html report](TCRunIJManualHTMLReport.md) from the run to allow for
easy debugging afterwards (it includes logs and screenshots).