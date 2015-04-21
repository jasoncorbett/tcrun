# Introduction #

Environments are files that store test case configuration data in ini files.  They are called environments
because they usually contain data that is specific to an environment you are testing in.

# Details #

Consider a web app that you are testsing.  There are probably several different places it is deployed
that you need to test.  Maybe the developers have their local boxes, and a shared development environment.
Then maybe there is a server for QA testing and a Mirror of production, and finally the production
environment.  Many of these "environments" will have different databases, different user accounts,
different urls to base your tests on.  Rather than writing different tests with this data, we can separate
out this configuration into ini files, and then all the tests can share the same data.  That is what
environments are for.

## Format ##

Environments are plain ini files.  Some mapping is required because the test case configuration data
given to test cases is in a flattened key=value map.  TCRunIJ takes the section name from the ini
file and prepends it to the key with a '.' as a separator.  For example:

```
[URLS]
selenium=http://code.google.com/p/selenium
intuit=http://www.intuit.com
```

This ini file would be turned into java HashMap that has the following values:

  * **URLS.selenium** as the key, and http://code.google.com/p/selenium as the value
  * **URLS.intuit** as the key, and http://www.intuit.com as the value

## How to use ##

As mentioned in [Basic Command Line Options](TCRunIJBasicCommandLineOptions.md), you should place your
ini files in the **conf** subdirectory of the tcrunij base directory.  Then you can specify which
environment to use with the **-e** or **--environment** command line option.  You should not include
the .ini suffix, just the name of the file without the .ini.

For example, if I placed a file named **dev.ini** in the **tcrunij/conf/** directory, I would specify
I wanted to use the configuration with the following command line:

```
./tcrunij.sh -e dev
```

## Availability ##

It is up to the test runner to provide this configuration data to the test cases.  In the case of
the TCApi runner (included with TCRunIJ), this is passed into the function `tcsetup`.  However if you
are basing your test on the `AbstractSimpleTestCase` or one of it's sub classes (like `AbstractSeleniumTest`),
you can use one of the `configValue` methods to get the configuration data you want.  For more
information see [TCApi's AbstractSimpleTest Documentation](TCRunIJTCApiAbstractSimpleTest.md).