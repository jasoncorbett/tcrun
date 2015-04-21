# Introduction #

TCRunIJ is written as a command line tool.  That's not to say it is impossible for a GUI to be written, just that
it's not even on the roadmap yet.  If you can run basic commands on the command line, and understand how to
navigate a directory structure, you should do fine.

This part of the manual will deal with how to find tests, but will briefly overview how to get help on the command
line.

# Details #

## Getting Help on the command line ##

Open up the command prompt, change into the directory where you unzipped tcrunij.  To
get command line help, use the -h parameter:

![http://wiki.tcrun.googlecode.com/hg/images/tcrunij-help.png](http://wiki.tcrun.googlecode.com/hg/images/tcrunij-help.png)
**Note: this image may be out of date, always run it yourself to see the latest options.**

## Finding Tests ##

Almost all the functionality of TCRunIJ lives inside it's plugins.  Some plugins are core, and are always activated,
while others require command line options to activate them.  Locating the tests to run is done by plugins that
require command line options (with input) to activate them.  There are three options that will be detailed in this
manual:

  * **--id**: find a test by it's _id_
  * **-f** or **--filter**: locating one or more tests by way of a filter.
  * **-l** or **--list**: listing tests instead of running them

Another way to run a set of tests is by using a [Test Plans](TCRunIJManualPlans.md), but there is another section devoted
to that.

## Listing (instead of running) Tests ##

Often when you want to find out how many tests you would run, or which ones would be included in a run, you can use
the **-l** or **--list** command line option.  This performs the same initialization for tcrunij, but does not run the
tests.  Instead this plugin will just print each test to be run.

So if you want to test out your filters, or see if the test you're trying to run is found by your command line options
the easiest way is to use **-l**.  The list plugin also can provide you with more information about a test.

Examples:

### Listing all found tests ###
```
./tcrunij.sh -a -l
```
Output:
![http://wiki.tcrun.googlecode.com/hg/images/tcrunij-list-all.png](http://wiki.tcrun.googlecode.com/hg/images/tcrunij-list-all.png)

### Listing all tests in a group ###
```
./tcrunij.sh -f group:using-abstract -l
```
Output:
![http://wiki.tcrun.googlecode.com/hg/images/tcrunij-list-group-using-abstract.png](http://wiki.tcrun.googlecode.com/hg/images/tcrunij-list-group-using-abstract.png)


## Finding tests by id ##

TCRunIJ currently only runs [TCApi](TCRunIJTCApiManual.md) (a framework written specifically for tcrunij) tests.  Each
framework (more specifically the framework's test runner) can detail what the _id_ of a test is.  In the case of
TCApi (since it's a one test per class model), the id is the full class name (package name included).

If you only want to run a single test, the easiest way to do it is via the **--id** command line parameter.  You can
provide the id of the test you want to run to the **--id** parameter and it will be run.  You can repeat the parameter
to run multiple tests, but that may be sub-optimal and you may want to look at filters as a way to find multiple tests.

Below is an example of the --id parameter:

### Running one test ###
```
./tcrunij.sh --id org.tcrun.examples.simple.selenium2.SearchForIntuit
```
Output:
![http://wiki.tcrun.googlecode.com/hg/images/tcrunij-run-search-intuit.png](http://wiki.tcrun.googlecode.com/hg/images/tcrunij-run-search-intuit.png)

## Using Filters ##

Filters offer a way to find tests for inclusion or exclusion based on attributes of the tests.  For example tests can
be placed in _groups_ by the test writer, allowing the test runner to find them by group.  There are several standard
filters included with tcrunij:

  * **id**: for the id filter you need to provide the test cases id.  For tcapi tests the full
> > class name (package and class name) is the id.  To find the id, you could do a list of all
> > tests, which will print out the id's.  To do a listing of all tests do `./tcrunij.sh -a -l`.
  * **package** or **pkg**: The package filter will include any tests that start with the provided package name.
> > This includes any tests that are sub packages of the provided package.  For example the
> > filter `pkg:org.tcrun` will include "org.tcrun.FooBarTest" as well as
> > "org.tcrun.tests.functional.ReopenBrowserTest".
  * **group** or **g**: Tests can be annotated with `@TestGroup("groupname")` annotation, which
> > places them in arbitrary groups.  You can use the group filter to include tests from a
> > specific group.
  * **excludeid**: Exclude any **previously** included tests that match the provided id.
  * **excludepkg**: Exclude any **previously** included tests that are in the provided package or a sub package.
  * **excludegroup**: Exclude any **previously** included tests that are in the provided group.

### Listing all tests in a group ###
```
./tcrunij.sh -f group:using-abstract -l
```
Output:
![http://wiki.tcrun.googlecode.com/hg/images/tcrunij-list-group-using-abstract.png](http://wiki.tcrun.googlecode.com/hg/images/tcrunij-list-group-using-abstract.png)

### Listing all tests in a group, excluding one by id ###
```
./tcrunij.sh -f group:using-abstract -f excludeid:org.tcrun.example.usingabstract.SearchForSeleniumTwo -l
```
Output:
![http://wiki.tcrun.googlecode.com/hg/images/tcrunij-list-group-using-abstract-with-exclude.png](http://wiki.tcrun.googlecode.com/hg/images/tcrunij-list-group-using-abstract-with-exclude.png)
