# legopoc

The FileWatcherLibrary  has the functions for any partner to subscribe to it to get event notifications which servers as a library to the partners
The PartnerApp is the implementation to mock each partner and how they can get their own FileWatcher system up and running , sending it events watching a particular directory.

To make it work on local
1. Create an empty soln in vs
2. Import both these solutions into vs
3. In the PartnerApp solution , add FileWatcherLibrary library in the project references.
4. (Not mandatory)If required you can add your own implementation of the partner processing any event that is received in the "ProcessFileEvent" function in the Partner.cs file inside the PartnerApp. Currently it just has some delay to simulate processing

5. Now you can open a cmd prompt and cd to <path>\PartnerApp\bin\Debug\net8.0 where you will see the PartnerApp.exe
6. Now run command like - `PartnerApp Partner1 "C:\Path\To\Directory1"` ( ensure that the directory exist otherwise you'll get an error)
7. Now try creating/updating files in the folder above and see it work.
