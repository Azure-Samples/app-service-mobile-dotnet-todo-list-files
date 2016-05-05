---
services: app-service\mobile, app-service\web, app-service
platforms: dotnet, xamarin
author: lindydonna
---

#Azure Mobile Apps - structured data sync with files

## Deploy

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

[Deploy the server project](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fapp-service-mobile-dotnet-todo-list-files%2Fmaster%2Fazuredeploy.json).

## Overview

The Azure Mobile Apps client and server SDK support offline sync of structured data with CRUD operations against the /tables endpoint. Generally this data is stored in a database or similar store, and generally these data stores cannot store large binary data efficiently. Also, some applications have related data that is stored elsewhere (e.g., blob storage, SharePoint), and it is useful to be able to create associations between records in the /tables endpoint and other data.

This sample adds support for images to the Mobile Apps todo list quickstart. 

### Features

The file management feature of the Azure Mobile Apps SDK removes these limitations, and supports the following:

- Secure client and server model, using SAS tokens (shared access signature) for client access to blob storage:
    - Turnkey methods for requesting and retrieving SAS tokens with particular permissions (e.g., upload, download)
    - Flexibility over SAS expiration policy

- Scalable and efficient communication. Client performs upload and download operations directly against blob storage, so the mobile backend does not create a bottleneck

- Flexible association of files and records. Files can be associated based on container name, blob name, or any other naming convention, by supplying a custom `IContainerNameResolver`. Association can be 1:1, 1:many, etc.

- Client-side support for association of files and records. Files are simply data that is related to a record, and do not need to be managed separately by the developer.

- Flexibility over how clients download files. The client SDK has callbacks to notify of added or removed files, and the app developer can decide whether to download files immediately and store them, or download later based on user action.

- Offline sync support for file upload and download. A client app can queue upload and download operations for when there is network connectivity.

