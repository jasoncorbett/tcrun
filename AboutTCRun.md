# History #

When starting on a new project at Intuit, several of us Software Engineers in QA needed to come up with
some different automation tools.  The product we were testing at the time wasn't automatable from some of
the existing tools on hand.  We found that the .NET automation library included with .NET 3.5 worked
fairly well for what we needed.  So we investigated different unit testing frameworks.  The main issue
we had was that we needed to have a different set of configurations based on which environment we were
testing.  We had a development environment, a QA environment, and stage and production environments.
There were different sets of URLs and user accounts that were needed for each.  None of the existing
tools made this easy.

We decided to write a simple framework that would allow configuration, and most of the other features
added to tcrun came thereafter.  It's grown a lot, and sure probably needs some refactoring, but it
works for us, and we thought it might work for others too.

We recently (January 2010) received permission to open source tcrun and this project was created.  The
goal is to develop it in the open.

# Why Another Framework #

Essentially the existing ones didn't give us what we needed, at least not easily.  There are several
reasons that we wanted to write our own tool:
  * Easily (from the command line) select one or a group of tests to run
  * Different logical organizations of tests (not strict hierarchal)
  * Configuration for tests built into the framework and EASILY accessible
  * Multiple testers able to add tests without stepping on each others code (separate DLLs)
  * Exportable framework with really low (hopefully none) prerequisites so that developers
> > could quickly grab the tests and run them without having extensive modifications to their
> > systems
  * Simple logging already configured for test cases

## Why not unit test frameworks ##

Well frankly the two are no exclusive.  One of the features planned is the ability to both use NUnit
assertions in our tests, and to be able to run NUnit tests.  Although we did implement our own
test framework, we're not excluding others.  TCRun is bigger than just unit test frameworks, our
target is for functional tests.  Unit test frameworks are usually (and rightfully so) targeted to be
run as part of a build before a product is deployed or installed.  Ours is meant to do the installing
and/or testing after it's been deployed or installed.

## What about tool X ##

Well there's probably a good reason, but you will need to just trust us, or ignore us. Maybe we should
integrate with whatever it is you want us to be using.  Feel free to file a bug if you think tcrun
could use integrations that aren't there, or is missing features.  Realize that we are not only developing
it, but actively using it.  We are very interested in your feedback.