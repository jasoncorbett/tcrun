# Introduction #

tcrun.exe is a simple executable file runs within the tcrun folder.  Simply download the tcrun folder and you have all you need to run it.

# Details #

## Running the inclued tests ##

Open up the command prompt, change into the directory where you unzipped tcrun.  If you want to see how it looks to run tests,
try running the ones we use to test tcrun itself.  On windows:

```
tcrun TCApi
```

You can also run the windows only tests (they do things like take screen shots):

```
tcrun WindowsOnly
```

All the output is in the results folder, explore away, just make sure you don't have files or directories "open" when you
are trying to run the tests, tcrun will complain.

On **NIX based systems you'll need Mono, and there is a tcrun.sh script to help you:**

```
./tcrun.sh TCApi
```

## What's included in the download ##

Contents of the tcrun folder:
  * **tcrun.exe** - The executable file you run from the command line to start your tests
  * **"conf" folder** - stores ini files that contain information that tests can access when running.
  * **"tests" folder** - where you need to place all your test dlls
  * **"lib" folder** - contains libraries needed for tcrun functionality along with any additional libraries you want to add.

Other folers used in tcrun (not automatically included):
  * **plans** - Where test plans are stored, see TestPlans for details
  * **resources** - Where environment specific resources (output and input files for a test) are stored.  See ResourceValidation.
  * **results** - Created after you run tests, the latest results are always in results\last