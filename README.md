GildedRose Api
==============

I'm a big fan of the [ServiceStack](https://servicestack.net/) framework, and I used it here - I could have just as easily went with native ASP.NET core APIs and libraries, but I find the ServiceStack framework more enjoyable honestly, particularly as a project (and technology needs) grow.  Everything else is pretty straight forward, but for reference here are some buzz words of things that were used:

* [NLog](https://nlog-project.org/) for logging
* Repository pattern for the very little/simple data access that is being done
* A message-based design for transferring data to/from services
* [JWT](https://jwt.io/) , API Key, and user/pw authentication options

It likely goes without saying, but this is .NET Core all the way (.NET Core 2.2. app for the hosts, .NET Standard 2.0 for the libs). You should have no trouble building/running/opening this on a non-Windows machine, I built it entirely on my iMac Pro using the [Rider IDE](https://www.jetbrains.com/rider/) (my preferred C# IDE these days).

