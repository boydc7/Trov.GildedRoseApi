GildedRose Api
==============

I'm a big fan of the [ServiceStack](https://servicestack.net/) framework, and I used it here - I could have just as easily went with native ASP.NET core APIs and libraries, but I find the ServiceStack framework more enjoyable honestly, particularly as a project (and technology needs) grow.  Everything else is pretty straight forward, but for reference here are some buzz words of things that were used:

* [NLog](https://nlog-project.org/) for logging
* Repository pattern for the very little/simple data access that is being done
* A message-based design for transferring data to/from services
* [JWT](https://jwt.io/) , API Key, and user/pw authentication options

It likely goes without saying, but this is .NET Core all the way (.NET Core 2.2. app for the hosts, .NET Standard 2.0 for the libs). You should have no trouble building/running/opening this on a non-Windows machine, I built it entirely on my iMac Pro using the [Rider IDE](https://www.jetbrains.com/rider/) (my preferred C# IDE these days).

The API listens on port 8888 by default, and you can view all metadata about request and response formats/requirements by visiting:

* [http://localhost:8888/metadata](http://localhost:8888/metadata)

And you can see examples of any/all API requests in the following POSTMAN collection:

* [https://www.getpostman.com/collections/35476d1cb2ae14ef95f2](https://www.getpostman.com/collections/35476d1cb2ae14ef95f2)

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

or in JSON response format like so:

```json
{
    "results": [
        {
            "id": "8b6b7c340d804c37af1545d57864677a",
            "name": "Test Item 5",
            "description": "Description for test itme 5",
            "price": 15
        },
    ...
    ]
}
```

You can also register new users with:

```json
POST /register
{
  "Email": "testuser2@trovtestapi.net",
  "FirstName": "",
  "LastName": "",
  "DisplayName": "",
  "Password": "",
  "ConfirmPassword": "",
}
```

#### PostOrder
To place an order (i.e. buy an item) make an authenticated request to:

```json
POST /orders
{
  "ItemIds": [<itemIds>...]
}
```

and you'll get back an OrderId:

```json
{
  "OrderId": "<New OrderId>"
}
```

To view a successfully placed order, request:

```json
GET /orders/<orderid>
```

You can only order valid items, and you can only GET orders of your own.

## [Deliverables](#deliverables)

1. A system that can process the two API requests via HTTP
   1. See above
2. Appropriate tests (unit, integration, etc)
   1. I opted only for actual integration tests here, there was minimal unit-like testable code (could have excercised the repositories likely)
   2. See the Trov.GildedRose.Api.IntegrationTests project
3. A quick explanation of:
   * Choice of data format
      * Thanks to the ServiceStack framework, the API supports JSON, JSV, CSV, and XML out of the box without any special consideration. If I had to pick a specific format only, it would be JSON - it checks all the boxes on base requirments for serializing relatively complex data, it's hugely popular, has large community support, is more or less native to JavaScript making it nearly ubiquitous. It's also supported by a wide variety of services/systems natively, include many a database. 
   * What authentication method was chosen and why
      * As mentioned above, I included both standard credential (i.e. user/pw) and secret API Key exchange for the primary authentication, along with JWT support for client claim and storage (you can currently also just cookie the credential token, but in a "real" app I'd remove that part in favor of strictly JWT for claim and exchange).  
      As for why, it kind of depends on the use case.  I'm making the general assumption here that this will be used in 2 primary scenarios:
         * An interactive website
         * Integration code 
      * For the prior, user/pw is about as standard as it gets, for pretty good reason - I'd probably add some reasonable password complexity and maybe MFA if needed to increase security here.
      * For the latter, secret key exchange is simple, fast, easy to use, high entropy, and provides reasonable security assuming the consumer can keep a secret. 
      * As for why JWT for claims - they are self contained (allowing for flexibility on the server in terms of separating services without direct access to credential stores), they are signed (i.e. they provide data-integrity), and they can be easily encrypted as well. They're also JSON, so see above for benefits there. 
