# Introduction #

Sometimes test cases need more than just code.  They might need data files to help define what should be tested.  One example is in using validation of resources.  Imagine you're testing an API, and you just want to verify that what was returned by a particular call last week is the same now.  In this scenario you can save the output of a call to a file (or resource), then load it in a future testing session for comparison.  This is what the ValidationFramework does, and it uses Resources to accomplish that task.


# Details #

## How it works ##

Resources are defined by environment name, and test class name.  In your ini file (environment ini file, see [Command Line Options](CommandLine.md)) you can define an environment name by including a section called `Environment` with a key called `name`.  This is how you define an environment name.  If you don't, the environment name is defaulted to the name of the ini file.

Under the tcrun directory, there is a directory called resources.  In there is where all loadable resources are stored.  The filesystem hierarchy will be:
```
  resources\environment name\full.test.class.name\resource file name
```

Any file inside the directory with the name of the test class's full name (with namespace) can be loaded as a resource. A `IResourceFactory` is automatically put inside the AbstractTestCase, so if you inherit from it you can use the property `ResourceFactory`.  But it's even easier than that if you've subclassed AbstractTestCase, just use the resource method, which you only have to supply the filename to.  The result of loading a resource is an object which implements the `IResource` interface.

## `IResource` ##

The `IResource` interface is defined as follows:

```
    /// <summary>
    /// A resource is content that the test needs to use.
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// A location is a string based identifier of where this resource is.  It should be unique to this resource.
        /// Two Resources that have the same location should be the same resource.  A location will be constructed by
        /// use of the <see cref="Resource.Test"/> and <see cref="Resource.Name"/> components.
        /// </summary>
        String Location { get; }

        /// <summary>
        /// The name is how the test case refers to a resource.  Each test case can have several resources, with unique names.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// A Test is the basic identifier of a resource.  This is the test's Type (class).
        /// </summary>
        Type Test { get; }

        /// <summary>
        /// A resource may not exist, and still have an object associated to it, this allows you to create a resource within a test case.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Get all the content of this resource (assuming it exists) in an array of bytes.
        /// </summary>
        /// <returns>An array of bytes containing all the resource's content.</returns>
        byte[] getContent();

        /// <summary>
        /// Get all the content of this resource (assuming it exists) as a string.
        /// </summary>
        /// <returns>The contents of the resource as a large string.</returns>
        String getContentAsString();

        /// <summary>
        /// Open a stream to the resource.  If the mode is write, the resource can be created if it does not already exist.
        /// If it does exist it will truncate the resource.  If the mode is ReadWrite and the resource exists it will be
        /// opened without truncating it.
        /// </summary>
        /// <param name="mode">The mode the stream is operating in.  Read or Write (or Read and Write).</param>
        /// <returns>A stream to the resource.</returns>
        Stream getStream(FileAccess mode);
    }
```