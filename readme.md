#Azure Mobile Apps - structured data sync with files

## Deploy

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

## Overview

The Azure Mobile Apps client and server SDK support offline sync of structured data with CRUD operations against the /tables endpoint. Generally this data is stored in a database or similar store, and generally these data stores cannot store large binary data efficiently. Also, some applications have related data that is stored elsewhere (e.g., blob storage, SharePoint), and it is useful to be able to create associations between records in the /tables endpoint and other data.

The file management feature of the Azure Mobile Apps SDK removes these limitations, and supports the following:

- Secure client and server model, using SAS tokens (shared access signature) for client access to blob storage:
    - Turnkey methods for requesting and retrieving SAS tokens with particular permissions (e.g., upload, download)
    - Flexibility over SAS expiration policy
- Scalable and efficient communication. Client performs upload and download operations directly against blob storage, so the mobile backend does not create a bottleneck

- Flexible association of files and records. Files can be associated based on container name, blob name, or any other naming convention, by supplying a custom `IContainerNameResolver`. Association can be 1:1, 1:many, etc.

- Client-side support for association of files and records. Files are simply data that is related to a record, and do not need to be managed separately by the developer.

- Flexible client-side file management. The client SDK uses file paths only, and does not place any requirements on how the files are stored on the device. 

- Flexibility over how clients download files. The client SDK has callbacks to notify of added or removed files, and the app developer can decide whether to download files immediately and store them, or download later based on user action.

- Offline sync support for file upload and download. A client app can queue upload and download operations for when there is network connectivity.


##Azure Mobile Server SDK for Files
In order to support the file management capabilities exposed by the client SDK, the following changes were made to the service:

 - A storage controller named ```TodoItemStorageController``` (inheriting from the new Mobile Apps type ```StorageController<T>```) was created
	 - Storage token issuance
	 - File delete operations
	 - File list operations
 - The storage account *connection string* was added to the service settings
	 - The default configuration key is *MS_AzureStorageAccountConnectionString*,  in the Connection Strings section of your Web App configuration.
	 - To use a different connection string value, pass it to the constructor of `StorageController`.
	 
###Storage API resources
The new controller exposes two sub-resources under the record it manages:

 - StorageToken
	 - HTTP POST : Creates a storage token
		 - ```/tables/{item_type_name}/{id}/MobileServiceFiles```
 - MobileServiceFiles
	 - HTTP GET: Retrieves a list of files associated with the record
		 - ```/tables/{item_type_name}/{id}/MobileServiceFiles```
	 - HTTP DELETE: Deletes the file specified in the file resource identifier
		 - ```/tables/{item_type_name}/{id}/MobileServiceFiles/{fileid}```

###IContainerNameResolver
The *To do list* sample uses the default behavior, with one container per record. If a custom container mapping or naming convention is desired, a custom ```IContainerNameResolver``` may be passed into the token generation or file management controller methods. If provided, this custom resolver will be used any time the runtime needs to resolve a container name for a record or file.


##Mobile Apps Client SDK
###Offline file management
The Azure Mobile Apps Client SDK provides offline file management support, allowing you to synchronize file changes when network connectivity is available.

The updated *To do list app* takes advantage of this functionality to expose the following features:

 - Allow users to associate files with *to do* items (multiple files per item)
 - All changes are local, until a user taps the *synchronize* button
 - Items and files created by other users are automatically downloaded by the application, making them available offline
 - Items and files deleted by other users are removed from the local device

When working in offline mode, file management operations are saved locally, until the application synchronizes those changes (typically when network availability is restored or when the user explicitly requests a synchronization via an application gesture).

The diagram below shows the sequence of operations for a file creation:

```sequence
Application code->Azure Mobile Apps SDK: Create file X
Azure Mobile Apps SDK->Azure Mobile Apps SDK: Queue create file X operation
Application code->Azure Mobile Apps SDK: Push file changes
Azure Mobile Apps SDK->Application code: Get file X data
Application code->Azure Mobile Apps SDK: File X data
Azure Mobile Apps SDK->Azure Storage: Upload file
```

It's important to understand that the Azure Mobile Services Client SDK will not store the file data. The client SDK will invoke your code when it needs File contents will be requested. The application (your code) decides how (and if) files are stored on the local device.

####IFileSyncHandler
The Azure Mobile Services SDK interacts with the application code as part of the file management and synchronization process. This communication takes place using the IFileSyncHandler implementation provided by the application (your code).

IFileSyncHandler is a simple interface with the following definition:

     public interface IFileSyncHandler
        {
            Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata);

            Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action);
        }

```GetDataSource``` is called when the Azure Mobile Services Client SDK needs the file data (e.g.: as part of the upload process). This gives you the ability manage how (and if) files are stored on the local device and return that information when needed.

```ProcessFileSynchronizationAction``` is invoked as part of the file synchronization flow. A file reference and a FileSynchronizationAction enumeration value are provided so you can decide how your application should handle that event (e.g. automatically downloading a file when it is created or updated, deleting a file from the local device when that file is deleted on the server).

When initializing the file synchronization runtime, your application must supply a concrete implementation of the ```IFileSyncHandler```, as shown below:

    MobileServiceClient client = new MobileServiceClient("app_url", "gateway_url", "application_key");

    // . . . Other initialization code (local store, sync context, etc.)
    client.InitializeFileSync(new MyFileSyncHandler(), store);

> The ```IFileSyncHandler``` implementation in the *To do list* application is defined in the ```TodoItemFileSyncHandler.cs``` file.

####Creating and uploading a file
The most common way of working with the file management API is through a set of extension methods on the ```IMobileServiceTable<T>``` interface, so in order to use the API, you must have a reference to the table you're working with.

    using Microsoft.WindowsAzure.MobileServices.Files;
    ...

    MobileServiceFile file = await myTable.AddFileAsync(myItem, "file_name");

In the offline scenario, the upload will occur when the application initiates a synchronization, when that happens, the runtime will begin processing the operations queue and, once it finds this operation, it will invoke the ```GetDataSource``` method on the ```IFileSynchHandler``` instance provided by the application in order to retrieve the file contents for the upload.

>The file management enabled version of the *To do list* application maintains the pattern used in 
>the *To do list* quick start and maintains all file management operations in the ```TodoItemManager.cs``` file

####Deleting a file
To delete a file, you can follow the same pattern described above and use the ```DeleteFileAsync``` method on the ```IMobileServiceTable<T>``` instance:

    using Microsoft.WindowsAzure.MobileServices.Files;
    ...

    await myTable.DeleteFileAsync(file);

In the offline scenario, the file deletion will occur when the application initiates a synchronization.

####Retrieve an item's files
As mentioned in the *Azure Mobile Services File Management* section, files are managed through its associated record. In order to retrieve an item's files, you can call the ```GetFilesAsync``` method on the  ```IMobileServiceTable<T>``` instance. 

    IEnumerable<MobileServiceFile> files = await myTable.GetFilesAsync(myItem);

This method returns a list of files associated with the data item provided. It's important to remember that this is a ***local*** operation and will return the files based on the state of the object when it was last synchronized.

To get an updated list of files from the server, you can initiate a sync operation as described in the *synchronizing file changes* section.

>On the *To do list* sample application, the item's images are retrieved in the ```TodoItemViewModel``` class.

####Client sync process

> NOTE:
> For clarity, this section describes the approach taken by the *To do list* application. This is temporary and **will** change in future iterations of the file management feature.

The *To do list* application detects record changes when performing a standard data pull operation and uses that as a trigger to retrieve potential file changes.

To detect changes, the *To do list* application uses a custom ```MobileServiceLocalStore``` (located under the *Helpers* project folder) that wraps the built in ```MobileServiceSQLiteStore``` and raises a change event when the runtime creates, updates or deletes records. This event is used by the application to initiate a file synchronization operation for the changed record.
