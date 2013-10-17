CassiniDev
==========

Fork of CassiniDev on CodePlex (https://cassinidev.codeplex.com/)

An enhanced Cassini for Framework 3.5 and 4.0 - Can be used as a standalone exe, for self-hosting ASP.net or as a reliable host for testing framworks including continuous integration scenarios.

----------------------------------------------------------------------------------------

CassiniDev 3.5.0.4

Cassini for Developers and Testers:  http://cassinidev.codeplex.com
 
CassiniDev is a branch of Microsoft’s Cassini sample.
 
While CassiniDev is currently suitable as a development and debugging host with a simple bootstrap kludge, it is not 
currently targeted as a replacement for the Visual Studio Development Server.
 
The goal of the project is to provide a robust testing web host that addresses many of the limitations and 
difficulties encountered when using Cassini and/or Visual Studio Development Server in automated testing scenarios.
 
Enhanced Hosting Options
·         Full support for any IP address. Not limited to localhost.
·         HostName support with option to temporarily add hosts file entry.
·         Port scan option. Dynamically assign an available port when specific port is not required or known to be available.
·         WaitOnPort: Length of time, in ms, to wait for specific port to become available.
·         TimeOut: Length of time, in ms, to sit idle before stopping server.
·         Single file GUI and Console applications.
 
Robust Testing Functionality
 
The test fixture is intended to address or mitigates many common issues encounter when rapidly spinning up and shutting down
a web host in a test environment, including restrictive apartment modes, AppDomainUnloadedException exceptions, erratic runtime
failures etc.
 
The current strategy is to offload the hosting to a controlled headless instance of the console application. So far this has proven
to be quite reliable in simple testing scenarios. Scenarios involving Continuous Integration are likely to surface unaddressed requirements.
 
Tested Scenarios:
·         MSTests using Visual Studio Test Runner.
·         MSTest tests using all TestDriven runner and coverage options.
·         NUnit tests using NUnit GUI and NUnit console.
·         NUnit tests using NUnit VS integration
·         NUnit tests using Resharper VS integration
·         NUnit tests using all TestDriven runner and coverage options.
To Do:
·         Test in CI systems.
·         Explore possibility of drop-in replacement of WebDev.WebServer.exe for F5 debugging in Visual Studio 2005/2008/2010:
Currently you may attach to CassiniDev or use a bootstrap console project for dev hosting and debugging.
01/01/10 - took a dive into webdev and looks to be quite an undertaking to build a replacement. Build a wrapper for webhost and ran into some stoppers. Will revisit later.
 
·         Start working on CI stories - how to checkout, build and test using CassiniDev.
·         Test IPv6 functionality thoroughly.
·         A few instrumentation points would improve the testing/debugging experience.
·          Application virtual path and single app hosting limitations:
·         I would like to see a scenario in which a virtual web directory can be described as the hosting environment allowing pointers to shared resources and perhaps multiple web applications
 
Command Line Argument Reference:
 
/?
Display Usage
 
/ApplicationPath:<string>
(short form /a)
NOTE: Omit trailing slash from quoted paths
 
/VirtualPath:<string>
(short form /v)
Default value:'/'
 
/HostName:<string>
(short form /h)
Is used to construct RootUrl for hosted application. HostName is optional unless AddHost is true. If null, ‘localhost’ or IP Address are used.
 
/AddHost[+|-]
(short form /ah)
Default value:'-'
Add temporary entry to hosts file to facilitate named DNS resolution. Entry is removed when server is stopped. Write permissions to the hosts file are required.
 
/IPMode:{Loopback|Any|Specific}
(short form /im)
Default value:'Loopback'
 
/IPAddress:<string>
(short form /i)
Ignored unless IPMode = ‘Specific’
 
/IPv6[+|-]
(short form /v6)
Default value:'-'
Ignored unless IPMode = ‘Any’ or ‘Specific’
 
/PortMode:{FirstAvailable|Specific}
(short form /pm)
Default value:'FirstAvailable'
If PortMode = ‘FirstAvailable’ the specified port range is scanned for the first available port.
 
/Port:{1|65535}
(short form /p)
Ignored unless PortMode = ‘Specific’
 
/PortRangeEnd: {1|65535}
(short form /pre)
Default value:'9000'
 
/PortRangeStart: <ushort>
(short form /prs)
Default value:'8080'
 
/WaitForPort: <int>
(short form /w)
Default value:'0'
Length of time, in ms, to wait for specific port to become available. 0 = no wait.
/TimeOut: <int>
(short form /t)
Default value:'0'
Length of time, in ms, to sit idle before stopping server. 0 = no timeout.
Usage Examples:
·         CassiniDev
Start GUI application idle.
·         CassiniDev /a:c:\projects
Start GUI or console application auto hosting c:\projects on http://localhost and the first available port starting with 8080.
·         CassiniDev /a:c:\projects /pm:Specific /p:8080
Start GUI or console application auto hosting c:\projects on http://localhost:8080/.
·         CassiniDev /a:c:\projects /im:Specific /i:192.168.0.1 /pm:specific /p:81 /v:myapp /ah+ /h:mycomputer.com
Start GUI or console application auto hosting c:\projects on http://mycomputer.com:81/myapp and add hosts file entry.
 
New in CassiniDev v3.5.0.4
·         Added Cassini hosting Fixture and supporting classes to facilitate use of CassiniDev in testing scenarios:
o   While CassiniDev and CassiniDev-console may be referenced as a library it is not recommended for testing scenarios.
A fixture class has been provided (CassiniDev.Testing.Fixture) that reliably hosts the console application in a separate process.
·         Include test projects demonstrating some possible scenarios for use of CassiniDev in integration/interaction/smoke testing of web based resources.
 
New in CassiniDev v3.5.0.3
·         Improved command line parsing.
·         Console version added for use in headless processes:
o   The console application can be run in a non-interactive session and requires that all supplied arguments are valid for the process to start.
o   The GUI application will reject invalid arguments with a dialog notification and present the UI for modification of arguments.
·         Both versions are standalone and require no GAC assembly.
·         Implemented arbitrary IP use including both IPv4 and IPv6 Any and Loopback.
·         Added port scanning to allow dynamic allocation of first available port in specified range.
·         Added hosts file utility. CassiniDev can dynamically add a temporary hosts file entry to allow dns resolution of application specific domains.
·         Implemented support for relative paths.
 
Branched from Cassini v3.5.0.2
 
New in Cassini v3.5.0.2
·         Fix for the default documents.
 
New in Cassini v3.5.0.1
·         Support for MVC friendly URLs (directory listing only overrides 404 responses for directories)
 
New in Cassini v3.5
·         Runs as a single EXE -- does not require an assembly in GAC
·         Supported IPv6-only configurations
·         Upgraded to support .NET Framework 3.5
·         Includes VS project file
·         License changed to Ms-PL
 
 