Cassini Developers Edition v3.5 README.TXT
------------------------------------------
http://cassinidev.codeplex.com/

This Cassini version requires .NET Framework v3.5

New in Cassini Developers Edition v3.5.0.3

*	Improved command line parsing.
*	Console version added. 
*	Both versions are standalone and require no GAC assembly.
*	Enabled arbitrary IP use including both IPv4 and IPv6 Any and Loopback.
*	Added port scanning to allow dynamic allocation of first available port
	in specified range.
*	Added hosts file utility. Cassinidev can dynamically add a temporary hosts
	file entry to allow dns resolution of application specific domains.
	(requires elevated process)
*   Implemented support for relative paths. (useful in testing scenarios)	


TODO: 
*	Explore possibility of drop-in replacement of WebDev.WebServer.exe for F5
	debugging in Visual Studio 2005/2008/2010.
*	Thoroughly test IPv6 functionality
			



---------------------------
CassiniDev.exe and CassiniDev-console.exe offer identical functionality.
The primary difference is that the console application can be run in a 
non-interactive session and requires that all supplied arguments are valid for
the process to start. The GUI application will reject invalid arguments with
a dialog notification and present the UI for modification of arguments.

CassiniDev.exe may be referenced as a library.



Cassinidev Command Line Argument Reference:
-----------------------------------------------------------
/ApplicationPath:<string>            (short form /a)

/VirtualPath:<string>                Default value:'/' (short form /v)

/HostName:<string>                   (short form /h)

/AddHost[+|-]                        Default value:'-' (short form /ah)

/IPMode:{Loopback|Any|Specific}      Default value:'Loopback' (short form /im)

/IPAddress:<string>                  IP address or constants 'any','loopback', 
                                     Default value:'loopback' (short form /i)
																		 
/IPv6[+|-]                           Default value:'-' (short form /v6)

/PortMode:{FirstAvailable|Specific}  Default value:'FirstAvailable' (short form /pm)

/Port:{MaxValue|MinValue}            (short form /p)

/PortRangeEnd:{MaxValue|MinValue}    Default value:'9000' (short form /pre)

/PortRangeStart:{MaxValue|MinValue}  Default value:'8080' (short form /prs)


----------------------------------------------
branched from Cassini v3.5.0.2
http://blogs.msdn.com/dmitryr/archive/2009/04/23/cassini-support-for-friendly-urls-routing.aspx
----------------------------------------------

This sample illustrates using the ASP.NET hosting APIs (System.Web.Hosting)
to create a simple managed Web Server with System.Net APIs.

New in Cassini v3.5.0.2
* Fix for the default documents

New in Cassini v3.5.0.1
* Support for MVC friendly URLs (directory listing only overrides 404 responses
for directories)

New in Cassini v3.5:
* Runs as a single EXE -- does not require an assembly in GAC
* Supported IPv6-only configurations
* Upgraded to support .NET Framework 3.5
* Includes VS project file
* License changed to Ms-PL

