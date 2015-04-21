# Introduction #

The validation framework in tcrun is put in to make record and validate type of tests easy.  These are particularly useful when testing an api, or output from a program.  It allows you to record the output, validate it manually, and make sure that each time the same input provides the same output.  There are several standard validators:

  * **XmlValidator**: compare 2 different xml files for similarities and differences (uses [xmldiffpatch api](http://msdn.microsoft.com/en-us/library/aa302295.aspx#xmldif_topic2))
  * **SerializableValidator**: serializes an object to a temp file, then does a xmldiffpatch compare against the saved version (resource).
  * **GuidListValidator**: compares a list of guids against a saved one

Plus you can define your own.  The most common usage of the validation framwork is from the validate method in [AbstractTestCase](AbstractTestCase.md).  It's also the simplest way to use it.

# Details #

A validator returns a `ValidationResult` object, which says if the validation worked, and what messages (error or success) it had.  These are there so they can be easily logged.  The most commonly used validator is the `SerializableValidator`.  It is an extension of the `XmlValidator`.

## The validator method ##

In [AbstractTestCase](AbstractTestCase.md) there is a method called validate, which is overloaded with defaults.  It returns true if validation succeeds and false if it does not.  It also logs the `ValidationResult` messages for you, so if something goes wrong (or right) you have a clue as to why.  You must pass into it an object to validate, and a validator type (a type object which implements the `IValidator` interface).  Example is included in [AbstractTestCase](AbstractTestCase.md).  The rest of this document goes into options of the existing validators, and how to write your own validator.

## Existing validators ##

### XmlValidator ###

The XmlValidator is a validator which compares a xml file against a resource file.  The object you pass in should be a `XmlDocument`.  It has several options and some default ones.

Default Options:
```
        public virtual XmlDiffOptions getOptionDefaults()
        {
            XmlDiffOptions retval = XmlDiffOptions.None |
                                    XmlDiffOptions.IgnoreComments |
                                    XmlDiffOptions.IgnorePI |
                                    XmlDiffOptions.IgnoreXmlDecl |
                                    XmlDiffOptions.IgnorePrefixes;

            return retval;
        }
```

This means that by default the xml validator won't compare comments, processing instructions, the xml declaration (at the top of the file), or prefixes.  You can customize the validator by passing in true or false to the following options that you can pass into the validator:

```
            Dictionary<String, XmlDiffOptions> all_options = new Dictionary<string, XmlDiffOptions>();
            all_options["IgnoreChildOrder"] = XmlDiffOptions.IgnoreChildOrder;
            all_options["IgnoreComments"] = XmlDiffOptions.IgnoreComments;
            all_options["IgnoreDtd"] = XmlDiffOptions.IgnoreDtd;
            all_options["IgnoreNamespaces"] = XmlDiffOptions.IgnoreNamespaces;
            all_options["IgnorePI"] = XmlDiffOptions.IgnorePI;
            all_options["IgnorePrefixes"] = XmlDiffOptions.IgnorePrefixes;
            all_options["IgnoreWhitespace"] = XmlDiffOptions.IgnoreWhitespace;
            all_options["IgnoreXmlDecl"] = XmlDiffOptions.IgnoreXmlDecl;
```


### SerializableValidator ###

The serializable validator serializes an object to xml, then uses the xml validator.  It adds the option IgnoreChildOrder to the set of standard options.

## Defining your own validators ##

You can define your own validators if the ones included don't suite your needs.  The `IValidator` interface and `ValidationResult` class are defined below:

```
    public interface IValidator
    {   
        ValidationResult validateResource(IResource p_resource, Object p_against, Dictionary<String, String> p_options, bool save);
        String extension();
    }   

    public class ValidationResult
    {   
        public bool Valid;
        public IList<String> Messages;

        public ValidationResult()
        {   
            Valid = false;
            Messages = new List<String>();
        }   
    }   
```