## Directory Browser

This project demonstrates how to build a Plain JavaScript SPA application front-end connecting to a C#.Net REST API using SOLID principles.

Built by Michael Lang as an interview assignment. The API was built in about 2 hours on day 1. The UI html/css/JS was built in about 3 hours on day 2.

The API is using .Net (Core) version 8.0 matching the assignment starting version. The only NuGet packages added were for Microsoft logging and configuration. Other libraries were avoided. SOLID principles were leveraged as much as possible.  If I had more time, I would add C# unit tests for at least the DefaultFileValidationService.

The UI is using the latest version plain JS, leveraging classes to encapsulate related functionality, the fetch api for API calls, and lots of DOM manipulation.  All libraries were avoided to show the requested css, html and JS skills.  State is maintained fully in the url via a hashtag of the current folder to be displayed.

### Screenshots

The main screen with some files seeded into the uploads directory.

![Main Screen](/screenshots/ui-showing-rootfolder-and-files.png)

A sub folder showing current folder name above the file list.

![Sub-Folder Screen](/screenshots/ui-showing-subfolder-and-files.png)
