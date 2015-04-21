# How To Write and Run a Basic Test Case in TCRun #

This short document will explain how to write and run a simple test case in C# using TCRun.
TCRun is not as much meant to be a full framework as it is to be a good runner interface
for functional test cases.  Right now there is a simple framework to allow you to write tests.

## Hello World Test Case ##

The goal: write a test that log's hello world and then passes.  Pretty simple right?  Fire up
your preffered C# IDE and let's get started.

### Create Your Test ###

First create a C# library project that references 2 dlls from the tcrun runtime directory.  The
2 dlls:

  * TCApi.dll from tcrun's lib directory
  * log4net.dll from tcrun's lib directory

Now for the code!

```
using QA.Common.TCApi;

namespace TCRun.Example
{
    [TCName("TCRun Example Test Case"),
     TCGroup("Example"),
     TCGuid("58b4dce3-e0ce-442b-bdcd-824cc9d7b5eb")]
    public class HelloWorldExampleTest : AbstractTestCase
    {
        public override TEST_RESULTS tcDoTest()
        {
            TCLog.Audit("Hello World from TCRun Example Test Case!");
            return TEST_RESULTS.PASS;
        }
    }
}

```

I didn't list all the "using" statements that you'll probably have as part of creating a standard C# file.  I only listed the one you will need to add.
TCRun uses an api called **TCApi** standing for Test Case Application Programming Interface.  TCApi is what provides all the running power for tcrun, in
fact tcrun is just a command line wrapper around TCApi.

After the namespace definition, I've Added some attributes to the class that our test is in.  These attributes describe your test.  TCApi tests are
usually one class per test case.  Although there are plans to allow tcrun to run other tests (such as NUnit tests) that can have more than one test per
test case.  For now you'll have to deal with one class per test case.  The attributes on this test give it a name (TCName), put it in a group (TCGroup),
and assign a unique GUID to it for identification.  You don't have to use any of these, but as you'll see when we go to run the test, they can help
in locating and running of your tests.  The name is used for reporting purposes.

This test class sub classes the AbstractTestCase from TCApi.  You don't have to do this, you can impliment an interface (ITestCase), however
AbstractTestCase provides some utilities and power to your test.  In particular it adds a logging framework, and some standard variables and functions.
We'll use those later to choose who we say hello to.

The only method in this test is tcDoTest, and it's the only one you HAVE to override (if you use AbstractTestCase).  There are 2 others, tcSetup and
tcCleanup, that are useful, but not for this basic of a test.  In the method you'll see that we issue a logging statement at the "Audit" level,
and then return a PASS as the result of the test.


### Give it to TCRun ###

Now you need to compile this test, creating a dll.  This is left as an excercise for the user, as it can be different depending on the IDE you are
using.

After you have the dll, copy it to the tests directory of tcrun.  Sometimes I add a post build event task that copies the dll for me to the tcrun
directory for automatic deployment of the test.


### Run the test ###

Up till now what you have seen is probably pretty standard, and possibly a little boring.  What comes next is the fun part.  TCRun is built with
a goal to make it as easy as possible to run the functional tests you want to run.  There are way more options than we will go into in this document,
but this will give you a good intro.  Another benefit to tcrun is that everything is intended to be self contained under the tcrun directory so
that passing off your tests to another person for running them is super easy.

Want to run your test, well do this:

```
tcrun TCRun.Example.HelloWorldExampleTest
```

Ok you ran it, and it's pretty easy to see how it chose which test to run.  Want to run it another way?  Ok here:

```
tcrun Example
```

Oh, yeah, you ran it by the group it's in.  That's real useful if you  have more than one test in a group, we don't.  What if you want to
do something weird like identify the test, even if you change the class name, or namespace name.  That can be useful if you have to
integrate with a test management system, and never want to have to update it.  Try this:

```
tcrun 58b4dce3-e0ce-442b-bdcd-824cc9d7b5eb
```

Yup, using the GUID works as well, and it's refactoring safe!

Where is the output, well you probably saw some when you ran tcrun, but did you know there was a log file created?  Yes there was, a
directory and log file just for your test.  Look in the results folder of tcrun.  The last test that ran always gets the output
put in the **last** directory.  There should be a sub directory for your test.  In that sub directory you'll see the default.log, with
a log statement from your test.

## Hello World Test Case Part 2 ##

Now that you've got some of the basics down, let's show what makes TCApi and tcrun different from some other frameworks.  Let's
say that you have 2 people you're going to demo tcrun to, and you want to make sure that tcrun says hello specifically to them.  Let's
say the first is named Sharon and the second Lee.  Should you have to write 2 tests that do the same thing except say hello to a
different person?  Nope, not with tcrun.  Let's make it a configuration, or more specifically an environment.  Environments (in tcrun)
are a collection of configuration parameters in an ini file that can help customize the settings for a test case.  This is useful if
you need to point to a different url or use a different account based on where your tests are running.

Here's how we'll customize the code:

```
using QA.Common.TCApi;

namespace TCRun.Example
{
    [TCName("TCRun Example Test Case"),
     TCGroup("Example"),
     TCGuid("58b4dce3-e0ce-442b-bdcd-824cc9d7b5eb")]
    public class HelloWorldExampleTest : AbstractTestCase
    {
        private string Person { get; set; }

        public override void tcSetup(Dictionary<string, string> configuration)
        {
            base.tcSetup(configuration);
            Person = configValue("SayHello.person");
        }

        public override TEST_RESULTS tcDoTest()
        {
            TCLog.AuditFormat("Hello {0} from TCRun Example Test Case!", Person);
            return TEST_RESULTS.PASS;
        }
    }
}

```

First we added a property to the test, Person of type string.  Next we added a new method: tcSetup.  In the base class AbstractTestCase there is already
an implementation of tcSetup, it assigns the configuration to a variable named tc\_info.  However if you try to access tc\_info with a key that isn't valid
all you get is an exception saying there was a missing key, not which key was missing.  So AbstractTestCase has a configValue method that will retrieve
the value if it exists, and throw an exception that contains the name of the missing key if the key doesn't exist.

The change to the tcDoTest method is simple, change the log message from Hello World to saying Hello to the person we're configured to say hello to.

### An Environment Configuration File ###

This is all good, but how do we get the name into the configuration file?  If you tried running the test after building the last code change you'll get
a result of Crash because it couldn't find "SayHello.person" configuration entry.  Now if you look in the conf directory of tcrun, you'll see default.ini.
TCRun has been using this file to get the configuration it passes to your test.  Since we want a more descriptive name than default, copy it to lee.ini and
add the following:

```
[SayHello]
person = Lee
```

This is a standard INI file, there are sections in brackets, etc.  Copy that last one to sharon.ini and replace Lee with Sharon.   You've now created
a "lee" environment and a "sharon" environment.  What may be different is how it is passed to the test case.  TCRun flattens out the ini file by
combining the section and key name with a dot into one key name.  So in the above example the person configuration under section SayHello is combined
into the key "SayHello.person" and passed as a flat dictionary to tests.

### Running with a different environment ###

How do you tell tcrun which environment you're in?  With the **-env** command line parameter.  Try this:

```
tcrun -env lee Example
```

I used the group to select the test case, but notice the -env lee part.  You don't point it to a file, tcrun expects all the environment
configurations to be in the conf directory.  You don't have to add the .ini, as tcrun only understands ini files (for now) so it adds it
for you.  Now if you check your results you should see a log message saying hello to Lee.  Try the same with Sharon.