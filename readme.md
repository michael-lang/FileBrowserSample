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


### AI usage and other help disclaimer

No AI plugins or services were used to generate any code in this application. I used Google search to remind myself of various javascript DOM traversal and manipulation functions which included some Google Gemini code samples of those functions in use.

No other persons helped create the code in this repository.

I have created many other applications, some of which include file upload capabilities. I was able to look at other code I had personally written over the past few years as inspiration.  Notably I have another public repo for a file upload API and Angular UI here: https://github.com/NexulAcademy/file-upload-demo. I am the creator of all Nexul Academy courses and code samples in the NexulAcademy organization repos.

For the SPA application classes I was inspired by an old SPA framework I wrote in 2013 leveraging closures instead of classes: https://github.com/michael-lang/jquery-auto-async. Fortunately, in this current project I was able to leverage JS classes which were generally available in 2016. Since 2016 I have worked on Angular applications using TypeScript.

### Courses

I have taught many developers in a professional context via Nexul Academy.  Here is a 43 minute video of me teaching how to create a SPA application using plain JavaScript in July 2020.

https://www.youtube.com/watch?v=b9j55ZG8dN8

I offer other C# and Angular training through Nexul Academy via GuruFyre.com.  I created GuruFyre with help from graduates I taught at Nexul Academy.

https://nexulacademy.gurufyre.com/