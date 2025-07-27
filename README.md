# ASP.NET Identity 2.1 AWS DynamoDB

This is a fork of [David Melendez's project on CodePlex](https://identitydynamodb.codeplex.com). The only differences between this repository and his:

* I removed the dependency on the older `AWSSDK` library.
* I added a dependency on the newer `AWSSDK.DynamoDBv2` and `AWSSDK.Core`.
* RoleManager.Roles lists roles in your database. It doesn't throw an UnsupportedException.

In my project I was running into ambiguous references because David's version required me to reference `AWSSDK`.

## Install from nuget.org

<https://www.nuget.org/packages/ElCamino.AspNet.Identity.Dynamo.v2/>

```
PM> Install-Package ElCamino.AspNet.Identity.Dynamo.v2
```

## Description

This project provides a high performance cloud solution for ASP.NET Identity 2.0 using AWS DynamoDB replacing the Entity Framework / MSSQL provider.

## More Information

This project is a plug-in to the ASP.NET Identity framework. A [basic understanding](http://www.asp.net/identity/overview/getting-started/introduction-to-aspnet-identity). of the ASP.NET identity framework is required.

By default, the ASP.NET Identity system stores all the user information in a Microsoft SQL database using an EntityFramework provider. This project is a replacement of the EntityFramework SQL provider to use AWS DynamoDb to persist user information such as (but not limited to): username/password, roles, claims and external login information.

In summary, the ASP.NET Identity 2.0 AWS DynamoDB provider allows one to remove the EntityFramework and Microsoft SQL dependencies from the default implementation of the ASP.NET Identity 2.0 storage provider and replace it with a high performance AWS DynamoDb solution.

Please refer to the [Documentation](http://identitydynamodb.codeplex.com/documentation?referringTitle=Home) for more detailed information about getting started. 

## Updating to the latest version of .NET

To update this project to the latest version of .NET, you will need to perform the following steps:

1.  **Update the Target Framework:**
    *   Open the `*.csproj` files in a text editor.
    *   Find the `<TargetFrameworkVersion>` element and change its value to the latest version of the .NET Framework (e.g., `v4.8`).
    *   You will need to do this for all projects in the solution.

2.  **Update NuGet Packages:**
    *   Open the solution in Visual Studio.
    *   Go to **Tools > NuGet Package Manager > Manage NuGet Packages for Solution**.
    *   Go to the **Updates** tab and update all packages to their latest versions.
    *   Pay special attention to the `Microsoft.AspNet.*` and `AWSSDK.*` packages.

3.  **Resolve Dependencies:**
    *   After updating the packages, you may encounter some dependency conflicts.
    *   You may need to manually edit the `packages.config` files to resolve these conflicts.
    *   You can also use the `Update-Package -reinstall` command in the Package Manager Console to reinstall all packages.

4.  **Build and Test:**
    *   Build the solution to ensure that all projects compile successfully.
    *   Run the tests to ensure that all functionality is still working as expected.

