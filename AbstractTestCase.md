# Introduction #

Although you only have to implement ITestCase to make a test case there are a large amount of advantages for inheriting from AbstractTestCase.  So many, that it's rare that the creators of the framework don't inherit.  Inheritance is one of the advantages to using a rich language runtime.

A summary of benefits include:
  * Logging to a test case specific file by default
  * Creation of screenshots enabled by default
  * an output directory created by default
  * a default handler for unexpected exceptions, and expected ones (like assertions)
  * default implementations of `tcSetup` and `tcCleanup`
  * validation framework included
  * assertions based on NUnit Constraints

# Details #

## Properties ##

### TCLog ###

`TCLog` is an implementation of `ITestCaseLogger` which extends log4net's ILog.  What does that mean?  It means that it is a logger implementation that has a few extra methods.

  * **New log level Audit**  The idea is that any log message that deals with critical proof of a test passing or failing should be handled by an "Audit" log message.  This aids in separating the log "noise" from important log statements.
  * **logScreenShot** A method designed to allow easy creation of screen shots.  You can pass in a name for the screen shot, or just let the framework name it (ScreenShot1.png, ScreenShot2.png, ...)
  * **logFile** Methods to allow copying files to the output for diagnosis later.  **THIS IS NOT YET IMPLEMENTED**

### ResourceFactory ###

A little known gem in the framework.  The resource framework allows you to associate files with your test case, and load them easily in your test.  This framework is used by the validation framework discussed later.  The methods are fairly simple.  Example:

```
  IResource foo = ResourceFactory.loadResource(this.GetType(), "foo.txt");
```

See ResourceFramework for more inforamation.

### Check ###

You can use NUnit's Assert class to perform assertions, and any assertion fails will result in a failed test case.  However, often in a functional testing environment you want _proof_ that your test got the correct values, or achieved certain states.  This is where the Check variable comes in.  The `Check` property has a copy of the NUnit `Assert.That` methods ([documentation](http://nunit.org/index.php?p=constraintModel&r=2.5.5)).  So why would you use `Check.That` instead of `Assert.That`?  For NUnit, the expected and actual values only get logged on failure, however for tcrun expected and actual get logged on success as well.

For more about how to use the asserts in tcrun, see [NUnit Asserts for your tests](NUnitAssertions.md).

## Instance Variables ##

### tc\_info ###

In the tcSetup method (which get's called first), a parameter of type `Dictionary<String, String>` is included.  If you call AbstractTestCase's base tcSetup implementation this will get set to tc\_info variable.  This contains information from the command line, and the environment ini file.  It can help you parameritize your tests to make them more flexible.

For proper use of getting values out see the `configValue` method below.

## Methods ##

### configValue ###

The `configValue` method get's an entry from the tc\_info dictionary.  The reason you would want to use it over just referencing the value, is for error reporting.  If the configuration entry is not in tc\_info, configValue will throw an exception detailing what the missing key was, whereas the MissingKey exception won't.  This can be helpful when trying to understand why your test crashed.

### resource ###

The resource method takes one parameter (a resource name) and returns a `IResource` object.  It is a shortcut for

```
    ResourceFactory.loadResource(this.GetType(), name);
```

### validate ###

The validate method returns a boolean, and is overloaded to provide defaults for several parameters.  You can provide options to a `IValidator` by passing in a dictionary of string to string mappings of key/value options.  The validate method automatically logs the messages from the `ValidationResult`.  Also with the -s option from the [Command Line Options](CommandLine.md) you can save the object to the resource file (seeding your test data).  Here are the overloaded method signatures:

```
       public bool validate(Object result, Type validator)
        {
            return validate(result, validator, new Dictionary<String, String>(), "result");
        }

        public bool validate(Object result, Type validator, Dictionary<String, String> options)
        {
            return validate(result, validator, options, "result");
        }

        public bool validate(Object result, Type validator, String resource_name)
        {
            return validate(result, validator, new Dictionary<String, String>(), resource_name);
        }

        public bool validate(Object result, Type validator, Dictionary<String, String> options, String resource_name)
```

The purpose of the validation framework is to provide a pluggable way to validate objects.  Probably the most used validator is the `SerializableValidator` class, which can validate any serializable object against a saved value.  See [ResourceValidation](ResourceValidation.md) for more details.  Below is a code example using the `SerializableValidator`:

```
    [TCName("Test the SerializableValidator"),
     TCGroup("TCApi"),
     TCGroup("TCApi/Validator"),
     TCGuid("58dd6519-e8d9-461d-91a6-3b5f0b480282")]
    public class SerializableValidatorTest : AbstractTestCase
    {   
        [Serializable,
         DataContract(Name="SerializationTest")]
        public class SerializationTest
        {   
            [DataMember]
            public String Name;
            [OptionalField]
            public String Description;
            [DataMember]
            public int Number;
            [DataMember]
            public Guid AGuid;
        }   

        private SerializationTest original;
        private SerializationTest identical;
        private SerializationTest propertyMissing;
        private SerializationTest propertyChange;


        public override void tcSetup(Dictionary<string, string> configuration)
        {   
            base.tcSetup(configuration);

            original = new SerializationTest();
            identical = new SerializationTest();
            propertyMissing = new SerializationTest();
            propertyChange = new SerializationTest();

            original.Name = "Foo";
            identical.Name = "Foo";
            propertyMissing.Name = "Foo";
            propertyChange.Name = "Bar";

            original.Description = "Description\r\nOf\r\nA\r\nSerialized\r\nTest\r\n";
            identical.Description = "Description\r\nOf\r\nA\r\nSerialized\r\nTest\r\n";
            propertyMissing.Description = null;
            propertyChange.Description = "Description\r\nOf\r\nA\r\nSerialized\r\nTest";

            original.Number = 247;
            identical.Number = 247;
            propertyMissing.Number = 247;
            propertyChange.Number = 248;

            original.AGuid = new Guid("77c4832b-0044-4326-a61a-1360a194fdd2");
            identical.AGuid = new Guid("77c4832b-0044-4326-a61a-1360a194fdd2");
            propertyChange.AGuid = Guid.NewGuid();
            propertyMissing.AGuid = new Guid("77c4832b-0044-4326-a61a-1360a194fdd2");
        }

        public override TEST_RESULTS tcDoTest()
        {
            TCLog.Audit("Validating original object");
            if (!validate(original, typeof(SerializableValidator)))
            {
                TCLog.Audit("TEST FAILURE: validation of original object failed.");
                return TEST_RESULTS.Fail;
            }

            if (!SaveValidation)
            {
                // we only want the original to be saved, not any of these
                TCLog.Audit("Validating identical object (identical properties)");
                if (!validate(identical, typeof(SerializableValidator)))
                {
                    TCLog.Audit("TEST FAILURE: validation of identical object failed!");
                    return TEST_RESULTS.Fail;
                }
                TCLog.Audit("Validating object with differences.");
                if (validate(propertyChange, typeof(SerializableValidator)))
                {
                    TCLog.Audit("TEST FAILURE: validation of changed object succeeded!");
                    return TEST_RESULTS.Fail;
                }
                TCLog.Audit("Validating object with empty GUID.");
                if (validate(propertyMissing, typeof(SerializableValidator)))
                {
                    TCLog.Audit("TEST FAILURE: validation of empty guid object succeeded!");
                    return TEST_RESULTS.Fail;
                }
            }
            return TEST_RESULTS.Pass;
        }
    }
```

This is a test of the validation framwork from the standard tcrun tests.