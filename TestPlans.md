# Introduction #

Test plans offer a repeatable way to run a set of tests in order.  They are simple text files with each line being a test identifier.  Comments are also allowed and provide a way for you to document what and why certain tests are in a test plan.  Test plans are the most common way of running a large number of tests.  They can help ensure that everyone who needs to run a set of tests run the same ones in the same order.

# Details #

In the plans subdirectory of tcrun, you can place plain text files.  These files can contain an ordered list of test cases.  You can identify the tests the same as you do on the [command line](CommandLine.md):

This can be any one of (listed in order of precedence):

  1. TCGuid test case [attribute](Attributes.md)
  1. TCNumber  test case [attribute](Attributes.md)
  1. Full test class name (with namespace)
  1. TCGroup membership (managed by [attribute](Attributes.md))

You can also place comments in the file by placing a `#` as the first character of a line.  Below is an example of a test plan:

```
# Below is the TCApi group of tests, which include all the tests used to test tcrun itself
TCApi

# If you're on windows you can uncomment the following
#WindowsOnly
```