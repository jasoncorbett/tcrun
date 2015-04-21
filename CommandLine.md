# Introduction #

To summarize tcrun's command line options, here is the output from the help:

```
.\tcrun.exe -h
Usage: tcrun [options] <-p plan|test.class.name|tcgroup>
Options:
	-l   | -list        List the tests instead of running them.
	-e   | -export      Export all the tests as a test plan.
	-v   | -version     Get the current version of tcrun.
	-h   | -help        Get command line help for this command.
	-s   | -save        Save the validation result as a resource.
	-d   | -debug       Set this run to be a debug run (enable debug logging).
	-env | -environment Set which environment ini file is loaded.
	-p   | -plan        The name of the test plan to load.
	-post              Post the results of this testing session
	-o   | -option      Options to be added to the configuration sent to test cases.
```

# Details #

## Finding and Running Tests ##

There are 2 ways in which you identify for tcrun which tests you want to run:

  1. command line arguments identifying the tests
  1. a [Test Plan](TestPlans.md)

To use a [test plan](TestPlans.md) you need to use the -p option and provide the plan name (minus the .txt filename extension).

To identify the tests on the command line, simply provide a test identifier for the test case.  This can be any one of (listed in order of precedence):

  1. TCGuid test case [attribute](Attributes.md)
  1. TCNumber  test case [attribute](Attributes.md)
  1. Full test class name (with namespace)
  1. TCGroup membership (managed by [attribute](Attributes.md))

## tcrun Options ##

There are several command line arguments which control the functionality of tcrun.  These include:

  * **-post** which causes results to be posted to [slick result database](http://code.google.com/p/slickqa)
  * **-d** which enables debug logging in the runtime.log (by default off)
  * **-l** which lists the tests that tcrun would run, instead of running them
  * **-h** which displays help, then exits
  * **-v** which displays tcrun version information, then exits

## test case options ##

The other parameters determine what get's passed to test cases.  Specifically:
  * **-env** which tells tcrun which ini file to load and pass to test cases (during tcSetup).  The ini files are stored in the conf directory, and when you specify one you should skip the .ini extension
  * **-o** which allows you to specify key=value options to add the to dictionary passed into tcSetup
  * **-s** which causes the [validation framework](ResourceValidation.md) to save the objects to be tested as resources.

## not implemented ##

The -e option is not implemented, and may be deprecated in a future release.