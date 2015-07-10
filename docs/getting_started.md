[TOC]

#File Management for Mobile Apps - preview client and server SDK for Xamarin.Forms

Prerequisites:
- Xamarin account 
- Visual Studio 2013 for publishing server packages
- Android or iOS emulator
- A provisioned Azure Mobile App
- An Azure Storage acount



##Azure Mobile Apps File Management
TODO: Describe the goals of the functionality exposed by the SDK, the patterns and other relevant implementation/behavior details.

Examples of what we need to document:

 - Communication pattern
	 - Client, storage, service, etc.
 - Data items/files relationships (file scoping)
 - SAS issuance, caching, etc.
 - Default "no server state" behavior
	 - Examples on how to extend 
 - ???

##Mobile Services Client SDK
###Offline file management
The Azure Mobile Services Client SDK provides offline file management support, allowing you to synchronize file changes when network connectivity is available.

The updated *To do list app* takes advantage of this functionality to expose the following features:

 - Allow users to associate files with *to do* items (multiple files per item)
 - All changes are local, until a user taps the *synchronize* button
 - Items and files created by other users are automatically downloaded by the application, making them available offline
 - Items and files deleted by other users are removed from the local device

When working in offline mode, file management operations are saved locally, until the application synchronizes those changes (typically when network availability is restored or when the user explicitly requests a synchronization via an application gesture).

The diagram below shows the sequence of operations for a file creation:

```sequence
Application code->Azure Mobile Services SDK: Create file X
Azure Mobile Services SDK->Azure Mobile Services SDK: Queue create file X operation
Application code->Azure Mobile Services SDK: Push file changes
Azure Mobile Services SDK->Application code: Get file X data
Application code->Azure Mobile Services SDK: File X data
Azure Mobile Services SDK->Storage: Upload file
```

It's important to understand that the Azure Mobile Services Client SDK will not store the file data. The client SDK will invoke your code when it needs File contents will be requested. The application (your code) decides how (and if) files are stored on the local device.

####IFileSyncHandler
The Azure Mobile Services SDK interacts with the application code as part of the file management and synchronization process. This communication takes place using the IFileSyncHandler implementation provided by the application (your code).

IFileSyncHandler is a simple interface with the following definition:

```c#
 public interface IFileSyncHandler
    {
        Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata);

        Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action);
    }
```
```GetDataSource``` is called when the Azure Mobile Services Client SDK needs the file data (e.g.: as part of the upload process). This gives you the ability manage how (and if) files are stored on the local device and return that information when needed.

```ProcessFileSynchronizationAction``` is invoked as part of the file synchronization flow. A file reference and a FileSynchronizationAction enumeration value are provided so you can decide how your application should handle that event (e.g. automatically downloading a file when it is created or updated, deleting a file from the local device when that file is deleted on the server).

When initializing the file synchronization runtime, your application must supply a concrete implementation of the ```IFileSyncHandler```, as shown below:

```c#
MobileServiceClient client = new MobileServiceClient("app_url", "gateway_url", "application_key");

// . . . Other initialization code (local store, sync context, etc.)
client.InitializeFileSync(new MyFileSyncHandler(), store);
```

> The ```IFileSyncHandler``` implementation in the *To do list* application is defined in the ```TodoItemFileSyncHandler.cs``` file.

####Creating and uploading a file
The most common way of working with the file management API is through a set of extension methods on the ```IMobileServiceTable<T>``` interface, so in order to use the API, you must have a reference to the table you're working with.

```c#
MobileServiceFile file = await myTable.AddFileAsync(myItem, "file_name");
``` 

The following using statement is also required:
```c#
using Microsoft.WindowsAzure.MobileServices.Files;
```

In the offline scenario, the upload will occur when the application initiates a synchronization, when that happens, the runtime will begin processing the operations queue and, once it finds this operation, it will invoke the ```GetDataSource``` method on the ```IFileSynchHandler``` instance provided by the application in order to retrieve the file contents for the upload.

>The file management enabled version of the *To do list* application maintains the pattern used in 
>the *To do list* quick start and maintains all file management operations in the ```TodoItemManager.cs``` file

####Deleting a file
To delete a file, you can follow the same pattern described above and use the ```DeleteFileAsync``` method on the ```IMobileServiceTable<T>``` instance:

```c#
await myTable.DeleteFileAsync(file);
``` 

As with the create file example, the following using statement is also required:
```c#
using Microsoft.WindowsAzure.MobileServices.Files;
```

In the offline scenario, the file deletion will occur when the application initiates a synchronization.

####Retrieve an item's files
As mentioned in the *Azure Mobile Services File Management* section, files are managed through its associated record. In order to retrieve an item's files, you can call the ```GetFilesAsync``` method on the  ```IMobileServiceTable<T>``` instance. 

```c#
IEnumerable<MobileServiceFile> files = await myTable.GetFilesAsync(myItem);
``` 
This method returns a list of files associated with the data item provided. It's important to remember that this is a ***local*** operation and will return the files based on the state of the object when it was last synchronized.

To get an updated list of files from the server, you can initiate a sync operation as described in the *synchronizing file changes* section.

>On the *To do list* sample application, the item's images are retrieved in the ```TodoItemViewModel``` class.

####Client sync process

> NOTE:
> For clarity, this section describes the approach taken by the *To do list* application. This is temporary and **will** change in future iterations of the file management feature.

The *To do list* application detects record changes when performing a standard data pull operation and uses that as a trigger to retrieve potential file changes.

To detect changes, the *To do list* application uses a custom ```MobileServiceLocalStore``` (located under the *Helpers* project folder) that wraps the built in ```MobileServiceSQLiteStore``` and raises a change event when the runtime creates, updates or deletes records. This event is used by the application to initiate a file synchronization operation for the changed record.

##Service SDK
In order to support the file management capabilities exposed by the client SDK, the following changes were made to the service:

 - A storage controller named ```TodoItemStorageController``` (inheriting from the new Mobile Apps type ```StorageController<T>```) was created
	 - Storage token issuance
	 - File delete operations
	 - File list operations
 - The storage account connection string was added to the service settings
	 - Currently, the configuration is named *mS_AzureStorageAccountConnectionString*
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

