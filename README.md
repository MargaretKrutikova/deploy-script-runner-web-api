# Deploy script runner Web API

This project is an ASP.NET Core web api that can be used for running any kinds of scripts located on the same machine with the api. The initial purpose of the project was to run deployment scripts to facilitate updating of web services on remote machines. The api can however be used to run general-purpose scripts like setting watchdog flags, performing some cleanup on the servers (e.g. clearing cache), updating some statuses etc.

The api is RESTful except for a few functional api methods for reloading configuration from the settings file. The implementation utilizes the concept of long running jobs to represent running scripts as API resources manipulated via URIs. The api is secured with [JSON web tokens](https://jwt.io/introduction/). There is a swagger documentation available for the api, see [swagger section](#swagger).

There is also a command line utility that helps to generate settings for `appsettings.json` and can be used for production settings when deploying the api live.

*NOTE: the project has been recently migrated to .net core version 2.0.*

## Getting Started

Clone the repository.
```shell
git clone https://github.com/MargaretKrutikova/deploy-script-runner-web-api.git
```

### Web API

1. Install latest [.NET core](https://www.microsoft.com/net/core). 
If latest is incompatible with the current version of .net core for this project (2.0)
download .NET core SDK 2.0 from [release archives](https://github.com/dotnet/core/blob/master/release-notes/download-archives/2.0.0-download.md).

2. Restore and run the web api
```shell
cd DeployService/DeployServiceWebApi
dotnet restore
dotnet run
```

3. Navigate to http://localhost:5000/api.

4. To test running scripts locally go to `resources` folder and edit `template-settings.json` with custom configuration and local paths to the scripts that the api will run.

Check section [deployment](#deployment) on how to setup the project in production.

### Settings utility

There is a small utility located under `tools\SettingsGeneratorUtility` that facilitates configuring some sections of the api's settings.

To run the utility execute

```
cd tools/SettingsGeneratorUtility/
dotnet run
```

Enter password for the authorized user of the api and the utility will output hash of the password and will generate a key that can be used for signing JWTs.

For more information on settings see section [configuration](#configuration).

### Swagger

In order to acces the api swagger documentation, setup and run the api locally first. The json documentation is available at
http://localhost:5000/swagger/v1/swagger.json and the swagger UI at http://localhost:5000/swagger/.

In order to access protected endpoints, authorization bearer should be sent in the header:
1. Call `/api/auth/token` with the right credentials. Enter username and password that are setup locally in the `appsettings.json`. If the template configuration in the `appsettings.json` hasn't been changed, enter `testUser` as username
and `testPassword` as password.
2. In the response body copy the value of the field `token`.
3. Click on the `Authorize` button in the right upper corner of the page.
4. In the field `api_key` enter `Bearer ` and paste the copied token.
*NOTE: it won't work with just the token without the string `Bearer `.*
5. Test the authorization by for example calling GET on `/api/jobs` and make sure you receive status 200.

## Deployment

The instructions are focused on deploying the web api in IIS on a live system.

1. Set up .NET core environment on the host machine
Install .net core [hosting bundle for .net core 2.0](https://aka.ms/dotnetcore.2.0.0-windowshosting).

Restart the system or execute `net stop was /y` followed by `net start w3svc` to pick up changes to the system PATH, see [microsoft guide](https://docs.microsoft.com/en-us/aspnet/core/publishing/iis?tabs=aspnetcore2x) for more details.

2. Publish the web api locaclly
```
cd DeployService/DeployServiceWebApi
dotnet restore
dotnet publish -c Release
```
The published files are located under `\bin\Release\netcoreapp2.0\publish`.

3. On the hosting machine create a folder for the application's deployment files and copy the files from the local `publish` folder into it.

*NOTE: settings on the live web api located in the appsettings.json have to be changed, see section configuration for more detail.*

3. Create application pool

*NOTE: .NET CLR Version should be set to No Managed code.*

**NOTE: if running scripts with svn commands set application pool to Local System
to avoid errors with missing svn config.*  

4. Create IIS Website on the hosting machine
Add IIS website which points to the application's deployment folder and uses the application pool created in the previous step. 

More detailed guide can be found on the microsoft docs on [how to host .net core on windows with iis](https://docs.microsoft.com/en-us/aspnet/core/publishing/iis?tabs=aspnetcore2x).

5. Create a folder with the scripts settings file.

An example is located under `resources\template-settings.json`.  All the paths should be configured for the hosting machines. The path to the file should be specified in `appsettings.json`, see section [configuration](#configuration) on how to configure `appsettings`.

## Configuration
`appsettings.json` contains following sections that can/should be configured on the hosting machine:

### Serilog

Logging configuration, for more information see [serilog settings configuration](https://github.com/serilog/serilog-settings-configuration) and [serilog sinks rolling file](https://github.com/serilog/serilog-sinks-rollingfile).

### JwtOptions

Must be configured for security reasons.

JSON web token options for managing jwt: signature key and tokens' expiration time. See [jwt.io](https://jwt.io/introduction/) for more information.

The signature key can be generated with the settings utility included in the project, see section [settings utility](#settings-utility) on how to generate a signature key.


### AuthorizationOptions

Must be configured for security reasons.

Currently there is support for only one user account that will be authorized when using the api. Username and password for the account should be specified in this section, username is included in clear text, password is hashed with `sha512` and encoded in `base64`.

Hash of the password can be generated using `SettingsGeneratorUtility` included in the project. See section [settings utility](#settings-utility) on how to get hash of the password.

### CorsOrigins

Origins that will be included in the response header `Access-Control-Allow-Origin`. For more see [mdn web docs](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Origin).

### JobCleanupIntervalMinutes

Sets time interval that will trigger cleanup of the information about jobs stored in memory
(all finished and running jobs are located in memory).

### DeploySettingsPath

Path to the file with paths to scripts that the api can run. There is a template file located under `resources\template-settings.json`.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details