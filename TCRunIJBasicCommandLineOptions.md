# Introduction #

TCRunIJ is designed for running from the command line.  All options are available via it's help option:

![http://wiki.tcrun.googlecode.com/hg/images/tcrunij-help.png](http://wiki.tcrun.googlecode.com/hg/images/tcrunij-help.png)

# Details #

## Environments ##

Environments are simply ini files that contain configuration information for your tests.  Particularly
when testing web apps (although the concept applies more broadly) you will probably have a
"test environment" and a "production environment".  URLs, usernames, and passwords are all examples
of data that may change depending on where you need to run the tests.

With tcrunij just put an ini file in the **conf** directory and specify it when you run the tests.
For example, if you were to use the "dev" environment, you would put a "dev.ini" in the conf
directory.  Example content of the dev.ini:

```
[URLS]
base=http://localhost/application
```

Then in your test you can reference that value.  If you have subclassed **org.tcrun.tcapi.AbstractSimpleTestCase**
you can then reference values using the configValue method:

```
private String baseUrl = null;

@Override
public void setup() throws Exception
{
	baseUrl = configValue("URLS.base");
}

```

The request and response is logged in the test case log, and if there is no URLS.base an intelligent
exception detailing the missing configuration key is thrown.

## Filters ##

For deciding which tests to run, filters are used.  The following is an example of how to use filters,
and then a list of which filters are currently implemented.

### Using filters ###

Filter's are easy to use and all the currently implemented filters have a similar syntax:

```
<filter prefix>:<filter value>
```

For example if you wanted to run the test with the id "org.tcrun.tests.functional.ReopenBrowserTest"
(for tcapi tests the class name is the id) you would use the command line:

```
./tcrunij.sh -f id:org.tcrun.tests.functional.ReopenBrowserTest
```

### Currently Implemented Filters ###

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

## Plans ##

[Test Plans](TCRunIJManualPlans.md) are lists of filters that are used in order.  They allow you to group
a set of tests in order in a repeatable fashion.  Plan files are plain text, and should have the
extension .txt, but you shouldn't provide the extension on the command line.  Plan files go in the plan
directories, and are specified with the -p or --plan options.  For example if I created the file
"plans/foo.txt" with the following content:

```
# A test plan which specifies the "foo" tests
# Hash marks at the begining of a line are comments
pkg:org.tcrun.foo
excludeid:org.tcrun.foo.BarTest
```

And ran the following command line:

```
./tcrunij.sh -p foo -l
```

I would get a list of all tests that tcrunij would run from the foo.txt plan file.  This would include
all tests that are in the package "org.tcrun.foo" or a sub package of it, except for the org.tcrun.foo.BarTest.

## Test Steps ##

In tcrunij, tests can have steps, which are a documentation of what the test is attempting to accomplish.
Tests which implement the org.tcrun.api.TestWithSteps can have those steps output after the test runs.  The
abstract class org.tcrun.tcapi.AbstractSimpleTestCase implements this interface and provides the method **step**
to allow you to document your test's steps as it is executing.  These steps are printed to the log file, and
could potentially be reported to a test case tracking system.

To see the steps output as you're running tests, use the `--print-steps` option, and after each result is
a list of steps and their expected result (if one is given, this is an optional parameter to the step
method).