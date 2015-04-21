# Introduction #

Test Plans are lists of filters that are used in order.  They allow you to group
a set of tests in order in a repeatable fashion.  Plan files are plain text, and should have the
extension .txt, but you shouldn't provide the extension on the command line.  Plan files go in the plan
directories, and are specified with the **-p** or **--plan** options.


# Details #

Many times while testing you want to make sure that everyone that runs a set of tests is running the same
set.  Plan files offer a way to put multiple filters in a specific order, and record that in a file.  Then
everyone who has access to that file can run the same tests in the same order.  This makes test runs
with tcrunij more predictable.

## Format ##

Plan files are plain text files with the _.txt_ filename extension.  They should be placed in a
subdirectory of tcrunij named **plans**.  The format can be summed up to the following rules:

  * Any line starting with a "#" mark is a comment
  * Empty lines are ignored
  * All other lines should contain a valid filter

**Note: If a line has an invalid filter, or a filter which does not produce any tests, it is ignored.**

For information on filters, see [Finding Tests](TCRunIJManualFindingTests.md).

## Order ##

One of the most important features of test plans is the ability to specify order.  This can also
affect exclusive filters.  A Test Plan is evaluated from top to bottom, in order.  Each line
that contains a filter is evaluated against a list of tests to run and a list of all tests found.

If the filter is an inclusive filter, it will add to the list of tests to be run from the list of
all tests found.  An exclusive filter (ones that start with exclude) will remove matching tests
from the list of tests to be run.  So exclusive filters affect everything **above** them in the
plan file, and nothing below.  If you want an exclusive filter to be _global_ (meaning against
the entire plan) put it at the end of the file.

Because of the way plan files are evaluated, and the fact that currently there is only a single
threaded test runner, you can be assured that all tests found in one line of the plan will finish
before all the tests in the next line.  However, **there is no garuntee of order within a filter!**
A filter can add tests in whatever order it wants to the list.  Technically a filter could
change the order of what other filters before it put in the list, but it is unlikely that any
core filter in tcrunij will ever do this.

## Example ##

Given the following list of tests (obtained by running `./tcrunij.sh -a -l`):

```
1 - id:org.tcrun.example.usingabstract.SearchForSeleniumTwo
    name:Google Search: Search for selenium 2.0
    group:using-abstract
2 - id:org.tcrun.example.usingabstract.SearchForWebDriver
    name:Google Search: Search for web driver
    group:using-abstract
3 - id:org.tcrun.example.usingabstract.SearchForTCRun
    name:Google Search: Search for tcrun
    group:using-abstract
4 - id:org.tcrun.examples.simple.selenium2.SearchForIntuit
------------------------------------------------------------
Total: 4 test(s).
```

And the following plan file named "plans/usingabstract.txt":

```
# A test plan which specifies the "usingabstract" tests
# Hash marks at the begining of a line are comments
pkg:org.tcrun.example.usingabstract
excludeid:org.tcrun.example.usingabstract.SearchForSeleniumTwo
```

And ran the following command line:

```
./tcrunij.sh -l -p usingabstract
```

The output would be:

```
1 - id:org.tcrun.example.usingabstract.SearchForWebDriver
    name:Google Search: Search for web driver
    group:using-abstract
2 - id:org.tcrun.example.usingabstract.SearchForTCRun
    name:Google Search: Search for tcrun
    group:using-abstract
------------------------------------------------------------
Total: 2 test(s).
```
