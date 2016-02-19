# ASP.NET Identity 2.1 AWS DynaomDB

This is a fork of [David Melendez's project on CodePlex](https://identitydynamodb.codeplex.com). The only differences between this repository and his:

* I removed the dependency on the older `AWSSDK` library.
* I added a dependency on the newer `AWSSDK.DynamoDBv2` and `AWSSDK.Core`.

In my project I was running into ambiguous references because David's version required me to reference `AWSSDK`.

## Description

This project provides a high performance cloud solution for ASP.NET Identity 2.0 using AWS DynamoDB replacing the Entity Framework / MSSQL provider.

## More Information

This project is a plug-in to the ASP.NET Identity framework. A [basic understanding](http://www.asp.net/identity/overview/getting-started/introduction-to-aspnet-identity). of the ASP.NET identity framework is required.

By default, the ASP.NET Identity system stores all the user information in a Microsoft SQL database using an EntityFramework provider. This project is a replacement of the EntityFramework SQL provider to use AWS DynamoDb to persist user information such as (but not limited to): username/password, roles, claims and external login information.

In summary, the ASP.NET Identity 2.0 AWS DynamoDB provider allows one to remove the EntityFramework and Microsoft SQL dependencies from the default implementation of the ASP.NET Identity 2.0 storage provider and replace it with a high performance AWS DynamoDb solution.

Please refer to the [Documentation](http://identitydynamodb.codeplex.com/documentation?referringTitle=Home) for more detailed information about getting started. 


