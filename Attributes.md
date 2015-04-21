# Introduction #

There are currently 4 attributes which have special use when creating tests:

  * **TCName**: define a name for your test, otherwise the class name is used as the name.  The name is used when "post"ing results to the [slick results database](http://code.google.com/p/slickqa). Also it is used when listing test cases using the -l [Command Line Option](CommandLine.md).
  * **TCGroup**: You can provide multiple TCGroup attributes, they are used for organizing your tests into groups.  Groups can be used to run a set of tests, both on the [Command Line](CommandLine.md), but also in [Test Plans](TestPlans.md).
  * **TCGuid**: You can assign a Guid to your test.  This is rarely useful but is available.  It is left over from before using a test case's class name was available from the [Command Line](CommandLine.md) or [Test Plans](TestPlans.md).  It can uniquely identify a test.
  * **TCNumber**: This allows you to specify a number (presumably a test case id from some tracking system) to identify your test.  It can be used from both the [Command Line](CommandLine.md) and [Test Plans](TestPlans.md).

# Details #

## TCName ##

The TCName attribute accepts a string that can provide a nice display name for your test.  It also provides nice in code documentation for your test.  The attribute is used when "post"ing results to the [slick results database](http://code.google.com/p/slickqa). Also it is used when listing test cases using the -l [Command Line Option](CommandLine.md).

An example:

```

    [TCName("TCApi: configValue throws exception (with config key) if config key missing"),
     TCGroup("TCApi")]
    public class ConfigValueTest : AbstractTestCase
```

You can see how this makes for a nice one line description for someone else looking at the test.  It is common practice to put a group or feature name at the beginning with a colon afterwords.

## TCGroup ##

Probably one of the most useful attributes.  This allows you to create adhoc group memberships by decorating your code.  Multiple `TCGroup` attributes can be included for a single test.  An example:

```

    [TCName("TCApi: Logger adds .png if missing"),
     TCGroup("TCApiExtended"),
     TCGroup("TCApiWindowsOnly")]
    public class LogScreenShotPngAdded : AbstractTestCase
```

In this example there are 2 groups on this test.  This example comes from the TCApiTests project that tests standard tcrun.  Because screen shots don't appear to work on mono in linux or mac, this test is given a `TCApiWindowsOnly` group and a `TCApiExtended`, but not the `TCApi` group.  That way it's not run on the build machine, but can easily be run from windows doing:

```
  .\tcrun.exe TCApiWindowsOnly
```

## TCGuid ##

The `TCGuid` attribute was created because originally with tcrun you couldn't use a class's name to load a test, instead you would add a `TCNumber` attribute and use the number.  However we had some tests that were in 2 different test tracking systems, and hence had to different numbers.  We wanted a way to uniquely identify a test, hence we applied a guid.  Later we added the ability to use the test class name, which is much more memorable and descriptive in a [Test Plan](TestPlans.md) than a Guid, but it remains in case you have a use for it.

A [bug](http://code.google.com/p/tcrun/issues/detail?id=11) was recently fixed with the TCGuid attribute.  Now if you provide an invalid GUID, then that guid is ignored.

Example use:

```
        [TCNumber(5413),
         TCGuid("6f47dd3a-9f4b-44ef-92fa-dcc4a4283793")]
        public class GetAllContactsTest : AbstractTestCase
```

## TCNumber ##

The `TCNumber` attribute is useful for identifying a test by it's number which may be the id of a test from a test tracking system.  Imagine you had such a system, then the command line for running any test would be `tcrun [number]`, making it easy to locate the test based on a number.

Example use:

```
        [TCNumber(5413),
         TCGuid("6f47dd3a-9f4b-44ef-92fa-dcc4a4283793")]
        public class GetAllContactsTest : AbstractTestCase
```