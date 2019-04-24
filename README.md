GildedRose Api
==============

I'm a big fan of the [ServiceStack](https://servicestack.net/) framework, and I used it here - I could have just as easily went with native ASP.NET core APIs and libraries, but I find the ServiceStack framework more enjoyable honestly, particularly as a project (and technology needs) grow.  Everything else is pretty straight forward, but for reference here are some buzz words of things that were used:

* [NLog](https://nlog-project.org/) for logging
* Repository pattern for the very little/simple data access that is being done
* A message-based design for transferring data to/from services
* [JWT](https://jwt.io/) , API Key, and user/pw authentication options

It likely goes without saying, but this is .NET Core all the way (.NET Core 2.2. app for the hosts, .NET Standard 2.0 for the libs). You should have no trouble building/running/opening this on a non-Windows machine, I built it entirely on my iMac Pro using the [Rider IDE](https://www.jetbrains.com/rider/) (my preferred C# IDE these days).

## [Authentication](#authentication)
2 different users a seeded on app startup when running in DEBUG mode, an admin and a non-admin. You can use the admin user to add new items, get any order, etc.  Regular users are only allowed to GET items, POST orders, and GET their own orders.

Admin user:
user: adminUser1@trovinterviewapi.com
pw: adminUser1Trov

Standard user:
user: testUser1@trovinterviewapi.com
pw: testUser1Trov

You can authenticate to the API with a:

```json
POST /auth/credentials
{
    "Username": "",
    "Password": ""
}
```

Which will give you back a JWT token you can use, or you can make a follow-up call to:

```json
GET /apikeys/live
```

Which will give you an API Key you can permanently use as a Bearer auth token in requests.

## [Requirements](#requirements)

I took the liberty of adding an Id to the Item model.  Naturally for inventory you could go the route of an item with a quantity, but a simple unique ID seems as reasonable for now.

#### GetItems
To get all items (a list of current inventory) make an unauthenticated request to:

```json
GET /items
```

and you'll get back a list of Item models:

```c#
public class Item : IHasStringId
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
}
```




https://www.getpostman.com/collections/35476d1cb2ae14ef95f2