# Introduction #

NUnit Assertion checks are supported in tcrun in 2 ways:

  1. First, an assertion failure will by default cause a Fail of a test case instead of a Crash.  This means you can use NUnit's Assert class and any failures will get logged and cause a failed test.
  1. Second, a copy of NUnit `Assert` class's That methods (which deal with the [constraint model](http://nunit.org/index.php?p=constraintModel&r=2.5.5)), are included in a property of AbstractTestCase called `Check`.  Assertions with `Check` will log on success as well as failure.

# Details #
The most common and recommended way to use NUnit assertions inside of a tcrun test case is to use the `Check` variable.  This allows use of NUnit's new Constraint model of testing ([NUnit Documentation](http://nunit.org/index.php?p=constraintModel&r=2.5.5)).  The `Check` property of AbstractTestCase will log successful checks as well.  It only has a copy of the `Assert` class's That methods.

Consider the following code, and how much the assertion model simplifies things.  First without asserts:

```
    if (expected == actualValue)
    {   
        TCLog.AuditFormat("PASS: The expected value of {0} is what was found.", actualValue);
        return TEST_RESULTS.Pass;
    }   
    else
    {   
        TCLog.AuditFormat("FAIL: The value was expected to be {0}, but was {1}.", expected, actualValue);
        return TEST_RESULTS.Fail;
    }   
```

Now with asserts:

```

    Check.That(expected, Is.EqualTo(actualValue));
    return TEST_RESULTS.Pass;
```

So what goes in the log on a successful constraint check?  The following is a log message from the tcrun tests that are run as part of the build process:

```
    [Thu, July 22 2010 10:01:51 AM,AUDIT,DoTest]: SUCCESSFUL CHECK:   Expected: String matching "tcrun version: \d\.\d\.\d\..*"   Actual:  "tcrun version: 1.0.0.SNAPSHOT-1279814492"
```

# How to add them to your project #

To be able to use the new asserts, you need to do 2 things:

  1. Add a reference to the nunit.framework.dll in the lib directory of tcrun
  1. Add a `using` statement for the namespace `NUnit.Framework`

example:

```
using NUnit.Framework;
```