# legopoc

The FileWatcherLibrary  has the functions for any partner to subscribe to it to get event notifications which servers as a library to the partners
The PartnerApp is the implementation to mock each partner and how they can get their own FileWatcher system up and running , sending it events watching a particular directory.

To make it work on local
1. Create an empty soln in vs
2. Import both these solutions into vs
3. In the PartnerApp solution , add FileWatcherLibrary library in the project references.
4. (Not mandatory)If required you can add your own implementation of the partner processing any event that is received in the "ProcessFileEvent" function in the Partner.cs file inside the PartnerApp. Currently it just has some delay to simulate processing

5. Now you can open a cmd prompt and cd to <path>\PartnerApp\bin\Debug\net8.0 where you will see the PartnerApp.exe
6. Now run command like - `PartnerApp Partner1 "C:\Path\To\Directory1"` ( ensure that the directory exist otherwise you'll get an error)  ( Partner1 is partner_id )
7. Now try creating/updating files in the folder above and see it work. You can create multiple partners in diff cmd prompts using different partner_id and paths.
8. (Not mandatory) If you want to simulate high file creation/update you can use (https://github.com/rohan-dassani/lego2/blob/main/lego2test1.0/FileGenerator.cs) \
        `FileGenerator generator = new FileGenerator(sourceDirectory);` \
        `await generator.GenerateFiles(fileCount, duration, threadCount , fileSizeInKB); // initialize the variables with the values you need.`
