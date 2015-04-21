# History #

See AboutTCRun

After seeing much success from tcrun, we evaluated [selenium 2](http://code.google.com/p/selenium)
and realized that this was going to quickly become an important standard in web automation.
We saw that the web driver api was stable enough for us to begin using it, but we wanted to
use Java for the ability to develop / test anywhere.  We wanted a tool like tcrun, but for
Java instead of .NET.  We decided to make our own, as we wanted something that worked the
way we as Software Engineers in QA did.  There are pleanty of unit test tools we could have
used, but we wanted something more suited to the way we worked as testers.

TCRunIJ was made to be similar to tcrun, but with a more flexible architecture.  It is plugin driven,
allowing us to add features without changing workflow or touching code that already works.

# Why Another Framework #

Essentially the existing ones didn't give us what we needed, at least not easily.  There are several
reasons that we wanted to write our own tool:
  * Easily (from the command line) select one or a group of tests to run
  * Different logical organizations of tests (not strict hierarchal)
  * Configuration for tests built into the framework and EASILY accessible
  * Simple logging already configured for test cases
  * Flexible architecture that potentially allows us to run tests from other frameworks

## Why not unit test frameworks ##

Well, frankly, the two are not exclusive.  One of the features planned is the ability to run JUnit
and TestNG tests along with TCApi tests.  Although we did implement our own
test framework, we're not excluding others.  TCRunIJ is bigger than just unit test frameworks, our
target is for functional tests.  Unit test frameworks are usually (and rightfully so) targeted to be
run as part of a build before a product is deployed or installed.  Ours is meant to do the installing
and/or testing after it's been deployed or installed.

## What about tool X ##

Well there's probably a good reason, but you will need to just trust us, or ignore us. Maybe we should
integrate with whatever it is you want us to be using.  Feel free to file a bug if you think tcrun
could use integrations that aren't there, or is missing features.  Realize that we are not only developing
it, but actively using it.  We are very interested in your feedback.