

# Introduction #

---


I will not attempt to explain certain build processes or how to do this with every IDE imaginable.  This HOWTO will focus
primarily on code and the tcapi-selenium toolkit.  Tools used in this HOWTO which will not recieve detailed coverage:

  * [Apache Maven](http://maven.apache.org), a build and project management solution
  * [Netbeans](http://netbeans.org), an IDE with excellent Java and Maven support built in

For learning maven, I suggest [Better builds with maven](http://www.maestrodev.com/support), it's a free book, and an excellent
way to learn maven.  Netbeans isn't hard to use, the main reason I use it is it's excellent integration with maven.  For those
wanting an Eclipse based tutorial, I'm planning on it, but you may have to wait a little.

You **WILL NEED** to install java, maven, and netbeans before starting this tutorial.  When downloading netbeans be sure to
download an IDE with Java support included.

If you wish to download all the sources for this project, follow the instructions
[here](http://code.google.com/p/tcrun/source/checkout?repo=tcrunij-examples).

You may also want a developer tool package for your browser (included with safari and chrome) so you can inspect web page
elements to figure out how to locate them on the page.  This is also not covered in this tutorial.

## Step One: Create the testing project ##

---

For the purpose of this tutorial, I won't have you create a maven pom file step by step, but rather download an existing one.
First create a directory for the tutorial project to go in, I'll call this directory `simple-selenium2`.  Inside that
directory place the maven pom file from [here](http://tcrunij-examples.tcrun.googlecode.com/hg/simple-selenium2/pom.xml).

If you're familiar with maven (or if you're not), below I've taken snippets that you may want to notice:

```
	<repositories>
		<repository>
			<id>tcrunij-repo</id>
			<url>http://maven.tcrun.org</url>
		</repository>
	</repositories>
```

In this section we add the tcrun maven repository.  TCRunIJ is not yet in the main maven repository, so we have to tell
maven where to get the dependencies for this project.

```
		<dependency>
			<groupId>org.tcrun</groupId>
			<artifactId>tcapi-selenium</artifactId>
			<version>[1.0.0-102,2.0.0)</version>
		</dependency>
		<dependency>
			<groupId>org.tcrun</groupId>
			<artifactId>tcrunij-tcapi</artifactId>
			<version>[1.0.0-102,2.0.0)</version>
		</dependency>
		<dependency>
			<groupId>org.tcrun</groupId>
			<artifactId>tcrunij-api</artifactId>
			<version>[1.0.0-102,2.0.0)</version>
		</dependency>
```

Here we add dependencies from tcrunij.  In tcrunij there is a "tcapi" framework that is a basic one test case per class
test framework.  We need the `tcrunij-tcapi` package to get that api.  The `tcapi-selenium` is a nice integration and
wrapper for the [web driver api](http://seleniumhq.org/docs/09_webdriver.html) from the
[selenium 2](http://code.google.com/p/selenium/) project.  Finally, the tcrunij-api has some classes that allow us to decorate
our tests with metadata.

We next need to add a package for our java source to live in.  In maven the sources we want to add will live in `src/main/java`,
so if we're using package `org.tcrun.examples.simple.selenium2` we will want to run (on `*`NIX / Mac)
`mkdir -p src/main/java/org/tcrun/examples/simple/selenium2` inside our `simple-selenium2` project root.

## Step Two: Open the project in Netbeans ##

---


This should be a very easy step.  Start netbeans, Select the menu `File`->`Open Project...`, then navigate to where you
stored the simple-selenium2 directory.  Netbeans should be able to recognize that this is a maven project, and open
the directory as a project.

After opening the project your netbeans **Projects** tab on the left should look similar to this:
![http://wiki.tcrun.googlecode.com/hg/images/simple-selenium2-project-tree.png](http://wiki.tcrun.googlecode.com/hg/images/simple-selenium2-project-tree.png)

## Step Three: Run through the test manually ##

---


This is important.  We need to see this working.  For this test, we'll be looking for a business on http://yp.com.  The steps
are pretty simple:

  1. Go to http://yp.com
  1. Type in "Intuit" into the search box
  1. Type in "Mountain" into the search location box
  1. Select "Mountain View, CA" in the ajax drop down
  1. Click the "Find" button
  1. Find the result named "Intuit, Inc."

Go ahead and run through the process manually.

## Step Four: Adding Page Classes ##

---

Page classes are a simple, easy to use abstraction of a Web Page.  They help you separate the definition and logic for
that page from your test so that the same logic can be re-used in other tests.  For more details on page classes, see
the [tcapi-selenium manual section](TCRunIJTCApiSeleniumPageClasses.md) on them.  We have 2 pages that we're interested in,
namely the home page, and the search results page.

Expand the section in Netbean's Project Pane named "Source Packages".  Right click on the package named `org.tcrun.examples.simple.selenium2`,
then select the menu item named New -> Java Class.  In the resulting Dialog put in the Class Name field "YPHomePage".  It
is a good practice to end the name of the page classes with "Page".  Click Finish, and paste in the following code.

Here is the code for YPHomePage.java, and following is the explanation:

```
package org.tcrun.examples.simple.selenium2;

import org.tcrun.tcapi.selenium.PageElement;
import org.tcrun.tcapi.selenium.SelfAwarePage;
import org.tcrun.tcapi.selenium.WebDriverWrapper;
import org.tcrun.tcapi.selenium.FindBy;

/**
 *
 * @author jcorbett
 */
public class YPHomePage implements SelfAwarePage<Object>
{
	public static PageElement SearchTermField = new PageElement("Search Term Field", FindBy.id("search-terms"));
	public static PageElement SearchLocationField = new PageElement("Search Location Field", FindBy.id("search-location"));
	public static PageElement FindButton = new PageElement("Find Button", FindBy.id("search-submit"));

	// Ajax Elements
	public static PageElement firstLocationContaining(String part)
	{
		return new PageElement("Location result containing " + part, FindBy.xpath("(//li[contains(text(), \"" + part + "\")])[1]" ));
	}


	@Override
	public boolean isCurrentPage(WebDriverWrapper browser)
	{
		return browser.exists(SearchTermField, false) && browser.exists(SearchLocationField, false);
	}

	@Override
	public void handlePage(WebDriverWrapper browser, Object context) throws Exception
	{
		throw new UnsupportedOperationException("Not supported yet.");
	}
}
```

First you will note that the YPHomePage implements an interface called `SelfAwarePage`.

```
public class YPHomePage implements SelfAwarePage<Object>
```

Page classes that are _Self Aware_ are
able to determine if the page that the browser is on is the page described, and can optionally _Handle_ the page.  They handle
the page based on a context object (Java Generics are used to specify the context object type).  In this case we won't be
using the context object, or the handle page ability, so we'll leave the default code generated by the IDE.

Next we have 3 static page element objects:

```
	public static PageElement SearchTermField = new PageElement("Search Term Field", FindBy.id("search-terms"));
	public static PageElement SearchLocationField = new PageElement("Search Location Field", FindBy.id("search-location"));
	public static PageElement FindButton = new PageElement("Find Button", FindBy.id("search-submit"));
```

A `PageElement` is an object that describes how to find a web element on a page.  It also gives the element a logging-friendly name.
In this case the first page element (arbitrarily named "Search Term Field") is located by it's HTML id "search-terms".  Similarly
the second search box, used to specify the location to search in, is located by the HTML id "search-location".  Finally the Find button
is also located by an id, "search-submit".

Next there is a static function, that returns a `PageElement`:

```
	// Ajax Elements
	public static PageElement firstLocationContaining(String part)
	{
		return new PageElement("Location result containing " + part, FindBy.xpath("(//li[contains(text(), \"" + part + "\")])[1]" ));
	}
```

When typing in the location field, an ajax dropdown list of possible locations is listed below the search field box.  Because the items
in the list don't have id's and are a little tricky to identify, this fuction returns a dynamic `PageElement` that looks for text in
the option, by using XPath.  Usually I recommend limiting the use of XPath, primarily because it can make the tests harder to read,
but it is an extremely powerful way to find elements.  For more on XPath, do a google search.

Finally we have the _Self Aware_ part of this page class:
```
	@Override
	public boolean isCurrentPage(WebDriverWrapper browser)
	{
		return browser.exists(SearchTermField, false) && browser.exists(SearchLocationField, false);
	}
```

This method simply returns true if the page currently displayed on the provided browser has the required items, specifically the
2 search boxes.  This method should usually uniquely identify your page from others on the site.  It should also return quickly.


---


The second page we need, is one with results.  What we're looking for on this page is a result with a particular name.
In this case, the results are links, so we can look for the result based on link text.  We also want a self aware page here
so that we can know when the page is ready.  Follow the same steps as for the last page class, but name this one
`YPSearchResultsPage`, and paste the following code in:

```
package org.tcrun.examples.simple.selenium2;

import org.tcrun.tcapi.selenium.FindBy;
import org.tcrun.tcapi.selenium.In;
import org.tcrun.tcapi.selenium.PageElement;
import org.tcrun.tcapi.selenium.SelfAwarePage;
import org.tcrun.tcapi.selenium.WebDriverWrapper;

/**
 *
 * @author jcorbett
 */
public class YPSearchResultsPage implements SelfAwarePage<Object>
{
	public static PageElement ResultsDiv = new PageElement("Div that contains all search results", FindBy.id("results"));

	public static PageElement resultByName(String name)
	{
		return new PageElement("Search result with name " + name, In.ParentElement(ResultsDiv), FindBy.linkText(name));
	}

	@Override
	public boolean isCurrentPage(WebDriverWrapper browser)
	{
		return browser.exists(ResultsDiv, false);
	}

	@Override
	public void handlePage(WebDriverWrapper browser, Object context) throws Exception
	{
		throw new UnsupportedOperationException("Not supported yet.");
	}
}
```

I won't go over the same things as in the last page, but mainly the differences.  In this page we have only one static PageElement:
```
	public static PageElement ResultsDiv = new PageElement("Div that contains all search results", FindBy.id("results"));
```
This element is a container for all the results on the page.  We'll use it in the `isCurrentPage` method:
```
	@Override
	public boolean isCurrentPage(WebDriverWrapper browser)
	{
		return browser.exists(ResultsDiv, false);
	}
```
Notice that we're basing the page on the existence of the results div, and we passed `false` as the second parameter to the exists
method.  The `false` is to tell the exists method not to log the check for existence.  This is there to make your logs easier to
follow, the isCurrentPage can sometimes be called repeatedly, and can add noise to your test's log file.

The only other thing useful in this page class is the following static method:
```
	public static PageElement resultByName(String name)
	{
		return new PageElement("Search result with name " + name, In.ParentElement(ResultsDiv), FindBy.linkText(name));
	}
```
Since we don't want to hard code the test's values into the page class, this method returns a PageElement based on the input given.
It also looks for this element inside a parent element of the results div.  This is just a precaution so that we don't find that
link somewhere else on the page, thus causing a false positive result.

## Step Five: Adding the test class ##

---


Since we have separated out all the details of the pages into our page classes, our test will be short and simple.  Create the
class the same way as we did for the page classes, but this time name the class `SearchForIntuit`, and paste the following code
in:

```
package org.tcrun.examples.simple.selenium2;

import java.util.UUID;
import org.tcrun.tcapi.TestResult;
import org.tcrun.tcapi.selenium.AbstractSeleniumTest;
import org.tcrun.tcapi.selenium.PageElement;
import static org.tcrun.tcapi.assertlib.MatchBuilder.*;

/**
 *
 * @author jcorbett
 */
public class SearchForIntuit extends AbstractSeleniumTest
{
	private String url;

	@Override
	public void setup() throws Exception
	{
		url = configValue("URL.yp", "http://yp.com");
	}

	@Override
	public TestResult test() throws Exception
	{
		step("Going to Yellow Pages Website " + url, "We end on a page that has the form we are expecting.");
		browser.goTo(url);
		check.that(browser.isCurrentPage(YPHomePage.class), Is.True());

		step("Type search term \"Intuit\" into the search box.");
		browser.type(YPHomePage.SearchTermField, "Intuit");

		step("Type \"Mountain\" into the location box, and click on first result.");
		browser.type(YPHomePage.SearchLocationField, "Mountain");
		PageElement moutainViewCAResult = YPHomePage.firstLocationContaining("View, CA");
		browser.waitForVisible(moutainViewCAResult);
		browser.click(moutainViewCAResult);

		step("Click the Find button", "Browser goes to a results page with Intuit Inc. in the results.");
		browser.click(YPHomePage.FindButton);
		browser.waitFor(YPSearchResultsPage.class);
		browser.takeScreenShot("search-results");
		check.that(browser.exists(YPSearchResultsPage.resultByName("Intuit Inc.")), Is.True());

		return TestResult.PASS;
	}

	@Override
	public UUID getTestUUID()
	{
		return UUID.fromString("d5d385af-7075-4b62-ae5b-d87864b9f6ab");
	}
}
```

Now, let me explain this, though hopefully this code is easy to read.  Let's start with the class definition:
```
public class SearchForIntuit extends AbstractSeleniumTest
```
So I've named the class based on what the test will be testing, it will be searching for Intuit.  I've extended
`AbstractSeleniumTest` as that gives me several things:
  * A `WebDriverWrapper` instance using a browser that can be selected at runtime.
  * Configuration from the environment ini and command line options
  * A test case logger
  * A convienant assertion method `check.that`, which logs both successes and failures
  * If I'm running more that one test, the browser instance is persistent (this can be changed either via
> > command line parameter or by environment ini setting)

Now on to the setup of the test, this is pretty simple:

```
	private String url;

	@Override
	public void setup() throws Exception
	{
		url = configValue("URL.yp", "http://yp.com");
	}
```
Here we declare a instance variable `url` in which we'll store the URL we should go to.  Inside the setup
we set the value.  We use the value of the setting `URL.yp` if it exists, or the default of `http://yp.com`.

Let's examine this test, step by step:
```
	@Override
	public TestResult test() throws Exception
	{
		step("Going to Yellow Pages Website " + url, "We end on a page that has the form we are expecting.");
		browser.goTo(url);
		check.that(browser.isCurrentPage(YPHomePage.class), Is.True());
```
To start out our test method, we have the declaration of a step.  Steps, and their expected results, help document
your intentions and actions in log files, and can be used by plugins in the framework.  This step is saying that
we're going to the yp.com home page.  After that we tell the browser to go to the home page, and the check that
the current page is the one we're expecting.  If this was not the case, an exception would be thrown and this
test would fail.

The next step:
```
		step("Type search term \"Intuit\" into the search box.");
		browser.type(YPHomePage.SearchTermField, "Intuit");
```

Short and sweet, type "Intuit" into the search terms field.  Notice that since the PageElement in the YPHomePage is
static we don't need to instantiate an instance of the class.  That's because PageElements are descriptions of
where to find the actual elements.

Next we do some ajaxy fun:
```
		step("Type \"Mountain\" into the location box, and click on first result.");
		browser.type(YPHomePage.SearchLocationField, "Mountain");
		PageElement moutainViewCAResult = YPHomePage.firstLocationContaining("View, CA");
		browser.waitForVisible(moutainViewCAResult);
		browser.click(moutainViewCAResult);
```
In this step we type "Mountain" into the location box.  When we do that we get an ajax dropdown of places that
match "Mountain".  We're looking for Mountain View, CA, so we find the first result in the list that contains
"View, CA".  Selenium 2 / WebDriver only want to click on an element that's visible.  With ajax we can
sometimes find an element before it's ready to click.  So we wait for the element to be "Visible", and then
we click it.

We could check the value now in the search location field to make sure it's Mountain View, CA.  Feel free to
add that check on your own!  Remember that code completion is your friend, especially when trying to find
the right check in the Is instance.

The final step:
```
		step("Click the Find button", "Browser goes to a results page with Intuit Inc. in the results.");
		browser.click(YPHomePage.FindButton);
		browser.waitFor(YPSearchResultsPage.class);
		browser.takeScreenShot("search-results");
		check.that(browser.exists(YPSearchResultsPage.resultByName("Intuit Inc.")), Is.True());
```
We click the Find button, then we wait for the search results page to show up.  The `WebDriverWrapper` instance
`browser` will wait (for a default maximum of 30 seconds) for the isCurrentPage method to return true.  We then
take a screenshot of the page, which get's placed right next to our test case log.

Finally we check that we can find a result link named "Intuit Inc.".
```
		return TestResult.PASS;
```
We have to explicitly define the result, if any of the checks had failed, they would have caused the test to fail,
so we can safely just return Pass at this point.

Oh wait, what's that other method?
```
	@Override
	public UUID getTestUUID()
	{
		return UUID.fromString("d5d385af-7075-4b62-ae5b-d87864b9f6ab");
	}
```
Well it's not directly related to your test, but each test should have a UUID (or GUID) assigned to them.  The idea
is that if you end up syncing your results with an online database the UUID is a refactor-safe way to identify your
test.  Although at the time of the writing of this tutorial nothing uses it, the idea is to keep it there for
future use.

## Step Six: Packaging and Running ##

---


If you've made it this far, congratulate yourself, I'm not a great writer, and I'm sure this is really dry material.
Now we get to have some real fun.  Running the tests you've written is the real fun, and I've got a couple of
surprises for you ;-)

First we have to package the test into a jar file for tcrunij.  If you've followed the steps above, just right click
in Netbeans on the project and click on the menu option "Build".  This will do a `mvn install` which compiles everything
and then sticks it in a jar file for you.

As tcrunij is a command line tool, you'll now have to go to the command line.  Download and extract tcrunij, and then in
the resulting tcrunij folder, under tests/tcapi place the jar you just built.  The jar file is located under the target
directory of your `simple-selenium2` project folder.

Then change into the directory of tcrunij, and run the following command:
for `*`NIX (including mac):
```
	./tcrunij.sh -a
```
for M$ Windows:
```
	tcrunij -a
```

Now you should see tcrunij spit something out like this:
```
0001-org.tcrun.examples.simple.selenium2.SearchForIntuit: PASS
----------------------------------------------------------------------------
PASS: 1
Total: 1
```

I know what you're thinking, "You said this would be fun, that was boring!"  Yeah, you're right.  But now take a look in the results
folder, and you should be able to find a result directory for your last run, and you'll find a directory for the test with a log in it.
But, this is the first time we're running the test, and since it kind of stinks to have to go find the log file after the run, let's
just include it in the output, run the same command as last time, but add a `-v` on the end, and you'll see this:
```
0001-org.tcrun.examples.simple.selenium2.SearchForIntuit: 
----------------------------------------------------------------------------
[14:10:50|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Configuration key 'browser.persistent' missing, using default 'true'.
[14:10:50|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Configuration key 'browser' missing, using default 'headless'.
[14:10:51|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Configuration key 'defaults.timeout' missing, using default '30'.
[14:10:51|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Configuration key 'URL.yp' missing, using default 'http://yp.com'.
[14:10:51|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Step 1 Description: Going to Yellow Pages Website http://yp.com
[14:10:51|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Step 1 Expected Result: We end on a page that has the form we are expecting.
[14:10:51|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Going to page 'http://yp.com'.
[14:11:06|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Successful Check: actual value is 'true', expected value is <true>
[14:11:06|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Step 2 Description: Type search term "Intuit" into the search box.
[14:11:06|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Step 2 Expected Result: no expected result given.
[14:11:06|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Clearing the text from element with name 'Search Term Field' and found 'By.id: search-terms'.
[14:11:06|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Typing text 'Intuit' in element with name 'Search Term Field' and found 'By.id: search-terms'.
[14:11:06|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Step 3 Description: Type "Mountain" into the location box, and click on first result.
[14:11:06|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Step 3 Expected Result: no expected result given.
[14:11:06|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Clearing the text from element with name 'Search Location Field' and found 'By.id: search-location'.
[14:11:06|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Typing text 'Mountain' in element with name 'Search Location Field' and found 'By.id: search-location'.
[14:11:07|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Waiting a max of 30 seconds for element 'Location result containing View, CA' found by By.xpath: (//li[contains(text(), "View, CA")])[1] to become visible.
[14:11:08|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Found element 'Location result containing View, CA' after 0 seconds, waiting for it to become visible.
[14:11:08|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Element 'Location result containing View, CA' was found visisble after 0 seconds.
[14:11:08|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Clicking on element with name 'Location result containing View, CA' and found 'By.xpath: (//li[contains(text(), "View, CA")])[1]'.
[14:11:08|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Step 4 Description: Click the Find button
[14:11:08|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Step 4 Expected Result: Browser goes to a results page with Intuit Inc. in the results.
[14:11:08|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Clicking on element with name 'Find Button' and found 'By.id: search-submit'.
[14:11:17|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Waiting for page 'org.tcrun.examples.simple.selenium2.YPSearchResultsPage' a max of 30 seconds.
[14:11:17|INFO |t.o.t.t.s.DefaultWebDriverWrapper]: Found page 'org.tcrun.examples.simple.selenium2.YPSearchResultsPage' after 0 seconds.
[14:11:17|WARN |t.o.t.t.s.DefaultWebDriverWrapper]: Requested screenshot by name 'search-results', but browser doesn't support taking screenshots.
[14:11:17|DEBUG|t.o.t.t.s.DefaultWebDriverWrapper]: Checking for existence of element 'Search result with name Intuit Inc.'.
[14:11:17|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Successful Check: actual value is 'true', expected value is <true>
[14:11:17|INFO |t.o.t.e.s.selenium2.SearchForIntuit]: Configuration key 'browser.persistent' missing, using default 'true'.
0001-org.tcrun.examples.simple.selenium2.SearchForIntuit: PASS
----------------------------------------------------------------------------
0001-org.tcrun.examples.simple.selenium2.SearchForIntuit: PASS
----------------------------------------------------------------------------
PASS: 1
Total: 1
```
Ok, logging to the console isn't too exciting, but it is nice to see it in real time.  And did you notice that in our test we didn't write
a single log line, yet in the output we have such descriptive logs?  Hint: this should be starting to get fun.  Now you probably didn't
see a browser pop up when you ran that.  That's because the default browser is the headless browser included with web driver based on
HTMLUnit.  But you probably want to see it run in a real browser.  Did you notice the second log statement?  The framework didn't find
an option for browser, so it used it's default Headless.  How do we switch?  By passing in an option.  If you have firefox on your
system, try adding the option `-o browser=ff`.  For IE try `-o browser=ie`.  For Chrome `-o browser=chrome`.  The output should be the
same.

Now for some more fun!  Download from the [Selenium Project Page](http://code.google.com/p/selenium) the selenium server standalone jar,
and put it on another machine.  Run it, and make note of the ip address or resolvable host name of that machine.

Now back to you're command line, and run the same command line, but add `-o remote=` and add the ip address or host name.  That's right,
you can run the same test on all the browsers on that machine!

Ok one more fun thing.  Like most test running tools, tcrunij includes
a html report.  Try running the test with `--html-report` if you want to generate you're own.  The only downside is that it is currently
written using ajax, which requires the html report to be on a web server.  I've generated the report for you for this test, and placed
it [here](http://tcrun.org/simple-selenium2/index.html).  I used this command line to get the report you can see:
```
./tcrunij.sh -a -o browser=ff --html-report 'Simple Selenium 2 Test Project for tcrunij'
```
You'll notice that there is an optional parameter to the `--html-report` which will allow you to name you're report.

