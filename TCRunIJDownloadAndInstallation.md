# Introduction #

tcrunij comes in 2 different packages: a gzipped tar ball and a zip file.  The first mostly for Mac / **NIX platforms and the latter
for windows.  Inside is shell scripts and bat files used to ease the pain of running it.  You will first need Java installed
to use tcrunij.**

# Details #

## Running from the command line ##

Open up the command prompt, change into the directory where you unzipped tcrunij.  To
get command line help, use the -h parameter:

![http://wiki.tcrun.googlecode.com/hg/images/tcrunij-help.png](http://wiki.tcrun.googlecode.com/hg/images/tcrunij-help.png)

## What's included in the download ##

Contents of the tcrun folder:
  * **tcrunij.sh** / **tcrunij.bat** - The executable file you run from the command line to start your tests
  * **"conf" folder** - stores ini files that contain information that tests can access when running, also the logging configuration.
  * **"tests/tcapi" folder** - where you need to place all your test jars
  * **"lib" folder** - contains jars needed for tcrunij functionality along with any additional jars you want to add.

Other folers used in tcrun (not automatically included):
  * **plans** - Where test plans are stored, see [TestPlans](TCRunIJTestPlans.md) for details
  * **results** - Created after you run tests