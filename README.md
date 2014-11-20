SqlConnTest
===========

Simple quick application that I wrote a few years ago that connects to a SQL Server database, and queries the database at a set interval. Wrote this to identify a connectivity issue. 

### Requirements

.NET Framework 3.5

### How do I configure this application?

Edit the app.config to set the desired connection string, query and interval (in seconds).

### What this application do?

The application will connect using the specified connections tring and execute the query at the desired interval. Closing the application will hide it in the system tray where you can double click on the icon to close the application.

### Where does it log to?

Everything is logged to SQLTesterLog.txt.